using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
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
        private const string MotionRecoveryAxisName = "_LMCAxis1";
        private const ushort MotionRecoveryAxisReference = 1;
        private const string MotionRecoveryOperation = "Move Absolute";

        internal static void RegisterMotionRecoveryIntegrationTests(
            ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.MotionRecovery.ActiveArmPromotesAtStartup",
                ActiveMotionArmPromotesAtStartup);
            tests.Add(
                "Wpf.MotionRecovery.EndpointMismatchIsZeroTcpRpc",
                MotionRecoveryEndpointMismatchIsZeroTcpAndRpc);
            tests.Add(
                "Wpf.MotionRecovery.BootIdMismatchRetainsReadOnlyConnection",
                MotionRecoveryBootIdMismatchIsMutationZero);
            tests.Add(
                "Wpf.MotionRecovery.MapRevisionMismatchRetainsReadOnlyConnection",
                MotionRecoveryMapRevisionMismatchIsMutationZero);
            tests.Add(
                "Wpf.MotionRecovery.TargetReferenceMismatchIsReadOnly",
                MotionRecoveryTargetReferenceMismatchIsReadOnly);
            tests.Add(
                "Wpf.MotionRecovery.ExactStopResolvesWithoutMoveReplay",
                ExactMotionRecoveryStopResolvesWithoutMoveReplay);
            tests.Add(
                "Wpf.MotionRecovery.StopMonitorAllowsPriorityPowerOffWithoutStopReplay",
                StopMonitorAllowsPriorityPowerOffWithoutStopReplay);
            tests.Add(
                "Wpf.MotionRecovery.GroupStopMonitorAllowsPriorityPowerOffWithoutStopReplay",
                GroupStopMonitorAllowsPriorityPowerOffWithoutStopReplay);
            tests.Add(
                "Wpf.MotionRecovery.StartupPromotionReplaceFailureKeepsExactLatchZeroTcp",
                StartupPromotionReplaceFailureKeepsExactLatchZeroTcp);
            tests.Add(
                "Wpf.MotionRecovery.MoveFreshCapabilityDriftIsZeroMutationAndZeroArm",
                MoveFreshCapabilityDriftIsZeroMutationAndZeroArm);
            tests.Add(
                "Wpf.MotionRecovery.SafetyFreshCapabilityDriftIsZeroMutation",
                SafetyFreshCapabilityDriftIsZeroMutation);
            tests.Add(
                "Wpf.MotionRecovery.StatusOnlyCannotResolveStartupRecovery",
                StatusOnlyCannotResolveStartupRecovery);
            tests.Add(
                "Wpf.MotionRecovery.PowerOffRequiresThreeConsecutiveSafeSamples",
                PowerOffRequiresThreeConsecutiveSafeSamples);
            tests.Add(
                "Wpf.MotionRecovery.PowerOffMonitorBlocksReplayAndAllowsPriorityStop",
                PowerOffMonitorBlocksReplayAndAllowsPriorityStop);
            tests.Add(
                "Wpf.MotionRecovery.PowerOffStatusFailureSecondClickResumesWithoutReplay",
                PowerOffStatusFailureSecondClickResumesWithoutReplay);
            tests.Add(
                "Wpf.MotionRecovery.PowerOffAcceptedBoundaryResumesStatusOnly",
                PowerOffAcceptedBoundaryResumesStatusOnly);
            tests.Add(
                "Wpf.MotionRecovery.PowerOffAcceptedBoundaryRecordsMotionSafetyBeforeProof",
                PowerOffAcceptedBoundaryRecordsMotionSafetyBeforeProof);
            tests.Add(
                "Wpf.MotionRecovery.PowerOffInterferenceRetainsPendingAndBlocksAxisReload",
                PowerOffInterferenceRetainsPendingAndBlocksAxisReload);
            tests.Add(
                "Wpf.MotionRecovery.PowerOffRejectedReplacementPreservesConfirmedInterference",
                PowerOffRejectedReplacementPreservesConfirmedInterference);
            tests.Add(
                "Wpf.MotionRecovery.AcceptedMoveFinalCapabilityDriftKeepsLatch",
                AcceptedMoveFinalCapabilityDriftKeepsLatch);
        }

        private static void ActiveMotionArmPromotesAtStartup()
        {
            var journalRoot = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                const int endpointPort = 5501;
                var identity = CreateArmedAxisMotionJournal(
                    journalRoot,
                    endpointPort,
                    DiagnosticsBootId,
                    DiagnosticMapRevision);

                window = CreateWindow(journalRoot, endpointPort);
                var journal = GetMotionUncertaintyJournal(window);
                AssertEx.True(journal.HasActiveRecord);
                AssertEx.Equal(identity, journal.CurrentRecord.Identity);
                AssertEx.Equal(
                    MotionUncertaintyState.RecoveryRequired,
                    journal.CurrentRecord.State,
                    "Startup must durably promote an active pre-dispatch arm.");
                AssertEx.True((bool)GetPrivateField(
                    window,
                    "motionUncertaintyRecoveredAtStartup"));
                AssertEx.True((bool)GetPrivateField(
                    window,
                    "motionMayBeActive"));
                AssertEx.Equal(
                    MotionRecoveryAxisName,
                    window.TextAxisName.Text);
                AssertEx.Equal(
                    endpointPort.ToString(CultureInfo.InvariantCulture),
                    window.TextRemotePort.Text);
                AssertEx.False(window.ButtonMoveAbsolute.IsEnabled);
                AssertEx.True(window.ButtonConnect.IsEnabled);

                ForceCloseMotionRecoveryWindow(window);
                window = null;

                using (var reopened = MotionUncertaintyJournal.Open(
                    GetMotionUncertaintyJournalDirectory(journalRoot)))
                {
                    AssertEx.True(reopened.HasActiveRecord);
                    AssertEx.Equal(
                        MotionUncertaintyState.RecoveryRequired,
                        reopened.CurrentRecord.State);
                    AssertEx.Equal(identity, reopened.CurrentRecord.Identity);
                }
            }
            finally
            {
                ForceCloseMotionRecoveryWindow(window);
                DeleteJournalDirectory(journalRoot);
            }
        }

        private static void MotionRecoveryEndpointMismatchIsZeroTcpAndRpc()
        {
            var journalRoot = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer())
                {
                    CreateArmedAxisMotionJournal(
                        journalRoot,
                        server.Port,
                        DiagnosticsBootId,
                        DiagnosticMapRevision);
                    window = CreateWindow(journalRoot, server.Port);
                    window.TextRemotePort.Text = DifferentPort(server.Port)
                        .ToString(CultureInfo.InvariantCulture);

                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Connect failed",
                            StringComparison.Ordinal),
                        "Endpoint drift did not fail before TCP connect.");

                    AssertEx.Equal(
                        0,
                        server.AcceptedClientCount,
                        "Endpoint mismatch reached the TCP listener.");
                    AssertEx.Equal(
                        0,
                        server.ReceivedRequests.Count,
                        "Endpoint mismatch sent an RPC request.");
                    AssertEx.Contains(
                        "does not match the durable motion recovery record",
                        window.TextExecutionLog.Text);
                    AssertEx.True(
                        GetMotionUncertaintyJournal(window).HasActiveRecord);
                }
            }
            finally
            {
                ForceCloseMotionRecoveryWindow(window);
                DeleteJournalDirectory(journalRoot);
            }
        }

        private static void MotionRecoveryBootIdMismatchIsMutationZero()
        {
            AssertConnectionIdentityMismatchIsMutationZero(
                DiagnosticsBootId + 1,
                DiagnosticMapRevision,
                "DiagnosticsBootId");
        }

        private static void MotionRecoveryMapRevisionMismatchIsMutationZero()
        {
            AssertConnectionIdentityMismatchIsMutationZero(
                DiagnosticsBootId,
                DiagnosticMapRevision + 1,
                "MapRevision");
        }

        private static void AssertConnectionIdentityMismatchIsMutationZero(
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

            var journalRoot = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    CreateArmedAxisMotionJournal(
                        journalRoot,
                        server.Port,
                        DiagnosticsBootId,
                        DiagnosticMapRevision);
                    window = CreateWindow(journalRoot, server.Port);
                    var journal = GetMotionUncertaintyJournal(window);
                    var preservedIdentity = journal.CurrentRecord.Identity;
                    var preservedState = journal.CurrentRecord.State;

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
                            + " mismatch did not retain the read-only connection.");

                    AssertEx.True(window.ButtonDiagnosticsCapabilities.IsEnabled);
                    AssertEx.True(window.ButtonCloseConnection.IsEnabled);
                    AssertEx.False(window.ButtonLookupAxis.IsEnabled);
                    AssertEx.False(window.ButtonStop.IsEnabled);
                    AssertEx.False(window.ButtonPowerOff.IsEnabled);
                    Click(window.ButtonDiagnosticsCapabilities);
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Refresh Diagnostics Capabilities completed",
                            StringComparison.Ordinal),
                        "Ordinary non-D5 read-only capability inspection was blocked.");

                    AssertNoMotionMutationRequests(
                        server.ReceivedRequests,
                        mismatchName
                            + " mismatch sent a motion mutation.");
                    AssertEx.Contains(
                        "RECOVERY IDENTITY READ-ONLY QUARANTINE",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "Stored BootId=0x",
                        window.TextExecutionLog.Text);
                    AssertEx.True(journal.HasActiveRecord);
                    AssertEx.Equal(
                        preservedIdentity,
                        journal.CurrentRecord.Identity);
                    AssertEx.Equal(preservedState, journal.CurrentRecord.State);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }

                using (var reopened = MotionUncertaintyJournal.Open(
                    GetMotionUncertaintyJournalDirectory(journalRoot)))
                {
                    AssertEx.True(reopened.HasActiveRecord);
                    AssertEx.Equal(
                        MotionUncertaintyState.RecoveryRequired,
                        reopened.CurrentRecord.State);
                    AssertEx.Equal(DiagnosticsBootId,
                        reopened.CurrentRecord.DiagnosticsBootId);
                    AssertEx.Equal(DiagnosticMapRevision,
                        reopened.CurrentRecord.MapRevision);
                }
            }
            finally
            {
                ForceCloseMotionRecoveryWindow(window);
                DeleteJournalDirectory(journalRoot);
            }
        }

        private static void MotionRecoveryTargetReferenceMismatchIsReadOnly()
        {
            const ushort returnedReference = 2;
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateExactMotionRecoveryConnectSteps(capabilities);
            steps.Add(MotionRecoveryAxisLookupStep(
                MotionRecoveryAxisName,
                returnedReference));
            steps.Add(D5AxisInfoStep(returnedReference));
            steps.Add(CloseStep());

            var journalRoot = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    CreateArmedAxisMotionJournal(
                        journalRoot,
                        server.Port,
                        DiagnosticsBootId,
                        DiagnosticMapRevision);
                    window = CreateWindow(journalRoot, server.Port);
                    ConnectExactMotionRecovery(window);

                    AssertEx.True(window.ButtonLookupAxis.IsEnabled);
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Load Axis failed",
                            StringComparison.Ordinal),
                        "A mismatched loaded axis reference did not fail closed.");

                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x103C),
                        "Recovery target lookup was not sent exactly once.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x202B),
                        "Recovery AxisInfo validation was not sent exactly once.");
                    AssertNoMotionMutationRequests(
                        server.ReceivedRequests,
                        "Reference mismatch sent a motion mutation.");
                    AssertEx.False(window.ButtonStop.IsEnabled);
                    AssertEx.False(window.ButtonPowerOff.IsEnabled);
                    AssertEx.Contains(
                        "loaded axis does not match",
                        window.TextExecutionLog.Text);
                    AssertEx.True(
                        GetMotionUncertaintyJournal(window).HasActiveRecord);

                    CloseRecoveryConnectionForTest(window);
                    ForceCloseMotionRecoveryWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                ForceCloseMotionRecoveryWindow(window);
                DeleteJournalDirectory(journalRoot);
            }
        }

        private static void ExactMotionRecoveryStopResolvesWithoutMoveReplay()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateExactMotionRecoveryConnectSteps(capabilities);
            steps.Add(D5AxisLookupStep(MotionRecoveryAxisReference));
            steps.Add(D5AxisInfoStep(MotionRecoveryAxisReference));
            steps.Add(MotionRecoveryCapabilitiesStep(
                12,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(MotionRecoveryStopStep(MotionRecoveryAxisReference));
            for (var sample = 0; sample < 3; sample++)
            {
                steps.Add(D5SafeAxisStatusStep());
            }
            steps.Add(MotionRecoveryCapabilitiesStep(
                13,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(MotionRecoveryCapabilitiesStep(
                14,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));

            steps.Add(CloseStep());

            var journalRoot = CreateJournalDirectory();
            MainWindow window = null;
            Guid identity = Guid.Empty;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    identity = CreateArmedAxisMotionJournal(
                        journalRoot,
                        server.Port,
                        DiagnosticsBootId,
                        DiagnosticMapRevision);
                    window = CreateWindow(journalRoot, server.Port);
                    ConnectExactMotionRecovery(window);

                    AssertNoMoveRequests(
                        server.ReceivedRequests,
                        "Reconnect replayed a Move before target lookup.");
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Load Axis completed",
                            StringComparison.Ordinal),
                        "The exact recovery axis did not load.");
                    AssertEx.True(window.ButtonStop.IsEnabled);
                    AssertEx.True(window.ButtonPowerOff.IsEnabled);
                    AssertEx.False(window.ButtonMoveAbsolute.IsEnabled);

                    Click(window.ButtonStop);
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Stop verified",
                            StringComparison.Ordinal),
                        "Stop and three stable samples did not complete recovery.");

                    var journal = GetMotionUncertaintyJournal(window);
                    AssertEx.False(journal.HasActiveRecord);
                    AssertEx.Equal(identity, journal.CurrentRecord.Identity);
                    AssertEx.Equal(
                        MotionUncertaintyState.Resolved,
                        journal.CurrentRecord.State);
                    AssertEx.False((bool)GetPrivateField(
                        window,
                        "motionMayBeActive"));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2022),
                        "Recovery Stop was not sent exactly once.");
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2023),
                        "Recovery unexpectedly sent PowerOff in the Stop path.");
                    AssertEx.Equal(
                        3,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2028),
                        "Recovery did not require exactly three stable status samples.");
                    AssertEx.Contains(
                        "StopAccepted=True, AckPresent=True",
                        window.TextAxisResult.Text);
                    AssertEx.Contains(
                        "TransportInvalidatedAtDeadline=False",
                        window.TextAxisResult.Text);
                    AssertNoMoveRequests(
                        server.ReceivedRequests,
                        "Exact recovery replayed a Move command.");

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }

                using (var reopened = MotionUncertaintyJournal.Open(
                    GetMotionUncertaintyJournalDirectory(journalRoot)))
                {
                    AssertEx.False(reopened.HasActiveRecord);
                    AssertEx.Equal(identity, reopened.CurrentRecord.Identity);
                    AssertEx.Equal(
                        MotionUncertaintyState.Resolved,
                        reopened.CurrentRecord.State,
                        "The durable Resolved tombstone did not survive window close.");
                }
            }
            finally
            {
                ForceCloseMotionRecoveryWindow(window);
                DeleteJournalDirectory(journalRoot);
            }
        }

        private static void StopMonitorAllowsPriorityPowerOffWithoutStopReplay()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var firstStatusReceived = new ManualResetEventSlim(false);
            var releaseFirstStatus = new ManualResetEventSlim(false);
            var steps = CreateExactMotionRecoveryConnectSteps(capabilities);
            steps.Add(D5AxisLookupStep(MotionRecoveryAxisReference));
            steps.Add(D5AxisInfoStep(MotionRecoveryAxisReference));
            steps.Add(MotionRecoveryCapabilitiesStep(
                12,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(MotionRecoveryStopStep(MotionRecoveryAxisReference));
            steps.Add(DelayedMotionRecoveryAxisStatusStep(
                true,
                firstStatusReceived,
                releaseFirstStatus));
            steps.Add(MotionRecoveryCapabilitiesStep(
                13,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(MotionRecoveryPowerOffStep(
                MotionRecoveryAxisReference));
            steps.Add(MotionRecoveryAxisStatusStep(false));
            steps.Add(MotionRecoveryAxisStatusStep(false));
            steps.Add(MotionRecoveryAxisStatusStep(false));
            steps.Add(MotionRecoveryCapabilitiesStep(
                14,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(MotionRecoveryCapabilitiesStep(
                15,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(CloseStep());

            var journalRoot = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (firstStatusReceived)
                using (releaseFirstStatus)
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    CreateRecoveryRequiredAxisMotionJournal(
                        journalRoot,
                        server.Port);
                    window = CreateWindow(journalRoot, server.Port);
                    ConnectExactMotionRecovery(window);
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => window.ButtonStop.IsEnabled,
                        "The Stop recovery command was not enabled.");

                    Click(window.ButtonStop);
                    WaitUntil(
                        () => firstStatusReceived.IsSet,
                        "The split Stop monitor did not reach 0x2028.");
                    AssertEx.False(
                        window.ButtonStop.IsEnabled,
                        "An accepted durable Stop must not be replayable during status-only verification.");
                    AssertEx.True(
                        window.ButtonPowerOff.IsEnabled,
                        "Power Off must remain available during Stop verification.");

                    var coordinator =
                        (LMCSendPriorityCoordinator)GetPrivateField(
                            window,
                            "sendPriorityCoordinator");
                    var firstGeneration = coordinator.CurrentGeneration;
                    var capabilityReadsBeforePowerOff =
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E00);
                    Click(window.ButtonPowerOff);
                    WaitUntil(
                        () => coordinator.CurrentGeneration > firstGeneration,
                        "Power Off did not reserve newer safety priority.");
                    releaseFirstStatus.Set();

                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Power Off verified",
                            StringComparison.Ordinal),
                        "Power Off did not preempt Stop status verification and complete stable proof.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2022),
                        "Preemption replayed the accepted Stop request.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2023),
                        "The explicit Power Off was not sent exactly once.");
                    AssertEx.Equal(
                        4,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2028),
                        "Expected one preempted Stop poll and three Power Off proof polls.");
                    AssertEx.Equal(
                        capabilityReadsBeforePowerOff + 3,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E00),
                        "Power Off must reuse the fresh motion pre-dispatch identity and retain separate Axis Power and motion final identity gates.");
                    AssertEx.Contains(
                        "was discarded because a newer Stop or Power Off request was reserved",
                        window.TextExecutionLog.Text);
                    AssertNoMoveRequests(
                        server.ReceivedRequests,
                        "Stop-to-PowerOff preemption replayed Move.");

                    AssertEx.True(
                        GetPrivateField(
                            window,
                            "pendingAxisStopWaitContinuation") == null,
                        "Stable Power Off proof did not retire the accepted Stop continuation.");
                    var axisCommandJournal =
                        GetAxisCommandJournal(window);
                    AssertEx.False(axisCommandJournal.HasActiveRecord);
                    AssertEx.Equal(
                        AxisCommandRecoveryOperation.Stop,
                        axisCommandJournal.CurrentRecord.Operation);
                    AssertEx.Equal(
                        AxisCommandRecoveryState.Resolved,
                        axisCommandJournal.CurrentRecord.State);
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2022));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                releaseFirstStatus.Set();
                ForceCloseMotionRecoveryWindow(window);
                DeleteJournalDirectory(journalRoot);
            }
        }

        private static void
            GroupStopMonitorAllowsPriorityPowerOffWithoutStopReplay()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var firstStatusReceived = new ManualResetEventSlim(false);
            var releaseFirstStatus = new ManualResetEventSlim(false);
            var steps = CreateExactMotionRecoveryConnectSteps(capabilities);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(MotionRecoveryCapabilitiesStep(
                12,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(GroupStopStep());
            steps.Add(DelayedMotionRecoveryGroupStatusStep(
                GroupEnableWaitPowerOn | GroupEnableWaitStandby,
                firstStatusReceived,
                releaseFirstStatus));
            steps.Add(MotionRecoveryCapabilitiesStep(
                13,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(GroupPowerOffStep());
            steps.Add(GroupEnableWaitStatusStep(0));
            steps.Add(GroupEnableWaitStatusStep(0));
            steps.Add(GroupEnableWaitStatusStep(0));
            steps.Add(MotionRecoveryCapabilitiesStep(
                14,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(CloseStep());

            var journalRoot = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (firstStatusReceived)
                using (releaseFirstStatus)
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    CreateRecoveryRequiredGroupMotionJournal(
                        journalRoot,
                        server.Port);
                    window = CreateWindow(journalRoot, server.Port);
                    ConnectExactMotionRecovery(window);
                    Click(window.ButtonLookupGroup);
                    WaitUntil(
                        () => string.Equals(
                            window.TextGroupReference.Text,
                            GroupEnableWaitReference.ToString(
                                CultureInfo.InvariantCulture),
                            StringComparison.Ordinal),
                        "The Group Stop recovery target did not load.");
                    WaitUntil(
                        () => window.ButtonGroupStop.IsEnabled,
                        "The Group Stop recovery command was not enabled.");

                    Click(window.ButtonGroupStop);
                    WaitUntil(
                        () => firstStatusReceived.IsSet,
                        "The split Group Stop monitor did not reach 0x2045.");
                    AssertEx.True(
                        window.ButtonGroupStop.IsEnabled,
                        "A newer Group Stop must remain available during status-only verification.");
                    AssertEx.True(
                        window.ButtonGroupPowerOff.IsEnabled,
                        "Group Power Off must remain available during Group Stop verification.");

                    var coordinator =
                        (LMCSendPriorityCoordinator)GetPrivateField(
                            window,
                            "sendPriorityCoordinator");
                    var firstGeneration = coordinator.CurrentGeneration;
                    Click(window.ButtonGroupPowerOff);
                    WaitUntil(
                        () => coordinator.CurrentGeneration > firstGeneration,
                        "Group Power Off did not reserve newer safety priority.");
                    releaseFirstStatus.Set();

                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Group Power Off verified",
                            StringComparison.Ordinal),
                        "Group Power Off did not preempt Group Stop verification and complete stable proof.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2085),
                        "Preemption replayed the accepted Group Stop request.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x204B),
                        "The explicit Group Power Off was not sent exactly once.");
                    AssertEx.Equal(
                        4,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045),
                        "Expected one preempted Group Stop poll and three Group Power Off proof polls.");
                    AssertEx.Contains(
                        "was discarded because a newer Stop or Power Off request was reserved",
                        window.TextExecutionLog.Text);

                    var pendingStop = GetPrivateField(
                        window,
                        "pendingGroupStopWaitContinuation")
                        as LMCGroupStopWaitContinuation;
                    AssertEx.True(
                        pendingStop != null && pendingStop.IsPending,
                        "A preempted Group Stop monitor must retain its accepted continuation until object cleanup.");
                    InvokePrivate(window, "ClearLoadedObjects");
                    AssertEx.True(
                        GetPrivateField(
                            window,
                            "pendingGroupStopWaitContinuation") == null,
                        "ClearLoadedObjects retained the volatile Group Stop continuation.");

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }

                using (var reopened = MotionUncertaintyJournal.Open(
                    GetMotionUncertaintyJournalDirectory(journalRoot)))
                {
                    AssertEx.False(
                        reopened.HasActiveRecord,
                        "Stable Group Power Off proof did not durably resolve motion uncertainty.");
                }
            }
            finally
            {
                releaseFirstStatus.Set();
                ForceCloseMotionRecoveryWindow(window);
                DeleteJournalDirectory(journalRoot);
            }
        }

        private static void
            StartupPromotionReplaceFailureKeepsExactLatchZeroTcp()
        {
            var journalRoot = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer())
                {
                    var identity = CreateArmedAxisMotionJournal(
                        journalRoot,
                        server.Port,
                        DiagnosticsBootId,
                        DiagnosticMapRevision);
                    var journalPath = Path.Combine(
                        GetMotionUncertaintyJournalDirectory(journalRoot),
                        MotionUncertaintyJournal.JournalFileName);
                    using (var replaceBlocker = new FileStream(
                        journalPath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read))
                    {
                        window = CreateWindow(journalRoot, server.Port);
                        var journal = GetMotionUncertaintyJournal(window);
                        AssertEx.True(journal.HasActiveRecord);
                        AssertEx.Equal(identity, journal.CurrentRecord.Identity);
                        AssertEx.Equal(
                            MotionUncertaintyState.ArmedBeforeDispatch,
                            journal.CurrentRecord.State,
                            "A failed startup File.Replace must retain the loaded Armed record.");
                        AssertEx.True((bool)GetPrivateField(
                            window,
                            "motionMayBeActive"));
                        AssertEx.Equal(
                            MotionUncertaintyTargetKind.Axis,
                            (MotionUncertaintyTargetKind)GetPrivateField(
                                window,
                                "motionTargetKind"));
                        AssertEx.Equal(
                            MotionRecoveryAxisReference,
                            (ushort)GetPrivateField(
                                window,
                                "motionTargetReference"));
                        AssertEx.Equal(
                            MotionRecoveryAxisName,
                            (string)GetPrivateField(
                                window,
                                "motionAxisName"));
                        AssertEx.Equal(
                            "127.0.0.1",
                            window.TextRemoteIp.Text);
                        AssertEx.Equal(
                            server.Port.ToString(
                                CultureInfo.InvariantCulture),
                            window.TextRemotePort.Text);
                        AssertEx.Contains(
                            "startup-promote-to-recovery",
                            (string)GetPrivateField(
                                window,
                                "motionUncertaintyJournalRuntimeError"));
                        AssertEx.Contains(
                            "Startup promotion failed",
                            window.TextExecutionLog.Text);

                        window.TextRemotePort.Text = DifferentPort(server.Port)
                            .ToString(CultureInfo.InvariantCulture);
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
                            "A promotion persistence failure lost the exact endpoint latch.");
                        AssertEx.Equal(
                            0,
                            server.AcceptedClientCount,
                            "Arbitrary endpoint recovery reached TCP after promotion failure.");
                        AssertEx.Equal(
                            0,
                            server.ReceivedRequests.Count,
                            "Arbitrary endpoint recovery sent RPC after promotion failure.");
                        AssertEx.True(journal.HasActiveRecord);

                        ForceCloseMotionRecoveryWindow(window);
                        window = null;
                    }
                }
            }
            finally
            {
                ForceCloseMotionRecoveryWindow(window);
                DeleteJournalDirectory(journalRoot);
            }
        }

        private static void MoveFreshCapabilityDriftIsZeroMutationAndZeroArm()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(MotionRecoveryAxisReference));
            steps.Add(D5AxisInfoStep(MotionRecoveryAxisReference));
            steps.Add(MotionRecoveryAxisStatusStep(true));
            steps.Add(D5StableAxisPositionStep(0));
            steps.Add(MotionRecoveryCapabilitiesStep(
                11,
                capabilities,
                DiagnosticsBootId + 1,
                DiagnosticMapRevision + 1));
            steps.Add(CloseStep());

            var journalRoot = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(journalRoot, server.Port);
                    ConnectWithoutMotionRecovery(window);
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Load Axis completed",
                            StringComparison.Ordinal),
                        "The baseline target lookup did not complete.");
                    AssertEx.True(window.ButtonMoveAbsolute.IsEnabled);

                    Click(window.ButtonMoveAbsolute);
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Move Absolute Send failed",
                            StringComparison.Ordinal),
                        "Fresh capability drift did not fail before Move dispatch.");

                    AssertNoMotionMutationRequests(
                        server.ReceivedRequests,
                        "Fresh post-lookup capability drift sent a Move mutation.");
                    AssertEx.False((bool)GetPrivateField(
                        window,
                        "motionMayBeActive"));
                    var journal = GetMotionUncertaintyJournal(window);
                    AssertEx.False(journal.HasActiveRecord);
                    AssertEx.True(
                        journal.CurrentRecord == null,
                        "Fresh capability drift armed a durable Move record.");
                    AssertEx.False(
                        File.Exists(journal.JournalFilePath),
                        "Fresh capability drift wrote a journal before failing.");
                    AssertEx.Contains(
                        "changed after target lookup",
                        window.TextExecutionLog.Text);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                ForceCloseMotionRecoveryWindow(window);
                DeleteJournalDirectory(journalRoot);
            }
        }

        private static void SafetyFreshCapabilityDriftIsZeroMutation()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateExactMotionRecoveryConnectSteps(capabilities);
            steps.Add(D5AxisLookupStep(MotionRecoveryAxisReference));
            steps.Add(D5AxisInfoStep(MotionRecoveryAxisReference));
            steps.Add(MotionRecoveryCapabilitiesStep(
                12,
                capabilities,
                DiagnosticsBootId + 1,
                DiagnosticMapRevision));
            steps.Add(MotionRecoveryCapabilitiesStep(
                13,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision + 1));
            steps.Add(CloseStep());

            var journalRoot = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    CreateRecoveryRequiredAxisMotionJournal(
                        journalRoot,
                        server.Port);
                    window = CreateWindow(journalRoot, server.Port);
                    ConnectExactMotionRecovery(window);
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Load Axis completed",
                            StringComparison.Ordinal),
                        "The exact recovery target did not load before drift testing.");

                    Click(window.ButtonStop);
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Stop Send failed",
                            StringComparison.Ordinal),
                        "BootId drift did not block Stop before mutation.");
                    Click(window.ButtonPowerOff);
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Power Off Send failed",
                            StringComparison.Ordinal),
                        "MapRevision drift did not block PowerOff before mutation.");

                    AssertNoMotionMutationRequests(
                        server.ReceivedRequests,
                        "Safety pre-dispatch identity drift sent a mutation.");
                    var journal = GetMotionUncertaintyJournal(window);
                    AssertEx.True(journal.HasActiveRecord);
                    AssertEx.Equal(
                        MotionUncertaintyState.RecoveryRequired,
                        journal.CurrentRecord.State);
                    AssertEx.True((bool)GetPrivateField(
                        window,
                        "motionMayBeActive"));
                    AssertEx.Contains(
                        "blocked before the safety mutation",
                        window.TextExecutionLog.Text);

                    CloseRecoveryConnectionForTest(window);
                    ForceCloseMotionRecoveryWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                ForceCloseMotionRecoveryWindow(window);
                DeleteJournalDirectory(journalRoot);
            }
        }

        private static void StatusOnlyCannotResolveStartupRecovery()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateExactMotionRecoveryConnectSteps(capabilities);
            steps.Add(D5AxisLookupStep(MotionRecoveryAxisReference));
            steps.Add(D5AxisInfoStep(MotionRecoveryAxisReference));
            for (var sample = 0; sample < 4; sample++)
            {
                steps.Add(MotionRecoveryAxisStatusStep(false));
            }

            steps.Add(MotionRecoveryCapabilitiesStep(
                12,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(MotionRecoveryCapabilitiesStep(
                13,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(MotionRecoveryStopStep(MotionRecoveryAxisReference));
            for (var sample = 0; sample < 3; sample++)
            {
                steps.Add(MotionRecoveryAxisStatusStep(false));
            }
            steps.Add(MotionRecoveryCapabilitiesStep(
                14,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(MotionRecoveryCapabilitiesStep(
                15,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));

            steps.Add(CloseStep());

            var journalRoot = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    CreateRecoveryRequiredAxisMotionJournal(
                        journalRoot,
                        server.Port);
                    window = CreateWindow(journalRoot, server.Port);
                    ConnectExactMotionRecovery(window);
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Load Axis completed",
                            StringComparison.Ordinal),
                        "The status-only recovery target did not load.");

                    Click(window.ButtonReadStatus);
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Read Axis Status failed",
                            StringComparison.Ordinal),
                        "Status-only safe samples incorrectly resolved startup recovery.");
                    var journal = GetMotionUncertaintyJournal(window);
                    AssertEx.True(journal.HasActiveRecord);
                    AssertEx.Equal(
                        MotionUncertaintyState.RecoveryRequired,
                        journal.CurrentRecord.State);
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2022));
                    AssertEx.Contains(
                        "status-only evidence",
                        window.TextExecutionLog.Text);

                    WaitUntil(
                        () => window.ButtonStop.IsEnabled,
                        "Stop was not re-enabled after status-only proof was rejected.");
                    Click(window.ButtonStop);
                    try
                    {
                        WaitUntil(
                            () => string.Equals(
                                window.TextOperationState.Text,
                                "Stop verified",
                                StringComparison.Ordinal),
                            "Explicit Stop acknowledgement and proof did not resolve recovery.");
                    }
                    catch (TimeoutException error)
                    {
                        throw MotionRecoveryTimeout(
                            "Explicit Stop acknowledgement and proof did not resolve recovery.",
                            window,
                            server.ReceivedRequests,
                            error);
                    }
                    AssertEx.False(journal.HasActiveRecord);
                    AssertEx.Equal(
                        MotionUncertaintyState.Resolved,
                        journal.CurrentRecord.State);
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2022));
                    AssertEx.Equal(
                        7,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2028));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                ForceCloseMotionRecoveryWindow(window);
                DeleteJournalDirectory(journalRoot);
            }
        }

        private static void PowerOffRequiresThreeConsecutiveSafeSamples()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var finalSafeSequenceReached = new ManualResetEventSlim(false);
            var releaseFinalSafeSequence = new ManualResetEventSlim(false);
            var steps = CreateExactMotionRecoveryConnectSteps(capabilities);
            steps.Add(D5AxisLookupStep(MotionRecoveryAxisReference));
            steps.Add(D5AxisInfoStep(MotionRecoveryAxisReference));
            steps.Add(MotionRecoveryCapabilitiesStep(
                12,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(MotionRecoveryPowerOffStep(
                MotionRecoveryAxisReference));
            steps.Add(MotionRecoveryAxisStatusStep(false));
            steps.Add(MotionRecoveryAxisStatusStep(true));
            steps.Add(MotionRecoveryAxisStatusStep(true));
            var gatedFirstFinalSafe = MotionRecoveryAxisStatusStep(false);
            gatedFirstFinalSafe.InspectRequest = request =>
            {
                AssertEx.Equal(
                    MotionRecoveryAxisReference,
                    TestFrame.ReadUInt16(request, 6));
                finalSafeSequenceReached.Set();
                AssertEx.True(
                    releaseFinalSafeSequence.Wait(
                        WaitTimeoutMilliseconds),
                    "The final PowerOff safe sequence was not released.");
            };
            steps.Add(gatedFirstFinalSafe);
            steps.Add(MotionRecoveryAxisStatusStep(false));
            steps.Add(MotionRecoveryAxisStatusStep(false));
            steps.Add(MotionRecoveryCapabilitiesStep(
                13,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(MotionRecoveryCapabilitiesStep(
                14,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(CloseStep());

            var journalRoot = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (finalSafeSequenceReached)
                using (releaseFinalSafeSequence)
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    CreateRecoveryRequiredAxisMotionJournal(
                        journalRoot,
                        server.Port);
                    window = CreateWindow(journalRoot, server.Port);
                    ConnectExactMotionRecovery(window);
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Load Axis completed",
                            StringComparison.Ordinal),
                        "The PowerOff recovery target did not load.");

                    WaitUntil(
                        () => window.ButtonPowerOff.IsEnabled,
                        "PowerOff was not enabled for the exact recovery target.");
                    var capabilityReadsBeforePowerOff =
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E00);
                    Click(window.ButtonPowerOff);
                    try
                    {
                        WaitUntil(
                            () => finalSafeSequenceReached.IsSet,
                            "PowerOff verification did not reach the reset safe sequence.");
                    }
                    catch (TimeoutException error)
                    {
                        throw MotionRecoveryTimeout(
                            "PowerOff verification did not reach the reset safe sequence.",
                            window,
                            server.ReceivedRequests,
                            error);
                    }
                    var journal = GetMotionUncertaintyJournal(window);
                    AssertEx.True(
                        journal.HasActiveRecord,
                        "One PowerOn=false sample followed by PowerOn=true samples resolved recovery.");
                    AssertEx.True((bool)GetPrivateField(
                        window,
                        "motionMayBeActive"));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2023));

                    releaseFinalSafeSequence.Set();
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Power Off verified",
                            StringComparison.Ordinal),
                        "Three consecutive PowerOn=false and Standstill samples did not resolve recovery.");
                    AssertEx.False(journal.HasActiveRecord);
                    AssertEx.Equal(
                        MotionUncertaintyState.Resolved,
                        journal.CurrentRecord.State);
                    AssertEx.Equal(
                        6,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2028));
                    AssertEx.Equal(
                        capabilityReadsBeforePowerOff + 3,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E00),
                        "The shared pre-dispatch identity plus separate Axis Power and motion final gates were not preserved.");
                    AssertNoMoveRequests(
                        server.ReceivedRequests,
                        "PowerOff recovery replayed Move.");

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                releaseFinalSafeSequence.Set();
                ForceCloseMotionRecoveryWindow(window);
                DeleteJournalDirectory(journalRoot);
            }
        }

        private static void PowerOffMonitorBlocksReplayAndAllowsPriorityStop()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var firstStatusReceived = new ManualResetEventSlim(false);
            var releaseFirstStatus = new ManualResetEventSlim(false);
            var steps = CreateExactMotionRecoveryConnectSteps(capabilities);
            steps.Add(D5AxisLookupStep(MotionRecoveryAxisReference));
            steps.Add(D5AxisInfoStep(MotionRecoveryAxisReference));
            steps.Add(MotionRecoveryCapabilitiesStep(
                12,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(MotionRecoveryPowerOffStep(
                MotionRecoveryAxisReference));
            steps.Add(DelayedMotionRecoveryAxisStatusStep(
                false,
                firstStatusReceived,
                releaseFirstStatus));
            steps.Add(MotionRecoveryCapabilitiesStep(
                13,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(MotionRecoveryStopStep(
                MotionRecoveryAxisReference));
            steps.Add(MotionRecoveryAxisStatusStep(false));
            steps.Add(MotionRecoveryAxisStatusStep(false));
            steps.Add(MotionRecoveryAxisStatusStep(false));
            steps.Add(MotionRecoveryCapabilitiesStep(
                14,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(MotionRecoveryCapabilitiesStep(
                15,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(MotionRecoveryCapabilitiesStep(
                16,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(MotionRecoveryPowerOffStep(
                MotionRecoveryAxisReference));
            steps.Add(MotionRecoveryAxisStatusStep(false));
            steps.Add(MotionRecoveryAxisStatusStep(false));
            steps.Add(MotionRecoveryAxisStatusStep(false));
            steps.Add(MotionRecoveryCapabilitiesStep(
                17,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(CloseStep());

            var journalRoot = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (firstStatusReceived)
                using (releaseFirstStatus)
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    CreateRecoveryRequiredAxisMotionJournal(
                        journalRoot,
                        server.Port);
                    window = CreateWindow(journalRoot, server.Port);
                    ConnectExactMotionRecovery(window);
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => window.ButtonPowerOff.IsEnabled,
                        "The first Power Off recovery command was not enabled.");

                    var capabilityReadsBeforePowerOff =
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E00);
                    Click(window.ButtonPowerOff);
                    WaitUntil(
                        () => firstStatusReceived.IsSet,
                        "The first split Power Off monitor did not reach 0x2028.");
                    AssertEx.False(
                        window.ButtonPowerOff.IsEnabled,
                        "The default Power Off button must not replay 0x2023 while status-only verification is already running.");
                    AssertEx.True(
                        window.ButtonStop.IsEnabled,
                        "Stop must remain available during Power Off verification.");
                    AssertEx.Contains(
                        "Resume Power Off Verification (No 0x2023 Replay)",
                        Convert.ToString(
                            window.ButtonPowerOff.Content,
                            CultureInfo.InvariantCulture));

                    var coordinator =
                        (LMCSendPriorityCoordinator)GetPrivateField(
                            window,
                            "sendPriorityCoordinator");
                    var firstGeneration = coordinator.CurrentGeneration;
                    Click(window.ButtonStop);
                    WaitUntil(
                        () => coordinator.CurrentGeneration
                            > firstGeneration,
                        "The newer explicit Stop did not reserve safety priority.");
                    releaseFirstStatus.Set();

                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Stop verified",
                            StringComparison.Ordinal),
                        "The newer explicit Stop did not complete status-only stable proof.");
                    var journal = GetMotionUncertaintyJournal(window);
                    AssertEx.False(journal.HasActiveRecord);
                    AssertEx.Equal(
                        MotionUncertaintyState.Resolved,
                        journal.CurrentRecord.State);
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2023));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2022));
                    AssertEx.Equal(
                        4,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2028));
                    AssertEx.Contains(
                        "was discarded because a newer Stop or Power Off request was reserved",
                        window.TextExecutionLog.Text);
                    AssertNoMoveRequests(
                        server.ReceivedRequests,
                        "Power Off-to-Stop preemption replayed Move.");

                    var axisPowerJournal =
                        (AxisPowerOnRecoveryJournal)GetPrivateField(
                            window,
                            "axisPowerOnRecoveryJournal");
                    AssertEx.True(axisPowerJournal.HasActiveRecord);
                    AssertEx.False(
                        axisPowerJournal.CurrentRecord.ExpectedPowerOn);
                    AssertEx.False(window.ButtonCloseConnection.IsEnabled);
                    AssertEx.True(window.ButtonPowerOff.IsEnabled);
                    AssertEx.Contains(
                        "No 0x2023 Replay",
                        Convert.ToString(
                            window.ButtonPowerOff.Content,
                            CultureInfo.InvariantCulture));

                    Click(window.ButtonPowerOff);
                    WaitUntil(
                        () => (bool)GetPrivateField(
                                window,
                                "axisPowerOffWaitInterferenceConfirmed")
                            && window.ButtonPowerOff.IsEnabled,
                        "The status-only Power Off check did not confirm the intervening Stop mutation.");
                    AssertEx.True(axisPowerJournal.HasActiveRecord);
                    AssertEx.False(window.ButtonCloseConnection.IsEnabled);
                    AssertEx.Contains(
                        "Power Off Again (Confirmed Interference)",
                        Convert.ToString(
                            window.ButtonPowerOff.Content,
                            CultureInfo.InvariantCulture));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2023),
                        "Status-only interference confirmation replayed 0x2023.");

                    Click(window.ButtonPowerOff);
                    try
                    {
                        WaitUntil(
                            () => string.Equals(
                                    window.TextOperationState.Text,
                                    "Power Off verified",
                                    StringComparison.Ordinal)
                                && !axisPowerJournal.HasActiveRecord
                                && window.ButtonCloseConnection.IsEnabled,
                            "The confirmed-interference Power Off replacement did not complete.");
                    }
                    catch (TimeoutException error)
                    {
                        throw MotionRecoveryTimeout(
                            "The confirmed-interference Power Off replacement did not complete.",
                            window,
                            server.ReceivedRequests,
                            error);
                    }
                    AssertEx.Equal(
                        2,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2023),
                        "The confirmed-interference path did not send exactly one explicit replacement 0x2023.");
                    AssertEx.Equal(
                        7,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2028));
                    AssertEx.Equal(
                        capabilityReadsBeforePowerOff + 6,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E00),
                        "Power Off/Stop recovery did not preserve first Power Off, Stop, Axis Stop final, motion final, interference-resume, and replacement final identity gates.");

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                releaseFirstStatus.Set();
                ForceCloseMotionRecoveryWindow(window);
                DeleteJournalDirectory(journalRoot);
            }
        }

        private static void
            PowerOffStatusFailureSecondClickResumesWithoutReplay()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(MotionRecoveryAxisReference));
            steps.Add(D5AxisInfoStep(MotionRecoveryAxisReference));
            steps.Add(MotionRecoveryCapabilitiesStep(
                11,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(MotionRecoveryPowerOffStep(
                MotionRecoveryAxisReference));
            steps.Add(MotionRecoveryAxisStatusFailureStep());
            steps.Add(MotionRecoveryAxisStatusStep(false));
            steps.Add(MotionRecoveryAxisStatusStep(false));
            steps.Add(MotionRecoveryAxisStatusStep(false));
            steps.Add(MotionRecoveryCapabilitiesStep(
                12,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(CloseStep());

            var journalRoot = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(journalRoot, server.Port);
                    ConnectWithoutMotionRecovery(window);
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => window.ButtonPowerOff.IsEnabled,
                        "The Power Off status-failure resume test did not load the axis.");

                    var capabilityReadsBeforePowerOff =
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E00);
                    Click(window.ButtonPowerOff);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Power Off verification failed",
                                StringComparison.Ordinal)
                            && window.ButtonPowerOff.IsEnabled,
                        "The first Power Off status failure did not settle with Resume available.");

                    var accepted = GetPrivateField(
                            window,
                            "pendingAxisPowerOffWaitContinuation")
                        as LMCAxisPowerOffWaitContinuation;
                    AssertEx.True(
                        accepted != null && accepted.IsPending,
                        "The accepted Power Off continuation was not retained after status failure.");
                    AssertEx.Contains(
                        "Resume Power Off Verification (No 0x2023 Replay)",
                        Convert.ToString(
                            window.ButtonPowerOff.Content,
                            CultureInfo.InvariantCulture));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2023));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2028));

                    Click(window.ButtonPowerOff);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Power Off verified",
                                StringComparison.Ordinal)
                            && GetPrivateField(
                                window,
                                "pendingAxisPowerOffWaitContinuation") == null,
                        "The second Power Off click did not complete status-only Resume.");

                    AssertEx.False(accepted.IsPending);
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2023));
                    AssertEx.Equal(
                        4,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2028));
                    AssertEx.Contains(
                        "Accepted Axis Power Off completed by status-only resume without replaying 0x2023.",
                        window.TextAxisResult.Text);
                    AssertEx.Equal(
                        capabilityReadsBeforePowerOff + 2,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E00));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                ForceCloseMotionRecoveryWindow(window);
                DeleteJournalDirectory(journalRoot);
            }
        }

        private static void PowerOffAcceptedBoundaryResumesStatusOnly()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(MotionRecoveryAxisReference));
            steps.Add(D5AxisInfoStep(MotionRecoveryAxisReference));
            steps.Add(MotionRecoveryPowerOffStep(
                MotionRecoveryAxisReference));
            steps.Add(MotionRecoveryCapabilitiesStep(
                11,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(MotionRecoveryAxisStatusStep(false));
            steps.Add(MotionRecoveryAxisStatusStep(false));
            steps.Add(MotionRecoveryAxisStatusStep(false));
            steps.Add(MotionRecoveryCapabilitiesStep(
                12,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(CloseStep());

            var journalRoot = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(journalRoot, server.Port);
                    ConnectWithoutMotionRecovery(window);
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => window.ButtonPowerOff.IsEnabled,
                        "The accepted-boundary Power Off test did not load the axis.");

                    var currentAxis = (LMCSingleAxis)GetPrivateField(
                        window,
                        "axis");
                    var accepted = currentAxis
                        .BeginPowerOffWaitForStableStateAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    AssertEx.True(
                        accepted.IsPending,
                        "Accepted continuation state after interference: Pending="
                        + accepted.IsPending
                        + ", Completed="
                        + accepted.IsCompleted
                        + ", Superseded="
                        + accepted.IsSuperseded
                        + ". Log="
                        + window.TextExecutionLog.Text);
                    var evidence = CapturePowerOffWaitEvidence(accepted);
                    var timeout = CreatePowerOffWaitTimeoutException(
                        evidence,
                        accepted);
                    var canceled = CreatePowerOffWaitCanceledException(
                        evidence,
                        accepted);

                    AssertEx.True(ReferenceEquals(
                        accepted,
                        InvokePrivateStatic(
                            "GetAxisPowerOffWaitContinuation",
                            timeout)));
                    AssertEx.True(ReferenceEquals(
                        evidence,
                        InvokePrivateStatic(
                            "GetAxisPowerOffWaitEvidence",
                            timeout)));
                    AssertEx.True(ReferenceEquals(
                        accepted,
                        InvokePrivateStatic(
                            "GetAxisPowerOffWaitContinuation",
                            canceled)));
                    AssertEx.True(ReferenceEquals(
                        evidence,
                        InvokePrivateStatic(
                            "GetAxisPowerOffWaitEvidence",
                            canceled)));

                    window.AxisPowerOffBeginAsyncOverride =
                        (candidate, cancellationToken) =>
                        {
                            AssertEx.True(ReferenceEquals(
                                currentAxis,
                                candidate));
                            return Task.FromException<
                                LMCAxisPowerOffWaitContinuation>(timeout);
                        };

                    var capabilityReadsBeforePowerOff =
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E00);
                    Click(window.ButtonPowerOff);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Power Off verified",
                                StringComparison.Ordinal)
                            && GetPrivateField(
                                window,
                                "pendingAxisPowerOffWaitContinuation") == null,
                        "The accepted Power Off boundary did not resume by status-only verification.");

                    AssertEx.False(accepted.IsPending);
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2023));
                    AssertEx.Equal(
                        3,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2028));
                    AssertEx.Contains(
                        "Begin deadline/cancellation boundary",
                        window.TextAxisResult.Text);
                    AssertEx.Contains(
                        "without replaying 0x2023",
                        window.TextAxisResult.Text);
                    AssertEx.Equal(
                        capabilityReadsBeforePowerOff + 2,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E00));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                ForceCloseMotionRecoveryWindow(window);
                DeleteJournalDirectory(journalRoot);
            }
        }

        private static void
            PowerOffAcceptedBoundaryRecordsMotionSafetyBeforeProof()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var firstStatusReceived = new ManualResetEventSlim(false);
            var releaseFirstStatus = new ManualResetEventSlim(false);
            var steps = CreateExactMotionRecoveryConnectSteps(capabilities);
            steps.Add(D5AxisLookupStep(MotionRecoveryAxisReference));
            steps.Add(D5AxisInfoStep(MotionRecoveryAxisReference));
            steps.Add(MotionRecoveryPowerOffStep(
                MotionRecoveryAxisReference));
            steps.Add(MotionRecoveryCapabilitiesStep(
                12,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(DelayedMotionRecoveryAxisStatusStep(
                false,
                firstStatusReceived,
                releaseFirstStatus));
            steps.Add(MotionRecoveryAxisStatusStep(false));
            steps.Add(MotionRecoveryAxisStatusStep(false));
            steps.Add(MotionRecoveryCapabilitiesStep(
                13,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(MotionRecoveryCapabilitiesStep(
                14,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(CloseStep());

            var journalRoot = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (firstStatusReceived)
                using (releaseFirstStatus)
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    var motionIdentity =
                        CreateRecoveryRequiredAxisMotionJournal(
                            journalRoot,
                            server.Port);
                    window = CreateWindow(journalRoot, server.Port);
                    ConnectExactMotionRecovery(window);
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => window.ButtonPowerOff.IsEnabled,
                        "The accepted-boundary motion recovery target did not load.");

                    var currentAxis = (LMCSingleAxis)GetPrivateField(
                        window,
                        "axis");
                    var accepted = currentAxis
                        .BeginPowerOffWaitForStableStateAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    var evidence = CapturePowerOffWaitEvidence(accepted);
                    var timeout = CreatePowerOffWaitTimeoutException(
                        evidence,
                        accepted);
                    window.AxisPowerOffBeginAsyncOverride =
                        (candidate, cancellationToken) =>
                        {
                            AssertEx.True(ReferenceEquals(
                                currentAxis,
                                candidate));
                            return Task.FromException<
                                LMCAxisPowerOffWaitContinuation>(timeout);
                        };

                    var capabilityReadsBeforePowerOff =
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E00);
                    Click(window.ButtonPowerOff);
                    WaitUntil(
                        () => firstStatusReceived.IsSet,
                        "The accepted-boundary recovery did not begin status-only proof.");
                    AssertEx.True((bool)GetPrivateField(
                        window,
                        "motionRecoveryRequiresExplicitSafetyCommand"));
                    AssertEx.True(
                        (int)GetPrivateField(
                            window,
                            "motionRecoverySafetyTrackingGeneration") > 0,
                        "The accepted boundary did not record the motion tracking generation before the first status proof.");
                    AssertEx.True(
                        (long)GetPrivateField(
                            window,
                            "motionRecoverySafetyGeneration") > 0,
                        "The accepted boundary did not record the safety request generation before the first status proof.");
                    releaseFirstStatus.Set();

                    try
                    {
                        WaitUntil(
                            () => string.Equals(
                                    window.TextOperationState.Text,
                                    "Power Off verified",
                                    StringComparison.Ordinal)
                                && !GetMotionUncertaintyJournal(window)
                                    .HasActiveRecord
                                && !((AxisPowerOnRecoveryJournal)
                                    GetPrivateField(
                                        window,
                                        "axisPowerOnRecoveryJournal"))
                                    .HasActiveRecord,
                            "The accepted-boundary Power Off proof did not resolve both durable journals.",
                            20000);
                    }
                    catch (TimeoutException error)
                    {
                        throw MotionRecoveryTimeout(
                            "The accepted-boundary Power Off proof did not resolve both durable journals.",
                            window,
                            server.ReceivedRequests,
                            error);
                    }

                    var motionJournal = GetMotionUncertaintyJournal(window);
                    var axisPowerJournal =
                        (AxisPowerOnRecoveryJournal)GetPrivateField(
                            window,
                            "axisPowerOnRecoveryJournal");
                    AssertEx.False(accepted.IsPending);
                    AssertEx.True(accepted.IsCompleted);
                    AssertEx.Equal(
                        motionIdentity,
                        motionJournal.CurrentRecord.Identity);
                    AssertEx.Equal(
                        MotionUncertaintyState.Resolved,
                        motionJournal.CurrentRecord.State);
                    AssertEx.Equal(
                        AxisPowerOnRecoveryState.Resolved,
                        axisPowerJournal.CurrentRecord.State);
                    AssertEx.False(
                        axisPowerJournal.CurrentRecord.ExpectedPowerOn);
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2023));
                    AssertEx.Equal(
                        3,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2028));
                    AssertEx.Equal(
                        capabilityReadsBeforePowerOff + 3,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E00),
                        "The accepted boundary did not preserve the shared pre-dispatch, Axis Power final, and motion final identity gates.");
                    AssertNoMoveRequests(
                        server.ReceivedRequests,
                        "Accepted-boundary Power Off replayed a Move command.");
                    AssertEx.Contains(
                        "was accepted for the exact durable motion identity",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "without replaying 0x2023",
                        window.TextAxisResult.Text);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                releaseFirstStatus.Set();
                ForceCloseMotionRecoveryWindow(window);
                DeleteJournalDirectory(journalRoot);
            }
        }

        private static void
            PowerOffInterferenceRetainsPendingAndBlocksAxisReload()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(MotionRecoveryAxisReference));
            steps.Add(D5AxisInfoStep(MotionRecoveryAxisReference));
            steps.Add(MotionRecoveryPowerOffStep(
                MotionRecoveryAxisReference));
            steps.Add(new FakeRpcStep(
                0x2024,
                TestFrame.Response(0, new byte[8])));
            steps.Add(MotionRecoveryCapabilitiesStep(
                11,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(MotionRecoveryCapabilitiesStep(
                12,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(MotionRecoveryPowerOffStep(
                MotionRecoveryAxisReference));
            steps.Add(MotionRecoveryAxisStatusStep(false));
            steps.Add(MotionRecoveryAxisStatusStep(false));
            steps.Add(MotionRecoveryAxisStatusStep(false));
            steps.Add(MotionRecoveryCapabilitiesStep(
                13,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(CloseStep());

            var journalRoot = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(journalRoot, server.Port);
                    ConnectWithoutMotionRecovery(window);
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => window.ButtonPowerOff.IsEnabled,
                        "The Power Off interference test did not load the axis.");

                    var currentAxis = (LMCSingleAxis)GetPrivateField(
                        window,
                        "axis");
                    var accepted = currentAxis
                        .BeginPowerOffWaitForStableStateAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    AssertEx.True(accepted.IsPending);
                    var reset = currentAxis.ResetAsync(CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    AssertEx.True(reset.IsSuccess);

                    window.AxisPowerOffBeginAsyncOverride =
                        (candidate, cancellationToken) =>
                        {
                            AssertEx.True(ReferenceEquals(
                                currentAxis,
                                candidate));
                            return Task.FromResult(accepted);
                        };

                    var capabilityReadsBeforePowerOff =
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E00);
                    Click(window.ButtonPowerOff);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Power Off verification failed",
                                StringComparison.Ordinal)
                            && !(bool)GetPrivateField(
                                window,
                                "safetyCommandRunning"),
                        "The intervening mutation did not fail Power Off attribution.");

                    AssertEx.True(
                        accepted.IsPending,
                        "Accepted continuation state after interference: Pending="
                        + accepted.IsPending
                        + ", Completed="
                        + accepted.IsCompleted
                        + ", Superseded="
                        + accepted.IsSuperseded
                        + ". Log="
                        + window.TextExecutionLog.Text);
                    AssertEx.True(
                        GetPrivateField(
                            window,
                            "pendingAxisPowerOffWaitContinuation") == null,
                        "Confirmed interference must detach the stale WPF continuation before explicit replacement.");
                    AssertEx.True(accepted.InterveningMutationDetected);
                    AssertEx.False(window.ButtonLookupAxis.IsEnabled);
                    AssertEx.False(window.TextAxisName.IsEnabled);
                    AssertEx.Contains(
                        "Axis Power Off interference was confirmed",
                        window.TextAxisResult.Text);
                    AssertEx.Contains(
                        "PowerOffMutationGeneration="
                            + accepted.PowerOffMutationGeneration,
                        window.TextAxisResult.Text);
                    AssertEx.Contains(
                        "ObservedMutationGeneration="
                            + accepted.ObservedMutationGeneration,
                        window.TextAxisResult.Text);
                    AssertEx.Contains(
                        "InterveningMutationDetected=True",
                        window.TextAxisResult.Text);
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2023));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2028));
                    AssertEx.True(
                        window.ButtonPowerOff.IsEnabled,
                        "Confirmed Power Off interference must allow one explicit replacement command.");
                    AssertEx.True((bool)GetPrivateField(
                        window,
                        "axisPowerOffWaitInterferenceConfirmed"));
                    AssertEx.Contains(
                        "Power Off Again (Confirmed Interference)",
                        Convert.ToString(
                            window.ButtonPowerOff.Content,
                            CultureInfo.InvariantCulture));

                    var requestCountBeforeBlockedLookup =
                        server.ReceivedRequests.Count;
                    InvokePrivate(
                        window,
                        "ButtonLookupAxis_Click",
                        null,
                        new RoutedEventArgs());
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Load Axis failed",
                                StringComparison.Ordinal)
                            && !(bool)GetPrivateField(
                                window,
                                "operationRunning"),
                        "The defensive blocked Load Axis handler did not settle before the explicit Power Off replacement.");
                    AssertEx.Equal(
                        requestCountBeforeBlockedLookup,
                        server.ReceivedRequests.Count,
                        "The defensive Load Axis handler sent wire traffic while an accepted Power Off continuation was pending.");
                    AssertEx.True(
                        GetPrivateField(
                            window,
                            "pendingAxisPowerOffWaitContinuation") == null,
                        "Blocked Load Axis must not revive the detached interference continuation.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2023));

                    window.AxisPowerOffBeginAsyncOverride = null;
                    Click(window.ButtonPowerOff);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Power Off verified",
                                StringComparison.Ordinal)
                            && GetPrivateField(
                                window,
                                "pendingAxisPowerOffWaitContinuation") == null,
                        "The confirmed-interference Power Off replacement did not complete.",
                        20000);

                    AssertEx.Equal(
                        2,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2023));
                    AssertEx.Equal(
                        3,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2028));
                    AssertEx.True(accepted.IsSuperseded);
                    AssertEx.False((bool)GetPrivateField(
                        window,
                        "axisPowerOffWaitInterferenceConfirmed"));
                    AssertEx.Equal(
                        capabilityReadsBeforePowerOff + 3,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E00));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                ForceCloseMotionRecoveryWindow(window);
                DeleteJournalDirectory(journalRoot);
            }
        }

        private static void
            PowerOffRejectedReplacementPreservesConfirmedInterference()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(MotionRecoveryAxisReference));
            steps.Add(D5AxisInfoStep(MotionRecoveryAxisReference));
            steps.Add(MotionRecoveryPowerOffStep(
                MotionRecoveryAxisReference));
            steps.Add(new FakeRpcStep(
                0x2024,
                TestFrame.Response(0, new byte[8])));
            steps.Add(MotionRecoveryCapabilitiesStep(
                11,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(MotionRecoveryCapabilitiesStep(
                12,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(MotionRecoveryRejectedPowerOffStep(
                MotionRecoveryAxisReference));
            steps.Add(CloseStep());

            var journalRoot = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(journalRoot, server.Port);
                    ConnectWithoutMotionRecovery(window);
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => window.ButtonPowerOff.IsEnabled,
                        "The rejected replacement Power Off test did not load the axis.");

                    var currentAxis = (LMCSingleAxis)GetPrivateField(
                        window,
                        "axis");
                    var original = currentAxis
                        .BeginPowerOffWaitForStableStateAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    AssertEx.True(
                        original.IsPending,
                        "Original continuation state after rejected replacement: Pending="
                        + original.IsPending
                        + ", Completed="
                        + original.IsCompleted
                        + ", Superseded="
                        + original.IsSuperseded
                        + ". Log="
                        + window.TextExecutionLog.Text);
                    var reset = currentAxis.ResetAsync(CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    AssertEx.True(reset.IsSuccess);

                    window.AxisPowerOffBeginAsyncOverride =
                        (candidate, cancellationToken) =>
                        {
                            AssertEx.True(ReferenceEquals(
                                currentAxis,
                                candidate));
                            return Task.FromResult(original);
                        };

                    var capabilityReadsBeforePowerOff =
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E00);
                    Click(window.ButtonPowerOff);
                    WaitUntil(
                        () => (bool)GetPrivateField(
                                window,
                                "axisPowerOffWaitInterferenceConfirmed")
                            && !(bool)GetPrivateField(
                                window,
                                "safetyCommandRunning"),
                        "The original Power Off did not reach confirmed interference.");
                    AssertEx.True(original.IsPending);
                    AssertEx.True(
                        GetPrivateField(
                            window,
                            "pendingAxisPowerOffWaitContinuation") == null,
                        "Confirmed interference must detach the stale WPF continuation before explicit replacement.");

                    window.AxisPowerOffBeginAsyncOverride = null;
                    Click(window.ButtonPowerOff);
                    WaitUntil(
                        () => CountRequestCommand(
                                server.ReceivedRequests,
                                0x2023) == 2
                            && !(bool)GetPrivateField(
                                window,
                                "safetyCommandRunning"),
                        "The explicit replacement Power Off rejection did not settle.");

                    AssertEx.True(
                        original.IsPending,
                        "Original continuation state after rejected replacement: Pending="
                        + original.IsPending
                        + ", Completed="
                        + original.IsCompleted
                        + ", Superseded="
                        + original.IsSuperseded
                        + ". Log="
                        + window.TextExecutionLog.Text);
                    AssertEx.False(original.IsSuperseded);
                    AssertEx.True(ReferenceEquals(
                        original,
                        GetPrivateField(
                            window,
                            "pendingAxisPowerOffWaitContinuation")));
                    AssertEx.True((bool)GetPrivateField(
                        window,
                        "axisPowerOffWaitInterferenceConfirmed"));
                    AssertEx.Equal(
                        capabilityReadsBeforePowerOff + 2,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E00));
                    AssertEx.Equal(
                        2,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2023));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2028));
                    AssertEx.True(window.ButtonPowerOff.IsEnabled);
                    AssertEx.Contains(
                        "Power Off Again (Confirmed Interference)",
                        Convert.ToString(
                            window.ButtonPowerOff.Content,
                            CultureInfo.InvariantCulture));
                    AssertEx.False(window.ButtonLookupAxis.IsEnabled);
                    AssertEx.False(window.TextAxisName.IsEnabled);

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
                        "Load Axis sent wire traffic after a replacement Power Off rejection.");
                    AssertEx.True(ReferenceEquals(
                        original,
                        GetPrivateField(
                            window,
                            "pendingAxisPowerOffWaitContinuation")));
                    AssertEx.True((bool)GetPrivateField(
                        window,
                        "axisPowerOffWaitInterferenceConfirmed"));

                    CloseRecoveryConnectionForTest(window);
                    ForceCloseMotionRecoveryWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                ForceCloseMotionRecoveryWindow(window);
                DeleteJournalDirectory(journalRoot);
            }
        }

        private static void AcceptedMoveFinalCapabilityDriftKeepsLatch()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(MotionRecoveryAxisReference));
            steps.Add(D5AxisInfoStep(MotionRecoveryAxisReference));
            steps.Add(MotionRecoveryAxisStatusStep(true));
            steps.Add(D5StableAxisPositionStep(0));
            steps.Add(MotionRecoveryCapabilitiesStep(
                11,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(MotionRecoveryMoveAbsoluteStep(
                MotionRecoveryAxisReference));
            for (var sample = 0; sample < 3; sample++)
            {
                steps.Add(MotionRecoveryAxisStatusStep(true));
            }

            steps.Add(MotionRecoveryCapabilitiesStep(
                12,
                capabilities,
                DiagnosticsBootId + 1,
                DiagnosticMapRevision + 1));
            steps.Add(CloseStep());

            var journalRoot = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(journalRoot, server.Port);
                    ConnectWithoutMotionRecovery(window);
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Load Axis completed",
                            StringComparison.Ordinal),
                        "The accepted-Move baseline target did not load.");
                    window.TextPosition.Text = "0";
                    AssertEx.True(window.ButtonMoveAbsolute.IsEnabled);

                    Click(window.ButtonMoveAbsolute);
                    try
                    {
                        WaitUntil(
                            () => string.Equals(
                                window.TextOperationState.Text,
                                "Move Absolute monitor failed",
                                StringComparison.Ordinal),
                            "Final capability drift did not retain accepted-Move uncertainty.");
                    }
                    catch (TimeoutException error)
                    {
                        throw MotionRecoveryTimeout(
                            "Final capability drift did not retain accepted-Move uncertainty.",
                            window,
                            server.ReceivedRequests,
                            error);
                    }

                    var journal = GetMotionUncertaintyJournal(window);
                    AssertEx.True(journal.HasActiveRecord);
                    AssertEx.Equal(
                        MotionUncertaintyState.RecoveryRequired,
                        journal.CurrentRecord.State);
                    AssertEx.Equal(
                        DiagnosticsBootId,
                        journal.CurrentRecord.DiagnosticsBootId);
                    AssertEx.Equal(
                        DiagnosticMapRevision,
                        journal.CurrentRecord.MapRevision);
                    AssertEx.True((bool)GetPrivateField(
                        window,
                        "motionMayBeActive"));
                    AssertEx.True((bool)GetPrivateField(
                        window,
                        "motionRecoveryRequiresExplicitSafetyCommand"));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x209F),
                        "Accepted Move was missing or replayed.");
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2022),
                        "Final identity drift sent an automatic Stop.");
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2023),
                        "Final identity drift sent an automatic PowerOff.");
                    AssertEx.Contains(
                        "final identity verification failed",
                        window.TextExecutionLog.Text);

                    CloseRecoveryConnectionForTest(window);
                    ForceCloseMotionRecoveryWindow(window);
                    window = null;
                    server.Verify();
                }

                using (var reopened = MotionUncertaintyJournal.Open(
                    GetMotionUncertaintyJournalDirectory(journalRoot)))
                {
                    AssertEx.True(reopened.HasActiveRecord);
                    AssertEx.Equal(
                        MotionUncertaintyState.RecoveryRequired,
                        reopened.CurrentRecord.State);
                }
            }
            finally
            {
                ForceCloseMotionRecoveryWindow(window);
                DeleteJournalDirectory(journalRoot);
            }
        }

        private static LMCAxisPowerOffWaitEvidence
            CapturePowerOffWaitEvidence(
            LMCAxisPowerOffWaitContinuation continuation)
        {
            var capture = typeof(LMCAxisPowerOffWaitContinuation).GetMethod(
                "CaptureEvidence",
                BindingFlags.Instance | BindingFlags.NonPublic);
            AssertEx.NotNull(capture);
            return (LMCAxisPowerOffWaitEvidence)capture.Invoke(
                continuation,
                new object[] { 1L });
        }

        private static LMCAxisPowerOffWaitTimeoutException
            CreatePowerOffWaitTimeoutException(
            LMCAxisPowerOffWaitEvidence evidence,
            LMCAxisPowerOffWaitContinuation continuation)
        {
            var constructor = typeof(LMCAxisPowerOffWaitTimeoutException)
                .GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[]
                    {
                        typeof(LMCAxisPowerOffWaitEvidence),
                        typeof(LMCAxisPowerOffWaitContinuation)
                    },
                    null);
            AssertEx.NotNull(constructor);
            return (LMCAxisPowerOffWaitTimeoutException)constructor.Invoke(
                new object[] { evidence, continuation });
        }

        private static LMCAxisPowerOffWaitCanceledException
            CreatePowerOffWaitCanceledException(
            LMCAxisPowerOffWaitEvidence evidence,
            LMCAxisPowerOffWaitContinuation continuation)
        {
            var constructor = typeof(LMCAxisPowerOffWaitCanceledException)
                .GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[]
                    {
                        typeof(LMCAxisPowerOffWaitEvidence),
                        typeof(LMCAxisPowerOffWaitContinuation),
                        typeof(OperationCanceledException),
                        typeof(CancellationToken)
                    },
                    null);
            AssertEx.NotNull(constructor);
            var cancellationToken = new CancellationToken(true);
            return (LMCAxisPowerOffWaitCanceledException)constructor.Invoke(
                new object[]
                {
                    evidence,
                    continuation,
                    new OperationCanceledException(cancellationToken),
                    cancellationToken
                });
        }

        private static object InvokePrivateStatic(
            string methodName,
            params object[] arguments)
        {
            var method = typeof(MainWindow).GetMethod(
                methodName,
                BindingFlags.Static | BindingFlags.NonPublic);
            AssertEx.NotNull(method);
            return method.Invoke(null, arguments);
        }

        private static List<FakeRpcStep>
            CreateExactMotionRecoveryConnectSteps(
            LMCDiagnosticCapability capabilities)
        {
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(MotionRecoveryCapabilitiesStep(
                11,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            return steps;
        }

        private static FakeRpcStep MotionRecoveryCapabilitiesStep(
            uint requestId,
            LMCDiagnosticCapability capabilities,
            uint diagnosticsBootId,
            uint mapRevision)
        {
            var payload = CapabilitiesPayload(requestId, capabilities, 0);
            TestFrame.WriteUInt32(payload, 24, mapRevision);
            TestFrame.WriteUInt32(payload, 64, diagnosticsBootId);
            return new FakeRpcStep(
                0x7E00,
                TestFrame.Response(0, payload))
            {
                InspectRequest = request => AssertEx.Equal(
                    requestId,
                    TestFrame.ReadUInt32(request, 12))
            };
        }

        private static FakeRpcStep MotionRecoveryAxisLookupStep(
            string axisName,
            ushort returnedReference)
        {
            var requestPayload = new byte[80];
            var axisNameBytes = Encoding.ASCII.GetBytes(axisName);
            Buffer.BlockCopy(
                axisNameBytes,
                0,
                requestPayload,
                0,
                axisNameBytes.Length);

            var responsePayload = new byte[6];
            TestFrame.WriteUInt16(
                responsePayload,
                4,
                returnedReference);
            return new FakeRpcStep(
                0x103C,
                TestFrame.Response(0, responsePayload))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    TestFrame.Request(0x103C, 0, requestPayload),
                    request)
            };
        }

        private static FakeRpcStep MotionRecoveryStopStep(
            ushort axisReference)
        {
            return new FakeRpcStep(
                0x2022,
                TestFrame.Response(0, new byte[8]))
            {
                InspectRequest = request => AssertEx.Equal(
                    axisReference,
                    TestFrame.ReadUInt16(request, 6))
            };
        }

        private static FakeRpcStep MotionRecoveryPowerOffStep(
            ushort axisReference)
        {
            return new FakeRpcStep(
                0x2023,
                TestFrame.Response(0, new byte[8]))
            {
                InspectRequest = request =>
                {
                    AssertEx.Equal(
                        axisReference,
                        TestFrame.ReadUInt16(request, 6));
                    AssertEx.Equal(
                        (byte)0,
                        request[12],
                        "Recovery PowerOff request carried PowerOn=true.");
                }
            };
        }

        private static FakeRpcStep MotionRecoveryRejectedPowerOffStep(
            ushort axisReference)
        {
            return new FakeRpcStep(
                0x2023,
                TestFrame.Response(0, TestFrame.Hex("01 00 F9 FF")))
            {
                InspectRequest = request =>
                {
                    AssertEx.Equal(
                        axisReference,
                        TestFrame.ReadUInt16(request, 6));
                    AssertEx.Equal(
                        (byte)0,
                        request[12],
                        "Replacement PowerOff request carried PowerOn=true.");
                }
            };
        }

        private static FakeRpcStep MotionRecoveryMoveAbsoluteStep(
            ushort axisReference)
        {
            return new FakeRpcStep(
                0x209F,
                TestFrame.Response(0, new byte[8]))
            {
                InspectRequest = request => AssertEx.Equal(
                    axisReference,
                    TestFrame.ReadUInt16(request, 6))
            };
        }

        private static FakeRpcStep MotionRecoveryAxisStatusStep(
            bool powerOn)
        {
            var payload = new byte[12];
            TestFrame.WriteUInt32(
                payload,
                0,
                0x02000000u | (powerOn ? 1u : 0u));
            return new FakeRpcStep(
                0x2028,
                TestFrame.Response(0, payload))
            {
                InspectRequest = request => AssertEx.Equal(
                    MotionRecoveryAxisReference,
                    TestFrame.ReadUInt16(request, 6))
            };
        }

        private static FakeRpcStep MotionRecoveryAxisStatusFailureStep()
        {
            return new FakeRpcStep(
                0x2028,
                TestFrame.Response(7, new byte[12]))
            {
                InspectRequest = request => AssertEx.Equal(
                    MotionRecoveryAxisReference,
                    TestFrame.ReadUInt16(request, 6))
            };
        }

        private static FakeRpcStep DelayedMotionRecoveryAxisStatusStep(
            bool powerOn,
            ManualResetEventSlim statusReceived,
            ManualResetEventSlim releaseStatus)
        {
            var step = MotionRecoveryAxisStatusStep(powerOn);
            step.InspectRequest = request =>
            {
                AssertEx.Equal(
                    MotionRecoveryAxisReference,
                    TestFrame.ReadUInt16(request, 6));
                statusReceived.Set();
            };
            step.BeforeResponse = () => AssertEx.True(
                releaseStatus.Wait(5000),
                "The delayed Power Off status response was not released.");
            return step;
        }

        private static FakeRpcStep DelayedMotionRecoveryGroupStatusStep(
            uint state,
            ManualResetEventSlim statusReceived,
            ManualResetEventSlim releaseStatus)
        {
            var step = GroupEnableWaitStatusStep(state);
            var inspectRequest = step.InspectRequest;
            step.InspectRequest = request =>
            {
                if (inspectRequest != null)
                {
                    inspectRequest(request);
                }

                statusReceived.Set();
            };
            step.BeforeResponse = () => AssertEx.True(
                releaseStatus.Wait(5000),
                "The delayed Group Stop status response was not released.");
            return step;
        }

        private static Guid CreateArmedAxisMotionJournal(
            string journalRoot,
            int endpointPort,
            uint diagnosticsBootId,
            uint mapRevision)
        {
            var identity = Guid.NewGuid();
            using (var journal = MotionUncertaintyJournal.Open(
                GetMotionUncertaintyJournalDirectory(journalRoot)))
            {
                journal.ArmBeforeDispatch(
                    identity,
                    "127.0.0.1",
                    endpointPort,
                    MotionUncertaintyTargetKind.Axis,
                    MotionRecoveryAxisName,
                    MotionRecoveryAxisReference,
                    MotionRecoveryOperation,
                    diagnosticsBootId,
                    mapRevision,
                    DateTime.UtcNow.AddSeconds(-5));
            }

            return identity;
        }

        private static Guid CreateRecoveryRequiredAxisMotionJournal(
            string journalRoot,
            int endpointPort)
        {
            var identity = CreateArmedAxisMotionJournal(
                journalRoot,
                endpointPort,
                DiagnosticsBootId,
                DiagnosticMapRevision);
            using (var journal = MotionUncertaintyJournal.Open(
                GetMotionUncertaintyJournalDirectory(journalRoot)))
            {
                var record = journal.CurrentRecord;
                journal.PromoteToRecoveryRequired(
                    identity,
                    DateTime.UtcNow < record.UpdatedUtc
                        ? record.UpdatedUtc
                        : DateTime.UtcNow);
            }

            return identity;
        }

        private static Guid CreateRecoveryRequiredGroupMotionJournal(
            string journalRoot,
            int endpointPort)
        {
            var identity = Guid.NewGuid();
            using (var journal = MotionUncertaintyJournal.Open(
                GetMotionUncertaintyJournalDirectory(journalRoot)))
            {
                journal.ArmBeforeDispatch(
                    identity,
                    "127.0.0.1",
                    endpointPort,
                    MotionUncertaintyTargetKind.Group,
                    GroupEnableWaitName,
                    GroupEnableWaitReference,
                    "Group Move Linear Absolute",
                    DiagnosticsBootId,
                    DiagnosticMapRevision,
                    DateTime.UtcNow.AddSeconds(-5));
                var record = journal.CurrentRecord;
                journal.PromoteToRecoveryRequired(
                    identity,
                    DateTime.UtcNow < record.UpdatedUtc
                        ? record.UpdatedUtc
                        : DateTime.UtcNow);
            }

            return identity;
        }

        private static string GetMotionUncertaintyJournalDirectory(
            string journalRoot)
        {
            return Path.Combine(
                journalRoot,
                "MotionUncertaintyRecovery");
        }

        private static MotionUncertaintyJournal
            GetMotionUncertaintyJournal(MainWindow window)
        {
            return (MotionUncertaintyJournal)GetPrivateField(
                window,
                "motionUncertaintyJournal");
        }

        private static void ConnectExactMotionRecovery(MainWindow window)
        {
            Click(window.ButtonConnect);
            WaitUntil(
                () => string.Equals(
                    window.TextOperationState.Text,
                    "Connect completed",
                    StringComparison.Ordinal)
                    && window.GridEtherCATTopology.Items.Count
                        == TopologyNodeCount,
                "The exact motion recovery connection did not complete.");
            AssertEx.True((bool)GetPrivateField(
                window,
                "motionMayBeActive"));
        }

        private static void ConnectWithoutMotionRecovery(MainWindow window)
        {
            Click(window.ButtonConnect);
            WaitUntil(
                () => string.Equals(
                    window.TextOperationState.Text,
                    "Connect completed",
                    StringComparison.Ordinal)
                    && window.GridEtherCATTopology.Items.Count
                        == TopologyNodeCount,
                "The baseline non-recovery connection did not complete.");
            AssertEx.False((bool)GetPrivateField(
                window,
                "motionMayBeActive"));
        }

        private static void CloseRecoveryConnectionForTest(
            MainWindow window)
        {
            var currentConnection = GetPrivateField(
                window,
                "connection") as LMCConnection;
            if (currentConnection == null)
            {
                return;
            }

            InvokePrivate(window, "DetachConnection", currentConnection);
            SetPrivateField(window, "connection", null);
            InvokePrivate(window, "ClearLoadedObjects");
            try
            {
                currentConnection.CloseConnectionAsync(
                    CancellationToken.None).GetAwaiter().GetResult();
            }
            finally
            {
                currentConnection.Dispose();
                InvokePrivate(window, "UpdateUiState");
            }

            WaitUntil(
                () => string.Equals(
                    window.TextConnectionState.Text,
                    "Disconnected",
                    StringComparison.Ordinal),
                "Recovery test cleanup did not close the connection.");
        }

        private static void ForceCloseMotionRecoveryWindow(
            MainWindow window)
        {
            if (window == null || !window.IsLoaded)
            {
                return;
            }

            try
            {
                var currentConnection = GetPrivateField(
                    window,
                    "connection") as LMCConnection;
                if (currentConnection != null)
                {
                    CloseRecoveryConnectionForTest(window);
                }
            }
            catch
            {
            }

            SetPrivateField(window, "allowWindowClose", true);
            window.Close();
            WaitUntil(
                () => !window.IsLoaded,
                "The forced motion-recovery test window did not close.",
                3000);
        }

        private static int DifferentPort(int port)
        {
            return port == 65535 ? 65534 : port + 1;
        }

        private static TimeoutException MotionRecoveryTimeout(
            string message,
            MainWindow window,
            IList<byte[]> requests,
            Exception innerException)
        {
            var commands = new StringBuilder();
            for (var index = 0; index < requests.Count; index++)
            {
                if (index != 0)
                {
                    commands.Append(',');
                }

                commands.Append(TestFrame.ReadUInt16(
                    requests[index],
                    0).ToString("X4", CultureInfo.InvariantCulture));
            }

            return new TimeoutException(
                message
                    + " State="
                    + window.TextOperationState.Text
                    + ", Commands="
                    + commands
                    + ", Log="
                    + window.TextExecutionLog.Text,
                innerException);
        }

        private static void AssertNoMotionMutationRequests(
            IList<byte[]> requests,
            string message)
        {
            for (var index = 0; index < requests.Count; index++)
            {
                var command = TestFrame.ReadUInt16(requests[index], 0);
                AssertEx.False(
                    IsMotionMutationCommand(command),
                    message
                        + " Command=0x"
                        + command.ToString(
                            "X4",
                            CultureInfo.InvariantCulture)
                        + ".");
            }
        }

        private static bool IsMotionMutationCommand(ushort command)
        {
            return command == 0x2022
                || command == 0x2023
                || command == 0x2024
                || command == 0x209F
                || command == 0x20A0
                || command == 0x20A2
                || command == 0x2047
                || command == 0x2048
                || command == 0x2049
                || command == 0x204A
                || command == 0x204B
                || command == 0x2085
                || command == 0x20A4
                || command == 0x20E7;
        }

        private static void AssertNoMoveRequests(
            IList<byte[]> requests,
            string message)
        {
            AssertEx.Equal(
                0,
                CountRequestCommand(requests, 0x209F),
                message);
            AssertEx.Equal(
                0,
                CountRequestCommand(requests, 0x20A0),
                message);
            AssertEx.Equal(
                0,
                CountRequestCommand(requests, 0x20A2),
                message);
            AssertEx.Equal(
                0,
                CountRequestCommand(requests, 0x20A4),
                message);
        }
    }
}

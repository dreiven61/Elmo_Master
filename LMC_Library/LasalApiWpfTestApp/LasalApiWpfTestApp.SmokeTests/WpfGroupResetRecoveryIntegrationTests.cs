using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using LasalMotionControlApiExample;
using LasalMotionControlLib;
using LasalMotionControlLib.Tests;

namespace LasalApiWpfTestApp.SmokeTests
{
    internal static partial class WpfMainWindowIntegrationTests
    {
        private const ushort GroupResetMemberOneReference = 1;
        private const ushort GroupResetMemberTwoReference = 2;

        private enum GroupResetSafetyNackKind
        {
            GroupStop,
            GroupPowerOff,
            GroupDisable
        }

        internal static void RegisterGroupResetRecoveryTests(
            ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.GroupResetRecovery.AcceptedFailureInvalidatesReadinessAndResumeIsStatusOnly",
                AcceptedFailureInvalidatesReadinessAndResumeIsStatusOnly);
            tests.Add(
                "Wpf.GroupResetRecovery.ConnectionLossPreservesDurableRecoveryWithoutReplay",
                ConnectionLossDropsSessionContinuationWithoutReplay);
            tests.Add(
                "Wpf.GroupResetRecovery.AckBoundaryLossPreservesUncertainDurableRecoveryWithoutReplay",
                AckBoundaryConnectionLossDropsUncertainStateWithoutReplay);
            tests.Add(
                "Wpf.GroupResetRecovery.DelayedResetResultPublishesOnlyAfterGroupStopNack",
                DelayedResetResultPublishesOnlyAfterGroupStopNack);
            tests.Add(
                "Wpf.GroupResetRecovery.SafeDisableSupersedesWithoutResetReplay",
                SafeDisableSupersedesWithoutResetReplay);
            tests.Add(
                "Wpf.GroupResetRecovery.ValidNackRetainsPreparation",
                ValidNackRetainsPreparation);
            tests.Add(
                "Wpf.GroupResetRecovery.MemberSnapshotFailureDoesNotDispatchAndRetainsPreparation",
                MemberSnapshotFailureDoesNotDispatchAndRetainsPreparation);
            tests.Add(
                "Wpf.GroupResetRecovery.GroupStopNackPreservesStatusOnlyResume",
                GroupStopNackPreservesStatusOnlyResume);
            tests.Add(
                "Wpf.GroupResetRecovery.GroupPowerOffNackPreservesStatusOnlyResume",
                GroupPowerOffNackPreservesStatusOnlyResume);
            tests.Add(
                "Wpf.GroupResetRecovery.GroupDisableNackPreservesStatusOnlyResume",
                GroupDisableNackPreservesStatusOnlyResume);
            tests.Add(
                "Wpf.GroupResetRecovery.CapturedMemberAxisStopSupersedesWithoutResetReplay",
                CapturedMemberAxisStopSupersedesWithoutResetReplay);
            tests.Add(
                "Wpf.GroupResetRecovery.CapturedMemberAxisPowerOffSupersedesWithoutResetReplay",
                CapturedMemberAxisPowerOffSupersedesWithoutResetReplay);
            tests.Add(
                "Wpf.GroupResetRecovery.CapturedMemberAxisStopNackPreservesStatusOnlyResume",
                CapturedMemberAxisStopNackPreservesStatusOnlyResume);
            tests.Add(
                "Wpf.GroupResetRecovery.CapturedMemberAxisPowerOffNackPreservesStatusOnlyResume",
                CapturedMemberAxisPowerOffNackPreservesStatusOnlyResume);
            tests.Add(
                "Wpf.GroupResetRecovery.UncertainSubmissionBlocksReplayUntilSafeDisable",
                UncertainSubmissionBlocksReplayUntilSafeDisable);
            tests.Add(
                "Wpf.GroupResetRecovery.LockedStandbyProofEnablesOnlySafeDisable",
                LockedStandbyProofEnablesOnlySafeDisable);
            tests.Add(
                "Wpf.GroupResetRecovery.AcceptedRestartAttachIsStatusOnly",
                AcceptedRestartAttachIsStatusOnly);
            tests.Add(
                "Wpf.GroupResetRecovery.UncertainRestartAttachIsStatusOnly",
                UncertainRestartAttachIsStatusOnly);
        }

        private static void AcceptedRestartAttachIsStatusOnly()
        {
            RunRestartAttachIsStatusOnly(false);
        }

        private static void UncertainRestartAttachIsStatusOnly()
        {
            RunRestartAttachIsStatusOnly(true);
        }

        private static void RunRestartAttachIsStatusOnly(
            bool outcomeUncertain)
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(GroupResetMembersStep());
            AddStableGroupResetClearRounds(steps, 3);
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            var callbackPort = ReserveGroupResetCallbackPort();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    ArmRestartGroupResetRecord(
                        journalDirectory,
                        server.Port,
                        callbackPort,
                        outcomeUncertain);
                    window = new MainWindow(journalDirectory);
                    window.Show();
                    WaitUntil(
                        () => window.IsLoaded,
                        "The Group Reset recovery window did not load.");
                    AssertEx.Equal(
                        callbackPort.ToString(CultureInfo.InvariantCulture),
                        window.TextCallbackPort.Text);

                    Click(window.ButtonConnect);
                    WaitUntilWithGroupResetDiagnostics(
                        () => window.GridEtherCATTopology.Items.Count
                                == TopologyNodeCount
                            && window.ButtonLookupGroup.IsEnabled,
                        "Exact Group Reset recovery reconnect did not complete.",
                        window,
                        server);
                    Click(window.ButtonLookupGroup);
                    WaitUntilWithGroupResetDiagnostics(
                        () => string.Equals(
                                window.TextGroupReference.Text,
                                GroupEnableWaitReference.ToString(
                                    CultureInfo.InvariantCulture),
                                StringComparison.Ordinal)
                            && window.ButtonGroupReset.IsEnabled,
                        "Durable Group Reset member attach did not publish a resumable continuation.",
                        window,
                        server);

                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x20D2));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(server.ReceivedRequests, 0x2049));
                    if (outcomeUncertain)
                    {
                        var preparation =
                            window.TextGroupPreparationState.Text;
                        AssertEx.Contains("outcome-uncertain", preparation);
                        AssertEx.Contains(
                            "prior 0x2049 outcome remains unknown",
                            preparation);
                        AssertEx.False(
                            preparation.IndexOf(
                                "ACK accepted",
                                StringComparison.OrdinalIgnoreCase) >= 0);
                    }

                    Click(window.ButtonGroupReset);
                    WaitUntilWithGroupResetDiagnostics(
                        () => !(bool)GetPrivateField(window, "operationRunning")
                            && !(bool)InvokePrivate(
                                window,
                                "HasUnresolvedGroupResetState"),
                        "Durable Group Reset status-only recovery did not resolve.",
                        window,
                        server);

                    AssertEx.Equal(
                        0,
                        CountRequestCommand(server.ReceivedRequests, 0x2049));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x20D2));
                    AssertEx.Equal(
                        3,
                        CountRequestCommand(server.ReceivedRequests, 0x2045));
                    AssertEx.Equal(
                        6,
                        CountRequestCommand(server.ReceivedRequests, 0x2028));
                    if (outcomeUncertain)
                    {
                        AssertEx.Contains(
                            "prior Group Reset outcome remains unknown",
                            window.TextGroupResult.Text);
                    }

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }

                using (var journal = GroupResetRecoveryJournal.Open(
                    System.IO.Path.Combine(
                        journalDirectory,
                        "GroupResetRecovery")))
                {
                    AssertEx.False(journal.HasActiveRecord);
                    AssertEx.Equal(
                        GroupResetRecoveryState.Resolved,
                        journal.CurrentRecord.State);
                }
            }
            finally
            {
                CloseWindowWithActiveRecoveryBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void ArmRestartGroupResetRecord(
            string journalDirectory,
            int rpcPort,
            int callbackPort,
            bool outcomeUncertain,
            uint diagnosticsBuild = 1)
        {
            using (var journal = GroupResetRecoveryJournal.Open(
                System.IO.Path.Combine(
                    journalDirectory,
                    "GroupResetRecovery")))
            {
                var record = journal.ArmBeforeDispatch(
                    Guid.NewGuid(),
                    "127.0.0.1",
                    rpcPort,
                    "127.0.0.1",
                    callbackPort,
                    diagnosticsBuild,
                    DiagnosticsBootId,
                    DiagnosticMapRevision,
                    GroupEnableWaitName,
                    GroupEnableWaitReference,
                    777,
                    new[]
                    {
                        new GroupResetRecoveryMember(
                            "LMCAXis1",
                            GroupResetMemberOneReference,
                            101),
                        new GroupResetRecoveryMember(
                            "LMCAXis2",
                            GroupResetMemberTwoReference,
                            102)
                    },
                    3,
                    DateTime.UtcNow);
                if (outcomeUncertain)
                {
                    journal.PromoteRecoveryRequired(
                        record,
                        DateTime.UtcNow.AddMilliseconds(1));
                }
                else
                {
                    journal.MarkAccepted(
                        record,
                        DateTime.UtcNow.AddMilliseconds(1));
                }
            }
        }

        private static int ReserveGroupResetCallbackPort()
        {
            using (var socket = new UdpClient(AddressFamily.InterNetwork))
            {
                socket.Client.Bind(
                    new IPEndPoint(IPAddress.Loopback, 0));
                return ((IPEndPoint)socket.Client.LocalEndPoint).Port;
            }
        }

        private static void
            AcceptedFailureInvalidatesReadinessAndResumeIsStatusOnly()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(
                11,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(GroupResetMembersStep());
            steps.Add(CreateGroupResetStep());
            steps.Add(GroupStopFailedStatusStep());
            AddStableGroupResetClearRounds(steps, 3);
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
                    SetPrivateField(window, "groupIdentityHomeCheckComplete", true);
                    SetPrivateField(window, "groupIdentityHomeCheckPassed", true);
                    SetPrivateField(window, "groupProfileLocked", true);
                    InvokePrivate(window, "UpdateUiState");

                    AssertEx.True(window.ButtonGroupReset.IsEnabled);
                    Click(window.ButtonGroupReset);
                    WaitUntilWithGroupResetDiagnostics(
                        () => !(bool)GetPrivateField(window, "operationRunning")
                            && (bool)GetPrivateField(
                                window,
                                "groupResetVerificationPending"),
                        "Accepted Group Reset status failure did not preserve the session continuation.",
                        window,
                        server);

                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x20D2));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x2049));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x2045));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(server.ReceivedRequests, 0x2028));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupActiveVerified"));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupIdentityConfigured"));
                    AssertEx.False(
                        (bool)GetPrivateField(
                            window,
                            "groupIdentityHomeCheckComplete"));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupProfileLocked"));

                    AssertGroupResetPendingAdmission(window);
                    AssertEx.Contains(
                        "No 0x2049 Replay",
                        Convert.ToString(
                            window.ButtonGroupReset.Content,
                            CultureInfo.InvariantCulture));

                    Click(window.ButtonGroupReset);
                    WaitUntilWithGroupResetDiagnostics(
                        () => !(bool)GetPrivateField(window, "operationRunning")
                            && !(bool)GetPrivateField(
                                window,
                                "groupResetVerificationPending"),
                        "Group Reset status-only resume did not complete stable full-clear proof.",
                        window,
                        server);

                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x20D2),
                        "Resume unexpectedly refreshed the pinned member snapshot.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x2049),
                        "Resume replayed Group Reset.");
                    AssertEx.Equal(
                        4,
                        CountRequestCommand(server.ReceivedRequests, 0x2045));
                    AssertEx.Equal(
                        6,
                        CountRequestCommand(server.ReceivedRequests, 0x2028));
                    AssertEx.False(
                        (bool)InvokePrivate(
                            window,
                            "HasUnresolvedGroupResetState"));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupActiveVerified"));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupIdentityConfigured"));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupProfileLocked"));
                    AssertEx.True(
                        window.ButtonGroupPowerOn.IsEnabled,
                        "Verified Reset must require a fresh Power On preparation.");
                    AssertEx.False(window.ButtonGroupEnable.IsEnabled);
                    AssertEx.False(window.ButtonSetKinTransform.IsEnabled);
                    AssertEx.False(window.ButtonGroupMoveLinear.IsEnabled);
                    AssertEx.True(window.ButtonCloseConnection.IsEnabled);
                    AssertEx.Contains(
                        "Power On, Set Identity, and Enable again",
                        window.TextGroupResult.Text);

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
            ConnectionLossDropsSessionContinuationWithoutReplay()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(
                11,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(GroupResetMembersStep());
            steps.Add(CreateGroupResetStep());
            var disconnectingStatus = GroupEnableWaitStatusStep(0);
            disconnectingStatus.CloseClientBeforeResponse = true;
            steps.Add(disconnectingStatus);

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

                    Click(window.ButtonGroupReset);
                    WaitUntilWithGroupResetDiagnostics(
                        () => !(bool)GetPrivateField(window, "operationRunning")
                            && !((LMCConnection)GetPrivateField(
                                    window,
                                    "connection"))
                                .IsConnected
                            && (bool)GetPrivateField(
                                window,
                                "groupResetSessionContinuationDiscarded"),
                        "Connection loss did not discard the session-bound Group Reset continuation.",
                        window,
                        server);

                    AssertEx.True(
                        (bool)InvokePrivate(
                            window,
                            "HasUnresolvedGroupResetState"));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupActiveVerified"));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupIdentityConfigured"));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupProfileLocked"));
                    AssertEx.False(window.ButtonCloseConnection.IsEnabled);
                    AssertEx.True(
                        window.ButtonConnect.IsEnabled,
                        "A disconnected durable-only Group Reset must allow exact reconnect.");
                    AssertEx.Contains(
                        "durable recovery record remains fail-closed",
                        window.TextExecutionLog.Text);

                    ForceCloseWindowWithActiveRecovery(window);
                    window = null;
                    server.Verify();
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x2049),
                        "Handle/session clear automatically replayed Group Reset.");
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void SafeDisableSupersedesWithoutResetReplay()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(GroupResetMembersStep());
            steps.Add(CreateGroupResetStep());
            steps.Add(GroupStopFailedStatusStep());
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(GroupDisableStep());
            AddStableGroupDisabledSteps(steps);
            steps.Add(CapabilitiesStep(13, capabilities));
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

                    Click(window.ButtonGroupReset);
                    WaitUntilWithGroupResetDiagnostics(
                        () => !(bool)GetPrivateField(window, "operationRunning")
                            && (bool)GetPrivateField(
                                window,
                                "groupResetVerificationPending"),
                        "Accepted Group Reset did not enter pending verification.",
                        window,
                        server);
                    AssertEx.True(window.ButtonGroupDisable.IsEnabled);

                    Click(window.ButtonGroupDisable);
                    WaitUntilWithGroupResetDiagnostics(
                        () => !(bool)GetPrivateField(window, "operationRunning")
                            && !(bool)InvokePrivate(
                                window,
                                "HasUnresolvedGroupResetState")
                            && GetGroupProfileLockRecoveryJournal(window)
                                .CurrentRecord.State
                                == GroupProfileLockRecoveryState.Resolved,
                        "Stable safe Disable did not supersede the pending Reset attribution.",
                        window,
                        server);

                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x2049));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x2048));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupActiveVerified"));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupIdentityConfigured"));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupProfileLocked"));
                    AssertEx.True(window.ButtonCloseConnection.IsEnabled);
                    AssertEx.Contains(
                        "superseded the pending Group Reset",
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

        private static void
            DelayedResetResultPublishesOnlyAfterGroupStopNack()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(
                11,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(GroupResetMembersStep());
            var delayedReset = CreateGroupResetStep();
            delayedReset.ResponseDelayMilliseconds = 400;
            steps.Add(delayedReset);
            steps.Add(GroupResetStopRejectedStep());
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
                    window.TextGroupResult.Text = "RESET_RESULT_SENTINEL";

                    Click(window.ButtonGroupReset);
                    WaitUntilWithGroupResetDiagnostics(
                        () => CountRequestCommand(
                            server.ReceivedRequests,
                            0x2049) == 1,
                        "The delayed Group Reset request did not start.",
                        window,
                        server);
                    Click(window.ButtonGroupStop);

                    WaitUntilWithGroupResetDiagnostics(
                        () => CountRequestCommand(
                                server.ReceivedRequests,
                                0x2085) == 1
                            && !(bool)GetPrivateField(
                                window,
                                "safetyCommandRunning")
                            && (bool)GetPrivateField(
                                window,
                                "groupResetSubmissionUncertain"),
                        "Rejected Group Stop did not publish the deferred uncertain Reset result.",
                        window,
                        server);

                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x2049));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x2085));
                    AssertEx.True(
                        (bool)InvokePrivate(
                            window,
                            "HasUnresolvedGroupResetState"));
                    AssertEx.True(
                        GetPrivateField(
                            window,
                            "pendingGroupResetWaitContinuation") == null);
                    AssertEx.Contains(
                        "submission outcome is uncertain",
                        window.TextGroupResult.Text);
                    AssertEx.Contains(
                        "held until the in-flight safety command",
                        window.TextExecutionLog.Text);
                    AssertEx.False(window.ButtonGroupReset.IsEnabled);
                    AssertEx.True(window.ButtonGroupStop.IsEnabled);
                    AssertEx.True(window.ButtonGroupPowerOff.IsEnabled);
                    AssertEx.True(window.ButtonGroupDisable.IsEnabled);

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
            AckBoundaryConnectionLossDropsUncertainStateWithoutReplay()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(
                11,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(GroupResetMembersStep());
            var disconnectingReset = CreateGroupResetStep();
            disconnectingReset.CloseClientBeforeResponse = true;
            steps.Add(disconnectingReset);

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

                    Click(window.ButtonGroupReset);
                    WaitUntilWithGroupResetDiagnostics(
                        () => !(bool)GetPrivateField(window, "operationRunning")
                            && !((LMCConnection)GetPrivateField(
                                    window,
                                    "connection"))
                                .IsConnected
                            && (bool)GetPrivateField(
                                window,
                                "groupResetSessionContinuationDiscarded"),
                        "Reset acknowledgement-boundary connection loss did not discard the session interlock.",
                        window,
                        server);

                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x2049));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(server.ReceivedRequests, 0x2045));
                    AssertEx.False(
                        (bool)GetPrivateField(
                            window,
                            "groupResetSubmissionUncertain"));
                    AssertEx.False(
                        (bool)GetPrivateField(
                            window,
                            "groupResetVerificationPending"));
                    AssertEx.True(
                        (bool)InvokePrivate(
                            window,
                            "HasUnresolvedGroupResetState"));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupActiveVerified"));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupIdentityConfigured"));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupProfileLocked"));
                    AssertEx.Contains(
                        "durable recovery record remains fail-closed",
                        window.TextExecutionLog.Text);
                    AssertEx.True(
                        window.ButtonConnect.IsEnabled,
                        "Outcome-uncertain durable Reset must allow exact reconnect.");

                    ForceCloseWindowWithActiveRecovery(window);
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

        private static void ValidNackRetainsPreparation()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(
                11,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(GroupResetMembersStep());
            steps.Add(CreateGroupResetRejectedStep());
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
                    SetPrivateField(window, "groupIdentityHomeCheckComplete", true);
                    SetPrivateField(window, "groupIdentityHomeCheckPassed", true);
                    SetPrivateField(window, "groupProfileLocked", true);
                    InvokePrivate(window, "UpdateUiState");

                    Click(window.ButtonGroupReset);
                    WaitUntilWithGroupResetDiagnostics(
                        () => !(bool)GetPrivateField(window, "operationRunning"),
                        "Rejected Group Reset did not return to idle.",
                        window,
                        server);

                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x2049));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(server.ReceivedRequests, 0x2045));
                    AssertEx.True(
                        (bool)GetPrivateField(window, "groupActiveVerified"));
                    AssertEx.True(
                        (bool)GetPrivateField(window, "groupIdentityConfigured"));
                    AssertEx.True(
                        (bool)GetPrivateField(
                            window,
                            "groupIdentityHomeCheckComplete"));
                    AssertEx.True(
                        (bool)GetPrivateField(window, "groupProfileLocked"));
                    AssertEx.False(
                        (bool)GetPrivateField(
                            window,
                            "groupResetSubmissionUncertain"));
                    AssertEx.False(
                        (bool)InvokePrivate(
                            window,
                            "HasUnresolvedGroupResetState"));
                    AssertEx.True(window.ButtonGroupReset.IsEnabled);
                    AssertEx.True(window.ButtonCloseConnection.IsEnabled);
                    AssertEx.Contains(
                        "existing preparation state was retained",
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

        private static void
            UncertainSubmissionBlocksReplayUntilSafeDisable()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(GroupResetMembersStep());
            steps.Add(CreateMalformedGroupResetStep());
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(GroupDisableStep());
            AddStableGroupDisabledSteps(steps);
            steps.Add(CapabilitiesStep(13, capabilities));
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
                    SetPrivateField(window, "groupIdentityHomeCheckComplete", true);
                    SetPrivateField(window, "groupIdentityHomeCheckPassed", true);
                    SetPrivateField(window, "groupProfileLocked", true);
                    InvokePrivate(window, "UpdateUiState");

                    Click(window.ButtonGroupReset);
                    WaitUntilWithGroupResetDiagnostics(
                        () => !(bool)GetPrivateField(window, "operationRunning")
                            && (bool)GetPrivateField(
                                window,
                                "groupResetSubmissionUncertain"),
                        "Malformed Reset acknowledgement did not establish the outcome-uncertain interlock.",
                        window,
                        server);

                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x2049));
                    AssertEx.True(
                        (bool)InvokePrivate(
                            window,
                            "HasUnresolvedGroupResetState"));
                    AssertEx.True(
                        GetPrivateField(
                            window,
                            "pendingGroupResetWaitContinuation") == null);
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupActiveVerified"));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupIdentityConfigured"));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupProfileLocked"));
                    AssertEx.False(window.ButtonGroupReset.IsEnabled);
                    AssertEx.False(window.ButtonConnect.IsEnabled);
                    AssertEx.False(window.ButtonCloseConnection.IsEnabled);
                    AssertEx.False(window.ButtonGroupPowerOn.IsEnabled);
                    AssertEx.False(window.ButtonGroupEnable.IsEnabled);
                    AssertEx.False(window.ButtonSetKinTransform.IsEnabled);
                    AssertEx.False(window.ButtonGroupMoveLinear.IsEnabled);
                    AssertEx.True(window.ButtonGroupReadStatus.IsEnabled);
                    AssertEx.True(window.ButtonGroupStop.IsEnabled);
                    AssertEx.True(window.ButtonGroupPowerOff.IsEnabled);
                    AssertEx.True(window.ButtonGroupDisable.IsEnabled);
                    AssertEx.Contains(
                        "Reset Replay Blocked",
                        Convert.ToString(
                            window.ButtonGroupReset.Content,
                            CultureInfo.InvariantCulture));

                    var reconnect =
                        (DiagnosticsAdmissionDecision)InvokePrivate(
                            window,
                            "EvaluateDiagnosticsAdmission",
                            DiagnosticsAdmissionOperation.ConnectOrReconnect,
                            true);
                    AssertEx.False(reconnect.IsAllowed);
                    var mutation =
                        (DiagnosticsAdmissionDecision)InvokePrivate(
                            window,
                            "EvaluateDiagnosticsAdmission",
                            DiagnosticsAdmissionOperation.NewLiveOrMutation,
                            true);
                    AssertEx.False(mutation.IsAllowed);
                    var safety =
                        (DiagnosticsAdmissionDecision)InvokePrivate(
                            window,
                            "EvaluateDiagnosticsAdmission",
                            DiagnosticsAdmissionOperation.SafetyControl,
                            true);
                    AssertEx.True(safety.IsAllowed);

                    Click(window.ButtonGroupDisable);
                    WaitUntilWithGroupResetDiagnostics(
                        () => !(bool)GetPrivateField(window, "operationRunning")
                            && !(bool)InvokePrivate(
                                window,
                                "HasUnresolvedGroupResetState")
                            && GetGroupProfileLockRecoveryJournal(window)
                                .CurrentRecord.State
                                == GroupProfileLockRecoveryState.Resolved,
                        "Safe Disable did not resolve the outcome-uncertain Reset interlock.",
                        window,
                        server);

                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x2049),
                        "Safe recovery replayed Group Reset.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x2048));
                    AssertEx.False(
                        (bool)GetPrivateField(
                            window,
                            "groupResetSubmissionUncertain"));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupActiveVerified"));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupIdentityConfigured"));
                    AssertEx.True(window.ButtonCloseConnection.IsEnabled);

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

        private static void GroupStopNackPreservesStatusOnlyResume()
        {
            RunSafetyNackPreservesStatusOnlyResume(
                GroupResetSafetyNackKind.GroupStop);
        }

        private static void GroupPowerOffNackPreservesStatusOnlyResume()
        {
            RunSafetyNackPreservesStatusOnlyResume(
                GroupResetSafetyNackKind.GroupPowerOff);
        }

        private static void GroupDisableNackPreservesStatusOnlyResume()
        {
            RunSafetyNackPreservesStatusOnlyResume(
                GroupResetSafetyNackKind.GroupDisable);
        }

        private static void RunSafetyNackPreservesStatusOnlyResume(
            GroupResetSafetyNackKind kind)
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(GroupResetMembersStep());
            steps.Add(CreateGroupResetStep());
            steps.Add(GroupStopFailedStatusStep());
            switch (kind)
            {
                case GroupResetSafetyNackKind.GroupStop:
                    steps.Add(GroupResetStopRejectedStep());
                    break;
                case GroupResetSafetyNackKind.GroupPowerOff:
                    steps.Add(GroupResetPowerOffRejectedStep());
                    break;
                case GroupResetSafetyNackKind.GroupDisable:
                    steps.Add(CapabilitiesStep(12, capabilities));
                    steps.Add(GroupDisableRejectedStep());
                    break;
                default:
                    throw new ArgumentOutOfRangeException("kind", kind, null);
            }
            AddStableGroupResetClearRounds(steps, 3);
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

                    Click(window.ButtonGroupReset);
                    WaitUntilWithGroupResetDiagnostics(
                        () => !(bool)GetPrivateField(window, "operationRunning")
                            && (bool)GetPrivateField(
                                window,
                                "groupResetVerificationPending"),
                        "Accepted Group Reset did not publish the pending continuation before the safety NACK test.",
                        window,
                        server);
                    var currentGroup = (LMCGroupAxis)GetPrivateField(
                        window,
                        "group");
                    var resetContinuation =
                        currentGroup.PendingGroupResetWaitContinuation;
                    AssertEx.NotNull(resetContinuation);
                    AssertEx.True(resetContinuation.IsPending);

                    switch (kind)
                    {
                        case GroupResetSafetyNackKind.GroupStop:
                            Click(window.ButtonGroupStop);
                            break;
                        case GroupResetSafetyNackKind.GroupPowerOff:
                            Click(window.ButtonGroupPowerOff);
                            break;
                        case GroupResetSafetyNackKind.GroupDisable:
                            Click(window.ButtonGroupDisable);
                            break;
                    }

                    WaitUntilWithGroupResetDiagnostics(
                        () => !(bool)GetPrivateField(window, "operationRunning")
                            && !(bool)GetPrivateField(
                                window,
                                "safetyCommandRunning")
                            && window.ButtonGroupReset.IsEnabled,
                        "Safety NACK did not preserve an available status-only Group Reset resume.",
                        window,
                        server);

                    AssertEx.True(
                        resetContinuation.IsPending,
                        "A valid safety NACK terminally superseded the pending Reset continuation.");
                    AssertEx.True(
                        (bool)GetPrivateField(
                            window,
                            "groupResetVerificationPending"));
                    AssertEx.True(
                        (bool)InvokePrivate(
                            window,
                            "HasUnresolvedGroupResetState"));
                    AssertEx.True(
                        ReferenceEquals(
                            resetContinuation,
                            GetPrivateField(
                                window,
                                "pendingGroupResetWaitContinuation")));
                    AssertEx.Contains(
                        "No 0x2049 Replay",
                        Convert.ToString(
                            window.ButtonGroupReset.Content,
                            CultureInfo.InvariantCulture));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x2049));

                    Click(window.ButtonGroupReset);
                    WaitUntilWithGroupResetDiagnostics(
                        () => !(bool)GetPrivateField(window, "operationRunning")
                            && !(bool)InvokePrivate(
                                window,
                                "HasUnresolvedGroupResetState"),
                        "Status-only Group Reset resume did not complete after the safety NACK.",
                        window,
                        server);

                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x2049),
                        "Status-only recovery replayed Group Reset after a safety NACK.");
                    AssertEx.Equal(
                        4,
                        CountRequestCommand(server.ReceivedRequests, 0x2045));
                    AssertEx.Equal(
                        6,
                        CountRequestCommand(server.ReceivedRequests, 0x2028));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupActiveVerified"));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupIdentityConfigured"));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupProfileLocked"));

                    switch (kind)
                    {
                        case GroupResetSafetyNackKind.GroupStop:
                            AssertEx.Equal(
                                1,
                                CountRequestCommand(
                                    server.ReceivedRequests,
                                    0x2085));
                            CloseConnectedWindow(window);
                            break;
                        case GroupResetSafetyNackKind.GroupPowerOff:
                            AssertEx.Equal(
                                1,
                                CountRequestCommand(
                                    server.ReceivedRequests,
                                    0x204B));
                            CloseConnectedWindow(window);
                            break;
                        case GroupResetSafetyNackKind.GroupDisable:
                            AssertEx.Equal(
                                1,
                                CountRequestCommand(
                                    server.ReceivedRequests,
                                    0x2048));
                            AssertEx.True(
                                GetGroupProfileLockRecoveryJournal(window)
                                    .CurrentRecord.IsActive,
                                "A rejected Group Disable must retain its own fail-closed recovery record.");
                            CloseConnectionWithActiveRecovery(window);
                            break;
                    }
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
            CapturedMemberAxisStopSupersedesWithoutResetReplay()
        {
            RunCapturedMemberAxisSafetySupersedesReset(false);
        }

        private static void
            CapturedMemberAxisPowerOffSupersedesWithoutResetReplay()
        {
            RunCapturedMemberAxisSafetySupersedesReset(true);
        }

        private static void RunCapturedMemberAxisSafetySupersedesReset(
            bool powerOff)
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(D5AxisLookupStep(GroupResetMemberOneReference));
            steps.Add(D5AxisInfoStep(GroupResetMemberOneReference));
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(GroupResetMembersStep());
            steps.Add(CreateGroupResetStep());
            steps.Add(GroupStopFailedStatusStep());
            if (powerOff)
            {
                steps.Add(CapabilitiesStep(12, capabilities));
                steps.Add(AxisPowerCommandStep(false));
                steps.Add(AxisPowerStatusStep(false));
                steps.Add(AxisPowerStatusStep(false));
                steps.Add(AxisPowerStatusStep(false));
                steps.Add(CapabilitiesStep(13, capabilities));
            }
            else
            {
                steps.Add(AxisStopCommandStep());
                steps.Add(AxisResetStatusStep(state: 0x02000000u));
                steps.Add(AxisResetStatusStep(state: 0x02000000u));
                steps.Add(AxisResetStatusStep(state: 0x02000000u));
                steps.Add(CapabilitiesStep(12, capabilities));
            }
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
                    Click(window.ButtonLookupAxis);
                    WaitUntilWithGroupResetDiagnostics(
                        () => string.Equals(
                                window.TextAxisReference.Text,
                                GroupResetMemberOneReference.ToString(
                                    CultureInfo.InvariantCulture),
                                StringComparison.Ordinal)
                            && window.ButtonStop.IsEnabled
                            && window.ButtonPowerOff.IsEnabled,
                        "The captured Group Reset member axis did not load.",
                        window,
                        server);
                    SetPrivateField(window, "groupIdentityHomeCheckComplete", true);
                    SetPrivateField(window, "groupIdentityHomeCheckPassed", true);
                    SetPrivateField(window, "groupProfileLocked", true);
                    InvokePrivate(window, "UpdateUiState");

                    Click(window.ButtonGroupReset);
                    WaitUntilWithGroupResetDiagnostics(
                        () => !(bool)GetPrivateField(window, "operationRunning")
                            && (bool)GetPrivateField(
                                window,
                                "groupResetVerificationPending"),
                        "Accepted Group Reset did not publish its continuation before the member-axis safety mutation.",
                        window,
                        server);
                    var resetContinuation = ((LMCGroupAxis)GetPrivateField(
                            window,
                            "group"))
                        .PendingGroupResetWaitContinuation;
                    AssertEx.NotNull(resetContinuation);
                    AssertEx.True(resetContinuation.IsPending);

                    if (powerOff)
                    {
                        Click(window.ButtonPowerOff);
                    }
                    else
                    {
                        Click(window.ButtonStop);
                    }
                    var completedState = powerOff
                        ? "Power Off verified"
                        : "Stop verified";
                    WaitUntilWithGroupResetDiagnostics(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                completedState,
                                StringComparison.Ordinal)
                            && !(bool)GetPrivateField(
                                window,
                                "safetyCommandRunning")
                            && (int)GetPrivateField(
                                window,
                                "safetyMonitorCount") == 0
                            && !(bool)InvokePrivate(
                                window,
                                "HasUnresolvedGroupResetState"),
                        "Captured-member axis safety mutation did not terminally supersede Reset.",
                        window,
                        server);

                    AssertEx.False(resetContinuation.IsPending);
                    AssertEx.Equal(
                        LMCGroupResetWaitContinuationState
                            .SupersededBySafetyMutation,
                        resetContinuation.State);
                    AssertEx.False(
                        (bool)GetPrivateField(
                            window,
                            "groupResetVerificationPending"));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x2049));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            powerOff ? (ushort)0x2023 : (ushort)0x2022));
                    AssertEx.Equal(
                        3,
                        CountRequestCommand(server.ReceivedRequests, 0x2028));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupActiveVerified"));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupIdentityConfigured"));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupProfileLocked"));
                    AssertEx.Contains(
                        "superseded the pending Group Reset",
                        window.TextExecutionLog.Text);
                    AssertEx.True(window.ButtonCloseConnection.IsEnabled);

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
            CapturedMemberAxisStopNackPreservesStatusOnlyResume()
        {
            RunCapturedMemberAxisSafetyNackPreservesReset(false);
        }

        private static void
            CapturedMemberAxisPowerOffNackPreservesStatusOnlyResume()
        {
            RunCapturedMemberAxisSafetyNackPreservesReset(true);
        }

        private static void RunCapturedMemberAxisSafetyNackPreservesReset(
            bool powerOff)
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(D5AxisLookupStep(GroupResetMemberOneReference));
            steps.Add(D5AxisInfoStep(GroupResetMemberOneReference));
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(GroupResetMembersStep());
            steps.Add(CreateGroupResetStep());
            steps.Add(GroupStopFailedStatusStep());
            if (powerOff)
            {
                steps.Add(CapabilitiesStep(12, capabilities));
                steps.Add(GroupResetAxisPowerOffRejectedStep());
            }
            else
            {
                steps.Add(GroupResetAxisStopRejectedStep());
            }
            AddStableGroupResetClearRounds(steps, 3);
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
                    Click(window.ButtonLookupAxis);
                    WaitUntilWithGroupResetDiagnostics(
                        () => string.Equals(
                                window.TextAxisReference.Text,
                                GroupResetMemberOneReference.ToString(
                                    CultureInfo.InvariantCulture),
                                StringComparison.Ordinal)
                            && window.ButtonStop.IsEnabled
                            && window.ButtonPowerOff.IsEnabled,
                        "The captured member axis did not load before the safety NACK test.",
                        window,
                        server);
                    SetPrivateField(window, "groupProfileLocked", true);
                    InvokePrivate(window, "UpdateUiState");

                    Click(window.ButtonGroupReset);
                    WaitUntilWithGroupResetDiagnostics(
                        () => !(bool)GetPrivateField(window, "operationRunning")
                            && (bool)GetPrivateField(
                                window,
                                "groupResetVerificationPending"),
                        "Accepted Group Reset did not enter pending verification before the captured-member NACK.",
                        window,
                        server);
                    var resetContinuation = ((LMCGroupAxis)GetPrivateField(
                            window,
                            "group"))
                        .PendingGroupResetWaitContinuation;
                    AssertEx.NotNull(resetContinuation);
                    AssertEx.True(resetContinuation.IsPending);

                    Click(powerOff
                        ? window.ButtonPowerOff
                        : window.ButtonStop);
                    WaitUntilWithGroupResetDiagnostics(
                        () => !(bool)GetPrivateField(window, "operationRunning")
                            && !(bool)GetPrivateField(
                                window,
                                "safetyCommandRunning")
                            && window.ButtonGroupReset.IsEnabled,
                        "Captured-member safety NACK did not preserve status-only Reset resume.",
                        window,
                        server);

                    AssertEx.True(resetContinuation.IsPending);
                    AssertEx.Equal(
                        LMCGroupResetWaitContinuationState.Pending,
                        resetContinuation.State);
                    AssertEx.True(
                        (bool)GetPrivateField(
                            window,
                            "groupResetVerificationPending"));
                    AssertEx.True(
                        (bool)InvokePrivate(
                            window,
                            "HasUnresolvedGroupResetState"));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x2049));

                    Click(window.ButtonGroupReset);
                    WaitUntilWithGroupResetDiagnostics(
                        () => !(bool)GetPrivateField(window, "operationRunning")
                            && !(bool)InvokePrivate(
                                window,
                                "HasUnresolvedGroupResetState"),
                        "Status-only Reset resume did not complete after the captured-member safety NACK.",
                        window,
                        server);

                    AssertEx.Equal(
                        LMCGroupResetWaitContinuationState.Completed,
                        resetContinuation.State);
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x2049));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            powerOff ? (ushort)0x2023 : (ushort)0x2022));
                    AssertEx.Equal(
                        4,
                        CountRequestCommand(server.ReceivedRequests, 0x2045));
                    AssertEx.Equal(
                        6,
                        CountRequestCommand(server.ReceivedRequests, 0x2028));
                    AssertEx.True(window.ButtonCloseConnection.IsEnabled);

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
            MemberSnapshotFailureDoesNotDispatchAndRetainsPreparation()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(
                11,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(GroupResetMembersRejectedStep());
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
                    SetPrivateField(window, "groupIdentityHomeCheckComplete", true);
                    SetPrivateField(window, "groupIdentityHomeCheckPassed", true);
                    SetPrivateField(window, "groupProfileLocked", true);
                    InvokePrivate(window, "UpdateUiState");

                    Click(window.ButtonGroupReset);
                    WaitUntilWithGroupResetDiagnostics(
                        () => !(bool)GetPrivateField(window, "operationRunning"),
                        "Rejected member snapshot did not return Group Reset to idle.",
                        window,
                        server);

                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x20D2));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(server.ReceivedRequests, 0x2049));
                    AssertEx.True(
                        (bool)GetPrivateField(window, "groupActiveVerified"));
                    AssertEx.True(
                        (bool)GetPrivateField(window, "groupIdentityConfigured"));
                    AssertEx.True(
                        (bool)GetPrivateField(
                            window,
                            "groupIdentityHomeCheckComplete"));
                    AssertEx.True(
                        (bool)GetPrivateField(window, "groupProfileLocked"));
                    AssertEx.False(
                        (bool)GetPrivateField(
                            window,
                            "groupResetSubmissionUncertain"));
                    AssertEx.False(
                        (bool)InvokePrivate(
                            window,
                            "HasUnresolvedGroupResetState"));
                    AssertEx.True(window.ButtonGroupReset.IsEnabled);
                    AssertEx.True(window.ButtonCloseConnection.IsEnabled);
                    AssertEx.Contains(
                        "Existing preparation state was retained",
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

        private static void LockedStandbyProofEnablesOnlySafeDisable()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(
                11,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(GroupResetMembersStep());
            steps.Add(CreateGroupResetStep());
            AddStableGroupResetClearRounds(
                steps,
                3,
                GroupEnableWaitPowerOn | GroupEnableWaitStandby);
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

                    Click(window.ButtonGroupReset);
                    WaitUntilWithGroupResetDiagnostics(
                        () => !(bool)GetPrivateField(window, "operationRunning")
                            && !(bool)InvokePrivate(
                                window,
                                "HasUnresolvedGroupResetState"),
                        "LockedStandby Reset proof did not complete.",
                        window,
                        server);

                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x2049));
                    AssertEx.Equal(
                        3,
                        CountRequestCommand(server.ReceivedRequests, 0x2045));
                    AssertEx.Equal(
                        6,
                        CountRequestCommand(server.ReceivedRequests, 0x2028));
                    AssertEx.True(
                        (bool)GetPrivateField(
                            window,
                            "groupResetObservedLockedStandby"));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupActiveVerified"));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupIdentityConfigured"));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupProfileLocked"));
                    AssertEx.True(
                        window.ButtonGroupDisable.IsEnabled,
                        "Observed LockedStandby must expose the fresh safe Disable path.");
                    AssertEx.False(window.ButtonGroupEnable.IsEnabled);
                    AssertEx.False(window.ButtonSetKinTransform.IsEnabled);
                    AssertEx.False(window.ButtonGroupMoveLinear.IsEnabled);
                    AssertEx.Contains(
                        "Observed Reset LockedStandby",
                        Convert.ToString(
                            window.ButtonGroupDisable.Content,
                            CultureInfo.InvariantCulture));
                    AssertEx.Contains(
                        "does not authorize motion",
                        window.TextGroupResult.Text);
                    AssertEx.True(window.ButtonCloseConnection.IsEnabled);

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

        private static void AssertGroupResetPendingAdmission(
            MainWindow window)
        {
            AssertEx.True(window.ButtonGroupReset.IsEnabled);
            AssertEx.False(window.ButtonGroupPowerOn.IsEnabled);
            AssertEx.False(window.ButtonGroupEnable.IsEnabled);
            AssertEx.False(window.ButtonSetKinTransform.IsEnabled);
            AssertEx.False(window.ButtonGroupMoveLinear.IsEnabled);
            AssertEx.False(window.ButtonCloseConnection.IsEnabled);
            AssertEx.True(window.ButtonGroupReadStatus.IsEnabled);
            AssertEx.True(window.ButtonGroupStop.IsEnabled);
            AssertEx.True(window.ButtonGroupPowerOff.IsEnabled);
            AssertEx.True(
                window.ButtonGroupDisable.IsEnabled,
                "Fail-closed Reset state must retain the safe Disable path.");

            var mutation = (DiagnosticsAdmissionDecision)InvokePrivate(
                window,
                "EvaluateDiagnosticsAdmission",
                DiagnosticsAdmissionOperation.NewLiveOrMutation,
                true);
            AssertEx.False(mutation.IsAllowed);
            var trackedMutation =
                (DiagnosticsAdmissionDecision)InvokePrivate(
                    window,
                    "EvaluateDiagnosticsAdmission",
                    DiagnosticsAdmissionOperation.TrackedD5Submit,
                    true);
            AssertEx.False(trackedMutation.IsAllowed);
            var trackedRead = (DiagnosticsAdmissionDecision)InvokePrivate(
                window,
                "EvaluateDiagnosticsAdmission",
                DiagnosticsAdmissionOperation.TrackedD5ReadOnlyInspection,
                true);
            AssertEx.True(trackedRead.IsAllowed);
            var cleanup = (DiagnosticsAdmissionDecision)InvokePrivate(
                window,
                "EvaluateDiagnosticsAdmission",
                DiagnosticsAdmissionOperation.ExistingResourceCleanup,
                true);
            AssertEx.True(cleanup.IsAllowed);
            var safety = (DiagnosticsAdmissionDecision)InvokePrivate(
                window,
                "EvaluateDiagnosticsAdmission",
                DiagnosticsAdmissionOperation.SafetyControl,
                true);
            AssertEx.True(safety.IsAllowed);
            var reconnect = (DiagnosticsAdmissionDecision)InvokePrivate(
                window,
                "EvaluateDiagnosticsAdmission",
                DiagnosticsAdmissionOperation.ConnectOrReconnect,
                true);
            AssertEx.False(reconnect.IsAllowed);
            AssertEx.False(window.ButtonConnect.IsEnabled);
        }

        private static void WaitUntilWithGroupResetDiagnostics(
            Func<bool> condition,
            string message,
            MainWindow window,
            FakeRpcServer server)
        {
            try
            {
                WaitUntil(condition, message);
            }
            catch (TimeoutException error)
            {
                var commands = new List<string>();
                foreach (var request in server.ReceivedRequests)
                {
                    commands.Add(
                        "0x"
                        + TestFrame.ReadUInt16(request, 0).ToString(
                            "X4",
                            CultureInfo.InvariantCulture));
                }

                throw new TimeoutException(
                    message
                    + " OperationState="
                    + window.TextOperationState.Text
                    + ", GroupResult="
                    + window.TextGroupResult.Text
                    + ", Requests="
                    + string.Join(",", commands.ToArray())
                    + ", Log="
                    + window.TextExecutionLog.Text,
                    error);
            }
        }

        private static FakeRpcStep GroupResetMembersStep()
        {
            var payload = new byte[1350];
            TestFrame.WriteUInt16(
                payload,
                0,
                GroupResetMemberOneReference);
            TestFrame.WriteUInt16(
                payload,
                2,
                GroupResetMemberTwoReference);
            TestFrame.WriteUInt16(payload, 32, 101);
            TestFrame.WriteUInt16(payload, 34, 102);
            WriteGroupResetMemberName(payload, 68, "LMCAXis1");
            WriteGroupResetMemberName(payload, 148, "LMCAXis2");
            payload[1348] = 2;

            return new FakeRpcStep(
                0x20D2,
                TestFrame.Response(0, payload))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    TestFrame.Request(
                        0x20D2,
                        GroupEnableWaitReference,
                        new byte[] { 1 }),
                    request)
            };
        }

        internal static FakeRpcStep[] CreateGroupResetProcessRpcSteps(
            int killBoundary,
            System.Threading.ManualResetEventSlim heldStatusRelease,
            System.Threading.ManualResetEventSlim heldStatusEntered)
        {
            if (killBoundary < 0 || killBoundary > 2)
            {
                throw new ArgumentOutOfRangeException("killBoundary");
            }

            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(GroupResetMembersStep());
            if (killBoundary >= 1)
            {
                steps.Add(CreateGroupResetStep());
            }

            if (killBoundary == 2)
            {
                AddStableGroupResetClearRounds(steps, 1);
                var heldStatus = GroupEnableWaitStatusStep(0);
                heldStatus.BeforeResponse = () =>
                {
                    if (heldStatusEntered != null)
                    {
                        heldStatusEntered.Set();
                    }
                    if (heldStatusRelease != null
                        && !heldStatusRelease.Wait(15000))
                    {
                        throw new TimeoutException(
                            "The held second-round Group Reset status response was not released.");
                    }
                };
                heldStatus.AllowClientDisconnectAfterRequest = true;
                heldStatus
                    .ContinueWithNextClientAfterResponseWriteDisconnect = true;
                steps.Add(heldStatus);
            }
            else
            {
                steps.Add(new FakeRpcStep(0, null)
                {
                    RequireClientDisconnectBeforeRequest = true,
                    ContinueWithNextClientAfterDisconnect = true
                });
            }

            steps.AddRange(CreateConnectAndTopologySteps(capabilities));
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(GroupResetMembersStep());
            AddStableGroupResetClearRounds(steps, 3);
            steps.Add(CloseStep());
            return steps.ToArray();
        }

        private static FakeRpcStep GroupResetMembersRejectedStep()
        {
            return new FakeRpcStep(
                0x20D2,
                TestFrame.Response(1, new byte[1350]))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    TestFrame.Request(
                        0x20D2,
                        GroupEnableWaitReference,
                        new byte[] { 1 }),
                    request)
            };
        }

        private static FakeRpcStep CreateGroupResetRejectedStep()
        {
            return new FakeRpcStep(
                0x2049,
                TestFrame.Response(
                    1,
                    TestFrame.Hex("00 00 00 00 00 00 00 00")))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    TestFrame.Request(
                        0x2049,
                        GroupEnableWaitReference,
                        new byte[] { 1 }),
                    request)
            };
        }

        private static FakeRpcStep CreateMalformedGroupResetStep()
        {
            return new FakeRpcStep(
                0x2049,
                TestFrame.Response(0, new byte[0]))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    TestFrame.Request(
                        0x2049,
                        GroupEnableWaitReference,
                        new byte[] { 1 }),
                    request)
            };
        }

        private static FakeRpcStep GroupResetStopRejectedStep()
        {
            return new FakeRpcStep(
                0x2085,
                TestFrame.Response(
                    1,
                    TestFrame.Hex("00 00 00 00 00 00 00 00")));
        }

        private static FakeRpcStep GroupResetPowerOffRejectedStep()
        {
            return new FakeRpcStep(
                0x204B,
                TestFrame.Response(
                    1,
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

        private static FakeRpcStep GroupResetAxisStopRejectedStep()
        {
            return new FakeRpcStep(
                0x2022,
                TestFrame.Response(
                    0,
                    TestFrame.Hex("01 00 F9 FF")));
        }

        private static FakeRpcStep GroupResetAxisPowerOffRejectedStep()
        {
            return new FakeRpcStep(
                0x2023,
                TestFrame.Response(
                    0,
                    TestFrame.Hex("01 00 F9 FF")))
            {
                InspectRequest = request => AssertEx.Equal(
                    (byte)0,
                    request[12])
            };
        }

        private static void WriteGroupResetMemberName(
            byte[] payload,
            int offset,
            string value)
        {
            for (var index = 0; index < value.Length; index++)
            {
                payload[offset + index] = checked((byte)value[index]);
            }
        }

        private static void AddStableGroupResetClearRounds(
            ICollection<FakeRpcStep> steps,
            int roundCount)
        {
            AddStableGroupResetClearRounds(steps, roundCount, 0);
        }

        private static void AddStableGroupResetClearRounds(
            ICollection<FakeRpcStep> steps,
            int roundCount,
            uint groupState)
        {
            for (var round = 0; round < roundCount; round++)
            {
                steps.Add(GroupEnableWaitStatusStep(groupState));
                steps.Add(GroupResetMemberStatusStep(
                    GroupResetMemberOneReference));
                steps.Add(GroupResetMemberStatusStep(
                    GroupResetMemberTwoReference));
            }
        }

        private static FakeRpcStep GroupResetMemberStatusStep(
            ushort axisReference)
        {
            var payload = new byte[12];
            var requestPayload = new byte[8];
            TestFrame.WriteInt32(requestPayload, 0, axisReference);
            TestFrame.WriteInt32(requestPayload, 4, 1);
            return new FakeRpcStep(
                0x2028,
                TestFrame.Response(0, payload))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    TestFrame.Request(
                        0x2028,
                        axisReference,
                        requestPayload),
                    request)
            };
        }
    }
}

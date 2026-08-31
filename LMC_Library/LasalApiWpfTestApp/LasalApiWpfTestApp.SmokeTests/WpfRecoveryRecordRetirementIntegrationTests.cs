using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using LasalMotionControlApiExample;
using LasalMotionControlLib;
using LasalMotionControlLib.Tests;

namespace LasalApiWpfTestApp.SmokeTests
{
    internal static partial class WpfMainWindowIntegrationTests
    {
        private const string RetirementTestOperator = "TEST\\operator";
        private const string RetirementTestReason =
            "Operator confirmed that the listed old-PLC outcomes remain unknown and retired their stale evidence.";

        internal static void RegisterRecoveryRecordRetirementIntegrationTests(
            ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.RecoveryRetirement.SetOperationModeStaleArchivesAfterIdleInterlock",
                SetOperationModeStaleArchivesAfterIdleInterlock);
            tests.Add(
                "Wpf.RecoveryRetirement.SetOperationModeCommittedDecisionFinalizesAtStartup",
                SetOperationModeCommittedDecisionFinalizesAtStartup);
            tests.Add(
                "Wpf.RecoveryRetirement.MismatchedRecordsArchiveResolveDisconnectAndRequireRestart",
                MismatchedRecordsArchiveResolveDisconnectAndRequireRestart);
            tests.Add(
                "Wpf.RecoveryRetirement.DiagnosticsMutationMismatchAllowsCloseExitAndPreservesBytes",
                DiagnosticsMutationMismatchAllowsCloseExitAndPreservesBytes);
            tests.Add(
                "Wpf.RecoveryRetirement.DiagnosticsMutationMismatchKeepsExactGroupResetAndAllowsExit",
                DiagnosticsMutationMismatchKeepsExactGroupResetAndAllowsExit);
            tests.Add(
                "Wpf.RecoveryRetirement.DiagnosticsMutationMismatchArchivesResolvesAndReopensLiveAdmission",
                DiagnosticsMutationMismatchArchivesResolvesAndReopensLiveAdmission);
            tests.Add(
                "Wpf.RecoveryRetirement.DiagnosticsMutationPendingDecisionFinalizesAtStartup",
                DiagnosticsMutationPendingDecisionFinalizesAtStartup);
            tests.Add(
                "Wpf.RecoveryRetirement.ExactCurrentRecordCannotRetire",
                ExactCurrentRecordCannotRetire);
            tests.Add(
                "Wpf.RecoveryRetirement.GroupResetBuildOnlyMismatchArchivesAndResolves",
                GroupResetBuildOnlyMismatchArchivesAndResolves);
            tests.Add(
                "Wpf.RecoveryRetirement.MixedExactAndStaleRetiresSubsetThenExactRecoveryOpensControl",
                MixedExactAndStaleRetiresSubsetThenExactRecoveryOpensControl);
            tests.Add(
                "Wpf.RecoveryRetirement.EvidenceChangeAfterConfirmationFailsClosed",
                EvidenceChangeAfterConfirmationFailsClosed);
            tests.Add(
                "Wpf.RecoveryRetirement.SessionChangeAfterConfirmationFailsClosed",
                SessionChangeAfterConfirmationFailsClosed);
            tests.Add(
                "Wpf.RecoveryRetirement.CapabilityChangeAfterConfirmationFailsClosed",
                CapabilityChangeAfterConfirmationFailsClosed);
            tests.Add(
                "Wpf.RecoveryRetirement.StartupPendingDecisionExactCasFinalizes",
                StartupPendingDecisionExactCasFinalizes);
            tests.Add(
                "Wpf.RecoveryRetirement.AxisQualificationVolatilePendingDecisionFinalizesBeforePromotion",
                AxisQualificationVolatilePendingDecisionFinalizesBeforePromotion);
        }

        private static void SetOperationModeStaleArchivesAfterIdleInterlock()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(CloseStep());
            var root = CreateRetirementTemporaryDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    var key = new LMCAxisSetOperationModeRecoveryKey(
                        1, 8, 1, checked(DiagnosticsBootId - 1),
                        DiagnosticMapRevision, 1, 2, 3, 4, 1,
                        LMCDriveOperationMode.ProfilePosition, 5000);
                    var journalPath = Path.Combine(root, "AxisSetOperationModeRecovery");
                    using (var journal = AxisSetOperationModeRecoveryJournal.Open(journalPath))
                    {
                        var armed = journal.ArmBeforeDispatch(
                            Guid.NewGuid(), "127.0.0.1", server.Port,
                            "_LMCAxis1", key, DateTime.UtcNow.AddMinutes(-1));
                        journal.PromoteToRecoveryRequired(armed, DateTime.UtcNow);
                    }
                    window = CreateWindow(root, server.Port);
                    ConnectIntoRetirementQuarantineWithDiagnostics(window, server, "SetOperationMode");
                    window.ExpanderSafetyAndRecoveryDetails.IsExpanded = true;
                    window.UpdateLayout();
                    var journalSource = window.AxisSetOperationModeRecoveryJournalForTests;
                    var originalBytes = File.ReadAllBytes(journalSource.JournalFilePath);
                    var requestsBeforeAcknowledgement = server.ReceivedRequests.Count;
                    AssertEx.Contains("RETIRE STALE | AxisSetOperationMode",
                        window.TextRecoveryIdentityRetirementSnapshot.Text);
                    AssertEx.True(window.CheckConfirmStaleRecoveryRetirement.IsEnabled);
                    AssertEx.False(window.ButtonArchiveAndRetireStaleRecovery.IsEnabled);
                    window.CheckConfirmStaleRecoveryRetirement.IsChecked = true;
                    // Drain the deferred ContextIdle traversal; immediate assertions miss this bug.
                    window.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                        new Action(() => { }));
                    AssertEx.True(window.ButtonArchiveAndRetireStaleRecovery.IsEnabled,
                        "Stale retirement must remain enabled after the deferred global interlock.");
                    AssertEx.True((bool)InvokePrivate(window,
                        "IsAllowedDuringAxisSetOperationModeRecovery",
                        window.ButtonArchiveAndRetireStaleRecovery));
                    AssertEx.False(window.AxisSetOperationModeStartButtonForTests.IsEnabled);
                    AssertEx.False(window.ButtonPowerOn.IsEnabled);
                    AssertEx.Equal(requestsBeforeAcknowledgement, server.ReceivedRequests.Count);
                    AssertControlCenterIsHitTestVisible(window,
                        window.ButtonArchiveAndRetireStaleRecovery, "Stale SetOperationMode retirement");

                    var exitCalled = false;
                    window.RecoveryRecordRetirementConfirmationOverride = (message, caption) =>
                    {
                        AssertEx.Contains("AxisSetOperationMode", message);
                        AssertEx.Contains("UNKNOWN", message);
                        return MessageBoxResult.Yes;
                    };
                    window.RecoveryRecordRetirementExitOverride = () => exitCalled = true;
                    Click(window.ButtonArchiveAndRetireStaleRecovery);
                    WaitUntil(() => exitCalled || window.TextOperationState.Text ==
                        "Archive and Retire Stale Recovery failed", "Retirement did not settle.");
                    AssertEx.True(exitCalled, window.TextExecutionLog.Text);
                    WaitForRetirementOperationToSettle(window);
                    AssertEx.False(journalSource.HasActiveRecord);
                    AssertEx.False(journalSource.CurrentRecord.HasTerminalOutcomeProof);
                    AssertEx.Equal(0u, journalSource.CurrentRecord.RetirementRequestId);
                    AssertEx.True(window.RecoveryRecordRetirementRestartRequired);
                    AssertEx.Equal("Disconnected", window.TextConnectionState.Text);
                    var ledger = (RecoveryRecordRetirementLedger)GetPrivateField(window,
                        "recoveryRecordRetirementLedger");
                    AssertEx.Equal(1, ledger.CommittedDecisions.Count);
                    AssertEx.SequenceEqual(originalBytes,
                        ledger.CommittedDecisions[0].SourceEvidence.GetOriginalBytes());
                    AssertEx.True(server.ReceivedRequests.All(request =>
                    {
                        var command = TestFrame.ReadUInt16(request, 0);
                        return command == 0x8080 || command == 0x405C || command == 0x405D
                            || command == 0x7E00 || command == 0x7E11 || command == 0x7E12;
                    }), "Archival may send only initialization, read-only diagnostics and close.");
                    using (var reopened = AxisSetOperationModeRecoveryJournal.Open(journalPath))
                    {
                        AssertEx.False(reopened.HasActiveRecord);
                        AssertEx.Equal(AxisSetOperationModeRecoveryState.OperatorRetired,
                            reopened.CurrentRecord.State);
                        AssertEx.False(reopened.CurrentRecord.HasTerminalOutcomeProof);
                    }
                    window.Close();
                    WaitUntil(() => !window.IsLoaded, "Retired window did not finish closing.");
                    window = CreateWindow(root, server.Port);
                    AssertEx.False(window.AxisSetOperationModeRecoveryInterlockForTests);
                    AssertEx.True(window.AxisSetOperationModeRecoveryJournalForTests.CurrentRecord != null);
                    window.Close();
                    WaitUntil(() => !window.IsLoaded, "Restarted window did not finish closing.");
                    window = null;
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteRetirementTemporaryDirectory(root);
            }
        }

        private static void SetOperationModeCommittedDecisionFinalizesAtStartup()
        {
            var root = CreateRetirementTemporaryDirectory();
            MainWindow window = null;
            try
            {
                using (var journal = AxisSetOperationModeRecoveryJournal.Open(
                    Path.Combine(root, "AxisSetOperationModeRecovery")))
                using (var ledger = RecoveryRecordRetirementLedger.Open(
                    Path.Combine(root, "RecoveryRecordRetirementLedger")))
                {
                    var key = CreateSetOperationModeRecoveryKey();
                    var armed = journal.ArmBeforeDispatch(Guid.NewGuid(),
                        "127.0.0.1", 4000, "_LMCAxis1", key, DateTime.UtcNow.AddMinutes(-1));
                    journal.PromoteToRecoveryRequired(armed, DateTime.UtcNow);
                    var evidence = journal.CaptureActiveRetirementEvidence();
                    ledger.CommitOperatorRetirement(evidence, "127.0.0.1", 4000,
                        key.DiagnosticsBuild, key.DiagnosticsBootId + 1, key.MapRevision,
                        RetirementTestOperator, RetirementTestReason, DateTime.UtcNow);
                    AssertEx.True(journal.HasActiveRecord);
                    // Simulate exit after durable decision but before source retirement.
                }
                window = CreateWindow(root, 4000);
                AssertEx.False(window.AxisSetOperationModeRecoveryInterlockForTests);
                AssertEx.Equal(AxisSetOperationModeRecoveryState.OperatorRetired,
                    window.AxisSetOperationModeRecoveryJournalForTests.CurrentRecord.State);
                AssertEx.Contains("crash-finalization applied exact-byte CAS", window.TextExecutionLog.Text);
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteRetirementTemporaryDirectory(root);
            }
        }

        private static void
            DiagnosticsMutationMismatchAllowsCloseExitAndPreservesBytes()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology
                | LMCDiagnosticCapability.EncoderTw20ErrorWarningReset
                | LMCDiagnosticCapability.EncoderTw19MultiturnPositionReset;
            var closeButtonSteps = CreateConnectAndTopologySteps(capabilities);
            closeButtonSteps.Add(CapabilitiesStep(11, capabilities));
            closeButtonSteps.Add(ExactCloseStep());
            var xExitSteps = CreateConnectAndTopologySteps(capabilities);
            xExitSteps.Add(ExactCloseStep());
            var root = CreateRetirementTemporaryDirectory();
            MainWindow window = null;
            try
            {
                Guid identity;
                byte[] sourceBytes;
                CreateLegacyDiagnosticsOutcomeUnverifiedRecord(
                    root,
                    checked(DiagnosticsBootId - 1),
                    out identity,
                    out sourceBytes);

                using (var server = new FakeRpcServer(
                    closeButtonSteps.ToArray()))
                {
                    window = CreateWindow(root, server.Port);
                    ConnectIntoRetirementQuarantine(window);

                    AssertEx.True(window.ButtonCloseConnection.IsEnabled);
                    AssertControlCenterIsHitTestVisible(
                        window,
                        window.ButtonCloseConnection,
                        "Close Connection");
                    AssertEx.False(window.ButtonPowerOn.IsEnabled);
                    AssertEx.False(window.ButtonGroupPowerOn.IsEnabled);
                    AssertEx.False(
                        window.ButtonAcknowledgePersistedMutation.IsEnabled);
                    var liveAdmission =
                        (DiagnosticsAdmissionDecision)InvokePrivate(
                            window,
                            "EvaluateDiagnosticsAdmission",
                            DiagnosticsAdmissionOperation.NewLiveOrMutation,
                            true);
                    AssertEx.False(liveAdmission.IsAllowed);
                    AssertEx.Equal(
                        DiagnosticsAdmissionDenialReason
                            .RecoveryIdentityReadOnly,
                        liveAdmission.DenialReason);
                    AssertEx.True(((DiagnosticsAdmissionDecision)
                        InvokePrivate(
                            window,
                            "EvaluateDiagnosticsAdmission",
                            DiagnosticsAdmissionOperation.CloseConnection,
                            true)).IsAllowed);
                    AssertEx.True(((DiagnosticsAdmissionDecision)
                        InvokePrivate(
                            window,
                            "EvaluateDiagnosticsAdmission",
                            DiagnosticsAdmissionOperation.CloseWindow,
                            true)).IsAllowed);
                    AssertEx.Contains(
                        "RECOVERY IDENTITY READ-ONLY QUARANTINE",
                        window.TextExecutionLog.Text);
                    AssertEx.False(
                        window.ExpanderSafetyAndRecoveryDetails.IsExpanded);
                    AssertEx.Equal(
                        Visibility.Visible,
                        window.PanelRecoveryIdentityRetirement.Visibility);
                    AssertEx.False(
                        window.PanelRecoveryIdentityRetirement.IsVisible);
                    AssertEx.Equal(
                        "Open Safety / Recovery Details",
                        (string)window.ButtonOpenEncoderRecoveryDetails
                            .Content);
                    var requestsBeforeOpeningRecoveryDetails =
                        server.ReceivedRequests.Count;
                    Click(window.ButtonOpenEncoderRecoveryDetails);
                    PumpDispatcherOnce();
                    AssertEx.True(
                        window.ExpanderSafetyAndRecoveryDetails.IsExpanded);
                    AssertEx.True(
                        window.PanelRecoveryIdentityRetirement.IsVisible);
                    AssertEx.Equal(
                        requestsBeforeOpeningRecoveryDetails,
                        server.ReceivedRequests.Count);

                    AssertEx.Equal(
                        LMCEncoderMaintenanceKind
                            .Tw19MultiturnPositionReset,
                        (LMCEncoderMaintenanceKind)window
                            .ComboEncoderMaintenanceKind.SelectedItem);
                    window.ComboTestResetAxis.SelectedItem = (ushort)1;
                    window.CheckTestResetPowerOffVerified.IsChecked = true;
                    window.CheckTestResetPhysicalPositionVerified.IsChecked =
                        true;
                    window.CheckTestResetExactTargetVerified.IsChecked = true;
                    window.CheckEncoderMaintenanceCompatibilityVerified
                        .IsChecked = true;
                    PumpDispatcherOnce();
                    AssertEx.True(
                        window.EncoderMaintenanceStepOneConfirmedForTests);
                    AssertEx.False(window.ButtonArmTestReset.IsEnabled);
                    AssertEx.False(window.ButtonExecuteTestReset.IsEnabled);
                    AssertEx.Contains(
                        "recovery-identity read-only quarantine",
                        window.TextEncoderMaintenanceArmGateStatus.Text);

                    var requestsBeforeForcedArm =
                        server.ReceivedRequests.Count;
                    Click(window.ButtonArmTestReset);
                    WaitUntil(
                        () => !(bool)GetPrivateField(
                                window,
                                "operationRunning")
                            && string.Equals(
                                window.TextOperationState.Text,
                                "Arm TEST ONLY Encoder Maintenance failed",
                                StringComparison.Ordinal),
                        "Forced Encoder Arm admission check did not settle.");
                    AssertEx.Equal(
                        requestsBeforeForcedArm,
                        server.ReceivedRequests.Count);
                    AssertEx.Contains(
                        "recovery-identity read-only quarantine",
                        window.TextExecutionLog.Text);
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E53));

                    var currentConnection = (LMCConnection)GetPrivateField(
                        window,
                        "connection");
                    var currentCapabilities = currentConnection.Diagnostics
                        .GetCapabilities();
                    SetPrivateField(
                        window,
                        "diagnosticCapabilities",
                        currentCapabilities);
                    var prepared = currentConnection.Diagnostics
                        .PrepareTw19MultiturnPositionReset(
                            (LMCTw19MultiturnPositionResetRequest)
                                window.ReadEncoderMaintenanceRequest(),
                            currentCapabilities,
                            LMCTw19MultiturnPositionResetExecuteToken
                                .Create());
                    SetPrivateField(
                        window,
                        "armedEncoderMaintenance",
                        prepared);
                    SetPrivateField(
                        window,
                        "armedEncoderMaintenanceFingerprint",
                        MainWindow.FormatEncoderMaintenanceIdentity(
                            prepared.RecoveryKey));
                    window.CheckTestResetFinalConfirmed.IsChecked = true;
                    InvokePrivate(window, "UpdateUiState");
                    PumpDispatcherOnce();
                    AssertEx.False(window.ButtonExecuteTestReset.IsEnabled);

                    var requestsBeforeForcedExecute =
                        server.ReceivedRequests.Count;
                    Click(window.ButtonExecuteTestReset);
                    WaitUntil(
                        () => !(bool)GetPrivateField(
                                window,
                                "operationRunning")
                            && string.Equals(
                                window.TextOperationState.Text,
                                "Execute TEST ONLY Encoder Maintenance failed",
                                StringComparison.Ordinal),
                        "Forced Encoder Execute admission check did not settle.");
                    AssertEx.Equal(
                        requestsBeforeForcedExecute,
                        server.ReceivedRequests.Count);
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E53));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E54));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E55));
                    AssertEx.SequenceEqual(
                        sourceBytes,
                        File.ReadAllBytes(Path.Combine(
                            root,
                            DiagnosticsMutationJournal.JournalFileName)));

                    Click(window.ButtonCloseConnection);
                    WaitUntil(
                        () => string.Equals(
                                window.TextConnectionState.Text,
                                "Disconnected",
                                StringComparison.Ordinal)
                            && string.Equals(
                                window.TextOperationState.Text,
                                "Close Connection completed",
                                StringComparison.Ordinal),
                        "Diagnostics mutation mismatch quarantine did not allow the Close Connection button to disconnect.");
                    AssertEx.Contains(
                        "DiagnosticsMutation=active-endpoint-unbound/reconnect-required",
                        window.TextRecoveryIdentityRetirementSnapshot.Text);
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x405D));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(server.ReceivedRequests, 0x2023));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(server.ReceivedRequests, 0x204A));
                    AssertNoMotionMutationRequests(
                        server.ReceivedRequests,
                        "Diagnostics mismatch Close Connection sent a motion or mutation RPC.");

                    window.Close();
                    WaitUntil(
                        () => !window.IsLoaded,
                        "Diagnostics mutation mismatch quarantine did not allow X exit after Close Connection.");
                    window = null;
                    server.Verify();
                }

                AssertEx.SequenceEqual(
                    sourceBytes,
                    File.ReadAllBytes(Path.Combine(
                        root,
                        DiagnosticsMutationJournal.JournalFileName)));

                using (var server = new FakeRpcServer(xExitSteps.ToArray()))
                {
                    window = CreateWindow(root, server.Port);
                    ConnectIntoRetirementQuarantine(window);
                    AssertEx.True(window.ButtonCloseConnection.IsEnabled);

                    window.Close();
                    WaitUntil(
                        () => !window.IsLoaded,
                        "Diagnostics mutation mismatch quarantine did not allow connected window X exit.");
                    window = null;
                    server.Verify();
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x405D));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(server.ReceivedRequests, 0x2023));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(server.ReceivedRequests, 0x204A));
                    AssertNoMotionMutationRequests(
                        server.ReceivedRequests,
                        "Diagnostics mismatch connected X exit sent a motion or mutation RPC.");
                }

                AssertEx.SequenceEqual(
                    sourceBytes,
                    File.ReadAllBytes(Path.Combine(
                        root,
                        DiagnosticsMutationJournal.JournalFileName)));
                using (var reopened = DiagnosticsMutationJournal.Open(root))
                {
                    AssertEx.True(reopened.HasActiveRecord);
                    AssertEx.Equal(identity, reopened.CurrentRecord.Identity);
                    AssertEx.Equal(
                        DiagnosticsMutationState.OutcomeUnverified,
                        reopened.CurrentRecord.State);
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteRetirementTemporaryDirectory(root);
            }
        }

        private static void
            DiagnosticsMutationMismatchKeepsExactGroupResetAndAllowsExit()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(MotionRecoveryCapabilitiesStep(
                11,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            var firstClose = ExactCloseStep();
            firstClose.CloseClientAfterResponseAndContinue = true;
            steps.Add(firstClose);
            steps.AddRange(CreateConnectAndTopologySteps(capabilities));
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
            steps.Add(MotionRecoveryCapabilitiesStep(
                13,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(ExactCloseStep());

            var root = CreateRetirementTemporaryDirectory();
            var callbackPort = ReserveGroupResetCallbackPort();
            MainWindow window = null;
            try
            {
                Guid diagnosticsIdentity;
                byte[] diagnosticsSourceBytes;
                CreateLegacyDiagnosticsOutcomeUnverifiedRecord(
                    root,
                    checked(DiagnosticsBootId - 1),
                    out diagnosticsIdentity,
                    out diagnosticsSourceBytes);
                Guid groupResetIdentity;
                byte[] groupResetSourceBytes;

                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    ArmRestartGroupResetRecord(
                        root,
                        server.Port,
                        callbackPort,
                        true);

                    using (var groupReset = GroupResetRecoveryJournal.Open(
                        Path.Combine(root, "GroupResetRecovery")))
                    {
                        AssertEx.True(groupReset.HasActiveRecord);
                        AssertEx.Equal(
                            GroupResetRecoveryState.RecoveryRequired,
                            groupReset.CurrentRecord.State);
                        AssertEx.Equal(
                            DiagnosticsBootId,
                            groupReset.CurrentRecord.DiagnosticsBootId);
                        AssertEx.Equal(
                            DiagnosticMapRevision,
                            groupReset.CurrentRecord.MapRevision);
                        groupResetIdentity = groupReset.CurrentRecord.Identity;
                        groupResetSourceBytes = File.ReadAllBytes(
                            groupReset.JournalFilePath);
                    }

                    window = CreateWindow(root, server.Port);
                    window.TextCallbackPort.Text = callbackPort.ToString(
                        System.Globalization.CultureInfo.InvariantCulture);
                    ConnectIntoRetirementQuarantineWithDiagnostics(
                        window,
                        server,
                        "first mixed session");

                    var snapshot =
                        window.TextRecoveryIdentityRetirementSnapshot.Text;
                    AssertEx.Contains(
                        "RETIRE STALE | DiagnosticsMutation",
                        snapshot);
                    AssertEx.Contains(
                        "KEEP EXACT CURRENT | GroupReset",
                        snapshot);
                    AssertEx.True(
                        ((DiagnosticsMutationJournal)GetPrivateField(
                            window,
                            "diagnosticsMutationJournal"))
                        .CurrentRecord.HasTypedSdoWriteMetadata);
                    AssertEx.True(window.ButtonCloseConnection.IsEnabled);
                    AssertControlCenterIsHitTestVisible(
                        window,
                        window.ButtonCloseConnection,
                        "Close Connection");
                    AssertEx.False(window.ButtonPowerOn.IsEnabled);
                    AssertEx.False(window.ButtonGroupPowerOn.IsEnabled);

                    var liveAdmission =
                        (DiagnosticsAdmissionDecision)InvokePrivate(
                            window,
                            "EvaluateDiagnosticsAdmission",
                            DiagnosticsAdmissionOperation.NewLiveOrMutation,
                            true);
                    AssertEx.False(liveAdmission.IsAllowed);
                    AssertEx.Equal(
                        DiagnosticsAdmissionDenialReason.UnresolvedMutation,
                        liveAdmission.DenialReason);
                    AssertEx.True(((DiagnosticsAdmissionDecision)
                        InvokePrivate(
                            window,
                            "EvaluateDiagnosticsAdmission",
                            DiagnosticsAdmissionOperation.CloseConnection,
                            true)).IsAllowed);
                    AssertEx.True(((DiagnosticsAdmissionDecision)
                        InvokePrivate(
                            window,
                            "EvaluateDiagnosticsAdmission",
                            DiagnosticsAdmissionOperation.CloseWindow,
                            true)).IsAllowed);

                    Click(window.ButtonCloseConnection);
                    WaitUntil(
                        () => string.Equals(
                                window.TextConnectionState.Text,
                                "Disconnected",
                                StringComparison.Ordinal)
                            && string.Equals(
                                window.TextOperationState.Text,
                                "Close Connection completed",
                                StringComparison.Ordinal),
                        "Mixed diagnostics/Group Reset quarantine did not allow Close Connection.");
                    AssertEx.Contains(
                        "DiagnosticsMutation=active-endpoint-unbound/reconnect-required",
                        window.TextRecoveryIdentityRetirementSnapshot.Text);
                    AssertEx.SequenceEqual(
                        diagnosticsSourceBytes,
                        File.ReadAllBytes(Path.Combine(
                            root,
                            DiagnosticsMutationJournal.JournalFileName)));
                    AssertEx.SequenceEqual(
                        groupResetSourceBytes,
                        File.ReadAllBytes(Path.Combine(
                            root,
                            "GroupResetRecovery",
                            GroupResetRecoveryJournal.JournalFileName)));

                    window.Close();
                    WaitUntil(
                        () => !window.IsLoaded,
                        "Mixed diagnostics/Group Reset window did not allow X exit after disconnect.");
                    window = null;

                    window = CreateWindow(root, server.Port);
                    window.TextCallbackPort.Text = callbackPort.ToString(
                        System.Globalization.CultureInfo.InvariantCulture);
                    ConnectIntoRetirementQuarantineWithDiagnostics(
                        window,
                        server,
                        "retirement mixed session");
                    snapshot =
                        window.TextRecoveryIdentityRetirementSnapshot.Text;
                    AssertEx.Contains(
                        "RETIRE STALE | DiagnosticsMutation",
                        snapshot);
                    AssertEx.Contains(
                        "KEEP EXACT CURRENT | GroupReset",
                        snapshot);

                    var confirmationCalled = false;
                    var exitCalled = false;
                    window.RecoveryRecordRetirementConfirmationOverride =
                        (message, caption) =>
                        {
                            confirmationCalled = true;
                            AssertEx.Contains(
                                "LEGACY DIAGNOSTICS WARNING",
                                message);
                            AssertEx.Contains(
                                "RETIRE STALE - records to archive and resolve",
                                message);
                            AssertEx.Contains(
                                "KEEP EXACT CURRENT - records left active",
                                message);
                            AssertEx.Contains("DiagnosticsMutation", message);
                            AssertEx.Contains("GroupReset", message);
                            return MessageBoxResult.Yes;
                        };
                    window.RecoveryRecordRetirementExitOverride = () =>
                    {
                        exitCalled = true;
                    };
                    window.CheckConfirmStaleRecoveryRetirement.IsChecked =
                        true;
                    PumpDispatcherOnce();
                    AssertEx.True(
                        window.ButtonArchiveAndRetireStaleRecovery.IsEnabled);
                    Click(window.ButtonArchiveAndRetireStaleRecovery);
                    WaitUntil(
                        () => exitCalled,
                        "Mixed diagnostics/Group Reset retirement did not request exit.");
                    WaitForRetirementOperationToSettle(window);

                    AssertEx.True(confirmationCalled);
                    AssertEx.True(
                        window.RecoveryRecordRetirementRestartRequired);
                    AssertEx.Equal(
                        "Disconnected",
                        window.TextConnectionState.Text);
                    var diagnostics =
                        (DiagnosticsMutationJournal)GetPrivateField(
                            window,
                            "diagnosticsMutationJournal");
                    AssertEx.False(diagnostics.HasActiveRecord);
                    AssertEx.Equal(
                        DiagnosticsMutationState.Resolved,
                        diagnostics.CurrentRecord.State);
                    AssertEx.Equal(
                        diagnosticsIdentity,
                        diagnostics.CurrentRecord.Identity);
                    var retainedGroupReset =
                        (GroupResetRecoveryJournal)GetPrivateField(
                            window,
                            "groupResetRecoveryJournal");
                    AssertEx.True(retainedGroupReset.HasActiveRecord);
                    AssertEx.Equal(
                        GroupResetRecoveryState.RecoveryRequired,
                        retainedGroupReset.CurrentRecord.State);
                    AssertEx.Equal(
                        groupResetIdentity,
                        retainedGroupReset.CurrentRecord.Identity);
                    AssertEx.SequenceEqual(
                        groupResetSourceBytes,
                        File.ReadAllBytes(retainedGroupReset.JournalFilePath));

                    var ledger =
                        (RecoveryRecordRetirementLedger)GetPrivateField(
                            window,
                            "recoveryRecordRetirementLedger");
                    AssertEx.Equal(1, ledger.CommittedDecisions.Count);
                    AssertEx.Equal(
                        RecoveryRecordOwner.DiagnosticsMutation,
                        ledger.CommittedDecisions.Single()
                            .SourceEvidence.Owner);

                    window.Close();
                    WaitUntil(
                        () => !window.IsLoaded,
                        "Mixed diagnostics/Group Reset retirement-success window did not exit.");
                    window = null;
                    server.Verify();
                    AssertEx.Equal(
                        2,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x405D));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2049));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2023));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x204A));
                    AssertNoMotionMutationRequests(
                        server.ReceivedRequests,
                        "Mixed diagnostics/Group Reset exit and retirement sent a mutation RPC.");
                }

                using (var diagnostics = DiagnosticsMutationJournal.Open(root))
                using (var groupReset = GroupResetRecoveryJournal.Open(
                    Path.Combine(root, "GroupResetRecovery")))
                {
                    AssertEx.False(diagnostics.HasActiveRecord);
                    AssertEx.Equal(
                        DiagnosticsMutationState.Resolved,
                        diagnostics.CurrentRecord.State);
                    AssertEx.Equal(
                        diagnosticsIdentity,
                        diagnostics.CurrentRecord.Identity);
                    AssertEx.True(groupReset.HasActiveRecord);
                    AssertEx.Equal(
                        GroupResetRecoveryState.RecoveryRequired,
                        groupReset.CurrentRecord.State);
                    AssertEx.Equal(
                        groupResetIdentity,
                        groupReset.CurrentRecord.Identity);
                    AssertEx.SequenceEqual(
                        groupResetSourceBytes,
                        File.ReadAllBytes(groupReset.JournalFilePath));
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteRetirementTemporaryDirectory(root);
            }
        }

        private static void
            DiagnosticsMutationMismatchArchivesResolvesAndReopensLiveAdmission()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology
                | LMCDiagnosticCapability.EncoderTw20ErrorWarningReset
                | LMCDiagnosticCapability.EncoderTw19MultiturnPositionReset;
            var steps = CreateConnectAndTopologySteps(capabilities);
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
            steps.Add(ExactCloseStep());
            var restartSteps = CreateConnectAndTopologySteps(capabilities);
            restartSteps.Add(D5AxisLookupStep(1));
            restartSteps.Add(D5AxisInfoStep(1));
            restartSteps.Add(ExactGroupLookupStep("_LMCRobotBase1"));
            restartSteps.Add(CapabilitiesStep(11, capabilities));
            restartSteps.Add(ExactAxisPowerOnStep());
            restartSteps.Add(AxisPowerStatusStep(true));
            restartSteps.Add(AxisPowerStatusStep(true));
            restartSteps.Add(AxisPowerStatusStep(true));
            restartSteps.Add(CapabilitiesStep(12, capabilities));
            restartSteps.Add(CapabilitiesStep(13, capabilities));
            restartSteps.Add(ExactAxisPowerOffStep());
            restartSteps.Add(AxisPowerStatusStep(false));
            restartSteps.Add(AxisPowerStatusStep(false));
            restartSteps.Add(AxisPowerStatusStep(false));
            restartSteps.Add(CapabilitiesStep(14, capabilities));
            restartSteps.Add(GroupPowerOnStep());
            restartSteps.Add(GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn));
            restartSteps.Add(GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn));
            restartSteps.Add(GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn));
            restartSteps.Add(GroupPowerOffStep());
            restartSteps.Add(GroupEnableWaitStatusStep(0));
            restartSteps.Add(GroupEnableWaitStatusStep(0));
            restartSteps.Add(GroupEnableWaitStatusStep(0));
            restartSteps.Add(ExactCloseStep());
            var root = CreateRetirementTemporaryDirectory();
            MainWindow window = null;
            MainWindow restartedWindow = null;
            try
            {
                Guid identity;
                byte[] sourceBytes;
                CreateLegacyDiagnosticsOutcomeUnverifiedRecord(
                    root,
                    checked(DiagnosticsBootId - 1),
                    out identity,
                    out sourceBytes);

                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(root, server.Port);
                    ConnectIntoRetirementQuarantine(window);
                    AssertEx.True(window.ButtonCloseConnection.IsEnabled);
                    AssertEx.Contains(
                        "RETIRE STALE | DiagnosticsMutation",
                        window.TextRecoveryIdentityRetirementSnapshot.Text);
                    AssertEx.Contains(
                        "LEGACY_ENDPOINT_OPERATOR_CLASSIFIED",
                        window.TextRecoveryIdentityRetirementSnapshot.Text);
                    AssertEx.True(
                        window.CheckConfirmStaleRecoveryRetirement.IsEnabled);

                    var exitCalled = false;
                    window.RecoveryRecordRetirementConfirmationOverride =
                        (message, caption) =>
                        {
                            AssertEx.Contains(
                                "LEGACY DIAGNOSTICS WARNING",
                                message);
                            AssertEx.Contains(
                                "contains no PLC endpoint",
                                message);
                            AssertEx.Contains(
                                "outcome remains UNKNOWN",
                                message);
                            return MessageBoxResult.Yes;
                        };
                    window.RecoveryRecordRetirementExitOverride = () =>
                    {
                        exitCalled = true;
                    };
                    window.CheckConfirmStaleRecoveryRetirement.IsChecked =
                        true;
                    PumpDispatcherOnce();
                    AssertEx.True(
                        window.ButtonArchiveAndRetireStaleRecovery.IsEnabled);
                    Click(window.ButtonArchiveAndRetireStaleRecovery);
                    WaitUntil(
                        () => exitCalled,
                        "Diagnostics mutation retirement did not request restart.");
                    WaitForRetirementOperationToSettle(window);

                    var journal =
                        (DiagnosticsMutationJournal)GetPrivateField(
                            window,
                            "diagnosticsMutationJournal");
                    AssertEx.False(journal.HasActiveRecord);
                    AssertEx.Equal(
                        DiagnosticsMutationState.Resolved,
                        journal.CurrentRecord.State);
                    AssertEx.Equal(identity, journal.CurrentRecord.Identity);
                    var ledger =
                        (RecoveryRecordRetirementLedger)GetPrivateField(
                            window,
                            "recoveryRecordRetirementLedger");
                    AssertEx.Equal(1, ledger.CommittedDecisions.Count);
                    var decision = ledger.CommittedDecisions.Single();
                    AssertEx.Equal(
                        RecoveryRecordOwner.DiagnosticsMutation,
                        decision.SourceEvidence.Owner);
                    AssertEx.Equal(
                        RecoveryEndpointEvidenceKind
                            .OperatorClassifiedLegacyEndpoint,
                        decision.SourceEvidence.EndpointEvidenceKind);
                    AssertEx.SequenceEqual(
                        sourceBytes,
                        decision.SourceEvidence.GetOriginalBytes());
                    AssertEx.Equal(
                        "Disconnected",
                        window.TextConnectionState.Text);
                    AssertEx.True(
                        window.RecoveryRecordRetirementRestartRequired);
                    AssertNoMotionMutationRequests(
                        server.ReceivedRequests,
                        "Diagnostics mutation retirement sent a motion or mutation RPC.");

                    window.Close();
                    WaitUntil(
                        () => !window.IsLoaded,
                        "Diagnostics retirement-success window did not close.");
                    window = null;
                    server.Verify();
                }

                using (var reopened = DiagnosticsMutationJournal.Open(root))
                {
                    AssertEx.False(reopened.HasActiveRecord);
                    AssertEx.Equal(
                        DiagnosticsMutationState.Resolved,
                        reopened.CurrentRecord.State);
                    AssertEx.Equal(identity, reopened.CurrentRecord.Identity);
                }

                using (var restartServer = new FakeRpcServer(
                    restartSteps.ToArray()))
                {
                    restartedWindow = CreateWindow(
                        root,
                        restartServer.Port);
                    var liveAdmissionAfterRestart =
                        (DiagnosticsAdmissionDecision)InvokePrivate(
                            restartedWindow,
                            "EvaluateDiagnosticsAdmission",
                            DiagnosticsAdmissionOperation.NewLiveOrMutation,
                            true);
                    AssertEx.True(liveAdmissionAfterRestart.IsAllowed);
                    AssertEx.False(
                        ((DiagnosticsMutationJournal)GetPrivateField(
                            restartedWindow,
                            "diagnosticsMutationJournal")).HasActiveRecord);

                    Click(restartedWindow.ButtonConnect);
                    WaitUntil(
                        () => string.Equals(
                                restartedWindow.TextConnectionState.Text,
                                "Connected",
                                StringComparison.Ordinal)
                            && restartedWindow.ButtonLookupAxis.IsEnabled
                            && restartedWindow.ButtonLookupGroup.IsEnabled,
                        "Diagnostics retirement restart window did not reconnect for Axis and Group lookup.");

                    restartedWindow.TextAxisName.Text = "_LMCAxis1";
                    restartedWindow.TextGroupName.Text = "_LMCRobotBase1";
                    PumpDispatcherOnce();
                    Click(restartedWindow.ButtonLookupAxis);
                    WaitUntil(
                        () => string.Equals(
                                restartedWindow.TextAxisReference.Text,
                                "1",
                                StringComparison.Ordinal)
                            && restartedWindow.ButtonPowerOn.IsEnabled,
                        "Diagnostics retirement restart did not restore Axis Power On readiness for _LMCAxis1.");
                    Click(restartedWindow.ButtonLookupGroup);
                    WaitUntil(
                        () => string.Equals(
                                restartedWindow.TextGroupReference.Text,
                                GroupEnableWaitReference.ToString(),
                                StringComparison.Ordinal)
                            && restartedWindow.ButtonPowerOn.IsEnabled
                            && restartedWindow.ButtonGroupPowerOn.IsEnabled,
                        "Diagnostics retirement restart did not restore Group Power On readiness for _LMCRobotBase1.");

                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            restartServer.ReceivedRequests,
                            0x2023));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            restartServer.ReceivedRequests,
                            0x204A));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            restartServer.ReceivedRequests,
                            0x204B));
                    AssertNoMotionMutationRequests(
                        restartServer.ReceivedRequests,
                        "Diagnostics retirement restart lookup sent a Servo On or mutation RPC.");

                    var diagnosticsJournal =
                        (DiagnosticsMutationJournal)GetPrivateField(
                            restartedWindow,
                            "diagnosticsMutationJournal");
                    var axisPowerJournal =
                        (AxisPowerOnRecoveryJournal)GetPrivateField(
                            restartedWindow,
                            "axisPowerOnRecoveryJournal");
                    var groupPowerJournal =
                        (GroupPowerRecoveryJournal)GetPrivateField(
                            restartedWindow,
                            "groupPowerRecoveryJournal");
                    AssertEx.False(diagnosticsJournal.HasActiveRecord);
                    AssertEx.Equal(
                        DiagnosticsMutationState.Resolved,
                        diagnosticsJournal.CurrentRecord.State);

                    Click(restartedWindow.ButtonPowerOn);
                    WaitUntil(
                        () => axisPowerJournal.CurrentRecord != null
                            && !axisPowerJournal.HasActiveRecord
                            && axisPowerJournal.CurrentRecord.State
                                == AxisPowerOnRecoveryState.Resolved
                            && restartedWindow.TextAxisResult.Text.IndexOf(
                                "Stable=3/3",
                                StringComparison.Ordinal) >= 0
                            && restartedWindow.ButtonPowerOff.IsEnabled,
                        "Diagnostics retirement restart did not finish stable Axis Power On verification.");
                    AssertEx.True(
                        axisPowerJournal.CurrentRecord.ExpectedPowerOn);
                    AssertEx.True(restartedWindow.ButtonGroupPowerOn.IsEnabled);
                    AssertEx.Equal(
                        1,
                        CountAxisPowerCommand(restartServer, true));
                    AssertEx.Equal(
                        0,
                        CountAxisPowerCommand(restartServer, false));
                    AssertEx.Equal(
                        3,
                        CountRequestCommand(
                            restartServer.ReceivedRequests,
                            0x2028));

                    Click(restartedWindow.ButtonPowerOff);
                    try
                    {
                        WaitUntil(
                            () => axisPowerJournal.CurrentRecord != null
                                && !axisPowerJournal.HasActiveRecord
                                && axisPowerJournal.CurrentRecord.State
                                    == AxisPowerOnRecoveryState.Resolved
                                && !axisPowerJournal.CurrentRecord
                                    .ExpectedPowerOn
                                && string.Equals(
                                    restartedWindow.TextOperationState.Text,
                                    "Power Off verified",
                                    StringComparison.Ordinal)
                                && restartedWindow.TextAxisResult.Text.IndexOf(
                                    "Stable PowerOff+Standstill=3/3",
                                    StringComparison.Ordinal) >= 0
                                && restartedWindow.ButtonGroupPowerOn.IsEnabled,
                            "Diagnostics retirement restart did not finish stable Axis Power Off verification.");
                    }
                    catch (TimeoutException error)
                    {
                        throw new TimeoutException(
                            error.Message
                                + " State="
                                + restartedWindow.TextOperationState.Text
                                + ", Result="
                                + restartedWindow.TextAxisResult.Text
                                + ", Journal="
                                + (axisPowerJournal.CurrentRecord == null
                                    ? "none"
                                    : axisPowerJournal.CurrentRecord.State
                                        + "/ExpectedPowerOn="
                                        + axisPowerJournal.CurrentRecord
                                            .ExpectedPowerOn)
                                + ", Requests="
                                + string.Join(
                                    ",",
                                    restartServer.ReceivedRequests.Select(
                                        request => "0x"
                                            + TestFrame.ReadUInt16(request, 0)
                                                .ToString("X4")))
                                + ", Log="
                                + restartedWindow.TextExecutionLog.Text,
                            error);
                    }
                    AssertEx.Equal(
                        1,
                        CountAxisPowerCommand(restartServer, true));
                    AssertEx.Equal(
                        1,
                        CountAxisPowerCommand(restartServer, false));
                    AssertEx.Equal(
                        6,
                        CountRequestCommand(
                            restartServer.ReceivedRequests,
                            0x2028));

                    Click(restartedWindow.ButtonGroupPowerOn);
                    WaitUntil(
                        () => groupPowerJournal.CurrentRecord != null
                            && !groupPowerJournal.HasActiveRecord
                            && groupPowerJournal.CurrentRecord.State
                                == GroupPowerRecoveryState.Resolved
                            && (bool)GetPrivateField(
                                restartedWindow,
                                "groupActiveVerified")
                            && restartedWindow.TextGroupResult.Text.IndexOf(
                                "Stable=3/3",
                                StringComparison.Ordinal) >= 0
                            && restartedWindow.ButtonGroupPowerOff.IsEnabled,
                        "Diagnostics retirement restart did not finish stable Group Power On verification.");
                    AssertEx.True(
                        groupPowerJournal.CurrentRecord.ExpectedPowerOn);
                    AssertEx.Equal(
                        2,
                        CountRequestCommand(
                            restartServer.ReceivedRequests,
                            0x2023));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            restartServer.ReceivedRequests,
                            0x204A));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            restartServer.ReceivedRequests,
                            0x204B));
                    AssertEx.Equal(
                        3,
                        CountRequestCommand(
                            restartServer.ReceivedRequests,
                            0x2045));
                    AssertEx.False(diagnosticsJournal.HasActiveRecord);
                    AssertEx.Equal(
                        DiagnosticsMutationState.Resolved,
                        diagnosticsJournal.CurrentRecord.State);

                    Click(restartedWindow.ButtonGroupPowerOff);
                    WaitUntil(
                        () => groupPowerJournal.CurrentRecord != null
                            && !groupPowerJournal.HasActiveRecord
                            && groupPowerJournal.CurrentRecord.State
                                == GroupPowerRecoveryState.Resolved
                            && !groupPowerJournal.CurrentRecord.ExpectedPowerOn
                            && !(bool)GetPrivateField(
                                restartedWindow,
                                "groupActiveVerified")
                            && string.Equals(
                                restartedWindow.TextOperationState.Text,
                                "Group Power Off verified",
                                StringComparison.Ordinal)
                            && restartedWindow.TextGroupResult.Text.IndexOf(
                                "Group Power Off verified",
                                StringComparison.Ordinal) >= 0
                            && restartedWindow.TextGroupResult.Text.IndexOf(
                                "Stable=3/3",
                                StringComparison.Ordinal) >= 0,
                        "Diagnostics retirement restart did not finish stable Group Power Off verification.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            restartServer.ReceivedRequests,
                            0x204A));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            restartServer.ReceivedRequests,
                            0x204B));
                    AssertEx.Equal(
                        6,
                        CountRequestCommand(
                            restartServer.ReceivedRequests,
                            0x2045));

                    var encoderKinds = new[]
                    {
                        LMCEncoderMaintenanceKind.Tw20ErrorWarningReset,
                        LMCEncoderMaintenanceKind.Tw19MultiturnPositionReset
                    };
                    var encoderCapabilities = new[]
                    {
                        LMCDiagnosticCapability
                            .EncoderTw20ErrorWarningReset,
                        LMCDiagnosticCapability
                            .EncoderTw19MultiturnPositionReset
                    };
                    for (var index = 0; index < encoderKinds.Length; index++)
                    {
                        restartedWindow.ComboEncoderMaintenanceKind
                            .SelectedItem = encoderKinds[index];
                        PumpDispatcherOnce();
                        AssertEx.False(
                            restartedWindow
                                .EncoderMaintenanceStepOneConfirmedForTests);
                        AssertEx.Contains(
                            "all Step 1 physical and encoder compatibility checks are required",
                            restartedWindow
                                .TextEncoderMaintenanceArmGateStatus.Text);
                        restartedWindow.CheckTestResetPowerOffVerified
                            .IsChecked = true;
                        restartedWindow.CheckTestResetPhysicalPositionVerified
                            .IsChecked = true;
                        restartedWindow.CheckTestResetExactTargetVerified
                            .IsChecked = true;
                        restartedWindow
                            .CheckEncoderMaintenanceCompatibilityVerified
                            .IsChecked = true;
                        PumpDispatcherOnce();

                        AssertEx.True(
                            restartedWindow.ButtonArmTestReset.IsEnabled);
                        AssertEx.False(
                            restartedWindow.ButtonExecuteTestReset.IsEnabled);
                        AssertEx.Contains(
                            "READY: current-session capability, recovery, and all Step 1 gates are open",
                            restartedWindow
                                .TextEncoderMaintenanceArmGateStatus.Text);
                        InvokePrivate(
                            restartedWindow,
                            "EnsureMaintenanceActionCanStart",
                            "Encoder maintenance readiness "
                                + encoderKinds[index],
                            LMCAdminFeature.None,
                            encoderCapabilities[index]);
                    }

                    CloseConnectedWindow(restartedWindow);
                    restartedWindow = null;
                    restartServer.Verify();
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            restartServer.ReceivedRequests,
                            0x405D));
                    AssertEx.Equal(
                        (ushort)0x405D,
                        TestFrame.ReadUInt16(
                            restartServer.ReceivedRequests.Last(),
                            0));
                    AssertEx.Equal(
                        1,
                        CountAxisPowerCommand(restartServer, true));
                    AssertEx.Equal(
                        1,
                        CountAxisPowerCommand(restartServer, false));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            restartServer.ReceivedRequests,
                            0x204A));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            restartServer.ReceivedRequests,
                            0x204B));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            restartServer.ReceivedRequests,
                            0x7E53));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            restartServer.ReceivedRequests,
                            0x7E54));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            restartServer.ReceivedRequests,
                            0x7E55));
                    AssertOnlyExpectedServoPowerMutationRequests(
                        restartServer.ReceivedRequests,
                        "Diagnostics retirement restart sent an unexpected motion or mutation RPC.");
                }

                using (var diagnostics = DiagnosticsMutationJournal.Open(root))
                using (var axisPower = AxisPowerOnRecoveryJournal.Open(
                    Path.Combine(root, "AxisPowerOnRecovery")))
                using (var groupPower = GroupPowerRecoveryJournal.Open(
                    Path.Combine(root, "GroupPowerRecovery")))
                {
                    AssertEx.False(diagnostics.HasActiveRecord);
                    AssertEx.Equal(
                        DiagnosticsMutationState.Resolved,
                        diagnostics.CurrentRecord.State);
                    AssertEx.Equal(identity, diagnostics.CurrentRecord.Identity);
                    AssertEx.False(axisPower.HasActiveRecord);
                    AssertEx.Equal(
                        AxisPowerOnRecoveryState.Resolved,
                        axisPower.CurrentRecord.State);
                    AssertEx.False(axisPower.CurrentRecord.ExpectedPowerOn);
                    AssertEx.False(groupPower.HasActiveRecord);
                    AssertEx.Equal(
                        GroupPowerRecoveryState.Resolved,
                        groupPower.CurrentRecord.State);
                    AssertEx.False(groupPower.CurrentRecord.ExpectedPowerOn);
                }
            }
            finally
            {
                CloseWindowBestEffort(restartedWindow);
                CloseWindowBestEffort(window);
                DeleteRetirementTemporaryDirectory(root);
            }
        }

        private static void
            DiagnosticsMutationPendingDecisionFinalizesAtStartup()
        {
            var root = CreateRetirementTemporaryDirectory();
            MainWindow window = null;
            try
            {
                Guid identity;
                byte[] sourceBytes;
                CreateLegacyDiagnosticsOutcomeUnverifiedRecord(
                    root,
                    checked(DiagnosticsBootId - 1),
                    out identity,
                    out sourceBytes);
                RecoveryJournalSourceEvidence evidence;
                using (var journal = DiagnosticsMutationJournal.Open(root))
                {
                    evidence = journal
                        .CaptureLegacyEndpointBoundRetirementEvidence(
                            "127.0.0.1",
                            4000);
                }

                using (var ledger = RecoveryRecordRetirementLedger.Open(
                    Path.Combine(
                        root,
                        "RecoveryRecordRetirementLedger")))
                {
                    var decision = ledger.CommitOperatorRetirement(
                        evidence,
                        evidence.EndpointIp,
                        evidence.EndpointPort,
                        DiagnosticsBootId,
                        DiagnosticMapRevision,
                        RetirementTestOperator,
                        RetirementTestReason,
                        DateTime.UtcNow);
                    AssertEx.True(decision.IsDurablyCommitted);
                    AssertEx.SequenceEqual(
                        sourceBytes,
                        decision.SourceEvidence.GetOriginalBytes());
                }

                window = new MainWindow(root)
                {
                    ShowActivated = false,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Left = -10000,
                    Top = -10000
                };
                window.Show();
                WaitUntil(
                    () => window.IsLoaded,
                    "Diagnostics mutation startup-finalization window did not load.");

                var journalInWindow =
                    (DiagnosticsMutationJournal)GetPrivateField(
                        window,
                        "diagnosticsMutationJournal");
                AssertEx.False(journalInWindow.HasActiveRecord);
                AssertEx.Equal(
                    DiagnosticsMutationState.Resolved,
                    journalInWindow.CurrentRecord.State);
                AssertEx.Equal(identity, journalInWindow.CurrentRecord.Identity);
                AssertEx.Contains(
                    "crash-finalization applied exact-byte CAS",
                    window.TextExecutionLog.Text);
                AssertEx.True(((DiagnosticsAdmissionDecision)InvokePrivate(
                    window,
                    "EvaluateDiagnosticsAdmission",
                    DiagnosticsAdmissionOperation.NewLiveOrMutation,
                    true)).IsAllowed);

                window.Close();
                WaitUntil(
                    () => !window.IsLoaded,
                    "Diagnostics mutation startup-finalization window did not close.");
                window = null;

                using (var reopened = DiagnosticsMutationJournal.Open(root))
                {
                    AssertEx.False(reopened.HasActiveRecord);
                    AssertEx.Equal(
                        DiagnosticsMutationState.Resolved,
                        reopened.CurrentRecord.State);
                    AssertEx.Equal(identity, reopened.CurrentRecord.Identity);
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteRetirementTemporaryDirectory(root);
            }
        }

        private static void
            MismatchedRecordsArchiveResolveDisconnectAndRequireRestart()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var currentBootId = checked(DiagnosticsBootId + 1);
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(MotionRecoveryCapabilitiesStep(
                11,
                capabilities,
                currentBootId,
                DiagnosticMapRevision));
            steps.Add(MotionRecoveryCapabilitiesStep(
                12,
                capabilities,
                currentBootId,
                DiagnosticMapRevision));
            steps.Add(MotionRecoveryCapabilitiesStep(
                13,
                capabilities,
                currentBootId,
                DiagnosticMapRevision));
            steps.Add(CloseStep());

            var root = CreateRetirementTemporaryDirectory();
            var callbackPort = ReserveGroupResetCallbackPort();
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
                    CreateGroupPowerRecoveryRecord(
                        root,
                        "127.0.0.1",
                        server.Port,
                        false,
                        GroupPowerRecoveryState.AcceptedAwaitingProof);
                    ArmRestartGroupResetRecord(
                        root,
                        server.Port,
                        callbackPort,
                        false);
                    byte[] axisSourceBytes;
                    byte[] groupSourceBytes;
                    byte[] groupResetSourceBytes;
                    using (var source = AxisPowerOnRecoveryJournal.Open(
                        Path.Combine(root, "AxisPowerOnRecovery")))
                    {
                        axisSourceBytes = File.ReadAllBytes(
                            source.JournalFilePath);
                    }
                    using (var source = GroupPowerRecoveryJournal.Open(
                        Path.Combine(root, "GroupPowerRecovery")))
                    {
                        groupSourceBytes = File.ReadAllBytes(
                            source.JournalFilePath);
                    }
                    window = CreateWindow(root, server.Port);
                    AssertEx.False(
                        window.ExpanderRpcCallbackEvidence.IsExpanded);
                    AssertEx.False(window.TextRpcInitialization.IsVisible);
                    AssertEx.False(window.TextCallbackCounters.IsVisible);
                    window.ExpanderRpcCallbackEvidence.IsExpanded = true;
                    window.UpdateLayout();
                    PumpDispatcherOnce();
                    AssertEx.True(window.TextRpcInitialization.IsVisible);
                    AssertEx.True(window.TextCallbackCounters.IsVisible);
                    window.ExpanderRpcCallbackEvidence.IsExpanded = false;
                    window.UpdateLayout();
                    PumpDispatcherOnce();
                    AssertEx.False(window.TextRpcInitialization.IsVisible);
                    AssertEx.False(window.TextCallbackCounters.IsVisible);
                    window.TextCallbackPort.Text =
                        callbackPort.ToString(
                            System.Globalization.CultureInfo.InvariantCulture);
                    groupResetSourceBytes = File.ReadAllBytes(
                        ((GroupResetRecoveryJournal)GetPrivateField(
                            window,
                            "groupResetRecoveryJournal")).JournalFilePath);
                    ConnectIntoRetirementQuarantine(window);
                    AssertEx.Equal(
                        Visibility.Visible,
                        window.PanelRecoveryIdentityRetirement.Visibility);
                    AssertEx.False(window.TextMotionWarning.IsVisible);
                    AssertEx.False(
                        window.ExpanderSafetyAndRecoveryDetails
                            .IsExpanded);
                    AssertEx.False(
                        window.TextRecoveryIdentityRetirementSnapshot.IsVisible);
                    AssertEx.False(
                        window.CheckConfirmStaleRecoveryRetirement.IsVisible);
                    AssertEx.False(
                        window.ButtonArchiveAndRetireStaleRecovery.IsVisible);
                    window.ExpanderSafetyAndRecoveryDetails
                        .IsExpanded = true;
                    window.UpdateLayout();
                    PumpDispatcherOnce();
                    AssertEx.True(window.TextMotionWarning.IsVisible);
                    AssertEx.True(
                        window.TextRecoveryIdentityRetirementSnapshot.IsVisible);
                    AssertEx.True(
                        window.CheckConfirmStaleRecoveryRetirement.IsVisible);
                    AssertEx.True(
                        window.ButtonArchiveAndRetireStaleRecovery.IsVisible);
                    AssertEx.True(
                        window.CheckConfirmStaleRecoveryRetirement.IsEnabled);
                    AssertControlCenterIsHitTestVisible(
                        window,
                        window.CheckConfirmStaleRecoveryRetirement,
                        "Stale recovery retirement confirmation");
                    AssertEx.Contains(
                        "AxisPower",
                        window.TextRecoveryIdentityRetirementSnapshot.Text);
                    AssertEx.Contains(
                        "GroupPower",
                        window.TextRecoveryIdentityRetirementSnapshot.Text);
                    AssertEx.Contains(
                        "GroupReset",
                        window.TextRecoveryIdentityRetirementSnapshot.Text);

                    var confirmationCalled = false;
                    var exitCalled = false;
                    window.RecoveryRecordRetirementConfirmationOverride =
                        (message, caption) =>
                        {
                            confirmationCalled = true;
                            AssertEx.Contains(
                                "Every listed outcome remains UNKNOWN",
                                message);
                            AssertEx.Contains("BootId", message);
                            return MessageBoxResult.Yes;
                        };
                    window.RecoveryRecordRetirementExitOverride = () =>
                    {
                        exitCalled = true;
                    };

                    window.CheckConfirmStaleRecoveryRetirement.IsChecked = true;
                    PumpDispatcherOnce();
                    AssertEx.True(
                        window.ButtonArchiveAndRetireStaleRecovery.IsEnabled);
                    AssertControlCenterIsHitTestVisible(
                        window,
                        window.ButtonArchiveAndRetireStaleRecovery,
                        "Archive and Retire Stale Recovery");
                    Click(window.ButtonArchiveAndRetireStaleRecovery);
                    WaitUntil(
                        () => exitCalled,
                        "Retirement did not request a process restart.");
                    WaitForRetirementOperationToSettle(window);

                    AssertEx.True(confirmationCalled);
                    AssertEx.True(
                        window.RecoveryRecordRetirementRestartRequired);
                    AssertEx.Equal(
                        "Disconnected",
                        window.TextConnectionState.Text);
                    AssertEx.False(((AxisPowerOnRecoveryJournal)GetPrivateField(
                        window,
                        "axisPowerOnRecoveryJournal")).HasActiveRecord);
                    AssertEx.False(((GroupPowerRecoveryJournal)GetPrivateField(
                        window,
                        "groupPowerRecoveryJournal")).HasActiveRecord);
                    AssertEx.False(((GroupResetRecoveryJournal)GetPrivateField(
                        window,
                        "groupResetRecoveryJournal")).HasActiveRecord);

                    var ledger =
                        (RecoveryRecordRetirementLedger)GetPrivateField(
                            window,
                            "recoveryRecordRetirementLedger");
                    AssertEx.Equal(3, ledger.CommittedDecisions.Count);
                    AssertEx.Equal(
                        3,
                        Directory.GetFiles(
                            ledger.DirectoryPath,
                            "*.retired",
                            SearchOption.TopDirectoryOnly).Length);
                    AssertEx.True(ledger.CommittedDecisions.Any(decision =>
                        RecoveryJournalSourceEvidence.ConstantTimeEquals(
                            axisSourceBytes,
                            decision.SourceEvidence.GetOriginalBytes())));
                    AssertEx.True(ledger.CommittedDecisions.Any(decision =>
                        RecoveryJournalSourceEvidence.ConstantTimeEquals(
                            groupSourceBytes,
                            decision.SourceEvidence.GetOriginalBytes())));
                    AssertEx.True(ledger.CommittedDecisions.Any(decision =>
                        RecoveryJournalSourceEvidence.ConstantTimeEquals(
                            groupResetSourceBytes,
                            decision.SourceEvidence.GetOriginalBytes())));
                    AssertNoMotionMutationRequests(
                        server.ReceivedRequests,
                        "Retirement sent a motion or mutation RPC.");

                    window.Close();
                    WaitUntil(
                        () => !window.IsLoaded,
                        "Retirement-success window did not close.");
                    window = null;
                    server.Verify();
                }

                AssertResolvedAxisGroupPowerAndGroupResetRecords(root);
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteRetirementTemporaryDirectory(root);
            }
        }

        private static void ExactCurrentRecordCannotRetire()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(MotionRecoveryCapabilitiesStep(
                11,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(CloseStep());

            var root = CreateRetirementTemporaryDirectory();
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
                    window = CreateWindow(root, server.Port);
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
                        "Exact-current recovery did not connect.");

                    AssertEx.Equal(
                        Visibility.Collapsed,
                        window.PanelRecoveryIdentityRetirement.Visibility);
                    AssertEx.False(
                        window.CheckConfirmStaleRecoveryRetirement.IsEnabled);
                    AssertEx.False(
                        window.ButtonArchiveAndRetireStaleRecovery.IsEnabled);
                    var journal =
                        (AxisPowerOnRecoveryJournal)GetPrivateField(
                            window,
                            "axisPowerOnRecoveryJournal");
                    AssertEx.True(journal.HasActiveRecord);
                    AssertEx.Equal(
                        DiagnosticsBootId,
                        journal.CurrentRecord.DiagnosticsBootId);
                    AssertEx.Equal(
                        0,
                        ((RecoveryRecordRetirementLedger)GetPrivateField(
                            window,
                            "recoveryRecordRetirementLedger"))
                        .CommittedDecisions.Count);

                    InvokePrivate(
                        window,
                        "ResolveAxisPowerOnRecoveryJournal",
                        "exact-current test cleanup");
                    InvokePrivate(window, "UpdateUiState");
                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteRetirementTemporaryDirectory(root);
            }
        }

        private static void
            GroupResetBuildOnlyMismatchArchivesAndResolves()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
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
            steps.Add(MotionRecoveryCapabilitiesStep(
                13,
                capabilities,
                DiagnosticsBootId,
                DiagnosticMapRevision));
            steps.Add(CloseStep());

            var root = CreateRetirementTemporaryDirectory();
            var callbackPort = ReserveGroupResetCallbackPort();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    ArmRestartGroupResetRecord(
                        root,
                        server.Port,
                        callbackPort,
                        false,
                        2);
                    window = CreateWindow(root, server.Port);
                    window.TextCallbackPort.Text =
                        callbackPort.ToString(
                            System.Globalization.CultureInfo.InvariantCulture);
                    ConnectIntoRetirementQuarantine(window);

                    AssertEx.Contains(
                        "GroupReset",
                        window.TextRecoveryIdentityRetirementSnapshot.Text);
                    AssertEx.Contains(
                        "Build=0x00000002",
                        window.TextRecoveryIdentityRetirementSnapshot.Text);
                    AssertEx.Contains(
                        "Build=0x00000001",
                        window.TextRecoveryIdentityRetirementSnapshot.Text);

                    var exitCalled = false;
                    window.RecoveryRecordRetirementConfirmationOverride =
                        (message, caption) =>
                        {
                            AssertEx.Contains("Stored Build", message);
                            return MessageBoxResult.Yes;
                        };
                    window.RecoveryRecordRetirementExitOverride = () =>
                    {
                        exitCalled = true;
                    };

                    window.CheckConfirmStaleRecoveryRetirement.IsChecked = true;
                    PumpDispatcherOnce();
                    AssertEx.True(
                        window.ButtonArchiveAndRetireStaleRecovery.IsEnabled);
                    Click(window.ButtonArchiveAndRetireStaleRecovery);
                    WaitUntil(
                        () => exitCalled,
                        "Build-only Group Reset retirement did not request restart.");
                    WaitForRetirementOperationToSettle(window);

                    var journal =
                        (GroupResetRecoveryJournal)GetPrivateField(
                            window,
                            "groupResetRecoveryJournal");
                    AssertEx.False(journal.HasActiveRecord);
                    var ledger =
                        (RecoveryRecordRetirementLedger)GetPrivateField(
                            window,
                            "recoveryRecordRetirementLedger");
                    AssertEx.Equal(1, ledger.CommittedDecisions.Count);
                    var decision = ledger.CommittedDecisions.Single();
                    AssertEx.Equal(
                        RecoveryRecordOwner.GroupReset,
                        decision.SourceEvidence.Owner);
                    AssertEx.Equal(
                        (uint)2,
                        decision.SourceEvidence.DiagnosticsBuild);
                    AssertEx.Equal(
                        (uint)1,
                        decision.CurrentDiagnosticsBuild);
                    AssertEx.Equal(
                        DiagnosticsBootId,
                        decision.CurrentDiagnosticsBootId);
                    AssertEx.Equal(
                        DiagnosticMapRevision,
                        decision.CurrentMapRevision);
                    AssertNoMotionMutationRequests(
                        server.ReceivedRequests,
                        "Build-only retirement sent a motion or mutation RPC.");

                    window.Close();
                    WaitUntil(
                        () => !window.IsLoaded,
                        "Build-only retirement window did not close.");
                    window = null;
                    server.Verify();
                }

                using (var reopened = GroupResetRecoveryJournal.Open(
                    Path.Combine(root, "GroupResetRecovery")))
                {
                    AssertEx.False(reopened.HasActiveRecord);
                    AssertEx.Equal(
                        GroupResetRecoveryState.Resolved,
                        reopened.CurrentRecord.State);
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteRetirementTemporaryDirectory(root);
            }
        }

        private static void
            MixedExactAndStaleRetiresSubsetThenExactRecoveryOpensControl()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology
                | LMCDiagnosticCapability.SDORead
                | LMCDiagnosticCapability.SDOWrite
                | LMCDiagnosticCapability.SDOReadGeneralInline;
            var steps = CreateConnectAndTopologySteps(capabilities);
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
            var retirementClose = CloseStep();
            retirementClose.CloseClientAfterResponseAndContinue = true;
            steps.Add(retirementClose);
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

            var root = CreateRetirementTemporaryDirectory();
            var callbackPort = ReserveGroupResetCallbackPort();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    CreateAxisPowerJournalRecord(
                        root,
                        server.Port,
                        true,
                        AxisPowerOnRecoveryState.AcceptedAwaitingProof);
                    ArmRestartGroupResetRecord(
                        root,
                        server.Port,
                        callbackPort,
                        false,
                        2);

                    window = CreateWindow(root, server.Port);
                    window.TextCallbackPort.Text =
                        callbackPort.ToString(
                            System.Globalization.CultureInfo.InvariantCulture);
                    ConnectIntoRetirementQuarantine(window);
                    var snapshot =
                        window.TextRecoveryIdentityRetirementSnapshot.Text;
                    AssertEx.Contains(
                        "KEEP EXACT CURRENT | AxisPower",
                        snapshot);
                    AssertEx.Contains(
                        "RETIRE STALE | GroupReset",
                        snapshot);

                    var confirmationCalled = false;
                    var exitCalled = false;
                    window.RecoveryRecordRetirementConfirmationOverride =
                        (message, caption) =>
                        {
                            confirmationCalled = true;
                            AssertEx.Contains(
                                "RETIRE STALE - records to archive and resolve",
                                message);
                            AssertEx.Contains(
                                "KEEP EXACT CURRENT - records left active",
                                message);
                            AssertEx.Contains("GroupReset", message);
                            AssertEx.Contains("AxisPower", message);
                            return MessageBoxResult.Yes;
                        };
                    window.RecoveryRecordRetirementExitOverride = () =>
                    {
                        exitCalled = true;
                    };

                    window.CheckConfirmStaleRecoveryRetirement.IsChecked = true;
                    PumpDispatcherOnce();
                    AssertEx.True(
                        window.ButtonArchiveAndRetireStaleRecovery.IsEnabled);
                    Click(window.ButtonArchiveAndRetireStaleRecovery);
                    WaitForRetirementOperationToSettle(window);
                    AssertEx.True(
                        exitCalled,
                        "Mixed retirement did not force restart. Log="
                            + window.TextExecutionLog.Text);
                    AssertEx.True(confirmationCalled);
                    AssertEx.Contains(
                        "exact-current recovery record(s) were kept active",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "exact status-only recovery before Motion, Power, or approved SDO Write controls open",
                        window.TextExecutionLog.Text);

                    var axisJournal =
                        (AxisPowerOnRecoveryJournal)GetPrivateField(
                            window,
                            "axisPowerOnRecoveryJournal");
                    var resetJournal =
                        (GroupResetRecoveryJournal)GetPrivateField(
                            window,
                            "groupResetRecoveryJournal");
                    AssertEx.True(axisJournal.HasActiveRecord);
                    AssertEx.False(resetJournal.HasActiveRecord);
                    var ledger =
                        (RecoveryRecordRetirementLedger)GetPrivateField(
                            window,
                            "recoveryRecordRetirementLedger");
                    AssertEx.Equal(1, ledger.CommittedDecisions.Count);
                    AssertEx.Equal(
                        RecoveryRecordOwner.GroupReset,
                        ledger.CommittedDecisions.Single()
                            .SourceEvidence.Owner);

                    window.Close();
                    WaitUntil(
                        () => !window.IsLoaded,
                        "Mixed-retirement window did not close.");
                    window = null;

                    window = CreateWindow(root, server.Port);
                    Click(window.ButtonConnect);
                    try
                    {
                        WaitUntil(
                            () => window.ButtonLookupAxis.IsEnabled,
                            "Restart did not reconnect for exact-current recovery.");
                    }
                    catch (TimeoutException error)
                    {
                        throw new TimeoutException(
                            error.Message
                                + " Requests="
                                + string.Join(
                                    ",",
                                    server.ReceivedRequests.Select(request =>
                                        "0x"
                                        + TestFrame.ReadUInt16(request, 0)
                                            .ToString("X4")))
                                + " Log="
                                + window.TextExecutionLog.Text,
                            error);
                    }
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => window.ButtonPowerOn.IsEnabled
                            && string.Equals(
                                window.TextAxisReference.Text,
                                "1",
                                StringComparison.Ordinal),
                        "Restart did not attach exact-current Axis Power recovery.");
                    AssertEx.Contains(
                        "No 0x2023 Replay",
                        Convert.ToString(
                            window.ButtonPowerOn.Content,
                            System.Globalization.CultureInfo.InvariantCulture));
                    Click(window.ButtonPowerOn);
                    axisJournal =
                        (AxisPowerOnRecoveryJournal)GetPrivateField(
                            window,
                            "axisPowerOnRecoveryJournal");
                    WaitUntil(
                        () => !axisJournal.HasActiveRecord
                            && window.ButtonPowerOff.IsEnabled
                            && window.ButtonMoveAbsolute.IsEnabled
                            && window.ComboD5SdoWriteQualificationTarget
                                .IsEnabled,
                        "Exact status-only recovery did not reopen Power, Motion, and SDO qualification controls.");
                    AssertEx.Equal(
                        AxisPowerOnRecoveryState.Resolved,
                        axisJournal.CurrentRecord.State);
                    AssertEx.Equal(
                        0,
                        server.ReceivedRequests.Count(request =>
                            TestFrame.ReadUInt16(request, 0) == 0x2023));
                    AssertEx.Equal(
                        1,
                        window.ComboD5SdoWriteQualificationTarget.Items.Count);
                    AssertEx.True(
                        window.CheckConfirmD5SdoWriteUi24Unused.IsEnabled);
                    AssertEx.True(
                        window.CheckConfirmD5SdoWriteOriginalRecorded.IsEnabled);
                    AssertEx.True(
                        window.CheckConfirmD5SdoWriteCaptureRunning.IsEnabled);
                    AssertEx.True(
                        window.CheckConfirmD5SdoWriteSingleWriter.IsEnabled);
                    var sdoReadiness =
                        window.TextD5SdoWriteQualificationGateStatus.Text;
                    AssertEx.Contains("SDK POLICY    PASS", sdoReadiness);
                    AssertEx.Contains(
                        "bit8/read=1 bit9/write=1 bit13/general=1",
                        sdoReadiness);
                    AssertEx.Contains("QUAL TARGET   PASS", sdoReadiness);
                    AssertEx.Contains("RUNNER        PASS", sdoReadiness);
                    AssertEx.Contains("JOURNAL       PASS", sdoReadiness);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteRetirementTemporaryDirectory(root);
            }
        }

        private static void EvidenceChangeAfterConfirmationFailsClosed()
        {
            AssertPostConfirmationChangeFailsClosed(
                RetirementChangeKind.Evidence);
        }

        private static void SessionChangeAfterConfirmationFailsClosed()
        {
            AssertPostConfirmationChangeFailsClosed(
                RetirementChangeKind.Session);
        }

        private static void CapabilityChangeAfterConfirmationFailsClosed()
        {
            AssertPostConfirmationChangeFailsClosed(
                RetirementChangeKind.Capability);
        }

        private static void AssertPostConfirmationChangeFailsClosed(
            RetirementChangeKind changeKind)
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var currentBootId = checked(DiagnosticsBootId + 1);
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(MotionRecoveryCapabilitiesStep(
                11,
                capabilities,
                currentBootId,
                DiagnosticMapRevision));
            steps.Add(MotionRecoveryCapabilitiesStep(
                12,
                capabilities,
                currentBootId,
                DiagnosticMapRevision));
            if (changeKind != RetirementChangeKind.Session)
            {
                steps.Add(MotionRecoveryCapabilitiesStep(
                    13,
                    capabilities,
                    changeKind == RetirementChangeKind.Capability
                        ? checked(currentBootId + 1)
                        : currentBootId,
                    DiagnosticMapRevision));
            }
            steps.Add(CloseStep());

            var root = CreateRetirementTemporaryDirectory();
            MainWindow window = null;
            LMCConnection originalConnection = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    CreateAxisPowerJournalRecord(
                        root,
                        server.Port,
                        false,
                        AxisPowerOnRecoveryState.AcceptedAwaitingProof);
                    window = CreateWindow(root, server.Port);
                    ConnectIntoRetirementQuarantine(window);

                    var confirmationCalled = false;
                    var exitCalled = false;
                    originalConnection = (LMCConnection)GetPrivateField(
                        window,
                        "connection");
                    window.RecoveryRecordRetirementConfirmationOverride =
                        (message, caption) =>
                        {
                            confirmationCalled = true;
                            return MessageBoxResult.Yes;
                        };
                    window.RecoveryRecordRetirementExitOverride = () =>
                    {
                        exitCalled = true;
                    };
                    window.RecoveryRecordRetirementAfterConfirmationTestHook =
                        () =>
                        {
                            if (changeKind == RetirementChangeKind.Evidence)
                            {
                                var journal =
                                    (AxisPowerOnRecoveryJournal)GetPrivateField(
                                        window,
                                        "axisPowerOnRecoveryJournal");
                                var current = journal.CurrentRecord;
                                journal.PromoteToRecoveryRequired(
                                    current.Identity,
                                    current.UpdatedUtc.AddTicks(1));
                            }
                            else if (changeKind
                                == RetirementChangeKind.Session)
                            {
                                SetPrivateField(window, "connection", null);
                            }
                        };

                    window.CheckConfirmStaleRecoveryRetirement.IsChecked = true;
                    PumpDispatcherOnce();
                    AssertEx.True(
                        window.ButtonArchiveAndRetireStaleRecovery.IsEnabled);
                    Click(window.ButtonArchiveAndRetireStaleRecovery);
                    WaitUntil(
                        () => confirmationCalled,
                        "Retirement confirmation override was not called.");
                    WaitForRetirementOperationToSettle(window);

                    AssertEx.False(exitCalled);
                    AssertEx.False(
                        window.RecoveryRecordRetirementRestartRequired);
                    var journalAfterFailure =
                        (AxisPowerOnRecoveryJournal)GetPrivateField(
                            window,
                            "axisPowerOnRecoveryJournal");
                    AssertEx.True(journalAfterFailure.HasActiveRecord);
                    AssertEx.Equal(
                        0,
                        ((RecoveryRecordRetirementLedger)GetPrivateField(
                            window,
                            "recoveryRecordRetirementLedger"))
                        .CommittedDecisions.Count);
                    AssertEx.Contains(
                        "FAILED",
                        window.TextExecutionLog.Text);
                    AssertNoMotionMutationRequests(
                        server.ReceivedRequests,
                        changeKind + " retirement failure sent a mutation.");

                    if (changeKind == RetirementChangeKind.Session)
                    {
                        SetPrivateField(
                            window,
                            "connection",
                            originalConnection);
                        InvokePrivate(window, "UpdateUiState");
                    }
                    InvokePrivate(
                        window,
                        "ResolveAxisPowerOnRecoveryJournal",
                        changeKind + " test cleanup");
                    InvokePrivate(window, "UpdateUiState");
                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                if (window != null
                    && changeKind == RetirementChangeKind.Session
                    && originalConnection != null
                    && GetPrivateField(window, "connection") == null)
                {
                    SetPrivateField(window, "connection", originalConnection);
                    InvokePrivate(window, "UpdateUiState");
                }
                CloseWindowBestEffort(window);
                DeleteRetirementTemporaryDirectory(root);
            }
        }

        private static void StartupPendingDecisionExactCasFinalizes()
        {
            var root = CreateRetirementTemporaryDirectory();
            MainWindow window = null;
            try
            {
                RecoveryJournalSourceEvidence evidence;
                using (var journal = AxisPowerOnRecoveryJournal.Open(
                    Path.Combine(root, "AxisPowerOnRecovery")))
                {
                    var armed = journal.ArmBeforeDispatch(
                        false,
                        "127.0.0.1",
                        4000,
                        "_LMCAxis1",
                        1,
                        DiagnosticsBootId,
                        DiagnosticMapRevision,
                        DateTime.UtcNow);
                    journal.MarkAccepted(
                        armed.Identity,
                        armed.UpdatedUtc.AddTicks(1));
                    evidence = journal.CaptureActiveRetirementEvidence();
                }

                var ledgerDirectory = Path.Combine(
                    root,
                    "RecoveryRecordRetirementLedger");
                using (var ledger = RecoveryRecordRetirementLedger.Open(
                    ledgerDirectory))
                {
                    ledger.CommitOperatorRetirement(
                        evidence,
                        evidence.EndpointIp,
                        evidence.EndpointPort,
                        checked(DiagnosticsBootId + 1),
                        DiagnosticMapRevision,
                        RetirementTestOperator,
                        RetirementTestReason,
                        DateTime.UtcNow.AddTicks(1));
                }

                window = new MainWindow(root)
                {
                    ShowActivated = false,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Left = -10000,
                    Top = -10000
                };
                window.Show();
                WaitUntil(
                    () => window.IsLoaded,
                    "Startup-retirement window did not load.");

                var journalInWindow =
                    (AxisPowerOnRecoveryJournal)GetPrivateField(
                        window,
                        "axisPowerOnRecoveryJournal");
                AssertEx.False(journalInWindow.HasActiveRecord);
                AssertEx.Equal(
                    AxisPowerOnRecoveryState.Resolved,
                    journalInWindow.CurrentRecord.State);
                AssertEx.Equal(
                    1,
                    ((RecoveryRecordRetirementLedger)GetPrivateField(
                        window,
                        "recoveryRecordRetirementLedger"))
                    .CommittedDecisions.Count);
                AssertEx.Contains(
                    "crash-finalization applied exact-byte CAS",
                    window.TextExecutionLog.Text);

                window.Close();
                WaitUntil(
                    () => !window.IsLoaded,
                    "Startup-retirement window did not close.");
                window = null;

                using (var reopened = AxisPowerOnRecoveryJournal.Open(
                    Path.Combine(root, "AxisPowerOnRecovery")))
                {
                    AssertEx.False(reopened.HasActiveRecord);
                    AssertEx.Equal(
                        AxisPowerOnRecoveryState.Resolved,
                        reopened.CurrentRecord.State);
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteRetirementTemporaryDirectory(root);
            }
        }

        private static void
            AxisQualificationVolatilePendingDecisionFinalizesBeforePromotion()
        {
            AssertAxisQualificationPendingDecisionFinalizesBeforePromotion(
                AxisQualificationRecoveryStage.ArmedBeforePowerOn);
            AssertAxisQualificationPendingDecisionFinalizesBeforePromotion(
                AxisQualificationRecoveryStage.MovePrepared);
        }

        private static void
            AssertAxisQualificationPendingDecisionFinalizesBeforePromotion(
                AxisQualificationRecoveryStage sourceStage)
        {
            var root = CreateRetirementTemporaryDirectory();
            MainWindow window = null;
            try
            {
                var journalDirectory = Path.Combine(
                    root,
                    "AxisQualificationRecovery");
                var createdUtc = DateTime.UtcNow.AddMinutes(-1);
                RecoveryJournalSourceEvidence evidence;
                Guid sourceIdentity;
                long sourceRevision;
                using (var journal = AxisQualificationRecoveryJournal.Open(
                    journalDirectory,
                    true))
                {
                    var record = journal.ArmBeforePowerOn(
                        "127.0.0.1",
                        4000,
                        1,
                        "_LMCAxis1",
                        1,
                        1,
                        DiagnosticsBootId,
                        DiagnosticMapRevision,
                        120,
                        230,
                        340,
                        450,
                        0,
                        5,
                        0,
                        createdUtc);
                    if (sourceStage
                        == AxisQualificationRecoveryStage.MovePrepared)
                    {
                        record = journal.MarkPowerOnAccepted(
                            record,
                            createdUtc.AddTicks(1));
                        record = journal.MarkPowerOnStable(
                            record,
                            createdUtc.AddTicks(2));
                        record = journal.PrepareMove(
                            record,
                            1000,
                            1120,
                            createdUtc.AddTicks(3));
                    }

                    AssertEx.Equal(sourceStage, record.Stage);
                    AssertEx.False(record.WasCrashPromoted);
                    sourceIdentity = record.Identity;
                    sourceRevision = record.RecordRevision;
                    evidence = journal.CaptureActiveRetirementEvidence();
                    AssertEx.Equal((int)sourceStage, evidence.StateCode);
                }

                using (var ledger = RecoveryRecordRetirementLedger.Open(
                    Path.Combine(root, "RecoveryRecordRetirementLedger")))
                {
                    var decision = ledger.CommitOperatorRetirement(
                        evidence,
                        evidence.EndpointIp,
                        evidence.EndpointPort,
                        checked(evidence.DiagnosticsBuild + 1),
                        evidence.DiagnosticsBootId,
                        evidence.MapRevision,
                        RetirementTestOperator,
                        RetirementTestReason,
                        evidence.UpdatedUtc.AddTicks(1));
                    AssertEx.True(decision.IsDurablyCommitted);
                    AssertEx.True(decision.MatchesSourceEvidence(evidence));
                }

                // Simulate a process crash after the ledger commit but before
                // the source journal could be resolved. MainWindow startup
                // must consume the exact pending decision before crash
                // promotion changes the volatile source bytes.
                window = new MainWindow(root)
                {
                    ShowActivated = false,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Left = -10000,
                    Top = -10000
                };
                window.Show();
                WaitUntil(
                    () => window.IsLoaded,
                    "Axis qualification startup-retirement window did not load.");

                var journalInWindow =
                    (AxisQualificationRecoveryJournal)GetPrivateField(
                        window,
                        "axisQualificationRecoveryJournal");
                AssertEx.False(journalInWindow.HasActiveRecord);
                var resolved = journalInWindow.CurrentRecord;
                AssertEx.NotNull(resolved);
                AssertEx.Equal(
                    AxisQualificationRecoveryStage.SafeResolved,
                    resolved.Stage);
                AssertEx.Equal(sourceIdentity, resolved.Identity);
                AssertEx.Equal(sourceRevision + 1, resolved.RecordRevision);
                AssertEx.False(
                    resolved.WasCrashPromoted,
                    sourceStage
                    + " was crash-promoted before exact retirement finalization.");
                AssertEx.Equal(
                    1,
                    ((RecoveryRecordRetirementLedger)GetPrivateField(
                        window,
                        "recoveryRecordRetirementLedger"))
                    .CommittedDecisions.Count);
                AssertEx.Contains(
                    "crash-finalization applied exact-byte CAS",
                    window.TextExecutionLog.Text);

                window.Close();
                WaitUntil(
                    () => !window.IsLoaded,
                    "Axis qualification startup-retirement window did not close.");
                window = null;

                using (var reopened = AxisQualificationRecoveryJournal.Open(
                    journalDirectory))
                {
                    AssertEx.False(reopened.HasActiveRecord);
                    AssertEx.NotNull(reopened.CurrentRecord);
                    AssertEx.Equal(
                        AxisQualificationRecoveryStage.SafeResolved,
                        reopened.CurrentRecord.Stage);
                    AssertEx.Equal(
                        sourceIdentity,
                        reopened.CurrentRecord.Identity);
                    AssertEx.False(reopened.CurrentRecord.WasCrashPromoted);
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteRetirementTemporaryDirectory(root);
            }
        }

        private static void ConnectIntoRetirementQuarantine(MainWindow window)
        {
            Click(window.ButtonConnect);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Connect completed",
                        StringComparison.Ordinal)
                    && string.Equals(
                        window.TextConnectionState.Text,
                        "Connected",
                        StringComparison.Ordinal)
                    && window.PanelRecoveryIdentityRetirement.Visibility
                        == Visibility.Visible,
                "Recovery identity mismatch did not enter retirement quarantine.");
        }

        private static void ConnectIntoRetirementQuarantineWithDiagnostics(
            MainWindow window,
            FakeRpcServer server,
            string context)
        {
            try
            {
                ConnectIntoRetirementQuarantine(window);
            }
            catch (TimeoutException error)
            {
                throw new TimeoutException(
                    error.Message
                        + " Context="
                        + context
                        + " Connection="
                        + window.TextConnectionState.Text
                        + " Operation="
                        + window.TextOperationState.Text
                        + " Requests="
                        + string.Join(
                            ",",
                            server.ReceivedRequests.Select(request =>
                                "0x"
                                    + TestFrame.ReadUInt16(request, 0)
                                        .ToString("X4")))
                        + " Log="
                        + window.TextExecutionLog.Text,
                    error);
            }
        }

        private static void
            CreateLegacyDiagnosticsOutcomeUnverifiedRecord(
                string root,
                uint storedBootId,
                out Guid identity,
                out byte[] sourceBytes)
        {
            identity = Guid.NewGuid();
            var createdUtc = DateTime.UtcNow.AddMinutes(-1);
            using (var journal = DiagnosticsMutationJournal.Open(root))
            {
                journal.Arm(
                    DiagnosticsMutationKind.SdoWrite,
                    identity,
                    createdUtc,
                    storedBootId,
                    DiagnosticMapRevision,
                    7,
                    "Slave=1,Object=0x2F00,SubIndex=24,Type=Int32,Length=4",
                    "WriteData=00-00-00-00",
                    new DiagnosticsSdoWriteMutationMetadata(
                        1,
                        0x2F00,
                        24,
                        LMCSignalValueType.Int32,
                        4,
                        1000,
                        new byte[] { 0, 0, 0, 0 }));
                journal.Transition(
                    identity,
                    DiagnosticsMutationState.AcceptedPendingTerminal,
                    createdUtc.AddSeconds(1),
                    3);
                journal.Transition(
                    identity,
                    DiagnosticsMutationState.OutcomeUnverified,
                    createdUtc.AddSeconds(2),
                    3);
                sourceBytes = File.ReadAllBytes(journal.JournalFilePath);
            }
        }

        private static void AssertControlCenterIsHitTestVisible(
            Window window,
            FrameworkElement control,
            string controlName)
        {
            AssertEx.True(control.ActualWidth > 0);
            AssertEx.True(control.ActualHeight > 0);
            var center = control.TransformToAncestor(window).Transform(
                new Point(
                    control.ActualWidth / 2.0,
                    control.ActualHeight / 2.0));
            var hit = window.InputHitTest(center) as DependencyObject;
            while (hit != null && !ReferenceEquals(hit, control))
            {
                hit = VisualTreeHelper.GetParent(hit);
            }
            AssertEx.True(
                ReferenceEquals(hit, control),
                controlName
                    + " center is clipped or covered by another control.");
        }

        private static FakeRpcStep ExactCloseStep()
        {
            var step = CloseStep();
            step.InspectRequest = request => AssertEx.SequenceEqual(
                TestFrame.Request(0x405D, 0, new byte[1]),
                request);
            return step;
        }

        private static FakeRpcStep ExactAxisPowerOnStep()
        {
            var step = AxisPowerCommandStep(true);
            step.InspectRequest = request => AssertEx.SequenceEqual(
                TestFrame.Request(
                    0x2023,
                    1,
                    TestFrame.Hex("01 00 00 00 01 01 00 01")),
                request);
            return step;
        }

        private static FakeRpcStep ExactAxisPowerOffStep()
        {
            var step = AxisPowerCommandStep(false);
            step.InspectRequest = request => AssertEx.SequenceEqual(
                TestFrame.Request(
                    0x2023,
                    1,
                    TestFrame.Hex("01 00 00 00 00 01 00 01")),
                request);
            return step;
        }

        private static void AssertOnlyExpectedServoPowerMutationRequests(
            IList<byte[]> requests,
            string message)
        {
            for (var index = 0; index < requests.Count; index++)
            {
                var command = TestFrame.ReadUInt16(requests[index], 0);
                if (IsMotionMutationCommand(command))
                {
                    AssertEx.True(
                        command == 0x2023
                            || command == 0x204A
                            || command == 0x204B,
                        message
                            + " Command=0x"
                            + command.ToString("X4")
                            + ".");
                }
            }
        }

        private static FakeRpcStep ExactGroupLookupStep(string groupName)
        {
            var step = GroupEnableWaitLookupStep();
            var payload = new byte[80];
            var nameBytes = System.Text.Encoding.ASCII.GetBytes(groupName);
            Buffer.BlockCopy(
                nameBytes,
                0,
                payload,
                0,
                nameBytes.Length);
            step.InspectRequest = request => AssertEx.SequenceEqual(
                TestFrame.Request(0x1042, 0, payload),
                request);
            return step;
        }

        private static void WaitForRetirementOperationToSettle(
            MainWindow window)
        {
            WaitUntil(
                () => !(bool)GetPrivateField(window, "operationRunning"),
                "Recovery retirement operation did not settle.");
        }

        private static void
            AssertResolvedAxisGroupPowerAndGroupResetRecords(string root)
        {
            using (var axis = AxisPowerOnRecoveryJournal.Open(
                Path.Combine(root, "AxisPowerOnRecovery")))
            using (var group = GroupPowerRecoveryJournal.Open(
                Path.Combine(root, "GroupPowerRecovery")))
            using (var groupReset = GroupResetRecoveryJournal.Open(
                Path.Combine(root, "GroupResetRecovery")))
            {
                AssertEx.False(axis.HasActiveRecord);
                AssertEx.False(group.HasActiveRecord);
                AssertEx.False(groupReset.HasActiveRecord);
                AssertEx.Equal(
                    AxisPowerOnRecoveryState.Resolved,
                    axis.CurrentRecord.State);
                AssertEx.Equal(
                    GroupPowerRecoveryState.Resolved,
                    group.CurrentRecord.State);
                AssertEx.Equal(
                    GroupResetRecoveryState.Resolved,
                    groupReset.CurrentRecord.State);
            }
        }

        private static string CreateRetirementTemporaryDirectory()
        {
            var path = Path.Combine(
                Path.GetTempPath(),
                "ElmoRetire",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static void DeleteRetirementTemporaryDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        private enum RetirementChangeKind
        {
            Evidence,
            Session,
            Capability
        }
    }
}

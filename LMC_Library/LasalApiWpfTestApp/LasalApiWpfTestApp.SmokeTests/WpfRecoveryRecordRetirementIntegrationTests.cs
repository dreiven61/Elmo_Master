using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
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
                "Wpf.RecoveryRetirement.MismatchedRecordsArchiveResolveDisconnectAndRequireRestart",
                MismatchedRecordsArchiveResolveDisconnectAndRequireRestart);
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
                    AssertEx.True(
                        window.CheckConfirmStaleRecoveryRetirement.IsEnabled);
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using LasalMotionControlLib;
using LasalMotionControlLib.Tests;

namespace LasalApiWpfTestApp.SmokeTests
{
    internal static class WpfMaintenanceActionIntegrationTests
    {
        private const uint Ds402DiagnosticsBuild = 0x613F0001U;
        private const uint Ds402DiagnosticsBootId = 0x10203040U;
        private const uint Ds402MapRevision = 0xE245539AU;
        private const uint Ds402OriginalRequestId = 0x55667788U;
        private const uint Ds402RecordGeneration = 7U;
        private const int LmcHomeExpectedActualPosition = -123456;
        private const uint LmcHomeRecordGeneration = 17U;
        private const uint LmcHomeRequiredEvidenceFlags = 0x0000003BU;
        private const uint LmcHomeStandstillState = 0x02000000U;

        private enum Ds402RetirementScenario
        {
            Success,
            Timeout,
            ResponseLoss,
            KeyMismatch,
            Malformed,
            SnapshotResultMismatch,
            SnapshotEvidenceMismatch
        }

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.MaintenanceUi.DisconnectedFailClosedAndLocalized",
                DisconnectedControlsFailClosedAndLocalize);
            tests.Add(
                "Wpf.MaintenanceUi.RestartRecordQuarantinesAllSends",
                RestartRecordQuarantinesAllMaintenanceSends);
            tests.Add(
                "Wpf.MaintenanceUi.EncoderMaintenanceAbsentFromGenericSdoTargets",
                EncoderMaintenanceIsAbsentFromGenericSdoTargets);
            tests.Add(
                "Wpf.MaintenanceUi.EncoderPreparedRequestDisablesRearmAndEnablesConfirmedExecuteZeroWire",
                EncoderPreparedRequestDisablesRearmAndEnablesConfirmedExecuteZeroWire);
            tests.Add(
                "Wpf.MaintenanceUi.LmcRecoveryKeyRoundTrip",
                LmcRecoveryKeyRoundTripsWithCurrentZeroIntent);
            tests.Add(
                "Wpf.MaintenanceUi.EncoderRecoveryKeyRoundTrip",
                EncoderMaintenanceRecoveryKeyRoundTrips);
            tests.Add(
                "Wpf.MaintenanceUi.Ds402HomeFixedMethod37ZeroOffset",
                Ds402HomeUsesFixedMethod37ZeroOffset);
            tests.Add(
                "Wpf.MaintenanceUi.Ds402HomeRestartRestoresRecoveryKeyImmediately",
                Ds402HomeRestartRestoresRecoveryKeyImmediately);
            tests.Add(
                "Wpf.MaintenanceUi.LmcHomeButtonTerminalRetiresBeforeJournalResolveAndLogsOutcome",
                LmcHomeButtonTerminalRetiresBeforeJournalResolveAndLogsOutcome);
            tests.Add(
                "Wpf.MaintenanceUi.Ds402HomeTerminalRetiresBeforeJournalResolve",
                Ds402HomeTerminalRetiresBeforeJournalResolve);
            tests.Add(
                "Wpf.MaintenanceUi.Ds402HomeRetirementTimeoutRetainsJournalZeroReplay",
                Ds402HomeRetirementTimeoutRetainsJournalZeroReplay);
            tests.Add(
                "Wpf.MaintenanceUi.Ds402HomeRetirementResponseLossRetainsJournalZeroReplay",
                Ds402HomeRetirementResponseLossRetainsJournalZeroReplay);
            tests.Add(
                "Wpf.MaintenanceUi.Ds402HomeRetirementMismatchRetainsJournalZeroReplay",
                Ds402HomeRetirementMismatchRetainsJournalZeroReplay);
            tests.Add(
                "Wpf.MaintenanceUi.Ds402HomeRetirementMalformedRetainsJournalZeroReplay",
                Ds402HomeRetirementMalformedRetainsJournalZeroReplay);
            tests.Add(
                "Wpf.MaintenanceUi.Ds402HomeRetirementResultMismatchRetainsJournalZeroReplay",
                Ds402HomeRetirementResultMismatchRetainsJournalZeroReplay);
            tests.Add(
                "Wpf.MaintenanceUi.Ds402HomeRetirementEvidenceMismatchRetainsJournalZeroReplay",
                Ds402HomeRetirementEvidenceMismatchRetainsJournalZeroReplay);
            tests.Add(
                "Wpf.MaintenanceUi.Ds402HomeRetirementResponseLossReconnectCompletesZeroReplay",
                Ds402HomeRetirementResponseLossReconnectCompletesZeroReplay);
        }

        private static void DisconnectedControlsFailClosedAndLocalize()
        {
            WithTemporaryWindow(
                LasalMotionControlApiExample.UiLanguage.English,
                null,
                window =>
                {
                    AssertEx.False(window.ButtonLmcHome.IsEnabled);
                    AssertEx.False(window.ButtonDs402Home.IsEnabled);
                    AssertEx.False(window.ButtonArmTestReset.IsEnabled);
                    AssertEx.False(window.ButtonExecuteTestReset.IsEnabled);
                    AssertEx.Equal(
                        LMCEncoderMaintenanceKind
                            .Tw19MultiturnPositionReset,
                        (LMCEncoderMaintenanceKind)window
                            .ComboEncoderMaintenanceKind.SelectedItem);
                    var defaultEncoderRequest =
                        window.ReadEncoderMaintenanceRequest();
                    AssertEx.True(
                        defaultEncoderRequest
                            is LMCTw19MultiturnPositionResetRequest);
                    AssertEx.Equal(
                        LMCEncoderMaintenanceKind
                            .Tw19MultiturnPositionReset,
                        defaultEncoderRequest.Kind);
                    AssertEx.Equal(1u, defaultEncoderRequest.CommandValue);
                    AssertEx.Equal(
                        "BLOCKED: connect to the PLC before arming encoder maintenance. No encoder-maintenance RPC was sent.",
                        window.TextEncoderMaintenanceArmGateStatus.Text);
                    AssertEx.Contains(
                        "Requires live mutation admission",
                        (string)window.ButtonArmTestReset.ToolTip);
                    AssertEx.Equal(
                        "Execute LMC Home Once",
                        (string)window.ButtonLmcHome.Content);
                    AssertEx.Equal(
                        "Step 1 - Arm Encoder Maintenance",
                        (string)window.ButtonArmTestReset.Content);
                    AssertEx.Equal(
                        "Open Safety / Recovery Details",
                        (string)window.ButtonOpenEncoderRecoveryDetails
                            .Content);
                    AssertEx.Equal(
                        "TEST ONLY - Encoder Maintenance (TW[20] / TW[19])",
                        (string)window.GroupEncoderMaintenance.Header);
                    AssertEx.Equal(
                        "Fixed writes: TW[20]=0x20FC:0x02 <- UInt16 1; TW[19]=0x20FC:0x01 <- UInt16 1",
                        window.TextEncoderMaintenanceFixedTargets.Text);
                    AssertEx.Equal(
                        "Step 1D: I independently verified that the selected drive supports this exact 0x20FC maintenance command.",
                        (string)window
                            .CheckEncoderMaintenanceCompatibilityVerified
                            .Content);
                    window.CheckTestResetPowerOffVerified.IsChecked = true;
                    window.CheckTestResetPhysicalPositionVerified.IsChecked =
                        true;
                    window.CheckTestResetExactTargetVerified.IsChecked = true;
                    AssertEx.False(
                        window.EncoderMaintenanceStepOneConfirmedForTests);
                    window.CheckEncoderMaintenanceCompatibilityVerified
                        .IsChecked = true;
                    AssertEx.True(
                        window.EncoderMaintenanceStepOneConfirmedForTests);
                    window.ComboTestResetAxis.SelectedItem = (ushort)2;
                    PumpUiOnce();
                    AssertEx.False(
                        window.EncoderMaintenanceStepOneConfirmedForTests);
                    window.TextTestResetTimeout.Text = "60001";
                    AssertEx.Throws<ArgumentOutOfRangeException>(
                        () => window.ReadEncoderMaintenanceRequest());
                    window.TextTestResetTimeout.Text = "60000";
                    window.ComboEncoderMaintenanceKind.SelectedItem =
                        LMCEncoderMaintenanceKind.Tw20ErrorWarningReset;
                    AssertEx.Equal(
                        60000U,
                        window.ReadEncoderMaintenanceRequest()
                            .TimeoutMilliseconds);
                    window.ComboEncoderMaintenanceKind.SelectedItem =
                        LMCEncoderMaintenanceKind
                            .Tw19MultiturnPositionReset;
                    var tw19 = window.ReadEncoderMaintenanceRequest();
                    AssertEx.True(
                        tw19 is LMCTw19MultiturnPositionResetRequest);
                    AssertEx.Equal(
                        LMCEncoderMaintenanceKind
                            .Tw19MultiturnPositionReset,
                        tw19.Kind);
                    AssertEx.Equal(
                        1u,
                        tw19.CommandValue);

                    window.ComboUiLanguage.SelectedIndex = 1;
                    PumpUiOnce();
                    AssertEx.Equal(
                        "LMC Home 1회 실행",
                        (string)window.ButtonLmcHome.Content);
                    AssertEx.Equal(
                        "1단계 - Encoder 유지보수 Arm",
                        (string)window.ButtonArmTestReset.Content);
                    AssertEx.Equal(
                        "안전 / 복구 상세 정보 열기",
                        (string)window.ButtonOpenEncoderRecoveryDetails
                            .Content);
                    AssertEx.Equal(
                        "차단: Encoder 유지보수를 arm하기 전에 PLC에 연결하십시오. Encoder 유지보수 RPC를 전송하지 않았습니다.",
                        window.TextEncoderMaintenanceArmGateStatus.Text);
                    AssertEx.Contains(
                        "Live mutation 승인",
                        (string)window.ButtonArmTestReset.ToolTip);
                    AssertEx.Equal(
                        "테스트 전용 - Encoder 유지보수 (TW[20] / TW[19])",
                        (string)window.GroupEncoderMaintenance.Header);
                    AssertEx.Equal(
                        "1단계 D: 선택한 drive가 이 정확한 0x20FC 유지보수 명령을 지원함을 독립적으로 확인했습니다.",
                        (string)window
                            .CheckEncoderMaintenanceCompatibilityVerified
                            .Content);
                    AssertEx.Contains(
                        "전송된 Home 동작이 없습니다.",
                        window.TextHomeResult.Text);
                });
        }

        private static void RestartRecordQuarantinesAllMaintenanceSends()
        {
            WithTemporaryWindow(
                LasalMotionControlApiExample.UiLanguage.Korean,
                root =>
                {
                    var journalDirectory = Path.Combine(
                        root,
                        "MaintenanceActionRecovery");
                    using (var journal =
                        LasalMotionControlApiExample
                            .MaintenanceActionRecoveryJournal.Open(
                                journalDirectory))
                    {
                        journal.ArmBeforeDispatch(
                            LasalMotionControlApiExample
                                .MaintenanceActionKind
                                .EncoderTw19MultiturnPositionReset,
                            "127.0.0.1",
                            4000,
                            0x01020304U,
                            0x11223344U,
                            0x55667788U,
                            "_LMCAxis2",
                            2,
                            1,
                            2,
                            3,
                            4,
                            0x12345678U,
                            "Schema=1;Semantic=Tw19MultiturnPositionReset;Kind=2;Profile=1;Drive=2;Socket=1;CommandValue=1;Object=0x20FC;Sub=0x01;Type=UInt16;TimeoutMilliseconds=1000;Evidence0=1;Evidence1=2;Evidence2=3;Evidence3=4",
                            DateTime.UtcNow);
                    }
                },
                window =>
                {
                    var record = window
                        .ActiveMaintenanceActionRecoveryRecordForTests;
                    AssertEx.NotNull(record);
                    AssertEx.Equal(
                        LasalMotionControlApiExample
                            .MaintenanceActionRecoveryState.RecoveryRequired,
                        record.State);
                    AssertEx.Equal(
                        LasalMotionControlApiExample
                            .MaintenanceActionKind
                            .EncoderTw19MultiturnPositionReset,
                        record.Action);
                    AssertEx.False(window.ButtonLmcHome.IsEnabled);
                    AssertEx.False(window.ButtonDs402Home.IsEnabled);
                    AssertEx.False(window.ButtonArmTestReset.IsEnabled);
                    AssertEx.False(window.ButtonExecuteTestReset.IsEnabled);
                    AssertEx.Contains(
                        "재전송 금지 복구 활성",
                        window.TextMaintenanceRecoveryStatus.Text);
                    AssertEx.Contains(
                        "EncoderTw19MultiturnPositionReset",
                        window.TextMaintenanceRecoveryStatus.Text);
                });
        }

        private static void EncoderMaintenanceIsAbsentFromGenericSdoTargets()
        {
            WithTemporaryWindow(
                LasalMotionControlApiExample.UiLanguage.English,
                null,
                window =>
                {
                    var exposed = window.ComboSdoWriteTarget.Items
                        .Cast<object>()
                        .OfType<LMCSdoWriteTarget>()
                        .Any(item => item.ObjectIndex == 0x20FC
                            && (item.SubIndex == 0x02
                                || item.SubIndex == 0x01));
                    AssertEx.False(
                        exposed,
                        "A dedicated TW[20]/TW[19] encoder-maintenance target leaked into the generic SDO target dropdown.");
                });
        }

        private static void
            EncoderPreparedRequestDisablesRearmAndEnablesConfirmedExecuteZeroWire()
        {
            var capabilities = LMCDiagnosticCapability
                    .EncoderTw20ErrorWarningReset
                | LMCDiagnosticCapability
                    .EncoderTw19MultiturnPositionReset;
            var steps = new[]
            {
                InitStep(),
                CallbackStep(),
                DiagnosticsCapabilitiesStep(1, capabilities),
                CloseStep()
            };

            using (var server = new FakeRpcServer(steps))
            {
                WithTemporaryWindow(
                    LasalMotionControlApiExample.UiLanguage.English,
                    null,
                    window =>
                    {
                        window.TextRemoteIp.Text = "127.0.0.1";
                        window.TextRemotePort.Text = server.Port.ToString();
                        using (var connection = new LMCConnection())
                        {
                            try
                            {
                                Connect(connection, server.Port);
                                var currentCapabilities = connection
                                    .Diagnostics.GetCapabilities();
                                SetPrivateField(
                                    window,
                                    "connection",
                                    connection);
                                SetPrivateField(
                                    window,
                                    "diagnosticCapabilities",
                                    currentCapabilities);

                                window.ComboEncoderMaintenanceKind
                                    .SelectedItem = LMCEncoderMaintenanceKind
                                        .Tw20ErrorWarningReset;
                                window.ComboTestResetAxis.SelectedItem =
                                    (ushort)1;
                                window.CheckTestResetPowerOffVerified
                                    .IsChecked = true;
                                window.CheckTestResetPhysicalPositionVerified
                                    .IsChecked = true;
                                window.CheckTestResetExactTargetVerified
                                    .IsChecked = true;
                                window
                                    .CheckEncoderMaintenanceCompatibilityVerified
                                    .IsChecked = true;
                                InvokePrivate(window, "UpdateUiState");
                                PumpUiOnce();

                                SetPrivateField(
                                    window,
                                    "axisCommandRecoveryJournalRuntimeError",
                                    "TEST unavailable Axis command journal");
                                InvokePrivate(window, "UpdateUiState");
                                AssertEx.False(
                                    window.ButtonArmTestReset.IsEnabled);
                                AssertEx.Contains(
                                    "unavailable journal",
                                    window
                                        .TextEncoderMaintenanceArmGateStatus
                                        .Text);
                                var axisJournalFailure = AssertEx.Throws<
                                    TargetInvocationException>(
                                    () => InvokePrivate(
                                        window,
                                        "EnsureMaintenanceActionCanStart",
                                        "TEST Encoder admission",
                                        LMCAdminFeature.None,
                                        LMCDiagnosticCapability
                                            .EncoderTw20ErrorWarningReset));
                                AssertEx.NotNull(axisJournalFailure.InnerException);
                                AssertEx.Contains(
                                    "Axis Stop/Reset journal",
                                    axisJournalFailure.InnerException.Message);
                                SetPrivateField(
                                    window,
                                    "axisCommandRecoveryJournalRuntimeError",
                                    null);

                                SetPrivateField(
                                    window,
                                    "motionUncertaintyJournalRuntimeError",
                                    "TEST unavailable motion journal");
                                InvokePrivate(window, "UpdateUiState");
                                AssertEx.False(
                                    window.ButtonArmTestReset.IsEnabled);
                                AssertEx.Contains(
                                    "unavailable journal",
                                    window
                                        .TextEncoderMaintenanceArmGateStatus
                                        .Text);
                                var motionJournalFailure = AssertEx.Throws<
                                    TargetInvocationException>(
                                    () => InvokePrivate(
                                        window,
                                        "EnsureMaintenanceActionCanStart",
                                        "TEST Encoder admission",
                                        LMCAdminFeature.None,
                                        LMCDiagnosticCapability
                                            .EncoderTw20ErrorWarningReset));
                                AssertEx.NotNull(
                                    motionJournalFailure.InnerException);
                                AssertEx.Contains(
                                    "motion journal",
                                    motionJournalFailure.InnerException.Message);
                                SetPrivateField(
                                    window,
                                    "motionUncertaintyJournalRuntimeError",
                                    null);

                                SetPrivateField(
                                    window,
                                    "groupProfileLockRecoveryRequired",
                                    true);
                                InvokePrivate(window, "UpdateUiState");
                                AssertEx.False(
                                    window.ButtonArmTestReset.IsEnabled);
                                AssertEx.Contains(
                                    "another unresolved mutation, recovery",
                                    window
                                        .TextEncoderMaintenanceArmGateStatus
                                        .Text);
                                var groupProfileFailure = AssertEx.Throws<
                                    TargetInvocationException>(
                                    () => InvokePrivate(
                                        window,
                                        "EnsureMaintenanceActionCanStart",
                                        "TEST Encoder admission",
                                        LMCAdminFeature.None,
                                        LMCDiagnosticCapability
                                            .EncoderTw20ErrorWarningReset));
                                AssertEx.NotNull(
                                    groupProfileFailure.InnerException);
                                AssertEx.Contains(
                                    "another unresolved mutation",
                                    groupProfileFailure.InnerException.Message);
                                SetPrivateField(
                                    window,
                                    "groupProfileLockRecoveryRequired",
                                    false);

                                SetPrivateField(
                                    window,
                                    "diagnosticCapabilities",
                                    null);
                                InvokePrivate(window, "UpdateUiState");
                                AssertEx.False(
                                    window.ButtonArmTestReset.IsEnabled);
                                AssertEx.Contains(
                                    "refresh current-session Diagnostics capabilities and identity",
                                    window
                                        .TextEncoderMaintenanceArmGateStatus
                                        .Text);
                                var capabilityFailure = AssertEx.Throws<
                                    TargetInvocationException>(
                                    () => InvokePrivate(
                                        window,
                                        "EnsureMaintenanceActionCanStart",
                                        "TEST Encoder admission",
                                        LMCAdminFeature.None,
                                        LMCDiagnosticCapability
                                            .EncoderTw20ErrorWarningReset));
                                AssertEx.NotNull(capabilityFailure.InnerException);
                                AssertEx.Contains(
                                    "Diagnostics capability/identity",
                                    capabilityFailure.InnerException.Message);
                                SetPrivateField(
                                    window,
                                    "diagnosticCapabilities",
                                    currentCapabilities);
                                InvokePrivate(window, "UpdateUiState");
                                PumpUiOnce();

                                AssertEx.True(
                                    window
                                        .EncoderMaintenanceStepOneConfirmedForTests);
                                AssertEx.True(
                                    window.ButtonArmTestReset.IsEnabled);
                                AssertEx.False(
                                    window.ButtonExecuteTestReset.IsEnabled);
                                AssertEx.Contains(
                                    "READY: current-session capability, recovery, and all Step 1 gates are open",
                                    window
                                        .TextEncoderMaintenanceArmGateStatus
                                        .Text);

                                var prepared = connection.Diagnostics
                                    .PrepareTw20EncoderErrorWarningReset(
                                        (LMCTw20EncoderErrorWarningResetRequest)
                                            window
                                                .ReadEncoderMaintenanceRequest(),
                                        currentCapabilities,
                                        LMCTw20EncoderErrorWarningResetExecuteToken
                                            .Create());
                                SetPrivateField(
                                    window,
                                    "armedEncoderMaintenance",
                                    prepared);
                                SetPrivateField(
                                    window,
                                    "armedEncoderMaintenanceFingerprint",
                                    LasalMotionControlApiExample.MainWindow
                                        .FormatEncoderMaintenanceIdentity(
                                            prepared.RecoveryKey));
                                InvokePrivate(window, "UpdateUiState");
                                PumpUiOnce();

                                AssertEx.False(
                                    window.ButtonArmTestReset.IsEnabled);
                                AssertEx.True(
                                    window.CheckTestResetFinalConfirmed
                                        .IsEnabled);
                                AssertEx.False(
                                    window.ButtonExecuteTestReset.IsEnabled);
                                AssertEx.Contains(
                                    "ARMED: the exact encoder-maintenance request is held in PC memory only",
                                    window
                                        .TextEncoderMaintenanceArmGateStatus
                                        .Text);

                                var requestCountBeforeRearm =
                                    server.ReceivedRequests.Count;
                                Click(window.ButtonArmTestReset);
                                PumpUiOnce();
                                AssertEx.Equal(
                                    requestCountBeforeRearm,
                                    server.ReceivedRequests.Count);
                                AssertEx.True(
                                    ReferenceEquals(
                                        prepared,
                                        GetPrivateField(
                                            window,
                                            "armedEncoderMaintenance")));
                                AssertEx.Contains(
                                    "already armed in PC memory",
                                    window.TextTestResetResult.Text);
                                AssertEx.Equal(
                                    0,
                                    CountCommand(server, 0x7E53));

                                window.CheckTestResetFinalConfirmed.IsChecked =
                                    true;
                                PumpUiOnce();
                                AssertEx.True(
                                    window.ButtonExecuteTestReset.IsEnabled);
                                AssertEx.Equal(
                                    0,
                                    CountCommand(server, 0x7E53));

                                window.CheckTestResetFinalConfirmed.IsChecked =
                                    false;
                                SetPrivateField(
                                    window,
                                    "armedEncoderMaintenance",
                                    null);
                                SetPrivateField(
                                    window,
                                    "armedEncoderMaintenanceFingerprint",
                                    null);
                                InvokePrivate(window, "UpdateUiState");
                                connection.CloseConnection();
                            }
                            finally
                            {
                                SetPrivateField(
                                    window,
                                    "axisCommandRecoveryJournalRuntimeError",
                                    null);
                                SetPrivateField(
                                    window,
                                    "motionUncertaintyJournalRuntimeError",
                                    null);
                                SetPrivateField(
                                    window,
                                    "groupProfileLockRecoveryRequired",
                                    false);
                                SetPrivateField(
                                    window,
                                    "groupProfileLockVerificationPending",
                                    false);
                                SetPrivateField(
                                    window,
                                    "groupProfileUnlockVerificationPending",
                                    false);
                                SetPrivateField(
                                    window,
                                    "armedEncoderMaintenance",
                                    null);
                                SetPrivateField(
                                    window,
                                    "armedEncoderMaintenanceFingerprint",
                                    null);
                                SetPrivateField(
                                    window,
                                    "connection",
                                    null);
                                InvokePrivate(window, "UpdateUiState");
                            }
                        }
                    });

                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x7E00));
                AssertEx.Equal(0, CountCommand(server, 0x7E53));
                AssertEx.Equal(1, CountCommand(server, 0x405D));
            }
        }

        private static void
            LmcRecoveryKeyRoundTripsWithCurrentZeroIntent()
        {
            var identity = new LMCHomeRecoveryKey(
                LMCAdmin.ProtocolSchemaVersion,
                0x12345678U,
                0x01020304U,
                0x11223344U,
                0x55667788U,
                1,
                2,
                3,
                4,
                3,
                -25,
                1000);
            var now = DateTime.UtcNow;
            var record = new LasalMotionControlApiExample
                .MaintenanceActionRecoveryRecord(
                    Guid.NewGuid(),
                    LasalMotionControlApiExample
                        .MaintenanceActionKind.LmcHome,
                    "127.0.0.1",
                    4000,
                    0x01020304U,
                    0x11223344U,
                    0x55667788U,
                    "_LMCAxis3",
                    3,
                    identity.ClientIntentId0,
                    identity.ClientIntentId1,
                    identity.ClientIntentId2,
                    identity.ClientIntentId3,
                    identity.OriginalRequestId,
                    LasalMotionControlApiExample.MainWindow
                        .FormatLmcHomeIdentity(identity),
                    LasalMotionControlApiExample
                        .MaintenanceActionRecoveryState
                        .ArmedBeforeDispatch,
                    now,
                    now);

            AssertEx.True(record.HasAnyClientIntent);
            var recreated = LasalMotionControlApiExample.MainWindow
                .RecreateLmcHomeRecoveryKey(record);
            AssertEx.True(identity.Equals(recreated));
            AssertEx.Equal(
                LMCHomeSemanticMode.CurrentPositionZero,
                recreated.SemanticMode);
            AssertEx.Equal(0, recreated.TargetPosition);
            AssertEx.Equal(
                0x01020304U,
                record.ObservedDiagnosticsBuild);
        }

        private static void EncoderMaintenanceRecoveryKeyRoundTrips()
        {
            var identity = new LMCEncoderMaintenanceRecoveryKey(
                LMCDiagnostics.ProtocolSchemaVersion,
                0x12345678U,
                0x01020304U,
                0x11223344U,
                0x55667788U,
                new LMCEncoderMaintenanceClientIntentId(1, 2, 3, 4),
                LMCEncoderMaintenanceKind.Tw19MultiturnPositionReset,
                7,
                3,
                LMCEncoderFeedbackSocket.Socket2,
                1000,
                new LMCEncoderMaintenanceCompatibilityEvidenceId(
                    5,
                    6,
                    7,
                    8));
            var now = DateTime.UtcNow;
            var record = new LasalMotionControlApiExample
                .MaintenanceActionRecoveryRecord(
                    Guid.NewGuid(),
                    LasalMotionControlApiExample.MaintenanceActionKind
                        .EncoderTw19MultiturnPositionReset,
                    "127.0.0.1",
                    4000,
                    identity.DiagnosticsBuild,
                    identity.DiagnosticsBootId,
                    identity.MapRevision,
                    "_LMCAxis3",
                    3,
                    identity.ClientIntentId.Word0,
                    identity.ClientIntentId.Word1,
                    identity.ClientIntentId.Word2,
                    identity.ClientIntentId.Word3,
                    identity.OriginalRequestId,
                    LasalMotionControlApiExample.MainWindow
                        .FormatEncoderMaintenanceIdentity(identity),
                    LasalMotionControlApiExample
                        .MaintenanceActionRecoveryState
                        .ArmedBeforeDispatch,
                    now,
                    now);

            var recreated = LasalMotionControlApiExample.MainWindow
                .RecreateEncoderMaintenanceRecoveryKey(record);
            AssertEx.True(identity.Equals(recreated));
            AssertEx.Equal(
                LMCEncoderMaintenanceKind.Tw19MultiturnPositionReset,
                recreated.Kind);
            AssertEx.Equal(1U, recreated.CommandValue);
            AssertEx.Equal(
                7U,
                recreated.CompatibilityEvidenceId.Word2);
        }

        private static void Ds402HomeUsesFixedMethod37ZeroOffset()
        {
            WithTemporaryWindow(
                LasalMotionControlApiExample.UiLanguage.English,
                null,
                window =>
                {
                    AssertEx.Equal(1, window.ComboDs402HomeMethod.Items.Count);
                    AssertEx.Equal(
                        LMCAxisDs402HomeParameters
                            .CurrentPositionZeroHomingMethod,
                        (int)window.ComboDs402HomeMethod.SelectedItem);
                    var parameters = window.ReadDs402HomeParameters();
                    AssertEx.Equal(
                        LMCAxisDs402HomeParameters
                            .CurrentPositionZeroHomingMethod,
                        parameters.HomingMethod);
                    AssertEx.Equal(
                        LMCAxisDs402HomeParameters
                            .CurrentPositionZeroHomeOffset,
                        parameters.Position);
                    AssertEx.Equal(0, parameters.Velocity);
                    AssertEx.Equal(0, parameters.Acceleration);
                    AssertEx.Equal(0, parameters.DistanceLimit);
                    AssertEx.Equal(0, parameters.TorqueLimit);

                    window.TextLmcHomeTimeout.Text = "100";
                    var lmcParameters = window.ReadLmcHomeParameters(-123);
                    AssertEx.Equal(-123, lmcParameters.ExpectedActualPosition);
                    AssertEx.Equal(0, lmcParameters.TargetPosition);
                    AssertEx.Equal(
                        LMCHomeSemanticMode.CurrentPositionZero,
                        lmcParameters.SemanticMode);
                    window.TextLmcHomeTimeout.Text = "99";
                    AssertEx.Throws<ArgumentOutOfRangeException>(
                        () => window.ReadLmcHomeParameters(-123));
                });
        }

        private static void
            Ds402HomeRestartRestoresRecoveryKeyImmediately()
        {
            var expected = CreateDs402HomeRecoveryKey();
            WithTemporaryWindow(
                LasalMotionControlApiExample.UiLanguage.English,
                root =>
                {
                    var journalDirectory = Path.Combine(
                        root,
                        "MaintenanceActionRecovery");
                    using (var journal =
                        LasalMotionControlApiExample
                            .MaintenanceActionRecoveryJournal.Open(
                                journalDirectory))
                    {
                        journal.ArmBeforeDispatch(
                            LasalMotionControlApiExample
                                .MaintenanceActionKind.Ds402Home,
                            "127.0.0.1",
                            4000,
                            expected.DiagnosticsBuild,
                            expected.DiagnosticsBootId,
                            expected.MapRevision,
                            "_LMCAxis2",
                            expected.AxisReference,
                            expected.ClientIntentId.Word0,
                            expected.ClientIntentId.Word1,
                            expected.ClientIntentId.Word2,
                            expected.ClientIntentId.Word3,
                            expected.RequestId,
                            "Schema=1;Method=37;HomeOffset=0;Velocity=0;Acceleration=0;DistanceLimit=0;TorqueLimit=0;BufferMode=Aborting;TimeoutMs=60000",
                            DateTime.UtcNow);
                    }
                },
                window =>
                {
                    var record = window
                        .ActiveMaintenanceActionRecoveryRecordForTests;
                    AssertEx.NotNull(record);
                    AssertEx.Equal(
                        LasalMotionControlApiExample
                            .MaintenanceActionRecoveryState.RecoveryRequired,
                        record.State);
                    AssertEx.Equal(
                        LasalMotionControlApiExample
                            .MaintenanceActionKind.Ds402Home,
                        record.Action);
                    var restored = GetPrivateField(
                        window,
                        "latestDs402HomeRecoveryKey")
                        as LMCAxisDs402HomeRecoveryKey;
                    AssertEx.NotNull(restored);
                    AssertEx.Equal(expected.SchemaVersion, restored.SchemaVersion);
                    AssertEx.Equal(expected.RequestId, restored.RequestId);
                    AssertEx.Equal(
                        expected.DiagnosticsBuild,
                        restored.DiagnosticsBuild);
                    AssertEx.Equal(
                        expected.DiagnosticsBootId,
                        restored.DiagnosticsBootId);
                    AssertEx.Equal(expected.MapRevision, restored.MapRevision);
                    AssertEx.Equal(
                        expected.ClientIntentId.Word0,
                        restored.ClientIntentId.Word0);
                    AssertEx.Equal(
                        expected.ClientIntentId.Word1,
                        restored.ClientIntentId.Word1);
                    AssertEx.Equal(
                        expected.ClientIntentId.Word2,
                        restored.ClientIntentId.Word2);
                    AssertEx.Equal(
                        expected.ClientIntentId.Word3,
                        restored.ClientIntentId.Word3);
                    AssertEx.Equal(
                        expected.AxisReference,
                        restored.AxisReference);
                    AssertEx.Equal(
                        expected.Parameters.HomingMethod,
                        restored.Parameters.HomingMethod);
                    AssertEx.Equal(
                        expected.Parameters.Position,
                        restored.Parameters.Position);
                    AssertEx.Equal(
                        expected.Parameters.Velocity,
                        restored.Parameters.Velocity);
                    AssertEx.Equal(
                        expected.Parameters.Acceleration,
                        restored.Parameters.Acceleration);
                    AssertEx.Equal(
                        expected.Parameters.DistanceLimit,
                        restored.Parameters.DistanceLimit);
                    AssertEx.Equal(
                        expected.Parameters.TorqueLimit,
                        restored.Parameters.TorqueLimit);
                    AssertEx.Equal(
                        expected.Parameters.BufferMode,
                        restored.Parameters.BufferMode);
                    AssertEx.Equal(
                        expected.Parameters.TimeoutMilliseconds,
                        restored.Parameters.TimeoutMilliseconds);
                });
        }

        private static void
            LmcHomeButtonTerminalRetiresBeforeJournalResolveAndLogsOutcome()
        {
            byte[] startRequest = null;
            var retirementRequestObserved =
                new TaskCompletionSource<bool>();
            var startStep = LmcHomeStartStep(
                request => startRequest = (byte[])request.Clone());
            var outcomeStep = LmcHomeOutcomeStep(
                0x7D18,
                () => startRequest,
                null);
            var retirementStep = LmcHomeOutcomeStep(
                0x7D19,
                () => startRequest,
                () => retirementRequestObserved.TrySetResult(true));
            retirementStep.ResponseDelayMilliseconds = 500;

            var steps = new[]
            {
                InitStep(),
                CallbackStep(),
                AxisLookupStep(2),
                AxisInfoStep(2),
                LmcDiagnosticsCapabilitiesStep(),
                LmcAdminCapabilitiesStep(),
                LmcDiagnosticsCapabilitiesStep(),
                LmcAdminCapabilitiesStep(),
                LmcHomeActualPositionStep(),
                startStep,
                LmcDiagnosticsCapabilitiesStep(),
                LmcAdminCapabilitiesStep(),
                outcomeStep,
                retirementStep,
                CloseStep()
            };
            var journalDirectory = string.Empty;

            using (var server = new FakeRpcServer(steps))
            {
                WithTemporaryWindow(
                    LasalMotionControlApiExample.UiLanguage.English,
                    root => journalDirectory = Path.Combine(
                        root,
                        "MaintenanceActionRecovery"),
                    window =>
                    {
                        window.TextRemoteIp.Text = "127.0.0.1";
                        window.TextRemotePort.Text = server.Port.ToString();
                        window.TextAxisName.Text = "_LMCAxis2";
                        window.TextLmcHomeTimeout.Text = "5000";

                        using (var connection = new LMCConnection())
                        {
                            try
                            {
                                Connect(connection, server.Port);
                                var axis = new LMCSingleAxis(
                                    connection,
                                    "_LMCAxis2");
                                SetPrivateField(
                                    window,
                                    "connection",
                                    connection);
                                SetPrivateField(window, "axis", axis);

                                var refresh = (Task)InvokePrivate(
                                    window,
                                    "RefreshMaintenanceCapabilitiesAsync",
                                    connection);
                                WaitForTask(
                                    refresh,
                                    "The initial LMC Home capability refresh did not complete.");
                                AssertEx.True(
                                    CaptureTaskFailure(refresh) == null);
                                InvokePrivate(window, "UpdateUiState");

                                window.CheckHomeOneShotConfirmed.IsChecked =
                                    true;
                                PumpUiOnce();
                                AssertEx.True(window.ButtonLmcHome.IsEnabled);
                                Click(window.ButtonLmcHome);
                                WaitForUiCondition(
                                    () => window.TextOperationState.Text
                                        == "LMC Home Start accepted; outcome pending",
                                    "The LMC Home button did not expose the accepted-start/pending-outcome boundary.");
                                AssertEx.NotNull(
                                    startRequest,
                                    "The fake server rejected the 0x7D13 request before constructing its acknowledgement. "
                                        + window.TextExecutionLog.Text);
                                AssertEx.Equal(
                                    "LMC Home Start accepted; outcome pending",
                                    window.TextOperationState.Text,
                                    window.TextExecutionLog.Text
                                        + " 0x7D13="
                                        + TestFrame.ToHex(startRequest));
                                AssertEx.Contains(
                                    "LMC Home Start (Current Position Zero) PASS.",
                                    window.TextExecutionLog.Text);
                                AssertEx.Contains(
                                    "LMC Home outcome pending. Use Read Home Status for exact completion proof.",
                                    window.TextExecutionLog.Text);
                                AssertEx.False(
                                    window.TextExecutionLog.Text.Contains(
                                        "LMC Home (Current Position Zero) PASS."));
                                AssertEx.NotNull(
                                    window
                                        .ActiveMaintenanceActionRecoveryRecordForTests,
                                    window.TextExecutionLog.Text);

                                var recovery = window
                                    .ActiveMaintenanceActionRecoveryRecordForTests;
                                AssertEx.Equal(
                                    LasalMotionControlApiExample
                                        .MaintenanceActionKind.LmcHome,
                                    recovery.Action);
                                AssertEx.Equal(
                                    LasalMotionControlApiExample
                                        .MaintenanceActionRecoveryState
                                        .RecoveryRequired,
                                    recovery.State);
                                AssertEx.NotNull(startRequest);
                                AssertEx.Equal(
                                    TestFrame.ReadUInt32(startRequest, 12),
                                    recovery.TransportCorrelationId);
                                AssertEx.Equal(1, CountCommand(server, 0x7D13));
                                AssertEx.Equal(0, CountCommand(server, 0x7D18));
                                AssertEx.Equal(0, CountCommand(server, 0x7D19));

                                AssertEx.True(
                                    window.ButtonReadHomeStatus.IsEnabled);
                                Click(window.ButtonReadHomeStatus);
                                WaitForUiCondition(
                                    () => retirementRequestObserved
                                            .Task.IsCompleted
                                        || string.Equals(
                                            window.TextOperationState.Text,
                                            "Read Home Status failed",
                                            StringComparison.Ordinal),
                                    "The exact Home outcome operation did not reach retirement or fail closed.");
                                AssertEx.True(
                                    retirementRequestObserved.Task.IsCompleted,
                                    "The exact 0x7D19 retirement request was not observed. "
                                        + window.TextExecutionLog.Text
                                        + " LastRequest="
                                        + TestFrame.ToHex(
                                            server.ReceivedRequests.Last()));
                                AssertEx.NotNull(
                                    window
                                        .ActiveMaintenanceActionRecoveryRecordForTests);

                                WaitForUiCondition(
                                    () => string.Equals(
                                            window.TextOperationState.Text,
                                            "Read Home Status completed",
                                            StringComparison.Ordinal)
                                        && window
                                            .ActiveMaintenanceActionRecoveryRecordForTests
                                            == null,
                                    "The matching terminal 0x7D19 response did not resolve the LMC Home journal.");
                                AssertEx.Contains(
                                    "Exact terminal 0x7D18 outcome and matching 0x7D19 retirement verified",
                                    window.TextHomeResult.Text);
                                AssertEx.Contains(
                                    "LMC Home outcome: RecordState=Succeeded; HomeSucceeded=True; OriginalStatus=0; OriginalErrorId=0; OriginalDetail=0 (None); AxisStatus=0x02000000; AxisError=0; RawDriveBefore=-123456; RawDriveAfter=-123456; ActualApplicationAfter=0; SetApplicationAfter=0; ActualInternalAfter=0; SetInternalAfter=0; DestinationInternalAfter=0; MasterInternalAfter=0; NativeCommandState=0; EvidenceFlags=0x0000003B; StopState=0x00000000 (0); RuntimePhase=7; RecordGeneration=17.",
                                    window.TextExecutionLog.Text);

                                AssertLmcHomeWireSequence(server);
                                connection.CloseConnection();
                            }
                            finally
                            {
                                SetPrivateField(window, "axis", null);
                                SetPrivateField(
                                    window,
                                    "connection",
                                    null);
                            }
                        }

                        window.Close();
                        WaitForUiCondition(
                            () => !window.IsLoaded,
                            "The LMC Home integration window did not close.");
                        using (var reopened = LasalMotionControlApiExample
                            .MaintenanceActionRecoveryJournal.Open(
                                journalDirectory))
                        {
                            AssertEx.False(reopened.HasActiveRecord);
                        }
                    });

                server.Verify();
                AssertLmcHomeWireSequence(server);
            }
        }

        private static void
            Ds402HomeTerminalRetiresBeforeJournalResolve()
        {
            AssertDs402HomeRetirementScenario(
                Ds402RetirementScenario.Success);
        }

        private static void
            Ds402HomeRetirementTimeoutRetainsJournalZeroReplay()
        {
            AssertDs402HomeRetirementScenario(
                Ds402RetirementScenario.Timeout);
        }

        private static void
            Ds402HomeRetirementResponseLossRetainsJournalZeroReplay()
        {
            AssertDs402HomeRetirementScenario(
                Ds402RetirementScenario.ResponseLoss);
        }

        private static void
            Ds402HomeRetirementMismatchRetainsJournalZeroReplay()
        {
            AssertDs402HomeRetirementScenario(
                Ds402RetirementScenario.KeyMismatch);
        }

        private static void
            Ds402HomeRetirementMalformedRetainsJournalZeroReplay()
        {
            AssertDs402HomeRetirementScenario(
                Ds402RetirementScenario.Malformed);
        }

        private static void
            Ds402HomeRetirementResultMismatchRetainsJournalZeroReplay()
        {
            AssertDs402HomeRetirementScenario(
                Ds402RetirementScenario.SnapshotResultMismatch);
        }

        private static void
            Ds402HomeRetirementEvidenceMismatchRetainsJournalZeroReplay()
        {
            AssertDs402HomeRetirementScenario(
                Ds402RetirementScenario.SnapshotEvidenceMismatch);
        }

        private static void
            Ds402HomeRetirementResponseLossReconnectCompletesZeroReplay()
        {
            var key = CreateDs402HomeRecoveryKey();
            var lostRetirement = RetirementStep(new byte[0], key);
            lostRetirement.CloseClientBeforeResponseAndContinue = true;
            var retryRetirement = RetirementStep(
                TestFrame.Response(
                    0,
                    Ds402HomeOutcomePayload(
                        3,
                        key,
                        Ds402RecordGeneration)),
                key);
            var steps = new[]
            {
                InitStep(),
                CallbackStep(),
                AxisLookupStep(2),
                AxisInfoStep(2),
                DiagnosticsCapabilitiesStep(1),
                AdminCapabilitiesStep(1),
                new FakeRpcStep(
                    0x7D16,
                    TestFrame.Response(
                        0,
                        Ds402HomeOutcomePayload(
                            2,
                            key,
                            Ds402RecordGeneration))),
                lostRetirement,
                InitStep(),
                CallbackStep(),
                AxisLookupStep(2),
                AxisInfoStep(2),
                DiagnosticsCapabilitiesStep(1),
                AdminCapabilitiesStep(1),
                new FakeRpcStep(
                    0x7D16,
                    TestFrame.Response(
                        0,
                        Ds402HomeOutcomePayload(
                            2,
                            key,
                            Ds402RecordGeneration))),
                retryRetirement,
                CloseStep()
            };

            using (var server = new FakeRpcServer(steps))
            {
                var root = Path.Combine(
                    Path.GetTempPath(),
                    "ElmoMaintenanceUiTests",
                    Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(root);
                LasalMotionControlApiExample.MainWindow firstWindow = null;
                LasalMotionControlApiExample.MainWindow retryWindow = null;
                try
                {
                    LasalMotionControlApiExample.UiLanguagePreferenceStore.Save(
                        Path.Combine(
                            root,
                            "UiLanguage",
                            "ui-language.txt"),
                        LasalMotionControlApiExample.UiLanguage.English);
                    var journalDirectory = Path.Combine(
                        root,
                        "MaintenanceActionRecovery");
                    Guid durableIdentity;
                    using (var journal = LasalMotionControlApiExample
                        .MaintenanceActionRecoveryJournal.Open(
                            journalDirectory))
                    {
                        var armed = journal.ArmBeforeDispatch(
                            LasalMotionControlApiExample
                                .MaintenanceActionKind.Ds402Home,
                            "127.0.0.1",
                            server.Port,
                            key.DiagnosticsBuild,
                            key.DiagnosticsBootId,
                            key.MapRevision,
                            "_LMCAxis2",
                            2,
                            key.ClientIntentId.Word0,
                            key.ClientIntentId.Word1,
                            key.ClientIntentId.Word2,
                            key.ClientIntentId.Word3,
                            key.RequestId,
                            "Schema=1;Method=37;HomeOffset=0;Velocity=0;Acceleration=0;DistanceLimit=0;TorqueLimit=0;BufferMode=Aborting;TimeoutMs=60000",
                            DateTime.UtcNow);
                        durableIdentity = journal.PromoteToRecoveryRequired(
                            armed,
                            key.RequestId,
                            armed.UpdatedUtc.AddTicks(1)).Identity;
                    }

                    firstWindow = CreateHiddenWindow(root);
                    var firstRecovery = firstWindow
                        .ActiveMaintenanceActionRecoveryRecordForTests;
                    AssertEx.NotNull(firstRecovery);
                    AssertEx.Equal(durableIdentity, firstRecovery.Identity);
                    using (var firstConnection = new LMCConnection())
                    {
                        try
                        {
                            Connect(firstConnection, server.Port);
                            var firstAxis = new LMCSingleAxis(
                                firstConnection,
                                "_LMCAxis2");
                            SetPrivateField(
                                firstWindow,
                                "connection",
                                firstConnection);
                            var firstCompletion = (Task)InvokePrivate(
                                firstWindow,
                                "ReadExactDs402HomeOutcomeAsync",
                                firstAxis,
                                firstRecovery);
                            WaitForTask(
                                firstCompletion,
                                "The response-loss retirement attempt did not complete.");
                            AssertEx.True(
                                CaptureTaskFailure(firstCompletion)
                                    is IOException);
                            AssertEx.Equal(
                                LMCConnectionState.Faulted,
                                firstConnection.State);
                            AssertEx.Equal(
                                durableIdentity,
                                firstWindow
                                    .ActiveMaintenanceActionRecoveryRecordForTests
                                    .Identity);
                        }
                        finally
                        {
                            SetPrivateField(
                                firstWindow,
                                "connection",
                                null);
                        }
                    }

                    firstWindow.Close();
                    WaitForUiCondition(
                        () => !firstWindow.IsLoaded,
                        "The response-loss window did not close.");

                    retryWindow = CreateHiddenWindow(root);
                    var retryRecovery = retryWindow
                        .ActiveMaintenanceActionRecoveryRecordForTests;
                    AssertEx.NotNull(retryRecovery);
                    AssertEx.Equal(durableIdentity, retryRecovery.Identity);
                    using (var retryConnection = new LMCConnection())
                    {
                        try
                        {
                            Connect(retryConnection, server.Port);
                            var retryAxis = new LMCSingleAxis(
                                retryConnection,
                                "_LMCAxis2");
                            SetPrivateField(
                                retryWindow,
                                "connection",
                                retryConnection);
                            var retryCompletion = (Task)InvokePrivate(
                                retryWindow,
                                "ReadExactDs402HomeOutcomeAsync",
                                retryAxis,
                                retryRecovery);
                            WaitForTask(
                                retryCompletion,
                                "The tombstone retirement retry did not complete.");
                            AssertEx.True(
                                CaptureTaskFailure(retryCompletion) == null);
                            AssertEx.True(retryWindow
                                .ActiveMaintenanceActionRecoveryRecordForTests
                                == null);
                        }
                        finally
                        {
                            if (retryConnection.State
                                == LMCConnectionState.Connected)
                            {
                                retryConnection.CloseConnection();
                            }

                            SetPrivateField(
                                retryWindow,
                                "connection",
                                null);
                        }
                    }

                    retryWindow.Close();
                    WaitForUiCondition(
                        () => !retryWindow.IsLoaded,
                        "The tombstone retry window did not close.");
                    using (var reopened = LasalMotionControlApiExample
                        .MaintenanceActionRecoveryJournal.Open(
                            journalDirectory))
                    {
                        AssertEx.False(reopened.HasActiveRecord);
                    }

                    server.Verify();
                    var homeCommands = server.ReceivedRequests
                        .Select(request => TestFrame.ReadUInt16(request, 0))
                        .Where(command => command == 0x7D15
                            || command == 0x7D16
                            || command == 0x7D17)
                        .ToArray();
                    AssertEx.Equal(4, homeCommands.Length);
                    AssertEx.Equal((ushort)0x7D16, homeCommands[0]);
                    AssertEx.Equal((ushort)0x7D17, homeCommands[1]);
                    AssertEx.Equal((ushort)0x7D16, homeCommands[2]);
                    AssertEx.Equal((ushort)0x7D17, homeCommands[3]);
                    AssertEx.Equal(0, CountCommand(server, 0x7D15));
                }
                finally
                {
                    if (retryWindow != null && retryWindow.IsLoaded)
                    {
                        retryWindow.Close();
                    }

                    if (firstWindow != null && firstWindow.IsLoaded)
                    {
                        firstWindow.Close();
                    }

                    if (Directory.Exists(root))
                    {
                        Directory.Delete(root, true);
                    }
                }
            }
        }

        private static void AssertDs402HomeRetirementScenario(
            Ds402RetirementScenario scenario)
        {
            var key = CreateDs402HomeRecoveryKey();
            var steps = CreateDs402HomeRetirementSteps(scenario, key);
            var journalDirectory = string.Empty;
            var durableIdentity = Guid.Empty;
            Exception observedFailure = null;
            var observedConnectionState = LMCConnectionState.Disconnected;

            using (var server = new FakeRpcServer(steps))
            {
                WithTemporaryWindow(
                    LasalMotionControlApiExample.UiLanguage.English,
                    root =>
                    {
                        journalDirectory = Path.Combine(
                            root,
                            "MaintenanceActionRecovery");
                        using (var journal = LasalMotionControlApiExample
                            .MaintenanceActionRecoveryJournal.Open(
                                journalDirectory))
                        {
                            var armed = journal.ArmBeforeDispatch(
                                LasalMotionControlApiExample
                                    .MaintenanceActionKind.Ds402Home,
                                "127.0.0.1",
                                server.Port,
                                key.DiagnosticsBuild,
                                key.DiagnosticsBootId,
                                key.MapRevision,
                                "_LMCAxis2",
                                2,
                                key.ClientIntentId.Word0,
                                key.ClientIntentId.Word1,
                                key.ClientIntentId.Word2,
                                key.ClientIntentId.Word3,
                                key.RequestId,
                                "Schema=1;Method=37;HomeOffset=0;Velocity=0;Acceleration=0;DistanceLimit=0;TorqueLimit=0;BufferMode=Aborting;TimeoutMs=60000",
                                DateTime.UtcNow);
                            var recovery = journal.PromoteToRecoveryRequired(
                                armed,
                                key.RequestId,
                                armed.UpdatedUtc.AddTicks(1));
                            durableIdentity = recovery.Identity;
                        }
                    },
                    window =>
                    {
                        var recovery = window
                            .ActiveMaintenanceActionRecoveryRecordForTests;
                        AssertEx.NotNull(recovery);
                        AssertEx.Equal(durableIdentity, recovery.Identity);

                        var options = new LMCConnectionOptions();
                        if (scenario == Ds402RetirementScenario.Timeout)
                        {
                            options.ReceiveTimeoutMilliseconds = 100;
                        }

                        using (var connection = new LMCConnection(options))
                        {
                            try
                            {
                                Connect(connection, server.Port);
                                var axis = new LMCSingleAxis(
                                    connection,
                                    "_LMCAxis2");
                                SetPrivateField(
                                    window,
                                    "connection",
                                    connection);

                                var completion = (Task)InvokePrivate(
                                    window,
                                    "ReadExactDs402HomeOutcomeAsync",
                                    axis,
                                    recovery);
                                WaitForTask(
                                    completion,
                                    "The DS402 Home query/retire flow did not complete.");
                                observedFailure = CaptureTaskFailure(completion);
                                observedConnectionState = connection.State;

                                if (scenario
                                    == Ds402RetirementScenario.Success)
                                {
                                    AssertEx.True(observedFailure == null);
                                    AssertEx.True(window
                                        .ActiveMaintenanceActionRecoveryRecordForTests
                                        == null);
                                    AssertEx.Contains(
                                        "0x7D17 retirement verified",
                                        window.TextHomeResult.Text);
                                }
                                else
                                {
                                    AssertEx.NotNull(observedFailure);
                                    var retained = window
                                        .ActiveMaintenanceActionRecoveryRecordForTests;
                                    AssertEx.NotNull(retained);
                                    AssertEx.Equal(
                                        durableIdentity,
                                        retained.Identity);
                                    AssertEx.Equal(
                                        LasalMotionControlApiExample
                                            .MaintenanceActionRecoveryState
                                            .RecoveryRequired,
                                        retained.State);
                                }
                            }
                            finally
                            {
                                if (connection.State
                                    == LMCConnectionState.Connected)
                                {
                                    connection.CloseConnection();
                                }

                                SetPrivateField(window, "connection", null);
                            }
                        }

                        window.Close();
                        WaitForUiCondition(
                            () => !window.IsLoaded,
                            "The DS402 Home retirement test window did not close.");
                        using (var reopened = LasalMotionControlApiExample
                            .MaintenanceActionRecoveryJournal.Open(
                                journalDirectory))
                        {
                            if (scenario
                                == Ds402RetirementScenario.Success)
                            {
                                AssertEx.False(reopened.HasActiveRecord);
                            }
                            else
                            {
                                AssertEx.True(reopened.HasActiveRecord);
                                AssertEx.Equal(
                                    durableIdentity,
                                    reopened.CurrentRecord.Identity);
                            }
                        }
                    });

                server.Verify();
                AssertDs402HomeWireSequence(server);
            }

            AssertDs402HomeRetirementFailure(
                scenario,
                observedFailure,
                observedConnectionState);
        }

        private static FakeRpcStep[] CreateDs402HomeRetirementSteps(
            Ds402RetirementScenario scenario,
            LMCAxisDs402HomeRecoveryKey key)
        {
            var steps = new List<FakeRpcStep>
            {
                InitStep(),
                CallbackStep(),
                AxisLookupStep(2),
                AxisInfoStep(2),
                DiagnosticsCapabilitiesStep(1),
                AdminCapabilitiesStep(1),
                new FakeRpcStep(
                    0x7D16,
                    TestFrame.Response(
                        0,
                        Ds402HomeOutcomePayload(
                            2,
                            key,
                            Ds402RecordGeneration)))
            };

            FakeRpcStep retirement;
            switch (scenario)
            {
                case Ds402RetirementScenario.Success:
                    retirement = RetirementStep(
                        TestFrame.Response(
                            0,
                            Ds402HomeOutcomePayload(
                                3,
                                key,
                                Ds402RecordGeneration)),
                        key);
                    steps.Add(retirement);
                    steps.Add(CloseStep());
                    break;
                case Ds402RetirementScenario.Timeout:
                    retirement = RetirementStep(
                        TestFrame.Response(
                            0,
                            Ds402HomeOutcomePayload(
                                3,
                                key,
                                Ds402RecordGeneration)),
                        key);
                    retirement.ResponseDelayMilliseconds = 500;
                    retirement.AllowClientDisconnectAfterRequest = true;
                    steps.Add(retirement);
                    break;
                case Ds402RetirementScenario.ResponseLoss:
                    retirement = RetirementStep(new byte[0], key);
                    retirement.CloseClientBeforeResponse = true;
                    steps.Add(retirement);
                    break;
                case Ds402RetirementScenario.KeyMismatch:
                    retirement = RetirementStep(
                        TestFrame.Response(
                            0,
                            AdminFailurePayload(
                                3,
                                LMCAdminDetailCode
                                    .Ds402HomeOutcomeKeyMismatch)),
                        key);
                    steps.Add(retirement);
                    steps.Add(CloseStep());
                    break;
                case Ds402RetirementScenario.Malformed:
                    retirement = RetirementStep(
                        TestFrame.Response(
                            0,
                            Ds402HomeOutcomePayload(
                                3,
                                key,
                                Ds402RecordGeneration + 1)),
                        key);
                    steps.Add(retirement);
                    steps.Add(ExpectedClientDisconnectStep());
                    break;
                case Ds402RetirementScenario.SnapshotResultMismatch:
                    var resultMismatch = Ds402HomeOutcomePayload(
                        3,
                        key,
                        Ds402RecordGeneration);
                    TestFrame.WriteUInt16(
                        resultMismatch,
                        16,
                        (ushort)LMCAxisDs402HomeOutcomeRecordState.Failed);
                    TestFrame.WriteUInt16(resultMismatch, 60, 1);
                    TestFrame.WriteInt16(resultMismatch, 62, -6);
                    TestFrame.WriteUInt32(
                        resultMismatch,
                        64,
                        (uint)LMCAdminDetailCode.NativeCommandRejected);
                    TestFrame.WriteUInt32(
                        resultMismatch,
                        84,
                        0x11223344U);
                    retirement = RetirementStep(
                        TestFrame.Response(0, resultMismatch),
                        key);
                    steps.Add(retirement);
                    steps.Add(CloseStep());
                    break;
                case Ds402RetirementScenario.SnapshotEvidenceMismatch:
                    var evidenceMismatch = Ds402HomeOutcomePayload(
                        3,
                        key,
                        Ds402RecordGeneration);
                    TestFrame.WriteUInt16(
                        evidenceMismatch,
                        68,
                        0x0027);
                    TestFrame.WriteUInt32(evidenceMismatch, 76, 101);
                    TestFrame.WriteUInt32(evidenceMismatch, 80, 201);
                    retirement = RetirementStep(
                        TestFrame.Response(0, evidenceMismatch),
                        key);
                    steps.Add(retirement);
                    steps.Add(CloseStep());
                    break;
                default:
                    throw new ArgumentOutOfRangeException("scenario");
            }

            return steps.ToArray();
        }

        private static void AssertDs402HomeRetirementFailure(
            Ds402RetirementScenario scenario,
            Exception failure,
            LMCConnectionState connectionState)
        {
            switch (scenario)
            {
                case Ds402RetirementScenario.Success:
                    AssertEx.True(failure == null);
                    AssertEx.Equal(
                        LMCConnectionState.Connected,
                        connectionState);
                    break;
                case Ds402RetirementScenario.Timeout:
                    AssertEx.True(
                        failure is IOException
                            || failure is TimeoutException);
                    AssertEx.Equal(
                        LMCConnectionState.Faulted,
                        connectionState);
                    break;
                case Ds402RetirementScenario.ResponseLoss:
                    AssertEx.True(failure is IOException);
                    AssertEx.Equal(
                        LMCConnectionState.Faulted,
                        connectionState);
                    break;
                case Ds402RetirementScenario.KeyMismatch:
                    AssertEx.True(
                        failure is
                            LMCAxisDs402HomeOutcomeRetirementException);
                    AssertEx.Equal(
                        LMCConnectionState.Connected,
                        connectionState);
                    break;
                case Ds402RetirementScenario.Malformed:
                    AssertEx.True(failure is InvalidDataException);
                    AssertEx.Equal(
                        LMCConnectionState.Faulted,
                        connectionState);
                    break;
                case Ds402RetirementScenario.SnapshotResultMismatch:
                case Ds402RetirementScenario.SnapshotEvidenceMismatch:
                    AssertEx.True(failure is InvalidDataException);
                    AssertEx.Equal(
                        LMCConnectionState.Connected,
                        connectionState);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("scenario");
            }
        }

        private static void AssertDs402HomeWireSequence(
            FakeRpcServer server)
        {
            var commands = server.ReceivedRequests
                .Select(request => TestFrame.ReadUInt16(request, 0))
                .Where(command => command == 0x7D15
                    || command == 0x7D16
                    || command == 0x7D17)
                .ToArray();
            AssertEx.Equal(2, commands.Length);
            AssertEx.Equal((ushort)0x7D16, commands[0]);
            AssertEx.Equal((ushort)0x7D17, commands[1]);
            AssertEx.Equal(0, CountCommand(server, 0x7D15));
            AssertEx.Equal(1, CountCommand(server, 0x7D16));
            AssertEx.Equal(1, CountCommand(server, 0x7D17));
        }

        private static FakeRpcStep LmcHomeStartStep(
            Action<byte[]> captureRequest)
        {
            var step = new FakeRpcStep(0x7D13, new byte[0]);
            step.InspectRequest = request =>
            {
                captureRequest(request);
                AssertEx.Equal((ushort)56, TestFrame.ReadUInt16(request, 4));
                AssertEx.Equal((ushort)2, TestFrame.ReadUInt16(request, 6));
                AssertEx.Equal((ushort)1, TestFrame.ReadUInt16(request, 8));
                AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(request, 10));
                AssertEx.Equal(
                    Ds402DiagnosticsBuild,
                    TestFrame.ReadUInt32(request, 16));
                AssertEx.Equal(
                    Ds402DiagnosticsBootId,
                    TestFrame.ReadUInt32(request, 20));
                AssertEx.Equal(
                    Ds402MapRevision,
                    TestFrame.ReadUInt32(request, 24));
                AssertEx.Equal((ushort)1, TestFrame.ReadUInt16(request, 44));
                AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(request, 46));
                AssertEx.Equal(
                    LmcHomeExpectedActualPosition,
                    TestFrame.ReadInt32(request, 48));
                AssertEx.Equal(0, TestFrame.ReadInt32(request, 52));
                AssertEx.Equal(5000U, TestFrame.ReadUInt32(request, 56));
                AssertEx.Equal(
                    0x454D4F48U,
                    TestFrame.ReadUInt32(request, 60));
                AssertEx.True(
                    TestFrame.ReadUInt32(request, 28) != 0
                        || TestFrame.ReadUInt32(request, 32) != 0
                        || TestFrame.ReadUInt32(request, 36) != 0
                        || TestFrame.ReadUInt32(request, 40) != 0);
            };
            step.ResponseFactory = request =>
            {
                var payload = CommonAdminPayload(
                    TestFrame.ReadUInt32(request, 12),
                    24);
                TestFrame.WriteUInt16(
                    payload,
                    16,
                    (ushort)LMCHomeSemanticMode.CurrentPositionZero);
                return TestFrame.Response(0, payload);
            };
            return step;
        }

        private static FakeRpcStep LmcHomeOutcomeStep(
            ushort command,
            Func<byte[]> getStartRequest,
            Action requestObserved)
        {
            var step = new FakeRpcStep(command, new byte[0]);
            step.InspectRequest = request =>
            {
                var start = getStartRequest();
                AssertEx.NotNull(start);
                var retiring = command == 0x7D19;
                AssertEx.Equal(
                    retiring ? (ushort)60 : (ushort)56,
                    TestFrame.ReadUInt16(request, 4));
                AssertEx.Equal((ushort)2, TestFrame.ReadUInt16(request, 6));
                AssertEx.Equal((ushort)1, TestFrame.ReadUInt16(request, 8));
                AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(request, 10));
                AssertEx.Equal(
                    TestFrame.ReadUInt32(start, 16),
                    TestFrame.ReadUInt32(request, 16));
                AssertEx.Equal(
                    TestFrame.ReadUInt32(start, 20),
                    TestFrame.ReadUInt32(request, 20));
                AssertEx.Equal(
                    TestFrame.ReadUInt32(start, 24),
                    TestFrame.ReadUInt32(request, 24));
                AssertEx.Equal(
                    TestFrame.ReadUInt32(start, 20),
                    TestFrame.ReadUInt32(request, 28));
                AssertEx.Equal(
                    TestFrame.ReadUInt32(start, 12),
                    TestFrame.ReadUInt32(request, 32));
                for (var word = 0; word < 4; word++)
                {
                    AssertEx.Equal(
                        TestFrame.ReadUInt32(start, 28 + (word * 4)),
                        TestFrame.ReadUInt32(request, 36 + (word * 4)));
                }

                AssertEx.Equal((ushort)1, TestFrame.ReadUInt16(request, 52));
                AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(request, 54));
                AssertEx.Equal(
                    LmcHomeExpectedActualPosition,
                    TestFrame.ReadInt32(request, 56));
                AssertEx.Equal(0, TestFrame.ReadInt32(request, 60));
                if (retiring)
                {
                    AssertEx.Equal(
                        LmcHomeRecordGeneration,
                        TestFrame.ReadUInt32(request, 64));
                }

                if (requestObserved != null)
                {
                    requestObserved();
                }
            };
            step.ResponseFactory = request => TestFrame.Response(
                0,
                LmcHomeOutcomePayload(
                    TestFrame.ReadUInt32(request, 12),
                    getStartRequest()));
            return step;
        }

        private static byte[] LmcHomeOutcomePayload(
            uint requestId,
            byte[] startRequest)
        {
            AssertEx.NotNull(startRequest);
            var payload = CommonAdminPayload(requestId, 144);
            TestFrame.WriteUInt16(
                payload,
                16,
                (ushort)LMCHomeOutcomeRecordState.Succeeded);
            TestFrame.WriteUInt16(
                payload,
                18,
                (ushort)LMCHomeSemanticMode.CurrentPositionZero);
            TestFrame.WriteUInt32(
                payload,
                20,
                TestFrame.ReadUInt32(startRequest, 16));
            TestFrame.WriteUInt32(
                payload,
                24,
                TestFrame.ReadUInt32(startRequest, 20));
            TestFrame.WriteUInt32(
                payload,
                28,
                TestFrame.ReadUInt32(startRequest, 24));
            TestFrame.WriteUInt32(
                payload,
                32,
                TestFrame.ReadUInt32(startRequest, 12));
            for (var word = 0; word < 4; word++)
            {
                TestFrame.WriteUInt32(
                    payload,
                    36 + (word * 4),
                    TestFrame.ReadUInt32(
                        startRequest,
                        28 + (word * 4)));
            }

            TestFrame.WriteUInt16(payload, 52, 2);
            TestFrame.WriteInt32(
                payload,
                56,
                LmcHomeExpectedActualPosition);
            TestFrame.WriteInt32(payload, 60, 0);
            TestFrame.WriteUInt32(payload, 64, 5000);
            TestFrame.WriteUInt32(payload, 76, LmcHomeStandstillState);
            TestFrame.WriteInt32(
                payload,
                84,
                LmcHomeExpectedActualPosition);
            TestFrame.WriteInt32(
                payload,
                88,
                LmcHomeExpectedActualPosition);
            TestFrame.WriteUInt32(
                payload,
                120,
                LmcHomeRequiredEvidenceFlags);
            TestFrame.WriteUInt32(payload, 124, 100);
            TestFrame.WriteUInt32(payload, 128, 200);
            TestFrame.WriteUInt32(payload, 136, 7);
            TestFrame.WriteUInt32(
                payload,
                140,
                LmcHomeRecordGeneration);
            return payload;
        }

        private static FakeRpcStep LmcHomeActualPositionStep()
        {
            var payload = new byte[8];
            TestFrame.WriteInt32(
                payload,
                0,
                LmcHomeExpectedActualPosition);
            return new FakeRpcStep(
                0x202E,
                TestFrame.Response(0, payload))
            {
                InspectRequest = request => AssertEx.Equal(
                    (ushort)2,
                    TestFrame.ReadUInt16(request, 6))
            };
        }

        private static FakeRpcStep LmcDiagnosticsCapabilitiesStep()
        {
            var step = new FakeRpcStep(0x7E00, new byte[0]);
            step.ResponseFactory = request =>
            {
                var payload = new byte[68];
                TestFrame.WriteUInt16(payload, 0, 1);
                TestFrame.WriteUInt32(
                    payload,
                    8,
                    TestFrame.ReadUInt32(request, 12));
                TestFrame.WriteUInt32(
                    payload,
                    16,
                    Ds402DiagnosticsBuild);
                TestFrame.WriteUInt32(payload, 24, Ds402MapRevision);
                TestFrame.WriteUInt32(payload, 40, 1000);
                TestFrame.WriteUInt16(payload, 44, 1320);
                TestFrame.WriteUInt16(payload, 46, 2040);
                TestFrame.WriteUInt32(
                    payload,
                    64,
                    Ds402DiagnosticsBootId);
                return TestFrame.Response(0, payload);
            };
            return step;
        }

        private static FakeRpcStep LmcAdminCapabilitiesStep()
        {
            var step = new FakeRpcStep(0x7D00, new byte[0]);
            step.ResponseFactory = request =>
            {
                var payload = CommonAdminPayload(
                    TestFrame.ReadUInt32(request, 12),
                    40);
                TestFrame.WriteUInt32(
                    payload,
                    16,
                    (uint)LMCAdminFeature.AxisHome);
                TestFrame.WriteUInt32(payload, 20, 0x3F);
                TestFrame.WriteUInt32(
                    payload,
                    24,
                    (uint)LMCGroupParameterSelection.All);
                TestFrame.WriteUInt16(payload, 28, 4);
                TestFrame.WriteUInt16(payload, 30, 1);
                TestFrame.WriteUInt16(payload, 32, 0x0100);
                TestFrame.WriteUInt16(payload, 34, 3);
                TestFrame.WriteUInt16(payload, 36, 5);
                return TestFrame.Response(0, payload);
            };
            return step;
        }

        private static void AssertLmcHomeWireSequence(
            FakeRpcServer server)
        {
            var commands = server.ReceivedRequests
                .Select(request => TestFrame.ReadUInt16(request, 0))
                .Where(command => command == 0x7D13
                    || command == 0x7D18
                    || command == 0x7D19)
                .ToArray();
            AssertEx.Equal(3, commands.Length);
            AssertEx.Equal((ushort)0x7D13, commands[0]);
            AssertEx.Equal((ushort)0x7D18, commands[1]);
            AssertEx.Equal((ushort)0x7D19, commands[2]);
            AssertEx.Equal(1, CountCommand(server, 0x7D13));
            AssertEx.Equal(1, CountCommand(server, 0x7D18));
            AssertEx.Equal(1, CountCommand(server, 0x7D19));
        }

        private static LMCAxisDs402HomeRecoveryKey
            CreateDs402HomeRecoveryKey()
        {
            return new LMCAxisDs402HomeRecoveryKey(
                LMCAdmin.ProtocolSchemaVersion,
                Ds402OriginalRequestId,
                Ds402DiagnosticsBuild,
                Ds402DiagnosticsBootId,
                Ds402MapRevision,
                new LMCAxisDs402HomeClientIntentId(
                    0x01234567U,
                    0x89ABCDEFU,
                    0x10203040U,
                    0x50607080U),
                2,
                new LMCAxisDs402HomeParameters(
                    60000));
        }

        private static FakeRpcStep RetirementStep(
            byte[] response,
            LMCAxisDs402HomeRecoveryKey key)
        {
            var step = new FakeRpcStep(0x7D17, response);
            step.InspectRequest = request =>
            {
                AssertEx.Equal(
                    (ushort)48,
                    TestFrame.ReadUInt16(request, 4));
                AssertEx.Equal(
                    key.RequestId,
                    TestFrame.ReadUInt32(request, 28));
                AssertEx.Equal(
                    Ds402RecordGeneration,
                    TestFrame.ReadUInt32(request, 52));
            };
            return step;
        }

        private static byte[] Ds402HomeOutcomePayload(
            uint requestId,
            LMCAxisDs402HomeRecoveryKey key,
            uint recordGeneration)
        {
            var payload = CommonAdminPayload(requestId, 92);
            TestFrame.WriteUInt16(
                payload,
                16,
                (ushort)LMCAxisDs402HomeOutcomeRecordState.Succeeded);
            TestFrame.WriteUInt32(payload, 20, key.DiagnosticsBuild);
            TestFrame.WriteUInt32(payload, 24, key.DiagnosticsBootId);
            TestFrame.WriteUInt32(payload, 28, key.MapRevision);
            TestFrame.WriteUInt32(payload, 32, key.RequestId);
            TestFrame.WriteUInt32(
                payload,
                36,
                key.ClientIntentId.Word0);
            TestFrame.WriteUInt32(
                payload,
                40,
                key.ClientIntentId.Word1);
            TestFrame.WriteUInt32(
                payload,
                44,
                key.ClientIntentId.Word2);
            TestFrame.WriteUInt32(
                payload,
                48,
                key.ClientIntentId.Word3);
            TestFrame.WriteUInt16(payload, 52, key.AxisReference);
            TestFrame.WriteInt32(
                payload,
                56,
                key.Parameters.HomingMethod);
            TestFrame.WriteUInt16(payload, 60, 0);
            TestFrame.WriteInt16(payload, 62, 0);
            TestFrame.WriteUInt32(payload, 64, 0);
            TestFrame.WriteUInt16(payload, 68, 0x1427);
            TestFrame.WriteInt32(payload, 72, 0);
            TestFrame.WriteUInt32(payload, 76, 100);
            TestFrame.WriteUInt32(payload, 80, 200);
            TestFrame.WriteUInt32(payload, 84, 0);
            TestFrame.WriteUInt32(payload, 88, recordGeneration);
            return payload;
        }

        private static byte[] AdminFailurePayload(
            uint requestId,
            LMCAdminDetailCode detailCode)
        {
            var payload = CommonAdminPayload(requestId, 16);
            TestFrame.WriteUInt16(payload, 4, 1);
            TestFrame.WriteInt16(payload, 6, -31000);
            TestFrame.WriteUInt32(payload, 12, (uint)detailCode);
            return payload;
        }

        private static byte[] CommonAdminPayload(
            uint requestId,
            int length)
        {
            var payload = new byte[length];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, requestId);
            return payload;
        }

        private static FakeRpcStep AdminCapabilitiesStep(uint requestId)
        {
            var payload = CommonAdminPayload(requestId, 40);
            TestFrame.WriteUInt32(
                payload,
                16,
                (uint)LMCAdminFeature.AxisDs402Home);
            TestFrame.WriteUInt32(payload, 20, 0x3F);
            TestFrame.WriteUInt32(
                payload,
                24,
                (uint)LMCGroupParameterSelection.All);
            TestFrame.WriteUInt16(payload, 28, 4);
            TestFrame.WriteUInt16(payload, 30, 1);
            TestFrame.WriteUInt16(payload, 32, 0x0100);
            TestFrame.WriteUInt16(payload, 34, 3);
            TestFrame.WriteUInt16(payload, 36, 4);
            return new FakeRpcStep(
                0x7D00,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep DiagnosticsCapabilitiesStep(
            uint requestId)
        {
            return DiagnosticsCapabilitiesStep(
                requestId,
                LMCDiagnosticCapability.None);
        }

        private static FakeRpcStep DiagnosticsCapabilitiesStep(
            uint requestId,
            LMCDiagnosticCapability capabilities)
        {
            var payload = new byte[68];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, requestId);
            TestFrame.WriteUInt32(payload, 16, Ds402DiagnosticsBuild);
            TestFrame.WriteUInt32(payload, 20, (uint)capabilities);
            TestFrame.WriteUInt32(payload, 24, Ds402MapRevision);
            TestFrame.WriteUInt32(payload, 40, 1000);
            TestFrame.WriteUInt16(payload, 44, 1320);
            TestFrame.WriteUInt16(payload, 46, 2040);
            TestFrame.WriteUInt32(payload, 64, Ds402DiagnosticsBootId);
            return new FakeRpcStep(
                0x7E00,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep AxisLookupStep(ushort axisReference)
        {
            var payload = new byte[6];
            TestFrame.WriteUInt16(payload, 4, axisReference);
            return new FakeRpcStep(
                0x103C,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep AxisInfoStep(ushort axisReference)
        {
            var payload = new byte[8];
            TestFrame.WriteUInt32(payload, 0, axisReference);
            return new FakeRpcStep(
                0x202B,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep InitStep()
        {
            var payload = new byte[24];
            TestFrame.WriteUInt32(payload, 0, 128);
            return new FakeRpcStep(
                0x8080,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep CallbackStep()
        {
            var step = new FakeRpcStep(0x405C, null);
            step.ResponseFactory = request => TestFrame.Response(
                0,
                CallbackResponsePayload(request));
            return step;
        }

        private static byte[] CallbackResponsePayload(byte[] request)
        {
            if (request.Length == 20)
            {
                AssertEx.Equal((ushort)12, TestFrame.ReadUInt16(request, 4));
                return new byte[4];
            }

            AssertEx.Equal(40, request.Length);
            AssertEx.Equal((ushort)32, TestFrame.ReadUInt16(request, 4));
            AssertEx.Equal(1u, TestFrame.ReadUInt32(request, 8));
            AssertEx.Equal((ushort)2, TestFrame.ReadUInt16(request, 20));
            AssertEx.Equal((ushort)52, TestFrame.ReadUInt16(request, 22));
            AssertEx.True(
                TestFrame.ReadUInt32(request, 24) != 0
                || TestFrame.ReadUInt32(request, 28) != 0,
                "The WPF version-2 callback registration cookie was zero.");

            var payload = new byte[20];
            TestFrame.WriteUInt16(payload, 4, 2);
            TestFrame.WriteUInt16(payload, 6, 52);
            TestFrame.WriteUInt32(payload, 8, Ds402DiagnosticsBootId);
            TestFrame.WriteUInt32(payload, 12, 1);
            return payload;
        }

        private static FakeRpcStep CloseStep()
        {
            return new FakeRpcStep(
                0x405D,
                TestFrame.Response(
                    0,
                    TestFrame.Hex("00 00 00 00")));
        }

        private static FakeRpcStep ExpectedClientDisconnectStep()
        {
            return new FakeRpcStep(0, new byte[0])
            {
                RequireClientDisconnectBeforeRequest = true
            };
        }

        private static void Connect(LMCConnection connection, int port)
        {
            connection.RpcInitConnection(
                "127.0.0.1",
                port,
                "127.0.0.1",
                0,
                LMCConnection.DefaultEventMask);
        }

        private static int CountCommand(
            FakeRpcServer server,
            ushort command)
        {
            return server.ReceivedRequests.Count(
                request => TestFrame.ReadUInt16(request, 0) == command);
        }

        private static Exception CaptureTaskFailure(Task task)
        {
            try
            {
                task.GetAwaiter().GetResult();
                return null;
            }
            catch (Exception error)
            {
                return error;
            }
        }

        private static object InvokePrivate(
            object target,
            string methodName,
            params object[] arguments)
        {
            var method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            AssertEx.NotNull(method);
            return method.Invoke(target, arguments);
        }

        private static void SetPrivateField(
            object target,
            string fieldName,
            object value)
        {
            var field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            AssertEx.NotNull(field);
            field.SetValue(target, value);
        }

        private static object GetPrivateField(
            object target,
            string fieldName)
        {
            var field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            AssertEx.NotNull(field);
            return field.GetValue(target);
        }

        private static void WaitForTask(Task task, string message)
        {
            var deadline = DateTime.UtcNow.AddSeconds(8);
            while (!task.IsCompleted && DateTime.UtcNow < deadline)
            {
                PumpUiOnce();
            }

            AssertEx.True(task.IsCompleted, message);
        }

        private static void WithTemporaryWindow(
            LasalMotionControlApiExample.UiLanguage language,
            Action<string> prepare,
            Action<LasalMotionControlApiExample.MainWindow> assertion)
        {
            var root = Path.Combine(
                Path.GetTempPath(),
                "ElmoMaintenanceUiTests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            LasalMotionControlApiExample.MainWindow window = null;
            try
            {
                var preferencePath = Path.Combine(
                    root,
                    "UiLanguage",
                    "ui-language.txt");
                LasalMotionControlApiExample.UiLanguagePreferenceStore.Save(
                    preferencePath,
                    language);
                if (prepare != null)
                {
                    prepare(root);
                }

                window = CreateHiddenWindow(root);
                assertion(window);
            }
            finally
            {
                if (window != null && window.IsLoaded)
                {
                    window.Close();
                    WaitForUiCondition(
                        () => !window.IsLoaded,
                        "The maintenance test MainWindow did not close.");
                }

                if (Directory.Exists(root))
                {
                    Directory.Delete(root, true);
                }
            }
        }

        private static LasalMotionControlApiExample.MainWindow
            CreateHiddenWindow(string root)
        {
            var window = new LasalMotionControlApiExample.MainWindow(root)
            {
                ShowActivated = false,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000
            };
            window.Show();
            WaitForUiCondition(
                () => window.IsLoaded,
                "The maintenance test MainWindow did not load.");
            return window;
        }

        private static void WaitForUiCondition(
            Func<bool> condition,
            string message)
        {
            var deadline = DateTime.UtcNow.AddSeconds(3);
            while (!condition() && DateTime.UtcNow < deadline)
            {
                PumpUiOnce();
            }

            AssertEx.True(condition(), message);
        }

        private static void PumpUiOnce()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() => frame.Continue = false));
            Dispatcher.PushFrame(frame);
        }

        private static void Click(System.Windows.Controls.Button button)
        {
            button.RaiseEvent(
                new RoutedEventArgs(
                    System.Windows.Controls.Button.ClickEvent,
                    button));
        }
    }
}

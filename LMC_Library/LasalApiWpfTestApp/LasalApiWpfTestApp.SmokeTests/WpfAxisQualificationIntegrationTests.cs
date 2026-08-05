using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using LasalMotionControlApiExample;
using LasalMotionControlLib;
using LasalMotionControlLib.Tests;

namespace LasalApiWpfTestApp.SmokeTests
{
    internal static partial class WpfMainWindowIntegrationTests
    {
        private const ushort AxisQualificationReference = 1;
        private const uint AxisQualificationReadyState = 0x02000003u;
        private const uint AxisQualificationMovingState = 0x00000003u;
        private const uint AxisQualificationPowerOffState = 0x02000002u;

        internal static void RegisterAxisQualificationIntegrationTests(
            ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.AxisQualification.UncheckedSafetyConfirmationIsZeroRpc",
                AxisQualificationUncheckedSafetyConfirmationIsZeroRpc);
            tests.Add(
                "Wpf.AxisQualification.InputEditInvalidatesSafetyAndIsZeroMutation",
                AxisQualificationInputEditInvalidatesSafetyAndIsZeroMutation);
            tests.Add(
                "Wpf.AxisQualification.PreWireCancelIsZeroRpcAndZeroMutation",
                AxisQualificationPreWireCancelIsZeroRpcAndZeroMutation);
            tests.Add(
                "Wpf.AxisQualification.BuildDriftBeforeMoveIsZeroMotionAndCleanupMutation",
                AxisQualificationBuildDriftBeforeMoveIsZeroMotionAndCleanupMutation);
            tests.Add(
                "Wpf.AxisQualification.HappyPathIsAcceptedOnceAndPowerOffSafe",
                AxisQualificationHappyPathIsAcceptedOnceAndPowerOffSafe);
            tests.Add(
                "Wpf.AxisQualification.CancelAfterMoveUsesSafeCleanupWithoutReplay",
                AxisQualificationCancelAfterMoveUsesSafeCleanupWithoutReplay);
            tests.Add(
                "Wpf.AxisQualification.ExternalStopIsReusedWithoutDuplicateSafetyMutation",
                AxisQualificationExternalStopIsReusedWithoutDuplicateSafetyMutation);
            tests.Add(
                "Wpf.AxisQualification.ExternalPowerOffIsReusedWithoutDuplicateSafetyMutation",
                AxisQualificationExternalPowerOffIsReusedWithoutDuplicateSafetyMutation);
            tests.Add(
                "Wpf.AxisQualification.RecoveredStableRecordCannotUseCurrentRunAdmissionBypass",
                RecoveredStableRecordCannotUseCurrentRunAdmissionBypass);
        }

        private static void
            RecoveredStableRecordCannotUseCurrentRunAdmissionBypass()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(CloseStep());

            var root = CreateAxisQualificationTemporaryDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    AxisQualificationRecoveryRecord persisted;
                    var createdUtc = DateTime.UtcNow.AddMinutes(-1);
                    using (var journal = AxisQualificationRecoveryJournal.Open(
                        Path.Combine(root, "AxisQualificationRecovery"),
                        true))
                    {
                        persisted = journal.ArmBeforePowerOn(
                            "127.0.0.1",
                            server.Port,
                            1,
                            "_LMCAxis1",
                            AxisQualificationReference,
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
                        persisted = journal.MarkPowerOnAccepted(
                            persisted,
                            createdUtc.AddTicks(1));
                        persisted = journal.MarkPowerOnStable(
                            persisted,
                            createdUtc.AddTicks(2));
                    }

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
                        "The exact Axis qualification recovery connection did not complete.");

                    var currentConnection = (LMCConnection)GetPrivateField(
                        window,
                        "connection");
                    var journalInWindow =
                        (AxisQualificationRecoveryJournal)GetPrivateField(
                            window,
                            "axisQualificationRecoveryJournal");
                    var recovered = journalInWindow.CurrentRecord;
                    AssertEx.NotNull(recovered);
                    AssertEx.Equal(
                        AxisQualificationRecoveryStage.PowerOnStable,
                        recovered.Stage);
                    AssertEx.Equal(persisted.Identity, recovered.Identity);
                    AssertEx.False(recovered.WasCrashPromoted);
                    AssertEx.Equal(
                        recovered.OwnerSessionGeneration,
                        GetConnectionSessionGeneration(currentConnection),
                        "The reconnect did not reproduce the coincident session generation.");
                    AssertEx.Equal(
                        recovered.EndpointIp,
                        (string)GetPrivateField(window, "connectedRemoteIp"));
                    AssertEx.Equal(
                        recovered.EndpointPort,
                        (int)GetPrivateField(window, "connectedRemotePort"));

                    SetPrivateField(window, "qualificationRunning", true);
                    AssertEx.True(
                        GetPrivateField(
                            window,
                            "currentAxisQualificationRecoveryIdentity")
                        == null,
                        "A restarted process unexpectedly inherited the current-run qualification token.");
                    AssertEx.False(
                        (bool)InvokePrivate(
                            window,
                            "IsCurrentAxisQualificationMutationScope"),
                        "A persisted record bypassed admission without a process-local identity token.");

                    var admission =
                        (DiagnosticsAdmissionDecision)InvokePrivate(
                            window,
                            "EvaluateDiagnosticsAdmission",
                            DiagnosticsAdmissionOperation.NewLiveOrMutation,
                            true);
                    AssertEx.False(admission.IsAllowed);
                    AssertEx.Equal(
                        DiagnosticsAdmissionDenialReason.AxisPowerOnUnresolved,
                        admission.DenialReason);

                    SetPrivateField(window, "qualificationRunning", false);
                    ForceCloseMotionRecoveryWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                if (window != null)
                {
                    SetPrivateField(window, "qualificationRunning", false);
                }
                ForceCloseMotionRecoveryWindow(window);
                DeleteAxisQualificationTemporaryDirectory(root);
            }
        }

        private static void
            AxisQualificationInputEditInvalidatesSafetyAndIsZeroMutation()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(AxisQualificationReference));
            steps.Add(D5AxisInfoStep(AxisQualificationReference));
            steps.Add(CloseStep());

            var root = CreateAxisQualificationTemporaryDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(root, server.Port);
                    ConnectAndLoadAxisForQualification(window);

                    window.CheckAxisQualificationTravelSafe.IsChecked = true;
                    window.CheckAxisQualificationIdentitySafe.IsChecked = true;
                    window.CheckAxisQualificationExclusiveOwner.IsChecked = true;
                    PumpDispatcherOnce();
                    AssertEx.True(window.ButtonRunAxisQualification.IsEnabled);
                    var requestCountBeforeEdit = server.ReceivedRequests.Count;

                    window.TextAxisQualificationDelta.Text = "200";
                    PumpDispatcherOnce();

                    AssertEx.False(
                        window.CheckAxisQualificationTravelSafe.IsChecked == true);
                    AssertEx.False(
                        window.CheckAxisQualificationIdentitySafe.IsChecked == true);
                    AssertEx.False(
                        window.CheckAxisQualificationExclusiveOwner.IsChecked == true);
                    AssertEx.False(window.ButtonRunAxisQualification.IsEnabled);
                    AssertEx.Equal(
                        requestCountBeforeEdit,
                        server.ReceivedRequests.Count,
                        "Editing a qualification input sent an RPC.");

                    InvokePrivate(
                        window,
                        "ButtonRunAxisQualification_Click",
                        window.ButtonRunAxisQualification,
                        new RoutedEventArgs(
                            System.Windows.Controls.Button.ClickEvent,
                            window.ButtonRunAxisQualification));
                    PumpDispatcherOnce();

                    AssertEx.Equal(
                        requestCountBeforeEdit,
                        server.ReceivedRequests.Count,
                        "The private Run handler bypassed the invalidated safety confirmation gate.");
                    AssertEx.Contains(
                        "No RPC was sent",
                        window.TextAxisQualificationProgress.Text);
                    AssertEx.Equal(0, CountAxisPowerCommand(server, true));
                    AssertEx.Equal(0, CountCommand(server, 0x20A0));
                    AssertEx.Equal(0, CountCommand(server, 0x2022));
                    AssertEx.Equal(0, CountAxisPowerCommand(server, false));
                    AssertAxisQualificationDurableRecordsNeverArmed(window);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteAxisQualificationTemporaryDirectory(root);
            }
        }

        private static void
            AxisQualificationUncheckedSafetyConfirmationIsZeroRpc()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(AxisQualificationReference));
            steps.Add(D5AxisInfoStep(AxisQualificationReference));
            steps.Add(CloseStep());

            var root = CreateAxisQualificationTemporaryDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(root, server.Port);
                    ConnectAndLoadAxisForQualification(window);

                    AssertEx.False(
                        window.CheckAxisQualificationTravelSafe.IsChecked == true);
                    AssertEx.False(
                        window.CheckAxisQualificationIdentitySafe.IsChecked == true);
                    AssertEx.False(
                        window.CheckAxisQualificationExclusiveOwner.IsChecked == true);
                    var requestCountBeforeAttempt =
                        server.ReceivedRequests.Count;

                    InvokePrivate(
                        window,
                        "ButtonRunAxisQualification_Click",
                        window.ButtonRunAxisQualification,
                        new RoutedEventArgs(
                            System.Windows.Controls.Button.ClickEvent,
                            window.ButtonRunAxisQualification));
                    PumpDispatcherOnce();

                    AssertEx.Equal(
                        requestCountBeforeAttempt,
                        server.ReceivedRequests.Count,
                        "Unchecked physical safety confirmation sent an RPC.");
                    AssertEx.Contains(
                        "No RPC was sent",
                        window.TextAxisQualificationProgress.Text);
                    AssertEx.Equal(0, CountAxisPowerCommand(server, true));
                    AssertEx.Equal(0, CountCommand(server, 0x20A0));
                    AssertEx.Equal(0, CountCommand(server, 0x2022));
                    AssertEx.Equal(0, CountAxisPowerCommand(server, false));
                    AssertAxisQualificationDurableRecordsNeverArmed(window);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteAxisQualificationTemporaryDirectory(root);
            }
        }

        private static void
            AxisQualificationPreWireCancelIsZeroRpcAndZeroMutation()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(AxisQualificationReference));
            steps.Add(D5AxisInfoStep(AxisQualificationReference));
            steps.Add(CloseStep());

            var root = CreateAxisQualificationTemporaryDirectory();
            MainWindow window = null;
            SemaphoreSlim commandGate = null;
            var gateHeld = false;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(root, server.Port);
                    ConnectAndLoadAxisForQualification(window);
                    window.CheckAxisQualificationTravelSafe.IsChecked = true;
                    window.CheckAxisQualificationIdentitySafe.IsChecked = true;
                    window.CheckAxisQualificationExclusiveOwner.IsChecked = true;
                    PumpDispatcherOnce();
                    AssertEx.True(window.ButtonRunAxisQualification.IsEnabled);

                    commandGate = (SemaphoreSlim)GetPrivateField(
                        window,
                        "commandSendGate");
                    commandGate.Wait();
                    gateHeld = true;
                    var requestCountBeforeRun = server.ReceivedRequests.Count;

                    Click(window.ButtonRunAxisQualification);
                    WaitUntil(
                        () => (bool)GetPrivateField(
                            window,
                            "qualificationRunning"),
                        "The pre-wire cancellation test did not start the runner.");
                    AssertEx.True(window.ButtonCancelAxisQualification.IsEnabled);
                    Click(window.ButtonCancelAxisQualification);
                    WaitUntil(
                        () => ((CancellationTokenSource)GetPrivateField(
                            window,
                            "qualificationCancellation"))
                            .IsCancellationRequested,
                        "The pre-wire cancellation token was not canceled.");

                    commandGate.Release();
                    gateHeld = false;
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "SingleAxisPowerMoveStopPowerOff aborted",
                            StringComparison.Ordinal),
                        "Pre-wire cancellation did not finish as ABORTED.");

                    AssertEx.Equal(
                        requestCountBeforeRun,
                        server.ReceivedRequests.Count,
                        "Pre-wire cancellation sent an RPC after the test acquired the command gate.");
                    AssertEx.Equal(2, CountCommand(server, 0x7E00));
                    AssertEx.Equal(0, CountAxisPowerCommand(server, true));
                    AssertEx.Equal(0, CountCommand(server, 0x20A0));
                    AssertEx.Equal(0, CountCommand(server, 0x2022));
                    AssertEx.Equal(0, CountAxisPowerCommand(server, false));
                    AssertEx.Equal(0, CountCommand(server, 0x2028));
                    AssertEx.Equal(0, CountCommand(server, 0x202E));
                    AssertEx.Contains(
                        "event=END|verdict=ABORTED",
                        window.TextAxisQualificationSummary.Text);
                    AssertEx.Contains(
                        "powerOn2023=0|moveRelative20A0=0|moveAbsolute209F=0|stop2022=0|powerOff2023=0",
                        window.TextExecutionLog.Text);
                    AssertAxisQualificationDurableRecordsNeverArmed(window);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                if (gateHeld && commandGate != null)
                {
                    commandGate.Release();
                }
                CloseWindowBestEffort(window);
                DeleteAxisQualificationTemporaryDirectory(root);
            }
        }

        private static void
            AxisQualificationBuildDriftBeforeMoveIsZeroMotionAndCleanupMutation()
        {
            const int deltaRaw = 120;
            const int velocityRaw = 230;
            const int accelerationRaw = 340;
            const int decelerationRaw = 450;
            const int jerkRaw = 0;
            const int startPositionRaw = 1000;
            const uint driftedDiagnosticsBuild = 2;

            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(AxisQualificationReference));
            steps.Add(D5AxisInfoStep(AxisQualificationReference));
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(AxisQualificationPowerStep(true));
            for (var sample = 0; sample < 3; sample++)
            {
                steps.Add(AxisQualificationStatusStep(
                    AxisQualificationReadyState));
            }
            steps.Add(CapabilitiesStep(13, capabilities));
            steps.Add(CapabilitiesStep(14, capabilities));
            steps.Add(AxisQualificationStatusStep(
                AxisQualificationReadyState));
            steps.Add(AxisQualificationPositionStep(startPositionRaw));

            // The generic motion dispatcher refreshes the PLC identity here.
            // Only DiagnosticsBuild changes; BootId and MapRevision remain
            // pinned. The qualification-specific final pre-wire validator
            // must reject before arming the motion journal or sending 0x20A0.
            steps.Add(AxisQualificationCapabilitiesStep(
                15,
                capabilities,
                driftedDiagnosticsBuild));
            // PowerOn was already proven and its child durable record resolved.
            // The parent sequence remains at PowerOnStable. Conservative
            // PowerOff cleanup rechecks the pinned Build and also rejects
            // before its 0x2023 wire boundary.
            steps.Add(AxisQualificationCapabilitiesStep(
                16,
                capabilities,
                driftedDiagnosticsBuild));
            steps.Add(CloseStep());

            var root = CreateAxisQualificationTemporaryDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(root, server.Port);
                    ConnectAndLoadAxisForQualification(window);
                    ConfigureAxisQualificationForTest(
                        window,
                        deltaRaw,
                        velocityRaw,
                        accelerationRaw,
                        decelerationRaw,
                        jerkRaw,
                        5);

                    Click(window.ButtonRunAxisQualification);
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "SingleAxisPowerMoveStopPowerOff failed",
                            StringComparison.Ordinal),
                        "DiagnosticsBuild drift before Move did not fail closed.");

                    AssertEx.Equal(1, CountAxisPowerCommand(server, true));
                    AssertEx.Equal(0, CountCommand(server, 0x20A0));
                    AssertEx.Equal(0, CountCommand(server, 0x2022));
                    AssertEx.Equal(0, CountAxisPowerCommand(server, false));
                    AssertEx.Equal(0, CountCommand(server, 0x209F));
                    AssertEx.Equal(4, CountCommand(server, 0x2028));
                    AssertEx.Equal(1, CountCommand(server, 0x202E));
                    AssertEx.Contains(
                        "DiagnosticsBuild, BootId, or MapRevision changed",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "safeState=UNPROVEN",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "powerOn2023=1|moveRelative20A0=0|moveAbsolute209F=0|stop2022=0|powerOff2023=0",
                        window.TextExecutionLog.Text);
                    AssertEx.False((bool)GetPrivateField(
                        window,
                        "motionMayBeActive"));
                    var powerJournal =
                        (AxisPowerOnRecoveryJournal)GetPrivateField(
                            window,
                            "axisPowerOnRecoveryJournal");
                    AssertEx.False(powerJournal.HasActiveRecord);
                    AssertEx.NotNull(powerJournal.CurrentRecord);
                    AssertEx.Equal(
                        AxisPowerOnRecoveryState.Resolved,
                        powerJournal.CurrentRecord.State);
                    AssertEx.False(
                        ((MotionUncertaintyJournal)GetPrivateField(
                            window,
                            "motionUncertaintyJournal")).HasActiveRecord);
                    AssertEx.False(
                        ((AxisCommandRecoveryJournal)GetPrivateField(
                            window,
                            "axisCommandRecoveryJournal")).HasActiveRecord);

                    var qualificationJournal =
                        (AxisQualificationRecoveryJournal)GetPrivateField(
                            window,
                            "axisQualificationRecoveryJournal");
                    AssertEx.True(
                        qualificationJournal.HasActiveRecord,
                        "Build drift retired the unresolved parent qualification sequence.");
                    AssertEx.NotNull(qualificationJournal.CurrentRecord);
                    AssertEx.Equal(
                        AxisQualificationRecoveryStage.PowerOnStable,
                        qualificationJournal.CurrentRecord.Stage,
                        "Build drift did not preserve the last proven parent sequence stage.");

                    AssertEx.False(
                        window.ButtonRunAxisQualification.IsEnabled,
                        "An unresolved parent qualification sequence allowed a new qualification mutation.");
                    AssertEx.False(window.ButtonPowerOn.IsEnabled);
                    AssertEx.False(window.ButtonReset.IsEnabled);
                    AssertEx.False(window.ButtonMoveAbsolute.IsEnabled);
                    AssertEx.False(window.ButtonMoveRelative.IsEnabled);
                    AssertEx.False(window.ButtonMoveVelocity.IsEnabled);
                    AssertEx.True(
                        window.ButtonStop.IsEnabled,
                        "Explicit safety Stop was not available during sequence recovery.");
                    AssertEx.True(
                        window.ButtonPowerOff.IsEnabled,
                        "Explicit safety Power Off was not available during sequence recovery.");
                    AssertEx.False(
                        window.ButtonCloseConnection.IsEnabled,
                        "Normal Close Connection remained available with an unresolved parent sequence.");

                    window.Close();
                    PumpDispatcherOnce();
                    AssertEx.True(
                        window.IsLoaded,
                        "Normal window close bypassed the unresolved parent sequence interlock.");
                    AssertEx.Contains(
                        "Window close is blocked while Single Axis qualification recovery is unresolved",
                        window.TextExecutionLog.Text);

                    ForceCloseMotionRecoveryWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                ForceCloseMotionRecoveryWindow(window);
                DeleteAxisQualificationTemporaryDirectory(root);
            }
        }

        private static void
            AxisQualificationHappyPathIsAcceptedOnceAndPowerOffSafe()
        {
            const int deltaRaw = 120;
            const int velocityRaw = 230;
            const int accelerationRaw = 340;
            const int decelerationRaw = 450;
            const int jerkRaw = 0;
            const int toleranceRaw = 5;
            const int startPositionRaw = 1000;
            const int targetPositionRaw = startPositionRaw + deltaRaw;

            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(AxisQualificationReference));
            steps.Add(D5AxisInfoStep(AxisQualificationReference));

            // Exact current identity preflight, Power On admission and final
            // durable Power On identity proof.
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(AxisQualificationPowerStep(true));
            for (var sample = 0; sample < 3; sample++)
            {
                steps.Add(AxisQualificationStatusStep(
                    AxisQualificationReadyState));
            }
            steps.Add(CapabilitiesStep(13, capabilities));

            // Fresh ready-to-move status and start position.
            steps.Add(CapabilitiesStep(14, capabilities));
            steps.Add(AxisQualificationStatusStep(
                AxisQualificationReadyState));
            steps.Add(AxisQualificationPositionStep(startPositionRaw));

            // Motion is armed against a fresh identity, sent once, observed
            // non-standstill, and then proven standstill three times.
            steps.Add(CapabilitiesStep(15, capabilities));
            steps.Add(AxisQualificationMoveRelativeStep(
                deltaRaw,
                velocityRaw,
                accelerationRaw,
                decelerationRaw,
                jerkRaw));
            steps.Add(AxisQualificationStatusStep(
                AxisQualificationMovingState));
            for (var sample = 0; sample < 3; sample++)
            {
                steps.Add(AxisQualificationStatusStep(
                    AxisQualificationReadyState));
            }
            steps.Add(AxisQualificationPositionStep(targetPositionRaw));
            steps.Add(AxisQualificationPositionStep(targetPositionRaw + 1));
            steps.Add(AxisQualificationPositionStep(targetPositionRaw - 1));
            steps.Add(CapabilitiesStep(16, capabilities));

            // Stop is deliberately after planned standstill. It is still an
            // accepted-once command with three stable status samples.
            steps.Add(CapabilitiesStep(17, capabilities));
            steps.Add(AxisQualificationStopStep(
                decelerationRaw,
                jerkRaw));
            for (var sample = 0; sample < 3; sample++)
            {
                steps.Add(AxisQualificationStatusStep(
                    AxisQualificationReadyState));
            }
            steps.Add(CapabilitiesStep(18, capabilities));
            // Stop completion then refreshes the active motion journal's
            // exact identity before clearing it.
            steps.Add(CapabilitiesStep(19, capabilities));

            // Power Off is sent once and must end with three stable
            // PowerOff+Standstill samples and a final exact identity proof.
            steps.Add(CapabilitiesStep(20, capabilities));
            steps.Add(AxisQualificationPowerStep(false));
            for (var sample = 0; sample < 3; sample++)
            {
                steps.Add(AxisQualificationStatusStep(
                    AxisQualificationPowerOffState));
            }
            steps.Add(CapabilitiesStep(21, capabilities));
            steps.Add(CloseStep());

            var root = CreateAxisQualificationTemporaryDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(root, server.Port);
                    ConnectAndLoadAxisForQualification(window);

                    window.TextAxisQualificationDelta.Text = deltaRaw.ToString(
                        CultureInfo.InvariantCulture);
                    window.TextAxisQualificationVelocity.Text =
                        velocityRaw.ToString(CultureInfo.InvariantCulture);
                    window.TextAxisQualificationAcceleration.Text =
                        accelerationRaw.ToString(CultureInfo.InvariantCulture);
                    window.TextAxisQualificationDeceleration.Text =
                        decelerationRaw.ToString(CultureInfo.InvariantCulture);
                    window.TextAxisQualificationJerk.Text = jerkRaw.ToString(
                        CultureInfo.InvariantCulture);
                    window.TextAxisQualificationTolerance.Text =
                        toleranceRaw.ToString(CultureInfo.InvariantCulture);
                    PumpDispatcherOnce();

                    window.CheckAxisQualificationTravelSafe.IsChecked = true;
                    window.CheckAxisQualificationIdentitySafe.IsChecked = true;
                    window.CheckAxisQualificationExclusiveOwner.IsChecked = true;
                    PumpDispatcherOnce();
                    AssertEx.True(
                        window.ButtonRunAxisQualification.IsEnabled,
                        "The fully confirmed live Axis qualification did not open its Run gate.");

                    Click(window.ButtonRunAxisQualification);
                    try
                    {
                        WaitUntil(
                            () => string.Equals(
                                window.TextOperationState.Text,
                                "SingleAxisPowerMoveStopPowerOff PASS",
                                StringComparison.Ordinal),
                            "The live Single Axis qualification did not reach PASS.",
                            15000);
                    }
                    catch (TimeoutException error)
                    {
                        throw new TimeoutException(
                            error.Message
                            + " State="
                            + window.TextOperationState.Text
                            + Environment.NewLine
                            + window.TextExecutionLog.Text,
                            error);
                    }

                    AssertEx.Equal(1, CountAxisPowerCommand(server, true));
                    AssertEx.Equal(1, CountCommand(server, 0x20A0));
                    AssertEx.Equal(1, CountCommand(server, 0x2022));
                    AssertEx.Equal(1, CountAxisPowerCommand(server, false));
                    AssertEx.Equal(0, CountCommand(server, 0x209F));
                    AssertEx.Equal(14, CountCommand(server, 0x2028));
                    AssertEx.Equal(4, CountCommand(server, 0x202E));
                    AssertEx.Contains(
                        "event=AXIS_QUALIFICATION_RESULT|verdict=PASS",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "safeState=POWER_OFF_STANDSTILL",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "event=END|verdict=PASS",
                        window.TextAxisQualificationSummary.Text);
                    AssertEx.False(
                        window.CheckAxisQualificationTravelSafe.IsChecked == true,
                        "Travel confirmation was not invalidated after the run.");
                    AssertEx.False(
                        window.CheckAxisQualificationIdentitySafe.IsChecked == true,
                        "Identity confirmation was not invalidated after the run.");
                    AssertEx.False(
                        window.CheckAxisQualificationExclusiveOwner.IsChecked == true,
                        "Ownership confirmation was not invalidated after the run.");
                    AssertAxisQualificationDurableRecordsInactive(window);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteAxisQualificationTemporaryDirectory(root);
            }
        }

        private static void ConnectAndLoadAxisForQualification(
            MainWindow window)
        {
            Click(window.ButtonConnect);
            WaitUntil(
                () => window.ButtonLookupAxis.IsEnabled,
                "The Axis qualification test did not connect.");
            Click(window.ButtonLookupAxis);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Load Axis completed",
                        StringComparison.Ordinal)
                    && GetPrivateField(window, "axis") != null,
                "The Axis qualification test did not load the Axis.");
        }

        private static void
            AxisQualificationCancelAfterMoveUsesSafeCleanupWithoutReplay()
        {
            const int deltaRaw = 120;
            const int velocityRaw = 230;
            const int accelerationRaw = 340;
            const int decelerationRaw = 450;
            const int jerkRaw = 0;
            const int startPositionRaw = 1000;

            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(AxisQualificationReference));
            steps.Add(D5AxisInfoStep(AxisQualificationReference));

            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(AxisQualificationPowerStep(true));
            for (var sample = 0; sample < 3; sample++)
            {
                steps.Add(AxisQualificationStatusStep(
                    AxisQualificationReadyState));
            }
            steps.Add(CapabilitiesStep(13, capabilities));
            steps.Add(CapabilitiesStep(14, capabilities));
            steps.Add(AxisQualificationStatusStep(
                AxisQualificationReadyState));
            steps.Add(AxisQualificationPositionStep(startPositionRaw));
            steps.Add(CapabilitiesStep(15, capabilities));
            steps.Add(AxisQualificationMoveRelativeStep(
                deltaRaw,
                velocityRaw,
                accelerationRaw,
                decelerationRaw,
                jerkRaw));
            steps.Add(AxisQualificationStatusStep(
                AxisQualificationMovingState));

            // Cancellation occurs after this moving sample has been parsed.
            // Stop must validate both the qualification and active durable
            // motion identities before its one accepted dispatch.
            steps.Add(CapabilitiesStep(16, capabilities));
            steps.Add(CapabilitiesStep(17, capabilities));
            steps.Add(AxisQualificationStopStep(
                decelerationRaw,
                jerkRaw));
            for (var sample = 0; sample < 3; sample++)
            {
                steps.Add(AxisQualificationStatusStep(
                    AxisQualificationReadyState));
            }
            steps.Add(CapabilitiesStep(18, capabilities));
            steps.Add(CapabilitiesStep(19, capabilities));

            // Stable Stop resolves the motion record. Power Off therefore
            // needs only its qualification identity refresh and final durable
            // Axis Power identity refresh.
            steps.Add(CapabilitiesStep(20, capabilities));
            steps.Add(AxisQualificationPowerStep(false));
            for (var sample = 0; sample < 3; sample++)
            {
                steps.Add(AxisQualificationStatusStep(
                    AxisQualificationPowerOffState));
            }
            steps.Add(CapabilitiesStep(21, capabilities));
            steps.Add(CloseStep());

            var root = CreateAxisQualificationTemporaryDirectory();
            MainWindow window = null;
            var cancelHookCalls = 0;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(root, server.Port);
                    ConnectAndLoadAxisForQualification(window);

                    window.TextAxisQualificationDelta.Text = deltaRaw.ToString(
                        CultureInfo.InvariantCulture);
                    window.TextAxisQualificationVelocity.Text =
                        velocityRaw.ToString(CultureInfo.InvariantCulture);
                    window.TextAxisQualificationAcceleration.Text =
                        accelerationRaw.ToString(CultureInfo.InvariantCulture);
                    window.TextAxisQualificationDeceleration.Text =
                        decelerationRaw.ToString(CultureInfo.InvariantCulture);
                    window.TextAxisQualificationJerk.Text = jerkRaw.ToString(
                        CultureInfo.InvariantCulture);
                    window.TextAxisQualificationTolerance.Text = "5";
                    PumpDispatcherOnce();
                    window.CheckAxisQualificationTravelSafe.IsChecked = true;
                    window.CheckAxisQualificationIdentitySafe.IsChecked = true;
                    window.CheckAxisQualificationExclusiveOwner.IsChecked = true;
                    PumpDispatcherOnce();

                    window.AxisQualificationMotionObservedTestHook = () =>
                    {
                        cancelHookCalls++;
                        Click(window.ButtonCancelAxisQualification);
                    };
                    Click(window.ButtonRunAxisQualification);
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "SingleAxisPowerMoveStopPowerOff aborted",
                            StringComparison.Ordinal),
                        "Cancel after Move ACK did not finish non-cancelable safe cleanup.",
                        15000);
                    window.AxisQualificationMotionObservedTestHook = null;

                    AssertEx.Equal(1, cancelHookCalls);
                    AssertEx.Equal(1, CountAxisPowerCommand(server, true));
                    AssertEx.Equal(
                        1,
                        CountCommand(server, 0x20A0),
                        "Cancel replayed or omitted the accepted Move Relative command.");
                    AssertEx.Equal(1, CountCommand(server, 0x2022));
                    AssertEx.Equal(1, CountAxisPowerCommand(server, false));
                    AssertEx.Equal(0, CountCommand(server, 0x209F));
                    AssertEx.Equal(11, CountCommand(server, 0x2028));
                    AssertEx.Equal(1, CountCommand(server, 0x202E));
                    AssertEx.Contains(
                        "event=CANCEL_REQUEST",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "event=AXIS_PRIMARY_PATH_INCOMPLETE",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "moveReplayCount=0",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "event=AXIS_STOP_PROOF|command=0x2022|commandCount=1",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "event=AXIS_POWER_OFF_PROOF|command=0x2023|commandCount=1",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "safeState=POWER_OFF_STANDSTILL",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "powerOffStableProven=True",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "event=END|verdict=ABORTED",
                        window.TextAxisQualificationSummary.Text);
                    AssertAxisQualificationDurableRecordsInactive(window);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                if (window != null)
                {
                    window.AxisQualificationMotionObservedTestHook = null;
                }
                CloseWindowBestEffort(window);
                DeleteAxisQualificationTemporaryDirectory(root);
            }
        }

        private static void
            AxisQualificationExternalStopIsReusedWithoutDuplicateSafetyMutation()
        {
            const int deltaRaw = 120;
            const int velocityRaw = 230;
            const int accelerationRaw = 340;
            const int decelerationRaw = 450;
            const int jerkRaw = 0;
            const int startPositionRaw = 1000;

            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(AxisQualificationReference));
            steps.Add(D5AxisInfoStep(AxisQualificationReference));

            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(AxisQualificationPowerStep(true));
            for (var sample = 0; sample < 3; sample++)
            {
                steps.Add(AxisQualificationStatusStep(
                    AxisQualificationReadyState));
            }
            steps.Add(CapabilitiesStep(13, capabilities));
            steps.Add(CapabilitiesStep(14, capabilities));
            steps.Add(AxisQualificationStatusStep(
                AxisQualificationReadyState));
            steps.Add(AxisQualificationPositionStep(startPositionRaw));
            steps.Add(CapabilitiesStep(15, capabilities));
            steps.Add(AxisQualificationMoveRelativeStep(
                deltaRaw,
                velocityRaw,
                accelerationRaw,
                decelerationRaw,
                jerkRaw));
            steps.Add(AxisQualificationStatusStep(
                AxisQualificationMovingState));

            // The ordinary Axis Stop button owns the only Stop mutation.
            // Its normal durable proof resolves both the Stop record and the
            // active motion record before the runner adopts it.
            steps.Add(CapabilitiesStep(16, capabilities));
            steps.Add(AxisQualificationStopStep(
                decelerationRaw,
                jerkRaw));
            for (var sample = 0; sample < 3; sample++)
            {
                steps.Add(AxisQualificationStatusStep(
                    AxisQualificationReadyState));
            }
            steps.Add(CapabilitiesStep(17, capabilities));
            steps.Add(CapabilitiesStep(18, capabilities));

            // The cancelled runner performs status-only adoption. It must not
            // replay Stop. Power Off remains necessary and is sent once by
            // the runner after the external Stop proof is adopted.
            for (uint requestId = 19; requestId <= 21; requestId++)
            {
                steps.Add(CapabilitiesStep(requestId, capabilities));
                steps.Add(AxisQualificationStatusStep(
                    AxisQualificationReadyState));
            }
            steps.Add(CapabilitiesStep(22, capabilities));
            steps.Add(AxisQualificationPowerStep(false));
            for (var sample = 0; sample < 3; sample++)
            {
                steps.Add(AxisQualificationStatusStep(
                    AxisQualificationPowerOffState));
            }
            steps.Add(CapabilitiesStep(23, capabilities));
            steps.Add(CloseStep());

            var root = CreateAxisQualificationTemporaryDirectory();
            MainWindow window = null;
            var safetyHookCalls = 0;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(root, server.Port);
                    ConnectAndLoadAxisForQualification(window);
                    window.ComboAxisUnit.SelectedIndex = 0;
                    window.TextDeceleration.Text = decelerationRaw.ToString(
                        CultureInfo.InvariantCulture);
                    window.TextJerk.Text = jerkRaw.ToString(
                        CultureInfo.InvariantCulture);
                    ConfigureAxisQualificationForTest(
                        window,
                        deltaRaw,
                        velocityRaw,
                        accelerationRaw,
                        decelerationRaw,
                        jerkRaw,
                        5);

                    window.AxisQualificationMotionObservedTestHook = () =>
                    {
                        window.AxisQualificationMotionObservedTestHook = null;
                        safetyHookCalls++;
                        AssertEx.True(
                            window.ButtonStop.IsEnabled,
                            "Axis Stop was not available during live qualification motion.");
                        Click(window.ButtonStop);
                    };
                    Click(window.ButtonRunAxisQualification);
                    try
                    {
                        WaitUntil(
                            () => string.Equals(
                                window.TextOperationState.Text,
                                "SingleAxisPowerMoveStopPowerOff aborted",
                                StringComparison.Ordinal),
                            "External Axis Stop was not adopted by the cancelled qualification runner.",
                            15000);
                    }
                    catch (TimeoutException error)
                    {
                        throw new TimeoutException(
                            error.Message
                            + " State="
                            + window.TextOperationState.Text
                            + Environment.NewLine
                            + window.TextExecutionLog.Text,
                            error);
                    }

                    AssertEx.Equal(1, safetyHookCalls);
                    AssertEx.Equal(1, CountAxisPowerCommand(server, true));
                    AssertEx.Equal(1, CountCommand(server, 0x20A0));
                    AssertEx.Equal(
                        1,
                        CountCommand(server, 0x2022),
                        "The qualification runner replayed the external Axis Stop.");
                    AssertEx.Equal(1, CountAxisPowerCommand(server, false));
                    AssertEx.Equal(0, CountCommand(server, 0x209F));
                    AssertEx.Equal(14, CountCommand(server, 0x2028));
                    AssertEx.Equal(1, CountCommand(server, 0x202E));
                    AssertEx.Contains(
                        "event=AXIS_EXTERNAL_SAFETY_VERIFY|operation=Axis Stop|mode=status_only|automaticReplayCount=0|verdict=START",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "operation=Axis Stop|safeState=STANDSTILL|samples=3|automaticReplayCount=0|verdict=PASS",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "event=AXIS_POWER_OFF_PROOF|command=0x2023|commandCount=1",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "powerOn2023=1|moveRelative20A0=1|moveAbsolute209F=0|stop2022=0|powerOff2023=1",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "status2028=11|position202E=1",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "event=END|verdict=ABORTED|reason=Axis Stop",
                        window.TextAxisQualificationSummary.Text);
                    AssertAxisQualificationDurableRecordsInactive(window);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                if (window != null)
                {
                    window.AxisQualificationMotionObservedTestHook = null;
                }
                CloseWindowBestEffort(window);
                DeleteAxisQualificationTemporaryDirectory(root);
            }
        }

        private static void
            AxisQualificationExternalPowerOffIsReusedWithoutDuplicateSafetyMutation()
        {
            const int deltaRaw = 120;
            const int velocityRaw = 230;
            const int accelerationRaw = 340;
            const int decelerationRaw = 450;
            const int jerkRaw = 0;
            const int startPositionRaw = 1000;

            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(AxisQualificationReference));
            steps.Add(D5AxisInfoStep(AxisQualificationReference));

            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(AxisQualificationPowerStep(true));
            for (var sample = 0; sample < 3; sample++)
            {
                steps.Add(AxisQualificationStatusStep(
                    AxisQualificationReadyState));
            }
            steps.Add(CapabilitiesStep(13, capabilities));
            steps.Add(CapabilitiesStep(14, capabilities));
            steps.Add(AxisQualificationStatusStep(
                AxisQualificationReadyState));
            steps.Add(AxisQualificationPositionStep(startPositionRaw));
            steps.Add(CapabilitiesStep(15, capabilities));
            steps.Add(AxisQualificationMoveRelativeStep(
                deltaRaw,
                velocityRaw,
                accelerationRaw,
                decelerationRaw,
                jerkRaw));
            steps.Add(AxisQualificationStatusStep(
                AxisQualificationMovingState));

            // The ordinary Axis Power Off button owns the only Power Off
            // mutation and resolves both durable power and motion evidence.
            steps.Add(CapabilitiesStep(16, capabilities));
            steps.Add(AxisQualificationPowerStep(false));
            for (var sample = 0; sample < 3; sample++)
            {
                steps.Add(AxisQualificationStatusStep(
                    AxisQualificationPowerOffState));
            }
            steps.Add(CapabilitiesStep(17, capabilities));
            steps.Add(CapabilitiesStep(18, capabilities));

            // The cancelled runner performs only three identity-pinned status
            // reads. It must replay neither Stop nor Power Off.
            for (uint requestId = 19; requestId <= 21; requestId++)
            {
                steps.Add(CapabilitiesStep(requestId, capabilities));
                steps.Add(AxisQualificationStatusStep(
                    AxisQualificationPowerOffState));
            }
            steps.Add(CloseStep());

            var root = CreateAxisQualificationTemporaryDirectory();
            MainWindow window = null;
            var safetyHookCalls = 0;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(root, server.Port);
                    ConnectAndLoadAxisForQualification(window);
                    ConfigureAxisQualificationForTest(
                        window,
                        deltaRaw,
                        velocityRaw,
                        accelerationRaw,
                        decelerationRaw,
                        jerkRaw,
                        5);

                    window.AxisQualificationMotionObservedTestHook = () =>
                    {
                        window.AxisQualificationMotionObservedTestHook = null;
                        safetyHookCalls++;
                        AssertEx.True(
                            window.ButtonPowerOff.IsEnabled,
                            "Axis Power Off was not available during live qualification motion.");
                        Click(window.ButtonPowerOff);
                    };
                    Click(window.ButtonRunAxisQualification);
                    try
                    {
                        WaitUntil(
                            () => string.Equals(
                                window.TextOperationState.Text,
                                "SingleAxisPowerMoveStopPowerOff aborted",
                                StringComparison.Ordinal),
                            "External Axis Power Off was not adopted by the cancelled qualification runner.",
                            15000);
                    }
                    catch (TimeoutException error)
                    {
                        throw new TimeoutException(
                            error.Message
                            + " State="
                            + window.TextOperationState.Text
                            + Environment.NewLine
                            + window.TextExecutionLog.Text,
                            error);
                    }

                    AssertEx.Equal(1, safetyHookCalls);
                    AssertEx.Equal(1, CountAxisPowerCommand(server, true));
                    AssertEx.Equal(1, CountCommand(server, 0x20A0));
                    AssertEx.Equal(
                        0,
                        CountCommand(server, 0x2022),
                        "The qualification runner sent Stop after external Axis Power Off.");
                    AssertEx.Equal(
                        1,
                        CountAxisPowerCommand(server, false),
                        "The qualification runner replayed the external Axis Power Off.");
                    AssertEx.Equal(0, CountCommand(server, 0x209F));
                    AssertEx.Equal(11, CountCommand(server, 0x2028));
                    AssertEx.Equal(1, CountCommand(server, 0x202E));
                    AssertEx.Contains(
                        "event=AXIS_EXTERNAL_SAFETY_VERIFY|operation=Axis Power Off|mode=status_only|automaticReplayCount=0|verdict=START",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "operation=Axis Power Off|safeState=POWER_OFF_STANDSTILL|samples=3|automaticReplayCount=0|verdict=PASS",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "powerOn2023=1|moveRelative20A0=1|moveAbsolute209F=0|stop2022=0|powerOff2023=0",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "status2028=8|position202E=1",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "powerOffStableProven=True",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "event=END|verdict=ABORTED|reason=Axis Power Off",
                        window.TextAxisQualificationSummary.Text);
                    AssertAxisQualificationDurableRecordsInactive(window);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                if (window != null)
                {
                    window.AxisQualificationMotionObservedTestHook = null;
                }
                CloseWindowBestEffort(window);
                DeleteAxisQualificationTemporaryDirectory(root);
            }
        }

        private static void ConfigureAxisQualificationForTest(
            MainWindow window,
            int deltaRaw,
            int velocityRaw,
            int accelerationRaw,
            int decelerationRaw,
            int jerkRaw,
            int toleranceRaw)
        {
            window.TextAxisQualificationDelta.Text = deltaRaw.ToString(
                CultureInfo.InvariantCulture);
            window.TextAxisQualificationVelocity.Text = velocityRaw.ToString(
                CultureInfo.InvariantCulture);
            window.TextAxisQualificationAcceleration.Text =
                accelerationRaw.ToString(CultureInfo.InvariantCulture);
            window.TextAxisQualificationDeceleration.Text =
                decelerationRaw.ToString(CultureInfo.InvariantCulture);
            window.TextAxisQualificationJerk.Text = jerkRaw.ToString(
                CultureInfo.InvariantCulture);
            window.TextAxisQualificationTolerance.Text = toleranceRaw.ToString(
                CultureInfo.InvariantCulture);
            PumpDispatcherOnce();

            window.CheckAxisQualificationTravelSafe.IsChecked = true;
            window.CheckAxisQualificationIdentitySafe.IsChecked = true;
            window.CheckAxisQualificationExclusiveOwner.IsChecked = true;
            PumpDispatcherOnce();
            AssertEx.True(
                window.ButtonRunAxisQualification.IsEnabled,
                "The fully confirmed live Axis qualification did not open its Run gate.");
        }

        private static void AssertAxisQualificationDurableRecordsInactive(
            MainWindow window)
        {
            AssertEx.False((bool)GetPrivateField(
                window,
                "motionMayBeActive"));
            AssertEx.False(
                ((MotionUncertaintyJournal)GetPrivateField(
                    window,
                    "motionUncertaintyJournal")).HasActiveRecord);
            AssertEx.False(
                ((AxisPowerOnRecoveryJournal)GetPrivateField(
                    window,
                    "axisPowerOnRecoveryJournal")).HasActiveRecord);
            AssertEx.False(
                ((AxisCommandRecoveryJournal)GetPrivateField(
                    window,
                    "axisCommandRecoveryJournal")).HasActiveRecord);

            var qualificationJournal =
                (AxisQualificationRecoveryJournal)GetPrivateField(
                    window,
                    "axisQualificationRecoveryJournal");
            AssertEx.False(
                qualificationJournal.HasActiveRecord,
                "The parent Axis qualification sequence remains active after safe completion.");
            AssertEx.NotNull(
                qualificationJournal.CurrentRecord,
                "The parent Axis qualification sequence was never persisted.");
            AssertEx.Equal(
                AxisQualificationRecoveryStage.SafeResolved,
                qualificationJournal.CurrentRecord.Stage,
                "The inactive parent Axis qualification sequence is not durably SafeResolved.");
        }

        private static void
            AssertAxisQualificationDurableRecordsNeverArmed(MainWindow window)
        {
            AssertEx.False((bool)GetPrivateField(
                window,
                "motionMayBeActive"));
            AssertEx.False(
                ((MotionUncertaintyJournal)GetPrivateField(
                    window,
                    "motionUncertaintyJournal")).HasActiveRecord);
            AssertEx.False(
                ((AxisPowerOnRecoveryJournal)GetPrivateField(
                    window,
                    "axisPowerOnRecoveryJournal")).HasActiveRecord);
            AssertEx.False(
                ((AxisCommandRecoveryJournal)GetPrivateField(
                    window,
                    "axisCommandRecoveryJournal")).HasActiveRecord);

            var qualificationJournal =
                (AxisQualificationRecoveryJournal)GetPrivateField(
                    window,
                    "axisQualificationRecoveryJournal");
            AssertEx.False(qualificationJournal.HasActiveRecord);
            AssertEx.Equal(
                null,
                qualificationJournal.CurrentRecord,
                "A zero-wire qualification path persisted a parent sequence record.");
        }

        private static FakeRpcStep AxisQualificationCapabilitiesStep(
            uint requestId,
            LMCDiagnosticCapability capabilities,
            uint diagnosticsBuild)
        {
            var payload = CapabilitiesPayload(requestId, capabilities, 0);
            TestFrame.WriteUInt32(payload, 16, diagnosticsBuild);
            return new FakeRpcStep(
                0x7E00,
                TestFrame.Response(0, payload))
            {
                InspectRequest = request => AssertEx.Equal(
                    requestId,
                    TestFrame.ReadUInt32(request, 12))
            };
        }

        private static FakeRpcStep AxisQualificationPowerStep(bool powerOn)
        {
            return new FakeRpcStep(
                0x2023,
                TestFrame.Response(0, new byte[4]))
            {
                InspectRequest = request =>
                {
                    AssertEx.Equal(16, request.Length);
                    AssertEx.Equal(
                        AxisQualificationReference,
                        TestFrame.ReadUInt16(request, 6));
                    AssertEx.Equal(1, TestFrame.ReadInt32(request, 8));
                    AssertEx.Equal(
                        (byte)(powerOn ? 1 : 0),
                        request[12]);
                    AssertEx.Equal((byte)1, request[13]);
                    AssertEx.Equal((byte)0, request[14]);
                    AssertEx.Equal((byte)1, request[15]);
                }
            };
        }

        private static FakeRpcStep AxisQualificationMoveRelativeStep(
            int deltaRaw,
            int velocityRaw,
            int accelerationRaw,
            int decelerationRaw,
            int jerkRaw)
        {
            return new FakeRpcStep(
                0x20A0,
                TestFrame.Response(0, new byte[4]))
            {
                InspectRequest = request =>
                {
                    AssertEx.Equal(40, request.Length);
                    AssertEx.Equal(
                        AxisQualificationReference,
                        TestFrame.ReadUInt16(request, 6));
                    AssertEx.Equal(deltaRaw, TestFrame.ReadInt32(request, 8));
                    AssertEx.Equal(
                        velocityRaw,
                        TestFrame.ReadInt32(request, 12));
                    AssertEx.Equal(
                        accelerationRaw,
                        TestFrame.ReadInt32(request, 16));
                    AssertEx.Equal(
                        decelerationRaw,
                        TestFrame.ReadInt32(request, 20));
                    AssertEx.Equal(jerkRaw, TestFrame.ReadInt32(request, 24));
                    AssertEx.Equal(
                        (int)LMC_DIRECTION.Shortest,
                        TestFrame.ReadInt32(request, 28));
                    AssertEx.Equal(1, TestFrame.ReadInt32(request, 32));
                    AssertEx.Equal(1, TestFrame.ReadInt32(request, 36));
                }
            };
        }

        private static FakeRpcStep AxisQualificationStopStep(
            int decelerationRaw,
            int jerkRaw)
        {
            return new FakeRpcStep(
                0x2022,
                TestFrame.Response(0, new byte[4]))
            {
                InspectRequest = request =>
                {
                    AssertEx.Equal(24, request.Length);
                    AssertEx.Equal(
                        AxisQualificationReference,
                        TestFrame.ReadUInt16(request, 6));
                    AssertEx.Equal(
                        decelerationRaw,
                        TestFrame.ReadInt32(request, 8));
                    AssertEx.Equal(jerkRaw, TestFrame.ReadInt32(request, 12));
                    AssertEx.Equal(1, TestFrame.ReadInt32(request, 16));
                    AssertEx.Equal(1, TestFrame.ReadInt32(request, 20));
                }
            };
        }

        private static FakeRpcStep AxisQualificationStatusStep(uint state)
        {
            var payload = new byte[12];
            TestFrame.WriteUInt32(payload, 0, state);
            return new FakeRpcStep(
                0x2028,
                TestFrame.Response(0, payload))
            {
                InspectRequest = request => AssertEx.Equal(
                    AxisQualificationReference,
                    TestFrame.ReadUInt16(request, 6))
            };
        }

        private static FakeRpcStep AxisQualificationPositionStep(
            int positionRaw)
        {
            var payload = new byte[8];
            TestFrame.WriteInt32(payload, 0, positionRaw);
            return new FakeRpcStep(
                0x202E,
                TestFrame.Response(0, payload))
            {
                InspectRequest = request => AssertEx.Equal(
                    AxisQualificationReference,
                    TestFrame.ReadUInt16(request, 6))
            };
        }

        private static string CreateAxisQualificationTemporaryDirectory()
        {
            var path = Path.Combine(
                Path.GetTempPath(),
                "ElmoAxisQualificationWpfTests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static void DeleteAxisQualificationTemporaryDirectory(
            string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
    }
}

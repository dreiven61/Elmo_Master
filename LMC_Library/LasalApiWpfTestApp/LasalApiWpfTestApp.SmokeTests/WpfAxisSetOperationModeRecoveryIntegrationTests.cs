using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using LasalMotionControlApiExample;
using LasalMotionControlLib;
using LasalMotionControlLib.Tests;

namespace LasalApiWpfTestApp.SmokeTests
{
    internal static partial class WpfMainWindowIntegrationTests
    {
        internal static void RegisterAxisSetOperationModeRecoveryTests(
            ICollection<TestCase> tests)
        {
            tests.Add("Wpf.SetOperationModeRecovery.PollRunningThenSucceededAndRetire",
                () => SetOperationModePollsToFinalOutcome(2));
            tests.Add("Wpf.SetOperationModeRecovery.PollRunningThenFailedRetiresWithoutPass",
                () => SetOperationModePollsToFinalOutcome(3));
            tests.Add("Wpf.SetOperationModeRecovery.PollRunningThenIndeterminateKeepsFence",
                () => SetOperationModePollsToFinalOutcome(5));
            tests.Add(
                "Wpf.SetOperationModeRecovery.StartupArmedPromotesAndLocksEndpoint",
                SetOperationModeStartupArmedPromotesAndLocksEndpoint);
            tests.Add(
                "Wpf.SetOperationModeRecovery.DynamicUiRequiresExplicitConfirmation",
                SetOperationModeDynamicUiRequiresExplicitConfirmation);
            tests.Add(
                "Wpf.SetOperationModeRecovery.CanonicalStartClickUsesSingleHandler",
                SetOperationModeCanonicalStartClickUsesSingleHandler);
            tests.Add(
                "Wpf.SetOperationModeRecovery.SelectorRemainsUsableWithoutPlcMask",
                SetOperationModeSelectorRemainsUsableWithoutPlcMask);
            tests.Add(
                "Wpf.SetOperationModeRecovery.OutcomeDiagnosticsExposePreflightAndWriteEvidence",
                SetOperationModeOutcomeDiagnosticsExposePreflightAndWriteEvidence);
            tests.Add(
                "Wpf.SetOperationModeRecovery.DefinitiveRejectArchivesAndClearsInterlock",
                SetOperationModeDefinitiveRejectArchivesAndClearsInterlock);
            tests.Add(
                "Wpf.SetOperationModeRecovery.RejectIdentityMismatchRetainsInterlock",
                SetOperationModeRejectIdentityMismatchRetainsInterlock);
        }

        private static void SetOperationModePollsToFinalOutcome(ushort finalState)
        {
            var key = new LMCAxisSetOperationModeRecoveryKey(
                1, 30, 1, DiagnosticsBootId, DiagnosticMapRevision,
                1, 2, 3, 4, 1, LMCDriveOperationMode.ProfilePosition, 5000);
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(new FakeRpcStep(0x7D00, null) { ResponseFactory = request =>
            {
                var payload = CommonPayload(40, TestFrame.ReadUInt32(request, 12));
                TestFrame.WriteUInt32(payload, 16, 0x700);
                TestFrame.WriteUInt16(payload, 28, 4);
                TestFrame.WriteUInt16(payload, 36, 6);
                TestFrame.WriteUInt16(payload, 38, 0x018A);
                return TestFrame.Response(0, payload);
            }});
            steps.Add(CapabilitiesStep(11, capabilities));
            var lookup = new byte[6];
            TestFrame.WriteUInt16(lookup, 4, 1);
            steps.Add(new FakeRpcStep(0x103C, TestFrame.Response(0, lookup)));
            var info = new byte[8];
            TestFrame.WriteUInt32(info, 0, 1);
            steps.Add(new FakeRpcStep(0x202B, TestFrame.Response(0, info)));
            steps.Add(ModePollingOutcomeStep(key, 1, 0x7D24));
            steps.Add(ModePollingOutcomeStep(key, 1, 0x7D24));
            steps.Add(ModePollingOutcomeStep(key, finalState, 0x7D24));
            if (finalState != 5)
                steps.Add(ModePollingOutcomeStep(key, finalState, 0x7D25));
            steps.Add(CloseStep());
            var root = CreateSetOperationModeTemporaryDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(root, server.Port);
                    Click(window.ButtonConnect);
                    WaitForConnectCompleted(window, "Mode polling setup did not connect.");
                    var journal = window.AxisSetOperationModeRecoveryJournalForTests;
                    var armed = journal.ArmBeforeDispatch(Guid.NewGuid(), "127.0.0.1",
                        server.Port, "_LMCAxis1", key, DateTime.UtcNow);
                    journal.PromoteToRecoveryRequired(armed, DateTime.UtcNow.AddMilliseconds(1));
                    window.RefreshAxisSetOperationModeRecoveryUiForTests();
                    Click(window.AxisSetOperationModeRecoverButtonForTests);
                    WaitUntil(() => server.ReceivedRequests.Count(r => TestFrame.ReadUInt16(r, 0) == 0x7D24) >= 1,
                        "No first outcome query.");
                    // Pump the real WPF dispatcher during the await; no early PASS,
                    // no journal release and no conflicting mutation while Running.
                    AssertEx.True(window.AxisSetOperationModeRecoveryInterlockForTests);
                    AssertEx.False(window.AxisSetOperationModeStartButtonForTests.IsEnabled);
                    AssertEx.False(window.TextExecutionLog.Text.Contains("Recover SetOperationMode Outcome PASS."));
                    WaitUntil(() => window.TextOperationState.Text ==
                        (finalState == 2 ? "Recover SetOperationMode Outcome completed" : "Recover SetOperationMode Outcome failed"),
                        "Polling did not settle at the expected final result.");
                    AssertEx.Equal(3, server.ReceivedRequests.Count(r => TestFrame.ReadUInt16(r, 0) == 0x7D24));
                    AssertEx.Equal(0, server.ReceivedRequests.Count(r => TestFrame.ReadUInt16(r, 0) == 0x7D23));
                    AssertEx.Equal(finalState == 5 ? 0 : 1,
                        server.ReceivedRequests.Count(r => TestFrame.ReadUInt16(r, 0) == 0x7D25));
                    AssertEx.Equal(finalState == 5, window.AxisSetOperationModeRecoveryInterlockForTests);
                    if (finalState == 5)
                    {
                        // The final UpdateUiState must not erase the failure guidance.
                        var status = (System.Windows.Controls.TextBlock)GetPrivateField(window,
                            "textAxisSetOperationModeRecoveryStatus");
                        AssertEx.Contains("OUTCOME QUERY REJECTED", status.Text);
                        window.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                            new Action(() => { }));
                        AssertEx.True(window.ButtonCloseConnection.IsEnabled);
                    }
                    if (finalState != 2)
                        AssertEx.False(window.TextExecutionLog.Text.Contains("Recover SetOperationMode Outcome PASS."));
                    CloseConnectedWindow(window);
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteSetOperationModeTemporaryDirectory(root);
            }
        }

        private static FakeRpcStep ModePollingOutcomeStep(
            LMCAxisSetOperationModeRecoveryKey key, ushort state, ushort command)
        {
            return new FakeRpcStep(command, null) { ResponseFactory = request =>
            {
                // Check the original intent and target on every poll/retirement.
                AssertEx.Equal(key.OriginalRequestId, TestFrame.ReadUInt32(request, 32));
                AssertEx.Equal((byte)1, request[54]);
                var id = TestFrame.ReadUInt32(request, 12);
                var payload = CommonPayload(state == 5 ? 16 : 112, id);
                if (state == 5)
                {
                    TestFrame.WriteUInt16(payload, 4, 1);
                    TestFrame.WriteInt16(payload, 6, -31000);
                    TestFrame.WriteUInt32(payload, 12, 46);
                    return TestFrame.Response(0, payload);
                }
                // Echo the frozen key from offset 8 of the request payload.
                Buffer.BlockCopy(request, 20, payload, 20, 44);
                TestFrame.WriteUInt16(payload, 16, state);
                payload[55] = state == 2 ? (byte)1 : (byte)8;
                TestFrame.WriteUInt32(payload, 72, 99);
                TestFrame.WriteUInt32(payload, 76, state == 1 ? 15u : 63u);
                TestFrame.WriteUInt32(payload, 80, 100);
                TestFrame.WriteUInt32(payload, 84, state == 1 ? 0u : 110u);
                TestFrame.WriteUInt32(payload, 92, 7);
                payload[96] = 8;
                TestFrame.WriteUInt16(payload, 104, 0x02D0);
                TestFrame.WriteUInt32(payload, 108, 1);
                if (state == 3)
                {
                    TestFrame.WriteUInt16(payload, 64, 1);
                    TestFrame.WriteInt16(payload, 66, -31000);
                    TestFrame.WriteUInt32(payload, 68, (uint)LMCAdminDetailCode.SetOperationModeExecutionFailed);
                }
                return TestFrame.Response(0, payload);
            }};
        }

        private static void SetOperationModeStartupArmedPromotesAndLocksEndpoint()
        {
            var root = CreateSetOperationModeTemporaryDirectory();
            MainWindow window = null;
            try
            {
                var journalDirectory = Path.Combine(
                    root,
                    "AxisSetOperationModeRecovery");
                using (var journal =
                    AxisSetOperationModeRecoveryJournal.Open(
                        journalDirectory))
                {
                    journal.ArmBeforeDispatch(
                        new Guid("00112233-4455-6677-8899-aabbccddeeff"),
                        "127.0.0.1",
                        4000,
                        "_LMCAxis1",
                        CreateSetOperationModeRecoveryKey(),
                        FixedSetOperationModeUtc());
                    AssertEx.Equal(
                        AxisSetOperationModeRecoveryState.ArmedBeforeDispatch,
                        journal.CurrentRecord.State);
                }

                window = new MainWindow(root);
                var recovered =
                    window.ActiveAxisSetOperationModeRecoveryRecordForTests;
                AssertEx.NotNull(recovered);
                AssertEx.Equal(
                    AxisSetOperationModeRecoveryState.RecoveryRequired,
                    recovered.State);
                AssertEx.True(
                    window.AxisSetOperationModeRecoveryInterlockForTests);
                AssertEx.NotNull(
                    window.AxisSetOperationModeRecoveryGroupForTests);
                AssertEx.False(
                    window.AxisSetOperationModeStartButtonForTests.IsEnabled);
                AssertEx.False(window.TextRemoteIp.IsEnabled);
                AssertEx.False(window.TextRemotePort.IsEnabled);
                AssertEx.Equal("127.0.0.1", window.TextRemoteIp.Text);
                AssertEx.Equal("4000", window.TextRemotePort.Text);
            }
            finally
            {
                if (window != null)
                {
                    window.Close();
                }
                DeleteSetOperationModeTemporaryDirectory(root);
            }
        }

        private static void SetOperationModeDynamicUiRequiresExplicitConfirmation()
        {
            var root = CreateSetOperationModeTemporaryDirectory();
            MainWindow window = null;
            try
            {
                window = new MainWindow(root);
                AssertEx.NotNull(
                    window.AxisSetOperationModeRecoveryGroupForTests);
                AssertEx.NotNull(
                    window.AxisSetOperationModeConfirmationForTests);
                AssertEx.False(
                    window.AxisSetOperationModeConfirmationForTests.IsChecked
                        == true);
                AssertEx.False(
                    window.AxisSetOperationModeStartButtonForTests.IsEnabled);
                AssertEx.False(
                    window.AxisSetOperationModeRecoveryInterlockForTests);

                var journal =
                    window.AxisSetOperationModeRecoveryJournalForTests;
                AssertEx.NotNull(journal);
                journal.ArmBeforeDispatch(
                    Guid.NewGuid(),
                    "127.0.0.1",
                    4000,
                    "_LMCAxis1",
                    CreateSetOperationModeRecoveryKey(),
                    FixedSetOperationModeUtc());
                window.RefreshAxisSetOperationModeRecoveryUiForTests();

                AssertEx.True(
                    window.AxisSetOperationModeRecoveryInterlockForTests);
                AssertEx.False(window.TextRemoteIp.IsEnabled);
                AssertEx.False(window.TextRemotePort.IsEnabled);
                AssertEx.False(
                    window.AxisSetOperationModeStartButtonForTests.IsEnabled);
            }
            finally
            {
                if (window != null)
                {
                    window.Close();
                }
                DeleteSetOperationModeTemporaryDirectory(root);
            }
        }

        private static void SetOperationModeCanonicalStartClickUsesSingleHandler()
        {
            var root = CreateSetOperationModeTemporaryDirectory();
            MainWindow window = null;
            try
            {
                window = new MainWindow(root);
                AssertEx.Equal(
                    0,
                    window.AxisSetOperationModeStartUiHandlerEntryCountForTests);

                window.RaiseAxisSetOperationModeStartClickForTests();

                AssertEx.Equal(
                    1,
                    window.AxisSetOperationModeStartUiHandlerEntryCountForTests);
            }
            finally
            {
                if (window != null)
                {
                    window.Close();
                }
                DeleteSetOperationModeTemporaryDirectory(root);
            }
        }

        private static void SetOperationModeSelectorRemainsUsableWithoutPlcMask()
        {
            var root = CreateSetOperationModeTemporaryDirectory();
            MainWindow window = null;
            try
            {
                window = new MainWindow(root);
                var selector = window.AxisSetOperationModeRequestedModeForTests;
                AssertEx.NotNull(selector);
                AssertEx.Equal(4, selector.Items.Count);
                AssertEx.Equal(
                    LMCDriveOperationMode.CyclicSynchronousPosition,
                    (LMCDriveOperationMode)selector.SelectedItem);
                AssertEx.True(selector.IsEnabled);
                AssertEx.False(window.AxisSetOperationModeStartButtonForTests.IsEnabled);
            }
            finally
            {
                if (window != null)
                {
                    window.Close();
                }
                DeleteSetOperationModeTemporaryDirectory(root);
            }
        }

        private static void
            SetOperationModeOutcomeDiagnosticsExposePreflightAndWriteEvidence()
        {
            var diagnostics = MainWindow
                .FormatAxisSetOperationModeOutcomeDiagnosticsForTests(
                    (sbyte)LMCDriveOperationMode.ProfilePosition,
                    (sbyte)LMCDriveOperationMode.CyclicSynchronousPosition,
                    (sbyte)LMCDriveOperationMode.CyclicSynchronousPosition,
                    LMCAxisSetOperationModeOutcomeRecordState.Failed,
                    1,
                    -31000,
                    (uint)LMCAdminDetailCode.SetOperationModeUnsafeState,
                    LMCAxisSetOperationModeEvidenceFlags.OwnerReleased
                        | LMCAxisSetOperationModeEvidenceFlags.ExecutorReusable,
                    0x0004,
                    0x00000001u,
                    0,
                    7);

            AssertEx.Contains("State=Failed", diagnostics);
            AssertEx.Contains("Requested=ProfilePosition(1)", diagnostics);
            AssertEx.Contains(
                "PreflightMode=CyclicSynchronousPosition(8)",
                diagnostics);
            AssertEx.Contains(
                "Observed=CyclicSynchronousPosition(8)",
                diagnostics);
            AssertEx.Contains(
                "Detail=SetOperationModeUnsafeState(44)",
                diagnostics);
            AssertEx.Contains("DS402=0x0004", diagnostics);
            AssertEx.Contains("Fault=False", diagnostics);
            AssertEx.Contains("OperationEnabled=True", diagnostics);
            AssertEx.Contains("PhysicalValid=True", diagnostics);
            AssertEx.Contains("Standstill=not-exported", diagnostics);
            AssertEx.Contains("WriteRequested=False", diagnostics);
            AssertEx.Contains("WriteDispatched=False", diagnostics);
            AssertEx.Contains("OwnerReleased=True", diagnostics);
            AssertEx.Contains("ExecutorReusable=True", diagnostics);
            AssertEx.Contains("Generation=7", diagnostics);
        }

        private static void
            SetOperationModeDefinitiveRejectArchivesAndClearsInterlock()
        {
            var root = CreateSetOperationModeTemporaryDirectory();
            MainWindow window = null;
            try
            {
                window = new MainWindow(root);
                var journal =
                    window.AxisSetOperationModeRecoveryJournalForTests;
                var key = CreateSetOperationModeRecoveryKey();
                var armed = journal.ArmBeforeDispatch(
                    Guid.NewGuid(),
                    "127.0.0.1",
                    4000,
                    "_LMCAxis1",
                    key,
                    FixedSetOperationModeUtc());
                var recoveryRequired = journal.PromoteToRecoveryRequired(
                    armed,
                    FixedSetOperationModeUtc().AddSeconds(1));
                window.RefreshAxisSetOperationModeRecoveryUiForTests();
                AssertEx.True(
                    window.AxisSetOperationModeRecoveryInterlockForTests);

                var evidencePath =
                    window.ResolveAxisSetOperationModeDefinitiveRejectionForTests(
                        recoveryRequired,
                        key,
                        1,
                        1,
                        -31000,
                        key.OriginalRequestId,
                        44,
                        false);
                window.RefreshAxisSetOperationModeRecoveryUiForTests();

                AssertEx.False(
                    window.AxisSetOperationModeRecoveryInterlockForTests);
                AssertEx.False(
                    window.AxisSetOperationModeRecoveryJournalForTests
                        .HasActiveRecord);
                AssertEx.True(File.Exists(evidencePath));
                var evidence = File.ReadAllText(evidencePath);
                AssertEx.Contains("ELMOASOMREJECT1", evidence);
                AssertEx.Contains(
                    "OriginalRequestId="
                        + key.OriginalRequestId.ToString(
                            CultureInfo.InvariantCulture),
                    evidence);
                AssertEx.Contains("ResponseDetailCode=44", evidence);
                AssertEx.Contains("ResponseErrorId=-31000", evidence);
                AssertEx.Contains(
                    "RequestedModeRaw="
                        + ((sbyte)LMCDriveOperationMode
                            .CyclicSynchronousPosition).ToString(
                                CultureInfo.InvariantCulture),
                    evidence);
                AssertEx.Contains(
                    "DiagnosticsBuild="
                        + key.DiagnosticsBuild.ToString(
                            CultureInfo.InvariantCulture),
                    evidence);
                AssertEx.Contains(
                    "DiagnosticsBootId="
                        + key.DiagnosticsBootId.ToString(
                            CultureInfo.InvariantCulture),
                    evidence);
                AssertEx.Contains(
                    "MapRevision="
                        + key.MapRevision.ToString(
                            CultureInfo.InvariantCulture),
                    evidence);
                AssertEx.Contains("RejectedKeyExact=True", evidence);
                AssertEx.Contains("OriginalJournalSha256=", evidence);
                AssertEx.Contains("OriginalJournalBase64=", evidence);
                AssertEx.Contains("SHA256=", evidence);
            }
            finally
            {
                if (window != null)
                {
                    window.Close();
                }
                DeleteSetOperationModeTemporaryDirectory(root);
            }
        }

        private static void
            SetOperationModeRejectIdentityMismatchRetainsInterlock()
        {
            var root = CreateSetOperationModeTemporaryDirectory();
            MainWindow window = null;
            try
            {
                window = new MainWindow(root);
                var journal =
                    window.AxisSetOperationModeRecoveryJournalForTests;
                var key = CreateSetOperationModeRecoveryKey();
                var armed = journal.ArmBeforeDispatch(
                    Guid.NewGuid(),
                    "127.0.0.1",
                    4000,
                    "_LMCAxis1",
                    key,
                    FixedSetOperationModeUtc());
                var recoveryRequired = journal.PromoteToRecoveryRequired(
                    armed,
                    FixedSetOperationModeUtc().AddSeconds(1));
                var mismatchedKey = new LMCAxisSetOperationModeRecoveryKey(
                    1,
                    key.OriginalRequestId + 1,
                    key.DiagnosticsBuild,
                    key.DiagnosticsBootId,
                    key.MapRevision,
                    key.ClientIntentId0,
                    key.ClientIntentId1,
                    key.ClientIntentId2,
                    key.ClientIntentId3,
                    key.AxisReference,
                    LMCDriveOperationMode.CyclicSynchronousPosition,
                    key.TimeoutMilliseconds);

                AssertEx.Throws<InvalidOperationException>(
                    () => window
                        .ResolveAxisSetOperationModeDefinitiveRejectionForTests(
                            recoveryRequired,
                            mismatchedKey,
                            1,
                            1,
                            -31000,
                            key.OriginalRequestId,
                            44,
                            false));

                AssertEx.True(
                    window.AxisSetOperationModeRecoveryJournalForTests
                        .HasActiveRecord);
                AssertEx.Equal(
                    AxisSetOperationModeRecoveryState.RecoveryRequired,
                    window.AxisSetOperationModeRecoveryJournalForTests
                        .CurrentRecord.State);
                AssertEx.Equal(
                    0,
                    Directory.GetFiles(
                        Path.Combine(root, "AxisSetOperationModeRecovery"),
                        "*.evidence").Length);
            }
            finally
            {
                if (window != null)
                {
                    window.Close();
                }
                DeleteSetOperationModeTemporaryDirectory(root);
            }
        }

        private static LMCAxisSetOperationModeRecoveryKey
            CreateSetOperationModeRecoveryKey()
        {
            return new LMCAxisSetOperationModeRecoveryKey(
                1,
                0x10203040u,
                0x11223344u,
                0x55667788u,
                0x99AABBCCu,
                0x01020304u,
                0x11121314u,
                0x21222324u,
                0x31323334u,
                1,
                LMCDriveOperationMode.CyclicSynchronousPosition,
                5000u);
        }

        private static DateTime FixedSetOperationModeUtc()
        {
            return new DateTime(
                2026,
                8,
                24,
                8,
                0,
                0,
                DateTimeKind.Utc);
        }

        private static string CreateSetOperationModeTemporaryDirectory()
        {
            var path = Path.Combine(
                Path.GetTempPath(),
                "ElmoSetOperationModeWpfSmoke",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static void DeleteSetOperationModeTemporaryDirectory(
            string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                return;
            }

            try
            {
                Directory.Delete(path, true);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}

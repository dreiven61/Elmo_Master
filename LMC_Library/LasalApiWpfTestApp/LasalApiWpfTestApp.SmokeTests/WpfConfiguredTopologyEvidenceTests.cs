using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using LasalMotionControlApiExample;
using LasalMotionControlLib;
using LasalMotionControlLib.Tests;

namespace LasalApiWpfTestApp.SmokeTests
{
    internal static partial class WpfMainWindowIntegrationTests
    {
        private static void RegisterConfiguredTopologyEvidenceTests(
            ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.Topology.ReloadComparisonInitialUnchangedChangedAndReport",
                ReloadComparisonInitialUnchangedChangedAndReport);
            tests.Add(
                "Wpf.Topology.FailedAndStaleReloadPreserveSuccessfulBaseline",
                FailedAndStaleReloadPreserveSuccessfulBaseline);
        }

        private static void
            ReloadComparisonInitialUnchangedChangedAndReport()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var canonical = CreateTopologyCanonicalBytes();
            var changedCanonical = CreateChangedTopologyCanonicalBytes();
            var changedRevision = ComputeTopologyRevision(changedCanonical);
            var steps = CreateConnectAndTopologySteps(capabilities);
            var requestId = 11u;
            AppendTopologyReloadSteps(
                steps,
                capabilities,
                canonical,
                TopologyRevision,
                ref requestId);
            AppendTopologyReloadSteps(
                steps,
                capabilities,
                changedCanonical,
                changedRevision,
                ref requestId);
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
                        () => HasConfiguredTopologyComparison(
                                window,
                                ConfiguredTopologyComparisonKind.Initial)
                            && window.GridEtherCATTopology.Items.Count
                                == TopologyNodeCount
                            && window.ButtonLoadEtherCATTopology.IsEnabled,
                        "The automatic topology load did not create the INITIAL configured baseline.");

                    var initial = GetConfiguredTopologySnapshot(window);
                    var initialEvidence = GetConfiguredTopologyEvidence(window);
                    AssertEx.Contains(
                        "ConfiguredComparison=INITIAL",
                        window.TextEtherCATTopologySummary.Text);
                    AssertEx.Contains(
                        "Configured comparison=INITIAL",
                        window.TextConfiguredTopologyComparison.Text);
                    AssertEx.Contains(
                        "BOUNDARY=CONFIGURED SCHEMA ONLY",
                        initialEvidence);
                    AssertEx.Contains(
                        "NOT PROOF=runtime EtherCAT discovery",
                        initialEvidence);
                    AssertEx.Contains(
                        "Endpoint=127.0.0.1:"
                            + server.Port.ToString(
                                CultureInfo.InvariantCulture),
                        initialEvidence);
                    AssertEx.True(
                        window.ButtonSaveConfiguredTopologyEvidence.IsEnabled);

                    var topology = (LMCEtherCATTopology)GetPrivateField(
                        window,
                        "etherCATTopology");
                    var differentEndpoint =
                        ConfiguredTopologySnapshot.Capture(
                            topology,
                            "127.0.0.2:4000",
                            999,
                            "endpoint policy test",
                            DateTime.UtcNow);
                    var endpointComparison =
                        ConfiguredTopologyComparison.Compare(
                            initial,
                            differentEndpoint);
                    AssertEx.Equal(
                        ConfiguredTopologyComparisonKind.Initial,
                        endpointComparison.Kind);
                    AssertEx.Contains(
                        "endpoint changed",
                        endpointComparison.Reason.ToLowerInvariant());

                    Click(window.ButtonLoadEtherCATTopology);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Load EtherCAT Topology completed",
                                StringComparison.Ordinal)
                            || string.Equals(
                                window.TextOperationState.Text,
                                "Load EtherCAT Topology failed",
                                StringComparison.Ordinal),
                        "The identical manual reload did not reach a terminal UI state.");
                    AssertEx.Equal(
                        ConfiguredTopologyComparisonKind.Unchanged,
                        GetConfiguredTopologyComparison(window).Kind,
                        "The identical manual reload was not classified UNCHANGED. Summary="
                            + window.TextEtherCATTopologySummary.Text
                            + " Log="
                            + window.TextExecutionLog.Text);
                    WaitUntil(
                        () => window.ButtonLoadEtherCATTopology.IsEnabled,
                        "The reload button did not re-enable after an identical topology load.");
                    var unchanged = GetConfiguredTopologySnapshot(window);
                    AssertEx.Equal(initial.Sha256, unchanged.Sha256);
                    AssertEx.Contains(
                        "UNCHANGED CONFIGURED SCHEMA",
                        window.TextConfiguredTopologyComparison.Text);

                    Click(window.ButtonLoadEtherCATTopology);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Load EtherCAT Topology completed",
                                StringComparison.Ordinal)
                            || string.Equals(
                                window.TextOperationState.Text,
                                "Load EtherCAT Topology failed",
                                StringComparison.Ordinal),
                        "The modified manual reload did not reach a terminal UI state.");
                    AssertEx.Equal(
                        ConfiguredTopologyComparisonKind.Changed,
                        GetConfiguredTopologyComparison(window).Kind,
                        "The modified manual reload was not classified CHANGED. Summary="
                            + window.TextEtherCATTopologySummary.Text
                            + " Log="
                            + window.TextExecutionLog.Text);
                    var changed = GetConfiguredTopologySnapshot(window);
                    AssertEx.False(
                        string.Equals(
                            unchanged.Sha256,
                            changed.Sha256,
                            StringComparison.Ordinal),
                        "A changed configured topology retained the old SHA-256.");
                    AssertEx.Equal(changedRevision, changed.TopologyRevision);
                    AssertEx.Contains(
                        "MODIFIED ORDER[0006]",
                        window.TextConfiguredTopologyComparison.Text);
                    AssertEx.Contains(
                        "SerialNumber=0x01020304",
                        window.TextConfiguredTopologyComparison.Text);
                    AssertEx.Contains(
                        "ConfiguredComparison=CHANGED",
                        window.TextEtherCATTopologySummary.Text);

                    var changedEvidence = GetConfiguredTopologyEvidence(window);
                    AssertEx.Contains("Comparison=CHANGED", changedEvidence);
                    AssertEx.Contains(
                        "[CURRENT ORDERED CONFIGURED ENTRIES]",
                        changedEvidence);
                    AssertEx.Contains(changed.Sha256, changedEvidence);
                    AssertEx.Contains(
                        "BootId=0x10203040",
                        changedEvidence);
                    AssertEx.Contains(
                        "MapRevision=0xE245539A",
                        changedEvidence);

                    var evidencePath = Path.Combine(
                        journalDirectory,
                        "configured-topology-evidence.txt");
                    ConfiguredTopologyComparison.SaveEvidence(
                        evidencePath,
                        changedEvidence);
                    var savedBytes = File.ReadAllBytes(evidencePath);
                    AssertEx.True(savedBytes.Length != 0);
                    AssertEx.False(
                        savedBytes.Length >= 3
                            && savedBytes[0] == 0xEF
                            && savedBytes[1] == 0xBB
                            && savedBytes[2] == 0xBF,
                        "Configured topology evidence was written with a UTF-8 BOM.");
                    AssertEx.Equal(
                        changedEvidence,
                        Encoding.UTF8.GetString(savedBytes));

                    Click(window.ButtonCloseConnection);
                    WaitUntil(
                        () => string.Equals(
                            window.TextConnectionState.Text,
                            "Disconnected",
                            StringComparison.Ordinal),
                        "The WPF connection did not close.");
                    AssertEx.True(
                        window.ButtonSaveConfiguredTopologyEvidence.IsEnabled,
                        "Last-success evidence was disabled after disconnect.");
                    AssertEx.Contains(
                        "Last successful baseline preserved",
                        window.TextConfiguredTopologyComparison.Text);
                    window.Close();
                    WaitUntil(
                        () => !window.IsLoaded,
                        "The WPF window did not close.");
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
            FailedAndStaleReloadPreserveSuccessfulBaseline()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var changedCanonical = CreateChangedTopologyCanonicalBytes();
            var changedRevision = ComputeTopologyRevision(changedCanonical);
            var delayedFinalChunkRequest = new ManualResetEventSlim(false);
            var steps = CreateConnectAndTopologySteps(capabilities);
            var requestId = 11u;

            steps.Add(CapabilitiesStep(requestId++, capabilities));
            steps.Add(CapabilitiesStep(requestId++, capabilities));
            steps.Add(new FakeRpcStep(
                0x7E11,
                TestFrame.Response(7, new byte[0])));
            requestId++;

            steps.Add(CapabilitiesStep(requestId++, capabilities));
            steps.Add(CapabilitiesStep(requestId++, capabilities));
            steps.Add(new FakeRpcStep(
                0x7E11,
                TestFrame.Response(
                    0,
                    TopologyInfoPayload(requestId++, changedRevision))));
            for (ushort startIndex = 0;
                startIndex < TopologyNodeCount;
                startIndex++)
            {
                var chunkStep = new FakeRpcStep(
                    0x7E12,
                    TestFrame.Response(
                        0,
                        TopologyChunkPayload(
                            requestId++,
                            startIndex,
                            changedCanonical,
                            changedRevision)));
                if (startIndex == TopologyNodeCount - 1)
                {
                    chunkStep.InspectRequest = request =>
                        delayedFinalChunkRequest.Set();
                    chunkStep.ResponseDelayMilliseconds = 350;
                }

                steps.Add(chunkStep);
            }

            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (delayedFinalChunkRequest)
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(journalDirectory, server.Port);
                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => HasConfiguredTopologyComparison(
                            window,
                            ConfiguredTopologyComparisonKind.Initial)
                            && window.ButtonLoadEtherCATTopology.IsEnabled,
                        "The initial configured topology baseline was not created.");

                    var baseline = GetConfiguredTopologySnapshot(window);
                    var baselineEvidence = GetConfiguredTopologyEvidence(window);
                    Click(window.ButtonLoadEtherCATTopology);
                    WaitUntil(
                        () => window.TextEtherCATTopologySummary.Text.IndexOf(
                            "LOAD FAILED (manual reload)",
                            StringComparison.Ordinal) >= 0
                            && window.ButtonLoadEtherCATTopology.IsEnabled,
                        "The ordinary topology reload failure was not rendered.");
                    AssertSuccessfulConfiguredBaselineUnchanged(
                        window,
                        baseline,
                        baselineEvidence,
                        "ordinary failed reload");
                    AssertEx.Contains(
                        "LOAD FAILED",
                        window.TextConfiguredTopologyComparison.Text);
                    AssertEx.Equal(0, window.GridEtherCATTopology.Items.Count);

                    Click(window.ButtonLoadEtherCATTopology);
                    WaitUntil(
                        () => delayedFinalChunkRequest.IsSet,
                        "The delayed final topology chunk was not requested.");
                    var originalConnection = (LMCConnection)GetPrivateField(
                        window,
                        "connection");
                    SetPrivateField(window, "connection", null);
                    WaitUntil(
                        () => window.TextConfiguredTopologyComparison.Text
                            .IndexOf(
                                "disconnected or replaced connection session",
                                StringComparison.Ordinal) >= 0,
                        "The stale-session topology response was not rejected.");
                    SetPrivateField(window, "connection", originalConnection);
                    InvokePrivate(window, "UpdateUiState");

                    AssertSuccessfulConfiguredBaselineUnchanged(
                        window,
                        baseline,
                        baselineEvidence,
                        "stale-session reload");
                    AssertEx.Equal(0, window.GridEtherCATTopology.Items.Count);
                    AssertEx.Contains(
                        "last successful comparison baseline and evidence remain unchanged",
                        window.TextConfiguredTopologyComparison.Text);

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

        private static void AppendTopologyReloadSteps(
            IList<FakeRpcStep> steps,
            LMCDiagnosticCapability capabilities,
            byte[] canonical,
            uint topologyRevision,
            ref uint requestId)
        {
            steps.Add(CapabilitiesStep(requestId++, capabilities));
            steps.Add(CapabilitiesStep(requestId++, capabilities));
            steps.Add(new FakeRpcStep(
                0x7E11,
                TestFrame.Response(
                    0,
                    TopologyInfoPayload(requestId++, topologyRevision))));
            for (ushort startIndex = 0;
                startIndex < TopologyNodeCount;
                startIndex++)
            {
                steps.Add(new FakeRpcStep(
                    0x7E12,
                    TestFrame.Response(
                        0,
                        TopologyChunkPayload(
                            requestId++,
                            startIndex,
                            canonical,
                            topologyRevision))));
            }
        }

        private static byte[] CreateChangedTopologyCanonicalBytes()
        {
            var canonical = CreateTopologyCanonicalBytes();
            TestFrame.WriteUInt32(
                canonical,
                (6 * 96) + 36,
                0x01020304u);
            return canonical;
        }

        private static ConfiguredTopologySnapshot
            GetConfiguredTopologySnapshot(MainWindow window)
        {
            var snapshot = (ConfiguredTopologySnapshot)GetPrivateField(
                window,
                "lastSuccessfulConfiguredTopologySnapshot");
            AssertEx.NotNull(snapshot);
            return snapshot;
        }

        private static ConfiguredTopologyComparison
            GetConfiguredTopologyComparison(MainWindow window)
        {
            var comparison = (ConfiguredTopologyComparison)GetPrivateField(
                window,
                "latestConfiguredTopologyComparison");
            AssertEx.NotNull(comparison);
            return comparison;
        }

        private static bool HasConfiguredTopologyComparison(
            MainWindow window,
            ConfiguredTopologyComparisonKind expected)
        {
            var comparison = GetPrivateField(
                window,
                "latestConfiguredTopologyComparison")
                as ConfiguredTopologyComparison;
            return comparison != null && comparison.Kind == expected;
        }

        private static string GetConfiguredTopologyEvidence(MainWindow window)
        {
            var evidence = (string)GetPrivateField(
                window,
                "latestConfiguredTopologyEvidence");
            AssertEx.NotNull(evidence);
            return evidence;
        }

        private static void AssertSuccessfulConfiguredBaselineUnchanged(
            MainWindow window,
            ConfiguredTopologySnapshot expectedSnapshot,
            string expectedEvidence,
            string scenario)
        {
            AssertEx.True(
                ReferenceEquals(
                    expectedSnapshot,
                    GetConfiguredTopologySnapshot(window)),
                scenario + " replaced the last successful configured baseline.");
            AssertEx.Equal(
                expectedSnapshot.Sha256,
                GetConfiguredTopologySnapshot(window).Sha256,
                scenario + " changed the last successful configured hash.");
            AssertEx.Equal(
                expectedEvidence,
                GetConfiguredTopologyEvidence(window),
                scenario + " replaced the last successful evidence report.");
            AssertEx.True(
                window.ButtonSaveConfiguredTopologyEvidence.IsEnabled,
                scenario + " disabled last-success evidence export.");
        }
    }
}

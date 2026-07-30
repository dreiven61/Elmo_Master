using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LasalMotionControlApiExample;
using LasalMotionControlLib;
using LasalMotionControlLib.Tests;

namespace LasalApiWpfTestApp.SmokeTests
{
    internal static partial class WpfMainWindowIntegrationTests
    {
        private const uint TopologyRevision = 0x15867EECu;
        private const uint DiagnosticMapRevision = 0xE245539Au;
        private const uint DiagnosticsBootId = 0x10203040u;
        private const uint CrevisCouplerNodeId = 0xEC000001u;
        private const uint FirstDriveNodeId = 0xEC000101u;
        private const uint CrevisInputNodeId = 0xEC010001u;
        private const uint CrevisInputIoReference = 0x00010001u;
        private const uint CrevisOutputNodeId = 0xEC010002u;
        private const uint CrevisOutputIoReference = 0x00010002u;
        private const ushort TopologyNodeCount = 7;
        private const ushort RecorderCatalogEntryCount = 4;
        private const int WaitTimeoutMilliseconds = 8000;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.Topology.AutoLoadRendersConfiguredCrevisWithLiveBitsOff",
                AutoLoadRendersConfiguredCrevisWithLiveBitsOff);
            tests.Add(
                "Wpf.Topology.FullCapabilityMonitorRendersHealthAndSelectedInputWithoutOutputShadowPoll",
                FullCapabilityMonitorRendersHealthAndSelectedInputWithoutOutputShadowPoll);
            tests.Add(
                "Wpf.Topology.LateManualReadsDoNotOverwriteNewSelectionOrOutputShadow",
                LateManualReadsDoNotOverwriteNewSelectionOrOutputShadow);
            tests.Add(
                "Wpf.Topology.LiveHealthAndDigitalInputErrorsRemainChannelIndependent",
                LiveHealthAndDigitalInputErrorsRemainChannelIndependent);
            tests.Add(
                "Wpf.Topology.CapabilityOffAutoLoadThenManualReloadRecoversCrevis",
                CapabilityOffAutoLoadThenManualReloadRecoversCrevis);
            RegisterConfiguredTopologyEvidenceTests(tests);
            tests.Add(
                "Wpf.ReadOnlyApi.AdminAndDriveReadsRenderTypedResults",
                ReadOnlyApiAdminAndDriveReadsRenderTypedResults);
            tests.Add(
                "Wpf.Sdo.OrdinaryInFlightKeepsWriteEditorEditable",
                OrdinaryInFlightKeepsWriteEditorEditable);
            tests.Add(
                "Wpf.Sdo.WriteConfirmationRequiresExactSecondClickWithoutModal",
                WriteConfirmationRequiresExactSecondClickWithoutModal);
            tests.Add(
                "Wpf.Sdo.InlineReadOneClickRendersTypedTerminalResult",
                InlineReadOneClickRendersTypedTerminalResult);
            tests.Add(
                "Wpf.Sdo.InlineReadAcceptedTimeoutPreservesTicketForManualCleanup",
                InlineReadAcceptedTimeoutPreservesTicketForManualCleanup);
            tests.Add(
                "Wpf.Sdo.InlineReadPreAcceptanceWaitCancelIsZeroSubmit",
                InlineReadPreAcceptanceWaitCancelIsZeroSubmit);
            tests.Add(
                "Wpf.Sdo.InlineReadAcceptedWaitCancelPreservesTicket",
                InlineReadAcceptedWaitCancelPreservesTicket);
            tests.Add(
                "Wpf.Sdo.InlineReadTerminalFailureUsesExactTerminalResolution",
                InlineReadTerminalFailureUsesExactTerminalResolution);
            tests.Add(
                "Wpf.Sdo.InlineReadGeneralCapabilityOffForcedAttemptIsZeroWire",
                InlineReadGeneralCapabilityOffForcedAttemptIsZeroWire);
            tests.Add(
                "Wpf.Sdo.WriteSameValueEmptyAllowlistForcedAttemptIsZeroWire",
                WriteSameValueEmptyAllowlistForcedAttemptIsZeroWire);
            tests.Add(
                "Wpf.Sdo.WriteSameValueTerminalEvidenceSurvivesUiRefresh",
                WriteSameValueTerminalEvidenceSurvivesUiRefresh);
            tests.Add(
                "Wpf.Sdo.PendingReadbackPreservesDraftAndExplicitLoadRestoresExactRequest",
                PendingReadbackPreservesDraftAndExplicitLoadRestoresExactRequest);
            tests.Add(
                "Wpf.Sdo.D5DisconnectTransportCloseYieldsToReservedSafetySend",
                D5DisconnectTransportCloseYieldsToReservedSafetySend);
            tests.Add(
                "Wpf.Sdo.D5DisconnectIrreversibleCommitIgnoresLateCancellation",
                D5DisconnectIrreversibleCommitIgnoresLateCancellation);
            tests.Add(
                "Wpf.Sdo.D5DisconnectMalformedCachedContractIsZeroWire",
                D5DisconnectMalformedCachedContractIsZeroWire);
            tests.Add(
                "Wpf.Sdo.D5DisconnectFullHandlerTwoSessionApplicationRecovery",
                D5DisconnectFullHandlerTwoSessionApplicationRecovery);
            tests.Add(
                "Wpf.Diagnostics.InvalidPiAndBulkRowsHideStaleRaw",
                InvalidPiAndBulkRowsHideStaleRaw);
            tests.Add(
                "Wpf.Sdo.RecoveredTypedWriteEmptyAllowlistForcedAttemptIsZeroWire",
                RecoveredTypedWriteEmptyAllowlistForcedAttemptIsZeroWire);
            tests.Add(
                "Wpf.Recorder.DoubleContractAdvertisedRemainsDormantAndZeroWire",
                DoubleContractAdvertisedRemainsDormantAndZeroWire);
            tests.Add(
                "Wpf.Recorder.ManualDoubleConfigureRoutingIsSeparatedAndFailClosed",
                ManualDoubleConfigureRoutingIsSeparatedAndFailClosed);
            RegisterRecorderManualDoubleAdapterIntegrationTests(tests);
            tests.Add(
                "Wpf.Recorder.AcceptedConfigureResultIsRetainedForExplicitCleanup",
                AcceptedConfigureResultIsRetainedForExplicitCleanup);
            tests.Add(
                "Wpf.Recorder.DormantGuardCoversRecoveryCommandRange",
                DormantGuardCoversRecoveryCommandRange);
            tests.Add(
                "Wpf.Recorder.DoubleRequestedConfigIdIsDeterministic",
                DoubleRequestedConfigIdIsDeterministic);
            tests.Add(
                "Wpf.Recorder.ActiveJournalKeepsRecoveryContractIndependentAndGateOff",
                ActiveJournalKeepsRecoveryContractIndependentAndGateOff);
            tests.Add(
                "Wpf.Recorder.DoubleJournalSecondWriterFailsClosed",
                DoubleJournalSecondWriterFailsClosed);
            tests.Add(
                "Wpf.Recorder.SemanticJournalConflictKeepsJournalUsable",
                SemanticJournalConflictKeepsJournalUsable);
            RegisterGroupEnableWaitTests(tests);
        }

        private static void SemanticJournalConflictKeepsJournalUsable()
        {
            AssertEx.False(
                MainWindow.IsRecorderDoubleJournalRuntimeFailure(
                    new InvalidOperationException(
                        "Conflicting third bank.")));
            AssertEx.True(
                MainWindow.IsRecorderDoubleJournalRuntimeFailure(
                    new IOException("Journal write failed.")));
            AssertEx.True(
                MainWindow.IsRecorderDoubleJournalRuntimeFailure(
                    new ObjectDisposedException("journal")));
        }

        private static void
            ManualDoubleConfigureRoutingIsSeparatedAndFailClosed()
        {
            var standardCalls = 0;
            var recoverableDoubleCalls = 0;
            Func<Task> standard = () =>
            {
                standardCalls++;
                return Task.CompletedTask;
            };
            Func<Task> recoverableDouble = () =>
            {
                recoverableDoubleCalls++;
                return Task.CompletedTask;
            };

            MainWindow
                .DispatchRecorderManualConfigureAsync(
                    false,
                    false,
                    standard,
                    recoverableDouble)
                .GetAwaiter()
                .GetResult();
            AssertEx.Equal(1, standardCalls);
            AssertEx.Equal(0, recoverableDoubleCalls);

            MainWindow
                .DispatchRecorderManualConfigureAsync(
                    true,
                    true,
                    standard,
                    recoverableDouble)
                .GetAwaiter()
                .GetResult();
            AssertEx.Equal(1, standardCalls);
            AssertEx.Equal(1, recoverableDoubleCalls);

            var blocked = AssertEx.Throws<InvalidOperationException>(
                () => MainWindow.DispatchRecorderManualConfigureAsync(
                        true,
                        false,
                        standard,
                        recoverableDouble)
                    .GetAwaiter()
                    .GetResult());
            AssertEx.Contains(
                "durable recoverable Configure route is CLOSED",
                blocked.Message);
            AssertEx.Equal(1, standardCalls);
            AssertEx.Equal(1, recoverableDoubleCalls);
        }

        private static void
            AcceptedConfigureResultIsRetainedForExplicitCleanup()
        {
            const uint acceptedConfigId = 0x31415926u;
            var capabilities = LMCDiagnosticCapability.EtherCATTopology
                | LMCDiagnosticCapability.SignalCatalog
                | LMCDiagnosticCapability.RecorderSingleBank;
            var steps = CreateConnectAndTopologySteps(capabilities, 1);
            steps.Add(CapabilitiesStep(11, capabilities, 1));
            steps.Add(new FakeRpcStep(
                0x7E01,
                TestFrame.Response(
                    0,
                    CatalogInfoPayload(12, RecorderCatalogEntryCount))));
            steps.Add(new FakeRpcStep(
                0x7E02,
                TestFrame.Response(
                    0,
                    CatalogChunkPayload(
                        13,
                        RecorderCatalogEntryCount))));
            steps.Add(CapabilitiesStep(14, capabilities, 1));

            using (var configureReceived = new ManualResetEventSlim(false))
            using (var releaseConfigure = new ManualResetEventSlim(false))
            {
                var configure = new FakeRpcStep(
                    0x7E40,
                    TestFrame.Response(
                        0,
                        RecorderConfigurePayload(
                            15,
                            acceptedConfigId,
                            RecorderCatalogEntryCount,
                            1000)))
                {
                    InspectRequest = request =>
                    {
                        AssertEx.Equal(15u, TestFrame.ReadUInt32(request, 12));
                        AssertEx.Equal(
                            DiagnosticMapRevision,
                            TestFrame.ReadUInt32(request, 16));
                        AssertEx.Equal(0u, TestFrame.ReadUInt32(request, 20));
                        AssertEx.Equal(
                            (ushort)1,
                            TestFrame.ReadUInt16(request, 24));
                        AssertEx.Equal(
                            RecorderCatalogEntryCount,
                            TestFrame.ReadUInt16(request, 26));
                        AssertEx.Equal(1000u, TestFrame.ReadUInt32(request, 28));
                        AssertEx.Equal(
                            (byte)LMCRecorderBufferMode.Single,
                            request[32]);
                        AssertEx.Equal(
                            (byte)LMCRecorderTriggerType.Manual,
                            request[33]);
                        AssertEx.Equal(
                            DiagnosticsBootId,
                            TestFrame.ReadUInt32(request, 60));
                        configureReceived.Set();
                        AssertEx.True(
                            releaseConfigure.Wait(5000),
                            "The delayed Recorder Configure response was not released.");
                    }
                };
                steps.Add(configure);
                steps.Add(new FakeRpcStep(
                    0x7E48,
                    TestFrame.Response(0, CommonPayload(16, 16))));
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
                            "Configured topology did not load before the Recorder test.");
                        WaitUntil(
                            () => window.ButtonLoadSignalCatalog.IsEnabled,
                            "Signal Catalog did not become available.");
                        Click(window.ButtonLoadSignalCatalog);
                        WaitUntil(
                            () => string.Equals(
                                window.TextOperationState.Text,
                                "Load Signal Catalog completed",
                                StringComparison.Ordinal),
                            "Signal Catalog did not load before Recorder Configure.");
                        AssertEx.True(window.ButtonConfigureRecorder.IsEnabled);

                        var coordinator =
                            (LMCSendPriorityCoordinator)GetPrivateField(
                                window,
                                "sendPriorityCoordinator");
                        Click(window.ButtonConfigureRecorder);
                        if (!configureReceived.Wait(2000))
                        {
                            throw new InvalidOperationException(
                                "Recorder Configure did not reach the delayed response barrier. State="
                                + window.TextOperationState.Text
                                + ", Log="
                                + window.TextExecutionLog.Text
                                + ", Commands="
                                + string.Join(
                                    ",",
                                    server.ReceivedRequests
                                        .Select(
                                            request => TestFrame.ReadUInt16(
                                                    request,
                                                    0)
                                                .ToString(
                                                    "X4",
                                                    CultureInfo.InvariantCulture))
                                        .ToArray()));
                        }
                        coordinator.ReservePrioritySend();
                        releaseConfigure.Set();

                        WaitUntil(
                            () => string.Equals(
                                window.TextOperationState.Text,
                                "Configure Recorder failed",
                                StringComparison.Ordinal),
                            "Recorder Configure did not report the discarded accepted result.");
                        AssertEx.Contains(
                            "was discarded because a newer Stop or Power Off request was reserved",
                            window.TextExecutionLog.Text);
                        AssertEx.Contains(
                            "Manual Recorder Configure accepted result",
                            window.TextRecorderSummary.Text);
                        AssertEx.Contains(
                            "same-session ownership was quarantined",
                            window.TextRecorderSummary.Text);

                        var retained =
                            (LMCRecorderConfigurationHandle)GetPrivateField(
                                window,
                                "recorderConfiguration");
                        AssertEx.NotNull(retained);
                        AssertEx.Equal(acceptedConfigId, retained.ConfigId);
                        AssertEx.True(retained.IsAcceptedResultRecoveryOnly);
                        AssertEx.False(retained.IsReleased);
                        AssertEx.True((bool)GetPrivateField(
                            window,
                            "recorderQualificationRecoveryReleaseOnly"));
                        AssertEx.False(window.ButtonStartRecorder.IsEnabled);
                        AssertEx.True(window.ButtonReleaseRecorder.IsEnabled);

                        Click(window.ButtonReleaseRecorder);
                        WaitUntil(
                            () => string.Equals(
                                window.TextOperationState.Text,
                                "Release Recorder completed",
                                StringComparison.Ordinal),
                            "The retained Recorder configuration did not complete explicit cleanup.");
                        AssertEx.True(retained.IsReleased);
                        AssertEx.Contains(
                            "Recorder resources released",
                            window.TextRecorderSummary.Text);
                        AssertEx.True(
                            GetPrivateField(window, "recorderConfiguration")
                                == null);

                        CloseConnectedWindow(window);
                        window = null;
                        server.Verify();
                        AssertRequestCommandSequence(
                            server.ReceivedRequests,
                            0x8080,
                            0x405C,
                            0x7E00,
                            0x7E00,
                            0x7E11,
                            0x7E12,
                            0x7E12,
                            0x7E12,
                            0x7E12,
                            0x7E12,
                            0x7E12,
                            0x7E12,
                            0x7E00,
                            0x7E01,
                            0x7E02,
                            0x7E00,
                            0x7E40,
                            0x7E48,
                            0x405D);
                    }
                }
                finally
                {
                    releaseConfigure.Set();
                    CloseWindowBestEffort(window);
                    DeleteJournalDirectory(journalDirectory);
                }
            }
        }

        private static void ReadOnlyApiAdminAndDriveReadsRenderTypedResults()
        {
            const ushort driveAxisReference = 2;
            const uint operationModeTicketId = 0x11111111u;
            const uint statusWordTicketId = 0x22222222u;
            const uint driveStatusModeTicketId = 0x33333333u;
            const uint driveErrorCodeTicketId = 0x44444444u;
            var capabilities = LMCDiagnosticCapability.EtherCATTopology
                | LMCDiagnosticCapability.SDORead
                | LMCDiagnosticCapability.SDOReadGeneralInline;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(AdminCapabilitiesStep(1));
            steps.Add(AdminCapabilitiesStep(2));
            steps.Add(AdminAxisParameterStep(
                3,
                1,
                LMCAxisParameterKey.SoftwareMinPosition,
                LMCAdminUnit.ApplicationUnits,
                -123456));
            steps.Add(AdminCapabilitiesStep(4));
            steps.Add(AdminGroupParametersStep(
                5,
                0x0100,
                LMCGroupParameterSelection.All,
                120000,
                340000,
                55));

            steps.Add(D5AxisLookupStep(driveAxisReference));
            steps.Add(D5AxisInfoStep(driveAxisReference));
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(D5SdoSubmitStep(
                13,
                operationModeTicketId,
                1100,
                driveAxisReference,
                0x6061,
                LMCSignalValueType.Int8,
                1,
                LMCSingleAxis.DefaultDriveReadTimeoutCycles));
            steps.Add(D5SdoOperationStatusStep(
                14,
                operationModeTicketId,
                1100,
                1110,
                LMCOperationState.Completed,
                LMCOperationOutcome.Success,
                LMCSignalValueType.Int8,
                new byte[] { 8 }));

            steps.Add(D5AxisLookupStep(driveAxisReference));
            steps.Add(D5AxisInfoStep(driveAxisReference));
            steps.Add(CapabilitiesStep(15, capabilities));
            steps.Add(DriveStatusAxisStep(driveAxisReference));
            steps.Add(CapabilitiesStep(16, capabilities));
            steps.Add(D5SdoSubmitStep(
                17,
                statusWordTicketId,
                1200,
                driveAxisReference,
                0x6041,
                LMCSignalValueType.BitField16,
                2,
                LMCSingleAxis.DefaultDriveReadTimeoutCycles));
            steps.Add(D5SdoOperationStatusStep(
                18,
                statusWordTicketId,
                1200,
                1210,
                LMCOperationState.Completed,
                LMCOperationOutcome.Success,
                LMCSignalValueType.BitField16,
                TestFrame.Hex("00 08")));
            steps.Add(CapabilitiesStep(19, capabilities));
            steps.Add(D5SdoSubmitStep(
                20,
                driveStatusModeTicketId,
                1220,
                driveAxisReference,
                0x6061,
                LMCSignalValueType.Int8,
                1,
                LMCSingleAxis.DefaultDriveReadTimeoutCycles));
            steps.Add(D5SdoOperationStatusStep(
                21,
                driveStatusModeTicketId,
                1220,
                1230,
                LMCOperationState.Completed,
                LMCOperationOutcome.Success,
                LMCSignalValueType.Int8,
                new byte[] { 8 }));

            steps.Add(D5AxisLookupStep(driveAxisReference));
            steps.Add(D5AxisInfoStep(driveAxisReference));
            steps.Add(CapabilitiesStep(22, capabilities));
            steps.Add(CapabilitiesStep(23, capabilities));
            steps.Add(D5SdoSubmitStep(
                24,
                driveErrorCodeTicketId,
                1300,
                driveAxisReference,
                0x603F,
                LMCSignalValueType.UInt16,
                2,
                LMCSingleAxis.DefaultDriveReadTimeoutCycles));
            steps.Add(D5SdoOperationStatusStep(
                25,
                driveErrorCodeTicketId,
                1300,
                1310,
                LMCOperationState.Completed,
                LMCOperationOutcome.Success,
                LMCSignalValueType.UInt16,
                TestFrame.Hex("10 23")));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(journalDirectory, server.Port);

                    AssertEx.False(window.ButtonReadAdminAxisParameter.IsEnabled);
                    AssertEx.False(window.ButtonReadAdminGroupParameters.IsEnabled);
                    AssertEx.False(window.ButtonGetDriveErrorCode.IsEnabled);

                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => window.GridEtherCATTopology.Items.Count
                                == TopologyNodeCount
                            && window.ButtonAdminCapabilities.IsEnabled,
                        "The Read-only API smoke did not complete connection and topology loading.");

                    AssertEx.False(window.ButtonReadAdminAxisParameter.IsEnabled);
                    AssertEx.False(window.ButtonReadAdminGroupParameters.IsEnabled);
                    AssertEx.True(window.ButtonGetDriveOperationMode.IsEnabled);
                    AssertEx.True(window.ButtonReadDriveStatus.IsEnabled);
                    AssertEx.True(window.ButtonGetDriveErrorCode.IsEnabled);

                    Click(window.ButtonAdminCapabilities);
                    WaitUntil(
                        () => window.ButtonReadAdminAxisParameter.IsEnabled
                            && window.ButtonReadAdminGroupParameters.IsEnabled,
                        "Admin capabilities did not enable the advertised read-only controls.");
                    AssertEx.Contains(
                        "Schema=1, Features=AxisParameterRead, GroupParameterRead, RequestId=1",
                        window.TextAdminCapabilities.Text);
                    AssertEx.Contains(
                        "PhysicalAxes=4, AxisParameterMask=0x0000003F",
                        window.TextAdminCapabilities.Text);
                    AssertEx.Contains(
                        "GroupRef=0x0100, GroupSelection=All",
                        window.TextAdminCapabilities.Text);

                    Click(window.ButtonReadAdminAxisParameter);
                    WaitUntil(
                        () => window.TextAdminAxisParameterResult.Text.IndexOf(
                                "RequestId=3",
                                StringComparison.Ordinal) >= 0,
                        "The semantic axis parameter result was not rendered.");
                    AssertEx.Contains(
                        "AxisRef=1, Key=SoftwareMinPosition, Value=-123456",
                        window.TextAdminAxisParameterResult.Text);
                    AssertEx.Contains(
                        "Type=Int32, Unit=ApplicationUnits, RequestId=3",
                        window.TextAdminAxisParameterResult.Text);

                    Click(window.ButtonReadAdminGroupParameters);
                    WaitUntil(
                        () => window.TextAdminGroupParameterResult.Text.IndexOf(
                                "RequestId=5",
                                StringComparison.Ordinal) >= 0,
                        "The semantic group parameter result was not rendered.");
                    AssertEx.Contains(
                        "GroupRef=0x0100, Selection=All, RequestId=5",
                        window.TextAdminGroupParameterResult.Text);
                    AssertEx.Contains(
                        "PathVelocityLimit=120000 ApplicationUnitsPerSecond",
                        window.TextAdminGroupParameterResult.Text);
                    AssertEx.Contains(
                        "PathAccelerationLimit=340000 ApplicationUnitsPerSecondSquared",
                        window.TextAdminGroupParameterResult.Text);
                    AssertEx.Contains(
                        "JerkTime=55 Milliseconds",
                        window.TextAdminGroupParameterResult.Text);

                    window.ComboDriveReadAxisReference.SelectedItem =
                        driveAxisReference;
                    Click(window.ButtonGetDriveOperationMode);
                    WaitUntil(
                        () => window.TextDriveReadResult.Text.IndexOf(
                                "TicketId=286331153",
                                StringComparison.Ordinal) >= 0,
                        "The typed drive operation mode result was not rendered.");
                    AssertEx.Contains(
                        "AxisRef=2, Mode=CyclicSynchronousPosition, Raw=8, Known=True",
                        window.TextDriveReadResult.Text);
                    AssertEx.Contains(
                        "TicketId=286331153, State=Completed, CompletionCycle=1110",
                        window.TextDriveReadResult.Text);

                    Click(window.ButtonReadDriveStatus);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Read Drive Status completed",
                                StringComparison.Ordinal)
                            && window.ButtonReadDriveStatus.IsEnabled,
                        "The non-atomic drive status operation did not return to idle.");
                    AssertEx.Contains(
                        "AxisRef=2, ReadSuccessful=True, Atomic=False",
                        window.TextDriveReadResult.Text);
                    AssertEx.Contains(
                        "LASAL State=0x00000020, AxisErrorFlags=0x0012",
                        window.TextDriveReadResult.Text);
                    AssertEx.Contains(
                        "DS402 0x6041:0=0x0800, DS402Fault=False, 0x6061:0=CyclicSynchronousPosition (raw 8)",
                        window.TextDriveReadResult.Text);
                    AssertEx.Contains(
                        "PositionLimit=True, DS402InternalLimit=True, AnyLimit=True",
                        window.TextDriveReadResult.Text);
                    AssertEx.Contains(
                        "StatusWordTicket=572662306, ModeTicket=858993459",
                        window.TextDriveReadResult.Text);

                    Click(window.ButtonGetDriveErrorCode);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Get Drive Error Code completed",
                                StringComparison.Ordinal)
                            && window.ButtonGetDriveErrorCode.IsEnabled,
                        "The typed drive error-code operation did not return to idle.");
                    AssertEx.Contains(
                        "AxisRef=2, DS402 0x603F:0=0x2310, HasError=True, ReadSuccessful=True",
                        window.TextDriveReadResult.Text);
                    AssertEx.Contains(
                        "TicketId=1145324612, State=Completed, CompletionCycle=1310",
                        window.TextDriveReadResult.Text);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                    AssertEx.Equal(40, server.ReceivedRequests.Count);
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void InvalidPiAndBulkRowsHideStaleRaw()
        {
            var catalogEntry = CreateStatusWordCatalogEntry();
            var staleValue = CreateSignalValueEntry(
                LMCSignalEntryStatus.SlaveOffline,
                LMCDiagnosticsDetailCode.SlaveOffline);
            var validValue = CreateSignalValueEntry(
                LMCSignalEntryStatus.Valid,
                LMCDiagnosticsDetailCode.None);
            var diagnosticRowType = typeof(MainWindow).GetNestedType(
                "DiagnosticSignalRow",
                BindingFlags.NonPublic);
            var bulkRowType = typeof(MainWindow).GetNestedType(
                "BulkValueRow",
                BindingFlags.NonPublic);
            AssertEx.True(diagnosticRowType != null);
            AssertEx.True(bulkRowType != null);

            var diagnosticRow = Activator.CreateInstance(
                diagnosticRowType,
                BindingFlags.Instance
                    | BindingFlags.Public
                    | BindingFlags.NonPublic,
                null,
                new object[] { catalogEntry, false },
                CultureInfo.InvariantCulture);
            diagnosticRowType.GetMethod(
                "UpdateValue",
                BindingFlags.Instance | BindingFlags.Public).Invoke(
                    diagnosticRow,
                    new object[] { staleValue, 200u });
            AssertEx.Equal(
                "UNAVAILABLE",
                (string)diagnosticRowType.GetProperty("RawValue").GetValue(
                    diagnosticRow,
                    null));
            AssertEx.Contains(
                "SlaveOffline",
                (string)diagnosticRowType.GetProperty("EntryStatus").GetValue(
                    diagnosticRow,
                    null));

            var bulkRow = Activator.CreateInstance(
                bulkRowType,
                BindingFlags.Instance
                    | BindingFlags.Public
                    | BindingFlags.NonPublic,
                null,
                new object[] { diagnosticRow, staleValue },
                CultureInfo.InvariantCulture);
            AssertEx.Equal(
                "UNAVAILABLE",
                (string)bulkRowType.GetProperty("RawValue").GetValue(
                    bulkRow,
                    null));
            AssertEx.Contains(
                "18",
                (string)bulkRowType.GetProperty("Detail").GetValue(
                    bulkRow,
                    null));

            diagnosticRowType.GetMethod(
                "UpdateValue",
                BindingFlags.Instance | BindingFlags.Public).Invoke(
                    diagnosticRow,
                    new object[] { validValue, 300u });
            AssertEx.False(string.Equals(
                "UNAVAILABLE",
                (string)diagnosticRowType.GetProperty("RawValue").GetValue(
                    diagnosticRow,
                    null),
                StringComparison.Ordinal));
        }

        private static LMCSignalCatalogEntry CreateStatusWordCatalogEntry()
        {
            return (LMCSignalCatalogEntry)Activator.CreateInstance(
                typeof(LMCSignalCatalogEntry),
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new object[]
                {
                    0x00100106u,
                    (ushort)5,
                    LMCSignalSourceKind.PdoInput,
                    (byte)1,
                    LMCSignalValueType.BitField16,
                    (byte)2,
                    (ushort)0,
                    LMCSignalAccessFlags.Readable,
                    LMCSignalFlags.ActivePdo
                        | LMCSignalFlags.PhysicalAxis
                        | LMCSignalFlags.InputMappedPhase,
                    (ushort)0x6041,
                    (byte)0,
                    LMCPdoDirection.DriveToMaster,
                    1,
                    1,
                    0,
                    65535,
                    "axis1.status_word"
                },
                CultureInfo.InvariantCulture);
        }

        private static LMCSignalValueEntry CreateSignalValueEntry(
            LMCSignalEntryStatus status,
            LMCDiagnosticsDetailCode detail)
        {
            return (LMCSignalValueEntry)Activator.CreateInstance(
                typeof(LMCSignalValueEntry),
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new object[]
                {
                    0x00100106u,
                    0x1237u,
                    LMCSignalValueType.BitField16,
                    status,
                    (uint)detail
                },
                CultureInfo.InvariantCulture);
        }

        private static LMCSdoWriteTarget CreateSdoWriteTarget(
            string displayName,
            ushort slaveReference,
            ushort objectIndex,
            byte subIndex)
        {
            return (LMCSdoWriteTarget)Activator.CreateInstance(
                typeof(LMCSdoWriteTarget),
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new object[]
                {
                    displayName,
                    slaveReference,
                    objectIndex,
                    subIndex,
                    LMCSignalValueType.Int32,
                    (ushort)4,
                    -1000L,
                    1000L
                },
                CultureInfo.InvariantCulture);
        }

        private static void
            ActiveJournalKeepsRecoveryContractIndependentAndGateOff()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology
                | LMCDiagnosticCapability.RecorderSingleBank
                | LMCDiagnosticCapability.RecorderDoubleBank;
            var steps = CreateConnectAndTopologySteps(capabilities, 2);
            steps.Add(CloseStep());
            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(journalDirectory, server.Port);
                    var journal =
                        (RecorderDoubleRecoveryJournal)GetPrivateField(
                            window,
                            "recorderDoubleRecoveryJournal");
                    journal.ArmBeforeConfigureDispatch(
                        new Guid(
                            "4b4f948e-4dc6-45ef-95ad-696438148bb8"),
                        new DateTime(
                            638892000040000000L,
                            DateTimeKind.Utc),
                        0x10203040u,
                        DiagnosticMapRevision,
                        0x31415926u);
                    InvokePrivate(window, "UpdateUiState");

                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => window.GridEtherCATTopology.Items.Count
                                == TopologyNodeCount
                            && window.TextQualificationRecorderCapability.Text
                                .IndexOf(
                                    "DoubleRecoveryContractReady=True",
                                    StringComparison.Ordinal) >= 0,
                        "The active journal did not retain an independent recovery capability contract.");

                    AssertEx.Contains(
                        "DoubleContractReady=False",
                        window.TextQualificationRecorderCapability.Text);
                    AssertEx.Contains(
                        "DoubleRecoveryContractReady=True",
                        window.TextQualificationRecorderCapability.Text);
                    AssertEx.Contains(
                        "ACTIVE DOUBLE RECOVERY RECORD.",
                        window.TextRecorderDoubleRecoveryStatus.Text);
                    AssertEx.False(
                        window.ButtonRunRecorderDoubleQualification.IsEnabled);
                    AssertEx.False(
                        window.ButtonReleaseRecorderDoubleRetained.IsEnabled);
                    AssertEx.False(
                        window.ButtonRecoverRecorderDoubleJournal.IsEnabled);
                    AssertEx.False(
                        window.CheckConfirmRecorderDoubleRelease.IsEnabled);
                    var releaseConfirmation =
                        window.CheckConfirmRecorderDoubleRelease.Content
                            .ToString();
                    AssertEx.Contains(
                        "4b4f948e-4dc6-45ef-95ad-696438148bb8",
                        releaseConfirmation);
                    AssertEx.Contains(
                        "Config 0x31415926/0",
                        releaseConfirmation);
                    AssertEx.Contains(
                        "4D/4A read-only identity and inventory discovery -> stop before Adopt/Release",
                        releaseConfirmation);

                    Click(window.ButtonRecoverRecorderDoubleJournal);
                    AssertEx.Equal(
                        "RecorderDoubleRecovery failed",
                        window.TextOperationState.Text);
                    AssertEx.Contains(
                        "ReconnectRecovery proof gate is CLOSED",
                        window.TextExecutionLog.Text);
                    AssertNoRecorderRequests(server.ReceivedRequests);

                    InvokePrivate(
                        window,
                        "DisposeRecorderDoubleRecoveryJournal");
                    InvokePrivate(window, "UpdateUiState");
                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                    AssertNoRecorderRequests(server.ReceivedRequests);
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void DoubleRequestedConfigIdIsDeterministic()
        {
            var identity = new Guid(new byte[]
            {
                0x12, 0x34, 0x56, 0x78,
                0x10, 0x20, 0x30, 0x40,
                0x50, 0x60, 0x70, 0x80,
                0x90, 0xA0, 0xB0, 0xC0
            });
            AssertEx.Equal(
                0x78563412u,
                MainWindow.CreateRecorderDoubleRequestedConfigId(identity));
            AssertEx.Equal(
                MainWindow.CreateRecorderDoubleRequestedConfigId(identity),
                MainWindow.CreateRecorderDoubleRequestedConfigId(identity));

            var zeroPrefixIdentity = new Guid(new byte[]
            {
                0, 0, 0, 0,
                1, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0
            });
            AssertEx.Equal(
                1u,
                MainWindow.CreateRecorderDoubleRequestedConfigId(
                    zeroPrefixIdentity));
        }

        private static void DoubleJournalSecondWriterFailsClosed()
        {
            var journalDirectory = CreateJournalDirectory();
            var doubleDirectory = Path.Combine(
                journalDirectory,
                "RecorderDoubleRecovery");
            MainWindow window = null;
            try
            {
                using (var owner = RecorderDoubleRecoveryJournal.Open(
                    doubleDirectory))
                {
                    owner.ArmBeforeConfigureDispatch(
                        Guid.NewGuid(),
                        new DateTime(
                            638892000030000000L,
                            DateTimeKind.Utc),
                        0x12345678u,
                        0x957F101Eu,
                        0x31415926u);

                    window = CreateWindow(
                        journalDirectory,
                        doubleDirectory,
                        1);
                    AssertEx.Contains(
                        "DOUBLE RECOVERY JOURNAL UNAVAILABLE.",
                        window.TextRecorderDoubleRecoveryStatus.Text);
                    AssertEx.Contains(
                        "IOException",
                        window.TextRecorderDoubleRecoveryStatus.Text);
                    AssertEx.False(
                        window.ButtonRecoverRecorderDoubleJournal.IsEnabled);

                    var admission =
                        (DiagnosticsAdmissionDecision)InvokePrivate(
                            window,
                            "EvaluateDiagnosticsAdmission",
                            DiagnosticsAdmissionOperation
                                .NewLiveOrMutation,
                            true);
                    AssertEx.False(admission.IsAllowed);
                    AssertEx.Equal(
                        DiagnosticsAdmissionDenialReason
                            .MutationJournalUnavailable,
                        admission.DenialReason);

                    window.Close();
                    WaitUntil(
                        () => !window.IsLoaded,
                        "A window with unavailable Double journal did not close cleanly.");
                    window = null;
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void DormantGuardCoversRecoveryCommandRange()
        {
            AssertEx.False(IsRecorderCommand(0x7E3F));
            AssertEx.True(IsRecorderCommand(0x7E40));
            AssertEx.True(IsRecorderCommand(0x7E49));
            AssertEx.True(IsRecorderCommand(0x7E4A));
            AssertEx.True(IsRecorderCommand(0x7E4B));
            AssertEx.True(IsRecorderCommand(0x7E4F));
            AssertEx.False(IsRecorderCommand(0x7E50));
        }

        private static void
            D5DisconnectTransportCloseYieldsToReservedSafetySend()
        {
            var window = new MainWindow();
            try
            {
                var coordinator = (LMCSendPriorityCoordinator)GetPrivateField(
                    window,
                    "sendPriorityCoordinator");
                var capturedGeneration = coordinator.CurrentGeneration;
                SetPrivateField(
                    window,
                    "qualificationSafetyGeneration",
                    capturedGeneration);
                coordinator.ReservePrioritySend();

                using (var owner = new LMCConnection())
                {
                    var abortMethod = typeof(MainWindow).GetMethod(
                        "AbortD5OldOwnerTransportAsync",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    AssertEx.NotNull(abortMethod);
                    var abortTask = (Task)abortMethod.Invoke(
                        window,
                        new object[]
                        {
                            owner,
                            CancellationToken.None
                        });
                    var rejection = AssertEx.Throws<InvalidOperationException>(
                        () => abortTask.GetAwaiter().GetResult());
                    AssertEx.Contains(
                        "Stop or Power Off",
                        rejection.Message);
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
            }
        }

        private static void
            D5DisconnectIrreversibleCommitIgnoresLateCancellation()
        {
            var window = new MainWindow();
            var cancellation = new CancellationTokenSource();
            try
            {
                SetPrivateField(window, "qualificationRunning", true);
                SetPrivateField(
                    window,
                    "qualificationCancellation",
                    cancellation);

                var commitTask = Task.Run(
                    () => InvokePrivate(
                        window,
                        "CommitQualificationIrreversibleOutcome",
                        "smoke-test D5 proof commit"));
                WaitUntil(
                    () => commitTask.IsCompleted,
                    "The background proof commit did not marshal to the WPF Dispatcher.");
                commitTask.GetAwaiter().GetResult();

                InvokePrivate(
                    window,
                    "CancelQualification",
                    "late smoke-test cancel",
                    false);

                AssertEx.False(cancellation.IsCancellationRequested);
                AssertEx.Equal(
                    1,
                    (int)GetPrivateField(
                        window,
                        "qualificationIrreversibleCommitState"));
                var log = (List<string>)GetPrivateField(
                    window,
                    "qualificationLogLines");
                AssertEx.Contains(
                    "event=LATE_CANCEL_IGNORED_AFTER_IRREVERSIBLE_COMMIT",
                    string.Join(Environment.NewLine, log));
            }
            finally
            {
                SetPrivateField(window, "qualificationRunning", false);
                SetPrivateField(window, "qualificationCancellation", null);
                cancellation.Dispose();
                CloseWindowBestEffort(window);
            }
        }

        private static void
            D5DisconnectMalformedCachedContractIsZeroWire()
        {
            var capabilities =
                LMCDiagnosticCapability.EtherCATTopology
                | LMCDiagnosticCapability.SDORead
                | LMCDiagnosticCapability.SDOReadGeneralInline;
            var steps = CreateConnectAndTopologySteps(capabilities);
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
                        "The D5 malformed-cache setup did not finish connecting.");

                    var cachedCapabilities = GetPrivateField(
                        window,
                        "diagnosticCapabilities");
                    SetProperty(
                        cachedCapabilities,
                        "BaseCycleTimeUs",
                        0u);
                    InvokePrivate(window, "UpdateUiState");
                    AssertEx.False(
                        window.ButtonRunD5SdoDisconnectRecoveryQualification
                            .IsEnabled);

                    var requestCountBeforeForcedStart =
                        server.ReceivedRequests.Count;
                    InvokePrivate(
                        window,
                        "ButtonRunD5SdoDisconnectRecoveryQualification_Click",
                        window.ButtonRunD5SdoDisconnectRecoveryQualification,
                        new RoutedEventArgs());
                    PumpDispatcherOnce();
                    AssertEx.Equal(
                        requestCountBeforeForcedStart,
                        server.ReceivedRequests.Count);
                    AssertEx.Contains(
                        "No RPC was sent",
                        window.TextD5SdoQualificationProgress.Text);

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
            D5DisconnectFullHandlerTwoSessionApplicationRecovery()
        {
            const uint baselineTicketId = 101;
            const uint oldProbeTicketId = 102;
            const uint firstRecoveryTicketId = 201;
            const uint secondRecoveryTicketId = 202;
            var capabilities =
                LMCDiagnosticCapability.EtherCATTopology
                | LMCDiagnosticCapability.SDORead
                | LMCDiagnosticCapability.SDOReadGeneralInline;
            var mixedCanonical = CreateMixedIoTopologyCanonicalBytes();
            var mixedTopologyRevision = ComputeTopologyRevision(
                mixedCanonical);
            var steps = CreateD5DisconnectTwoSessionSteps(
                capabilities,
                mixedCanonical,
                mixedTopologyRevision,
                baselineTicketId,
                oldProbeTicketId,
                firstRecoveryTicketId,
                secondRecoveryTicketId);

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(journalDirectory, server.Port);
                    ((DispatcherTimer)GetPrivateField(
                        window,
                        "topologyIoLiveMonitorTimer")).Stop();
                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => window.GridEtherCATTopology.Items.Count
                                == TopologyNodeCount
                            && window
                                .ButtonRunD5SdoDisconnectRecoveryQualification
                                .IsEnabled,
                        "The D5 two-session setup did not finish connecting.");

                    var oldConnection =
                        (LMCConnection)GetPrivateField(window, "connection");
                    AssertEx.True(oldConnection.IsConnected);
                    AssertEx.True(oldConnection.IsRpcInitialized);
                    AssertEx.True(oldConnection.IsCallbackListenerRunning);

                    Click(
                        window
                            .ButtonRunD5SdoDisconnectRecoveryQualification);
                    WaitUntil(
                        () => (bool)GetPrivateField(
                            window,
                            "qualificationRunning"),
                        "The D5 two-session handler did not start.");
                    WaitUntil(
                        () => !(bool)GetPrivateField(
                            window,
                            "qualificationRunning"),
                        "The D5 two-session handler did not finish.",
                        15000);

                    AssertEx.Equal(
                        "D5SdoAbruptDisconnectApplicationRecovery PASS",
                        window.TextOperationState.Text,
                        "Progress="
                            + window.TextD5SdoQualificationProgress.Text
                            + Environment.NewLine
                            + "ExecutionLog="
                            + window.TextExecutionLog.Text);
                    var newConnection =
                        (LMCConnection)GetPrivateField(window, "connection");
                    AssertEx.False(
                        ReferenceEquals(oldConnection, newConnection),
                        "The GUI retained the old disconnected owner.");
                    AssertEx.False(oldConnection.IsConnected);
                    AssertEx.False(oldConnection.IsRpcInitialized);
                    AssertEx.False(oldConnection.IsCallbackListenerRunning);
                    AssertEx.True(oldConnection.RpcCloseResponse == null);
                    AssertEx.True(newConnection.IsConnected);
                    AssertEx.True(newConnection.IsRpcInitialized);
                    AssertEx.True(newConnection.IsCallbackListenerRunning);
                    AssertEx.Equal(2, server.AcceptedClientCount);

                    var loadedTopology =
                        (LMCEtherCATTopology)GetPrivateField(
                            window,
                            "etherCATTopology");
                    AssertEx.NotNull(loadedTopology);
                    AssertEx.Equal(
                        mixedTopologyRevision,
                        loadedTopology.TopologyRevision);
                    AssertEx.Equal(3, CountCrevisRows(window));
                    var mixedRow = FindTopologyRow(
                        window,
                        "GL_9086_1_Slot001");
                    AssertEx.Equal(
                        "32",
                        GetRowString(mixedRow, "InputBits"));
                    AssertEx.Equal(
                        "32",
                        GetRowString(mixedRow, "OutputBits"));

                    AssertEx.True(
                        (bool)InvokePrivate(
                            window,
                            "HasCachedD5ReadQualificationContract"));
                    var quarantine =
                        (D5SdoQuarantineLedger)GetPrivateField(
                            window,
                            "d5SdoQualificationQuarantine");
                    AssertEx.Equal(0, quarantine.Count);
                    AssertEx.False(quarantine.HasEntries);

                    var qualificationLog = string.Join(
                        Environment.NewLine,
                        (List<string>)GetPrivateField(
                            window,
                            "qualificationLogLines"));
                    AssertEx.Contains(
                        "verdict=PASS_APPLICATION_RECOVERY",
                        qualificationLog);
                    AssertEx.Contains(
                        "firstRecoveryTicket="
                            + firstRecoveryTicketId.ToString(
                                CultureInfo.InvariantCulture),
                        qualificationLog);
                    AssertEx.Contains(
                        "secondRecoveryTicket="
                            + secondRecoveryTicketId.ToString(
                                CultureInfo.InvariantCulture),
                        qualificationLog);
                    AssertEx.Contains(
                        "firstRecoverySubmitAttempts=1",
                        qualificationLog);
                    AssertEx.Contains(
                        "secondRecoverySubmitAttempts=1",
                        qualificationLog);
                    AssertEx.Contains(
                        "resourceBusyRejections=0",
                        qualificationLog);
                    AssertEx.Contains(
                        "orphanQualified=false",
                        qualificationLog);
                    AssertEx.False(
                        qualificationLog.IndexOf(
                            "event=D5_DISCONNECT_RECOVERY_SCOPE",
                            StringComparison.Ordinal) >= 0,
                        "A successful two-session recovery emitted a failure recovery scope.");

                    AssertNoRpcCloseInSession(server, 1);
                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                    AssertEx.Equal(29, CountRequestsInSession(server, 1));
                    AssertEx.Equal(22, CountRequestsInSession(server, 2));
                    AssertNoRpcCloseInSession(server, 1);
                    AssertEx.Equal(
                        1,
                        CountCommandInSession(server, 2, 0x405D));
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void AutoLoadRendersConfiguredCrevisWithLiveBitsOff()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
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
                                == TopologyNodeCount
                            && window.TextEtherCATTopologySummary.Text.IndexOf(
                                "Configured CREVIS entries=3",
                                StringComparison.Ordinal) >= 0,
                        "CREVIS topology auto-load did not reach the rendered state.");

                    AssertEx.Contains(
                        "[CREVIS topology / editable SDO draft]",
                        window.Title);
                    AssertEx.Equal(
                        "Load CREVIS / Topology",
                        Convert.ToString(
                            window.ButtonLoadEtherCATTopology.Content,
                            CultureInfo.InvariantCulture));
                    WaitUntil(
                        () => string.Equals(
                            window.TextCrevisQuickStatus.Text,
                            window.TextEtherCATTopologySummary.Text,
                            StringComparison.Ordinal),
                        "Top CREVIS quick status did not mirror the detailed topology summary.");
                    AssertEx.Contains(
                        "Configured CREVIS entries=3",
                        window.TextCrevisQuickStatus.Text);
                    AssertEx.Equal(
                        "CFG slave",
                        Convert.ToString(
                            window.GridEtherCatHealth.Columns[1].Header,
                            CultureInfo.InvariantCulture));
                    for (ushort axis = 1; axis <= 4; axis++)
                    {
                        AssertEx.Equal(
                            axis.ToString(CultureInfo.InvariantCulture),
                            Convert.ToString(
                                InvokePrivate(
                                    window,
                                    "ResolveConfiguredSlaveIndex",
                                    axis),
                                CultureInfo.InvariantCulture));
                    }

                    AssertEx.Equal(
                        (int)TopologyNodeCount,
                        window.GridEtherCATTopology.Items.Count);
                    AssertEx.Equal(3, CountCrevisRows(window));
                    AssertEx.Contains(
                        "Load=automatic post-connect load",
                        window.TextEtherCATTopologySummary.Text);
                    AssertEx.Contains(
                        "LiveHealth=not advertised",
                        window.TextEtherCATTopologySummary.Text);
                    AssertEx.Contains(
                        "DigitalInput=not advertised",
                        window.TextEtherCATTopologySummary.Text);
                    AssertEx.Contains(
                        "DigitalOutput=not advertised",
                        window.TextEtherCATTopologySummary.Text);
                    AssertEx.False(window.ButtonReadSelectedNodeHealth.IsEnabled);
                    AssertEx.False(window.ButtonReadSelectedDigitalInput.IsEnabled);
                    AssertEx.False(window.ButtonReadSelectedDigitalOutput.IsEnabled);
                    AssertEx.False(window.ButtonSubmitDigitalOutputWrite.IsEnabled);
                    var noLiveEvidence = CaptureTopologyIoLiveEvidence(window);
                    AssertEx.Equal(0, noLiveEvidence.Records.Count);
                    AssertEx.False(
                        window.ButtonSaveTopologyIoLiveEvidence.IsEnabled);
                    AssertEx.Contains(
                        "retained=0, dropped=0",
                        window.TextTopologyIoLiveEvidenceSummary.Text);
                    AssertEx.False(
                        window.ButtonRunRecorderDoubleQualification.IsEnabled);
                    AssertEx.False(
                        window.ButtonRunD5SdoContentionQualification.IsEnabled,
                        "D5 contention qualification must stay disabled without SDO Read general-inline capability.");
                    AssertEx.False(
                        window.ButtonRunD5SdoTimeoutQualification.IsEnabled,
                        "D5 timeout qualification must stay disabled without SDO Read general-inline capability.");
                    AssertEx.False(
                        window.ButtonRunD5SdoQueuedCancelQualification.IsEnabled,
                        "D5 queued-cancel qualification must stay disabled without SDO Read general-inline capability.");
                    AssertEx.False(
                        window.ButtonRunD5SdoDisconnectRecoveryQualification.IsEnabled,
                        "D5 abrupt-disconnect recovery must stay disabled without SDO Read general-inline capability.");

                    var requestCountBeforeForcedStart =
                        server.ReceivedRequests.Count;
                    InvokePrivate(
                        window,
                        "ButtonRunD5SdoDisconnectRecoveryQualification_Click",
                        window.ButtonRunD5SdoDisconnectRecoveryQualification,
                        new RoutedEventArgs());
                    PumpDispatcherOnce();
                    AssertEx.Equal(
                        requestCountBeforeForcedStart,
                        server.ReceivedRequests.Count);
                    AssertEx.Contains(
                        "No RPC was sent",
                        window.TextD5SdoQualificationProgress.Text);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                    AssertNoLiveIoRequests(server.ReceivedRequests);
                    AssertNoRecorderRequests(server.ReceivedRequests);
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void
            FullCapabilityMonitorRendersHealthAndSelectedInputWithoutOutputShadowPoll()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology
                | LMCDiagnosticCapability.EtherCATNodeHealth
                | LMCDiagnosticCapability.DigitalIORead;
            var steps = CreateConnectAndTopologySteps(capabilities);

            var couplerHealthStep = new FakeRpcStep(
                0x7E13,
                TestFrame.Response(
                    0,
                    NodeHealthPayload(
                        11,
                        CrevisCouplerNodeId,
                        201,
                        false)));
            couplerHealthStep.InspectRequest = request =>
                AssertNodeHealthRequest(
                    request,
                    11,
                    CrevisCouplerNodeId);
            steps.Add(couplerHealthStep);

            var selectedInputStep = new FakeRpcStep(
                0x7E22,
                TestFrame.Response(
                    0,
                    DigitalInputPayload(
                        12,
                        CrevisInputIoReference,
                        CrevisInputNodeId,
                        32,
                        0xA5A55A5Au,
                        202)));
            selectedInputStep.InspectRequest = request =>
                AssertDigitalInputRequest(
                    request,
                    12,
                    CrevisInputIoReference,
                    32);
            steps.Add(selectedInputStep);

            var driveHealthStep = new FakeRpcStep(
                0x7E13,
                TestFrame.Response(
                    0,
                    NodeHealthPayload(
                        13,
                        FirstDriveNodeId,
                        203,
                        true)));
            driveHealthStep.InspectRequest = request =>
                AssertNodeHealthRequest(request, 13, FirstDriveNodeId);
            steps.Add(driveHealthStep);
            steps.Add(
                CapabilitiesStep(
                    14,
                    LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(journalDirectory, server.Port);
                    var monitorTimer =
                        (DispatcherTimer)GetPrivateField(
                            window,
                            "topologyIoLiveMonitorTimer");
                    monitorTimer.Stop();

                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => window.GridEtherCATTopology.Items.Count
                                == TopologyNodeCount
                            && window.ButtonReadSelectedNodeHealth.IsEnabled,
                        "Full-capability CREVIS topology auto-load did not reach the idle rendered state.");

                    AssertEx.Contains(
                        "Load=automatic post-connect load",
                        window.TextEtherCATTopologySummary.Text);
                    AssertEx.Contains(
                        "Configured CREVIS entries=3",
                        window.TextEtherCATTopologySummary.Text);
                    AssertEx.Contains(
                        "LiveHealth=advertised",
                        window.TextEtherCATTopologySummary.Text);
                    AssertEx.Contains(
                        "DigitalInput=advertised",
                        window.TextEtherCATTopologySummary.Text);
                    AssertEx.Contains(
                        "DigitalOutput=not advertised",
                        window.TextEtherCATTopologySummary.Text);

                    var couplerRow = FindTopologyRow(
                        window,
                        "GL_9086_11");
                    var inputRow = FindTopologyRow(
                        window,
                        "GL_9086_1_Slot001");
                    var outputRow = FindTopologyRow(
                        window,
                        "GL_9086_1_Slot011");
                    var firstDriveRow = FindTopologyRow(window, "Elmo_11");

                    window.GridEtherCATTopology.SelectedItem = inputRow;
                    WaitUntil(
                        () => window.ButtonReadSelectedDigitalInput.IsEnabled,
                        "The configured CREVIS input row did not enable its exact read path.");

                    InvokePrivate(
                        window,
                        "TopologyIoLiveMonitorTimer_Tick",
                        null,
                        EventArgs.Empty);
                    WaitUntil(
                        () => string.Equals(
                            GetRowString(couplerRow, "LiveOnline"),
                            "Yes",
                            StringComparison.Ordinal),
                        "The live node-health sample did not reach the CREVIS coupler row.");
                    AssertEx.Equal(
                        "0x08",
                        GetRowString(couplerRow, "LiveEtherCATState"));
                    AssertEx.Equal(
                        "H=201",
                        GetRowString(couplerRow, "LiveCycle"));

                    InvokePrivate(
                        window,
                        "TopologyIoLiveMonitorTimer_Tick",
                        null,
                        EventArgs.Empty);
                    WaitUntil(
                        () => string.Equals(
                            GetRowString(inputRow, "LiveDigitalInput"),
                            "0xA5A55A5A",
                            StringComparison.Ordinal),
                        "The selected CREVIS digital input did not reach the live UI row.");
                    AssertEx.Contains(
                        "Name=GL_9086_1_Slot001",
                        window.TextSelectedDigitalIO.Text);
                    AssertEx.Contains(
                        "Direction=Input, BitWidth=32",
                        window.TextSelectedDigitalIO.Text);
                    AssertEx.Contains(
                        "Value=0x00000000A5A55A5A",
                        window.TextSelectedDigitalIO.Text);
                    AssertEx.True(
                        GetPrivateField(window, "selectedDigitalOutputShadow")
                            == null,
                        "A selected-input live sample must not create an output shadow.");

                    window.GridEtherCATTopology.SelectedItem = outputRow;
                    WaitUntil(
                        () => window.ButtonReadSelectedDigitalOutput.IsEnabled,
                        "The configured CREVIS output row did not expose its explicit shadow-read button.");
                    AssertEx.False(
                        window.ButtonSubmitDigitalOutputWrite.IsEnabled,
                        "Output write must remain disabled with capability bit 17 off and no explicit shadow.");

                    InvokePrivate(
                        window,
                        "TopologyIoLiveMonitorTimer_Tick",
                        null,
                        EventArgs.Empty);
                    WaitUntil(
                        () => string.Equals(
                            GetRowString(firstDriveRow, "LiveOnline"),
                            "Yes",
                            StringComparison.Ordinal),
                        "Selecting an output row did not leave the background monitor on its health-only path.");
                    AssertEx.Equal(
                        "Selected digital I/O has not been read.",
                        window.TextSelectedDigitalIO.Text);
                    AssertEx.Equal(
                        "-",
                        GetRowString(outputRow, "LiveDigitalInput"));
                    AssertEx.True(
                        GetPrivateField(window, "selectedDigitalOutputShadow")
                            == null,
                        "The background monitor must never populate the write-authorizing output shadow.");

                    var liveEvidence = CaptureTopologyIoLiveEvidence(window);
                    AssertEx.Equal(3, liveEvidence.Records.Count);
                    AssertEx.Equal((ulong)0, liveEvidence.DroppedOldestCount);
                    AssertEx.Equal((ulong)3, liveEvidence.LastSequence);
                    AssertEx.True(
                        window.ButtonSaveTopologyIoLiveEvidence.IsEnabled);
                    AssertEx.Contains(
                        "retained=3, dropped=0",
                        window.TextTopologyIoLiveEvidenceSummary.Text);

                    var couplerEvidence = liveEvidence.Records[0];
                    AssertEx.Equal(
                        TopologyIoLiveEvidenceOrigin.Auto,
                        couplerEvidence.Context.Origin);
                    AssertEx.Equal(
                        TopologyIoLiveEvidenceKind.Health,
                        couplerEvidence.Context.Kind);
                    AssertEx.Equal(
                        TopologyIoLiveEvidenceOutcome.Success,
                        couplerEvidence.Outcome);
                    AssertEx.Equal(DiagnosticsBootId,
                        couplerEvidence.Context.DiagnosticsBootId);
                    AssertEx.Equal(DiagnosticMapRevision,
                        couplerEvidence.Context.MapRevision);
                    AssertEx.Equal((uint)capabilities,
                        couplerEvidence.Context.CapabilityBits);
                    AssertEx.Equal(TopologyRevision,
                        couplerEvidence.Context.TopologyRevision);
                    AssertEx.Equal(CrevisCouplerNodeId,
                        couplerEvidence.Context.NodeId);
                    AssertEx.Equal("automatic post-connect load",
                        couplerEvidence.Context.TopologyLoadOrigin);
                    AssertEx.Equal(11u,
                        couplerEvidence.Context.RequestId.Value);
                    AssertEx.Equal(201u,
                        couplerEvidence.CycleCounter.Value);
                    AssertEx.Equal(2u,
                        couplerEvidence.PlcSnapshotSequence.Value);
                    AssertEx.Equal(201000UL,
                        couplerEvidence.PlcTimestampMicroseconds.Value);

                    var inputEvidence = liveEvidence.Records[1];
                    AssertEx.Equal(
                        TopologyIoLiveEvidenceOrigin.Auto,
                        inputEvidence.Context.Origin);
                    AssertEx.Equal(
                        TopologyIoLiveEvidenceKind.DI,
                        inputEvidence.Context.Kind);
                    AssertEx.Equal(CrevisInputIoReference,
                        inputEvidence.Context.IOReference.Value);
                    AssertEx.Equal(12u,
                        inputEvidence.Context.RequestId.Value);
                    AssertEx.Equal("Input",
                        inputEvidence.Context.RequestedDirection);
                    AssertEx.Equal((byte)32,
                        inputEvidence.Context.RequestedBitWidth.Value);
                    AssertEx.Equal(202u,
                        inputEvidence.CycleCounter.Value);
                    AssertEx.Equal(0xA5A55A5AUL,
                        inputEvidence.Value.Value);
                    AssertEx.Equal(0xFFFFFFFFUL,
                        inputEvidence.ValidMask.Value);

                    Click(window.ButtonDiagnosticsCapabilities);
                    WaitUntil(
                        () => window.TextEtherCATTopologySummary.Text.IndexOf(
                                "LiveHealth=not advertised",
                                StringComparison.Ordinal) >= 0
                            && window.TextEtherCATTopologySummary.Text.IndexOf(
                                "DigitalInput=not advertised",
                                StringComparison.Ordinal) >= 0,
                        "Capability downgrade did not refresh the CREVIS summary.");
                    AssertEx.Equal(
                        "UNAVAILABLE",
                        GetRowString(couplerRow, "LiveOnline"));
                    AssertEx.Contains(
                        "Health=UNAVAILABLE",
                        GetRowString(couplerRow, "LiveQuality"));
                    AssertEx.Equal(
                        "UNAVAILABLE",
                        GetRowString(inputRow, "LiveDigitalInput"));
                    AssertEx.Contains(
                        "DI=UNAVAILABLE",
                        GetRowString(inputRow, "LiveQuality"));
                    AssertEx.False(
                        window.ButtonReadSelectedNodeHealth.IsEnabled);
                    AssertEx.False(
                        window.ButtonReadSelectedDigitalOutput.IsEnabled);
                    AssertEx.Equal(
                        window.TextEtherCATTopologySummary.Text,
                        window.TextCrevisQuickStatus.Text);
                    var retainedAfterCapabilityDowngrade =
                        CaptureTopologyIoLiveEvidence(window);
                    AssertEx.Equal(
                        3,
                        retainedAfterCapabilityDowngrade.Records.Count);
                    AssertEx.True(
                        window.ButtonSaveTopologyIoLiveEvidence.IsEnabled,
                        "Capability downgrade must preserve export of historical current-session evidence.");

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                    AssertFullCapabilityMonitorRequestSequence(
                        server.ReceivedRequests);
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void
            LateManualReadsDoNotOverwriteNewSelectionOrOutputShadow()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology
                | LMCDiagnosticCapability.EtherCATNodeHealth
                | LMCDiagnosticCapability.DigitalIORead;
            var healthRequestReached = new ManualResetEventSlim(false);
            var releaseHealthResponse = new ManualResetEventSlim(false);
            var inputRequestReached = new ManualResetEventSlim(false);
            var releaseInputResponse = new ManualResetEventSlim(false);
            var outputRequestReached = new ManualResetEventSlim(false);
            var releaseOutputResponse = new ManualResetEventSlim(false);
            var steps = CreateConnectAndTopologySteps(capabilities);

            var healthStep = new FakeRpcStep(
                0x7E13,
                TestFrame.Response(
                    0,
                    NodeHealthPayload(
                        11,
                        CrevisCouplerNodeId,
                        301,
                        false)));
            healthStep.InspectRequest = request =>
            {
                AssertNodeHealthRequest(
                    request,
                    11,
                    CrevisCouplerNodeId);
                healthRequestReached.Set();
                if (!releaseHealthResponse.Wait(WaitTimeoutMilliseconds))
                {
                    throw new TimeoutException(
                        "The delayed manual node-health response was not released.");
                }
            };
            steps.Add(healthStep);

            var inputStep = new FakeRpcStep(
                0x7E22,
                TestFrame.Response(
                    0,
                    DigitalInputPayload(
                        12,
                        CrevisInputIoReference,
                        CrevisInputNodeId,
                        32,
                        0x0F0F00FFu,
                        302)));
            inputStep.InspectRequest = request =>
            {
                AssertDigitalInputRequest(
                    request,
                    12,
                    CrevisInputIoReference,
                    32);
                inputRequestReached.Set();
                if (!releaseInputResponse.Wait(WaitTimeoutMilliseconds))
                {
                    throw new TimeoutException(
                        "The delayed manual digital-input response was not released.");
                }
            };
            steps.Add(inputStep);

            var outputStep = new FakeRpcStep(
                0x7E22,
                TestFrame.Response(
                    0,
                    DigitalOutputPayload(
                        13,
                        CrevisOutputIoReference,
                        CrevisOutputNodeId,
                        32,
                        0x55AA00FFu,
                        303,
                        0x01020304u)));
            outputStep.InspectRequest = request =>
            {
                AssertDigitalOutputRequest(
                    request,
                    13,
                    CrevisOutputIoReference,
                    32);
                outputRequestReached.Set();
                if (!releaseOutputResponse.Wait(WaitTimeoutMilliseconds))
                {
                    throw new TimeoutException(
                        "The delayed manual output-shadow response was not released.");
                }
            };
            steps.Add(outputStep);
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (healthRequestReached)
                using (releaseHealthResponse)
                using (inputRequestReached)
                using (releaseInputResponse)
                using (outputRequestReached)
                using (releaseOutputResponse)
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    try
                    {
                        window = CreateWindow(journalDirectory, server.Port);
                    ((DispatcherTimer)GetPrivateField(
                        window,
                        "topologyIoLiveMonitorTimer")).Stop();
                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => window.GridEtherCATTopology.Items.Count
                                == TopologyNodeCount
                            && window.ButtonReadSelectedNodeHealth.IsEnabled,
                        "The topology did not become ready for delayed manual-read smoke.");

                    var couplerRow = FindTopologyRow(
                        window,
                        "GL_9086_11");
                    var inputRow = FindTopologyRow(
                        window,
                        "GL_9086_1_Slot001");
                    var outputRow = FindTopologyRow(
                        window,
                        "GL_9086_1_Slot011");

                    window.GridEtherCATTopology.SelectedItem = couplerRow;
                    Click(window.ButtonReadSelectedNodeHealth);
                    WaitUntil(
                        () => healthRequestReached.IsSet,
                        "The delayed node-health request did not reach the fake PLC.");
                    window.GridEtherCATTopology.SelectedItem = inputRow;
                    AssertEx.Equal(
                        "Selected node health has not been read.",
                        window.TextSelectedNodeHealth.Text);
                    releaseHealthResponse.Set();
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Read Selected EtherCAT Node Health completed",
                            StringComparison.Ordinal),
                        "The delayed node-health operation did not complete.");
                    AssertEx.Equal(
                        "Yes",
                        GetRowString(couplerRow, "LiveOnline"));
                    AssertEx.Equal(
                        "H=301",
                        GetRowString(couplerRow, "LiveCycle"));
                    AssertEx.Equal(
                        "Selected node health has not been read.",
                        window.TextSelectedNodeHealth.Text);

                    WaitUntil(
                        () => window.ButtonReadSelectedDigitalInput.IsEnabled,
                        "The CREVIS input row did not become ready for its delayed read.");
                    Click(window.ButtonReadSelectedDigitalInput);
                    WaitUntil(
                        () => inputRequestReached.IsSet,
                        "The delayed digital-input request did not reach the fake PLC.");
                    window.GridEtherCATTopology.SelectedItem = outputRow;
                    AssertEx.Equal(
                        "Selected digital I/O has not been read.",
                        window.TextSelectedDigitalIO.Text);
                    releaseInputResponse.Set();
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Read Selected Digital Input completed",
                            StringComparison.Ordinal),
                        "The delayed digital-input operation did not complete.");
                    AssertEx.Equal(
                        "0x0F0F00FF",
                        GetRowString(inputRow, "LiveDigitalInput"));
                    AssertEx.Equal(
                        "H=-; DI=302",
                        GetRowString(inputRow, "LiveCycle"));
                    AssertEx.Equal(
                        "Selected digital I/O has not been read.",
                        window.TextSelectedDigitalIO.Text);
                    AssertEx.True(
                        GetPrivateField(window, "selectedDigitalOutputShadow")
                            == null,
                        "A late digital-input response must not create an output shadow.");

                    WaitUntil(
                        () => window.ButtonReadSelectedDigitalOutput.IsEnabled,
                        "The CREVIS output row did not become ready for its delayed shadow read.");
                    Click(window.ButtonReadSelectedDigitalOutput);
                    WaitUntil(
                        () => outputRequestReached.IsSet,
                        "The delayed output-shadow request did not reach the fake PLC.");
                    window.GridEtherCATTopology.SelectedItem = inputRow;
                    AssertEx.Equal(
                        "Selected digital I/O has not been read.",
                        window.TextSelectedDigitalIO.Text);
                    releaseOutputResponse.Set();
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Read Selected Digital Output Shadow completed",
                            StringComparison.Ordinal),
                        "The delayed output-shadow operation did not complete.");

                    AssertEx.Equal(
                        "Selected digital I/O has not been read.",
                        window.TextSelectedDigitalIO.Text);
                    AssertEx.Equal(
                        "-",
                        window.TextDigitalOutputExpectedRevision.Text);
                    AssertEx.Equal(
                        "0x0F0F00FF",
                        GetRowString(inputRow, "LiveDigitalInput"));
                    AssertEx.True(
                        GetPrivateField(window, "selectedDigitalOutputShadow")
                            == null,
                        "A late output-shadow response must not authorize the new selection.");
                    AssertEx.False(window.ButtonSubmitDigitalOutputWrite.IsEnabled);

                    var manualEvidence = CaptureTopologyIoLiveEvidence(window);
                    AssertEx.Equal(
                        2,
                        manualEvidence.Records.Count,
                        "Manual output-shadow reads must not be journaled as DI evidence.");
                    AssertEx.Equal(
                        TopologyIoLiveEvidenceOrigin.Manual,
                        manualEvidence.Records[0].Context.Origin);
                    AssertEx.Equal(
                        TopologyIoLiveEvidenceKind.Health,
                        manualEvidence.Records[0].Context.Kind);
                    AssertEx.Equal(11u,
                        manualEvidence.Records[0].Context.RequestId.Value);
                    AssertEx.Equal(
                        TopologyIoLiveEvidenceOrigin.Manual,
                        manualEvidence.Records[1].Context.Origin);
                    AssertEx.Equal(
                        TopologyIoLiveEvidenceKind.DI,
                        manualEvidence.Records[1].Context.Kind);
                    AssertEx.Equal(12u,
                        manualEvidence.Records[1].Context.RequestId.Value);
                    AssertEx.Equal(0x0F0F00FFUL,
                        manualEvidence.Records[1].Value.Value);
                    AssertEx.True(
                        window.ButtonSaveTopologyIoLiveEvidence.IsEnabled);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                        AssertLateManualReadRequestSequence(
                            server.ReceivedRequests);
                    }
                    finally
                    {
                        releaseHealthResponse.Set();
                        releaseInputResponse.Set();
                        releaseOutputResponse.Set();
                    }
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }

            VerifyMixedIoAutoInputPreservesOutputShadowDetail();
            VerifyInvalidatedManualFailureReportsFailedWithoutUiMutation();
        }

        private static void VerifyMixedIoAutoInputPreservesOutputShadowDetail()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology
                | LMCDiagnosticCapability.EtherCATNodeHealth
                | LMCDiagnosticCapability.DigitalIORead;
            var canonical = CreateMixedIoTopologyCanonicalBytes();
            var mixedTopologyRevision = ComputeTopologyRevision(canonical);
            var steps = CreateConnectAndTopologyStepsForCanonical(
                capabilities,
                canonical,
                mixedTopologyRevision);
            var outputStep = new FakeRpcStep(
                0x7E22,
                TestFrame.Response(
                    0,
                    DigitalOutputPayload(
                        11,
                        CrevisInputIoReference,
                        CrevisInputNodeId,
                        32,
                        0x55AA00FFu,
                        401,
                        0x01020304u,
                        mixedTopologyRevision)));
            outputStep.InspectRequest = request =>
                AssertDigitalOutputRequest(
                    request,
                    11,
                    CrevisInputIoReference,
                    32,
                    mixedTopologyRevision);
            steps.Add(outputStep);

            var automaticInputStep = new FakeRpcStep(
                0x7E22,
                TestFrame.Response(
                    0,
                    DigitalInputPayload(
                        12,
                        CrevisInputIoReference,
                        CrevisInputNodeId,
                        32,
                        0xA5A55A5Au,
                        402,
                        mixedTopologyRevision)));
            automaticInputStep.InspectRequest = request =>
                AssertDigitalInputRequest(
                    request,
                    12,
                    CrevisInputIoReference,
                    32,
                    mixedTopologyRevision);
            steps.Add(automaticInputStep);

            var manualInputStep = new FakeRpcStep(
                0x7E22,
                TestFrame.Response(
                    0,
                    DigitalInputPayload(
                        13,
                        CrevisInputIoReference,
                        CrevisInputNodeId,
                        32,
                        0x0FF00FF0u,
                        403,
                        mixedTopologyRevision)));
            manualInputStep.InspectRequest = request =>
                AssertDigitalInputRequest(
                    request,
                    13,
                    CrevisInputIoReference,
                    32,
                    mixedTopologyRevision);
            steps.Add(manualInputStep);
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(journalDirectory, server.Port);
                    ((DispatcherTimer)GetPrivateField(
                        window,
                        "topologyIoLiveMonitorTimer")).Stop();
                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => window.GridEtherCATTopology.Items.Count
                                == TopologyNodeCount
                            && window.ButtonReadSelectedNodeHealth.IsEnabled,
                        "The mixed-I/O topology did not reach the idle rendered state.");

                    var mixedRow = FindTopologyRow(
                        window,
                        "GL_9086_1_Slot001");
                    AssertEx.Equal("32", GetRowString(mixedRow, "InputBits"));
                    AssertEx.Equal("32", GetRowString(mixedRow, "OutputBits"));
                    window.GridEtherCATTopology.SelectedItem = mixedRow;
                    WaitUntil(
                        () => window.ButtonReadSelectedDigitalInput.IsEnabled
                            && window.ButtonReadSelectedDigitalOutput.IsEnabled,
                        "The mixed CREVIS row did not expose both explicit read directions.");

                    Click(window.ButtonReadSelectedDigitalOutput);
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Read Selected Digital Output Shadow completed",
                            StringComparison.Ordinal),
                        "The mixed-row output-shadow read did not complete.");
                    AssertEx.Contains(
                        "Direction=Output, BitWidth=32",
                        window.TextSelectedDigitalIO.Text);
                    AssertEx.Contains(
                        "OutputRevision=0x01020304",
                        window.TextSelectedDigitalIO.Text);
                    AssertEx.Equal(
                        "0x01020304",
                        window.TextDigitalOutputExpectedRevision.Text);
                    var outputShadow = GetPrivateField(
                        window,
                        "selectedDigitalOutputShadow");
                    AssertEx.NotNull(outputShadow);
                    var outputDetail = window.TextSelectedDigitalIO.Text;

                    var monitorPolicy = GetPrivateField(
                        window,
                        "topologyIoLiveMonitorPolicy");
                    SetPrivateField(
                        monitorPolicy,
                        "scheduleSelectedInputNext",
                        true);
                    InvokeTopologyIoMonitorTick(window);
                    WaitUntil(
                        () => string.Equals(
                            GetRowString(mixedRow, "LiveDigitalInput"),
                            "0xA5A55A5A",
                            StringComparison.Ordinal),
                        "The mixed-row automatic input sample did not reach its row cache.");
                    AssertEx.Equal(outputDetail, window.TextSelectedDigitalIO.Text);
                    AssertEx.True(
                        ReferenceEquals(
                            outputShadow,
                            GetPrivateField(
                                window,
                                "selectedDigitalOutputShadow")),
                        "Automatic DI polling replaced the write-authorizing output shadow.");
                    AssertEx.Equal(
                        "0x01020304",
                        window.TextDigitalOutputExpectedRevision.Text);

                    WaitUntil(
                        () => window.ButtonReadSelectedDigitalInput.IsEnabled,
                        "The mixed row did not return to idle after automatic DI polling.");
                    Click(window.ButtonReadSelectedDigitalInput);
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Read Selected Digital Input completed",
                            StringComparison.Ordinal),
                        "The mixed-row manual input read did not complete.");
                    AssertEx.Contains(
                        "Direction=Input, BitWidth=32",
                        window.TextSelectedDigitalIO.Text);
                    AssertEx.Contains(
                        "Value=0x000000000FF00FF0",
                        window.TextSelectedDigitalIO.Text);
                    AssertEx.True(
                        GetPrivateField(window, "selectedDigitalOutputShadow")
                            == null,
                        "A manual mixed-row input read did not revoke the previous output shadow.");
                    AssertEx.Equal(
                        "-",
                        window.TextDigitalOutputExpectedRevision.Text);
                    AssertEx.False(
                        window.CheckConfirmDigitalOutputWrite.IsChecked == true);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                    AssertMixedIoShadowPreservationRequestSequence(
                        server.ReceivedRequests);
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void
            VerifyInvalidatedManualFailureReportsFailedWithoutUiMutation()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology
                | LMCDiagnosticCapability.EtherCATNodeHealth
                | LMCDiagnosticCapability.DigitalIORead;
            var requestReached = new ManualResetEventSlim(false);
            var releaseResponse = new ManualResetEventSlim(false);
            var malformedHealth = NodeHealthPayload(
                11,
                CrevisCouplerNodeId,
                501,
                false);
            TestFrame.WriteUInt32(malformedHealth, 40, 3);
            var steps = CreateConnectAndTopologySteps(capabilities);
            var healthStep = new FakeRpcStep(
                0x7E13,
                TestFrame.Response(0, malformedHealth));
            healthStep.InspectRequest = request =>
            {
                AssertNodeHealthRequest(
                    request,
                    11,
                    CrevisCouplerNodeId);
                requestReached.Set();
                if (!releaseResponse.Wait(WaitTimeoutMilliseconds))
                {
                    throw new TimeoutException(
                        "The invalidated manual-read failure was not released.");
                }
            };
            steps.Add(healthStep);
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (requestReached)
                using (releaseResponse)
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    try
                    {
                        window = CreateWindow(
                            journalDirectory,
                            server.Port);
                        ((DispatcherTimer)GetPrivateField(
                            window,
                            "topologyIoLiveMonitorTimer")).Stop();
                        Click(window.ButtonConnect);
                        WaitUntil(
                            () => window.GridEtherCATTopology.Items.Count
                                    == TopologyNodeCount
                                && window.ButtonReadSelectedNodeHealth
                                    .IsEnabled,
                            "The topology did not become ready for invalidated-failure smoke.");

                        var couplerRow = FindTopologyRow(
                            window,
                            "GL_9086_11");
                        window.GridEtherCATTopology.SelectedItem = couplerRow;
                        Click(window.ButtonReadSelectedNodeHealth);
                        WaitUntil(
                            () => requestReached.IsSet,
                            "The invalidated manual node-health request did not reach the fake PLC.");

                        InvokePrivate(window, "ClearTopologyIoState");
                        AssertEx.Equal(
                            0,
                            window.GridEtherCATTopology.Items.Count);
                        releaseResponse.Set();
                        WaitUntil(
                            () => string.Equals(
                                window.TextOperationState.Text,
                                "Read Selected EtherCAT Node Health failed",
                                StringComparison.Ordinal),
                            "A failed response for an invalidated topology was misreported as completed.");

                        AssertEx.Equal(
                            "Health=not sampled",
                            GetRowString(couplerRow, "LiveQuality"));
                        AssertEx.Equal(
                            "Selected node health has not been read.",
                            window.TextSelectedNodeHealth.Text);
                        AssertEx.Equal(
                            0,
                            window.GridEtherCATTopology.Items.Count);
                        AssertEx.True(
                            window.TextOperationState.Text.IndexOf(
                                "completed",
                                StringComparison.Ordinal) < 0,
                            "The invalidated failed RPC was reported as completed.");
                        AssertEx.Equal(
                            0,
                            CaptureTopologyIoLiveEvidence(window)
                                .Records.Count,
                            "A response invalidated by topology clear entered the live evidence journal.");
                        AssertEx.False(
                            window.ButtonSaveTopologyIoLiveEvidence.IsEnabled);

                        CloseConnectedWindow(window);
                        window = null;
                        server.Verify();
                        AssertInvalidatedManualFailureRequestSequence(
                            server.ReceivedRequests);
                    }
                    finally
                    {
                        releaseResponse.Set();
                    }
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void
            LiveHealthAndDigitalInputErrorsRemainChannelIndependent()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology
                | LMCDiagnosticCapability.EtherCATNodeHealth
                | LMCDiagnosticCapability.DigitalIORead;
            var malformedHealth = NodeHealthPayload(
                13,
                CrevisInputNodeId,
                303,
                false);
            TestFrame.WriteUInt32(malformedHealth, 40, 3);
            var malformedInput = DigitalInputPayload(
                15,
                CrevisInputIoReference,
                CrevisInputNodeId,
                32,
                0xAAAAAAAAu,
                305);
            TestFrame.WriteUInt32(malformedInput, 52, 1);

            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(new FakeRpcStep(
                0x7E13,
                TestFrame.Response(
                    0,
                    NodeHealthPayload(
                        11,
                        CrevisInputNodeId,
                        301,
                        false)))
            {
                InspectRequest = request => AssertNodeHealthRequest(
                    request,
                    11,
                    CrevisInputNodeId)
            });
            steps.Add(new FakeRpcStep(
                0x7E22,
                TestFrame.Response(
                    0,
                    DigitalInputPayload(
                        12,
                        CrevisInputIoReference,
                        CrevisInputNodeId,
                        32,
                        0x0F0F00FFu,
                        302)))
            {
                InspectRequest = request => AssertDigitalInputRequest(
                    request,
                    12,
                    CrevisInputIoReference,
                    32)
            });
            steps.Add(new FakeRpcStep(
                0x7E13,
                TestFrame.Response(0, malformedHealth))
            {
                InspectRequest = request => AssertNodeHealthRequest(
                    request,
                    13,
                    CrevisInputNodeId)
            });
            steps.Add(new FakeRpcStep(
                0x7E22,
                TestFrame.Response(
                    0,
                    DigitalInputPayload(
                        14,
                        CrevisInputIoReference,
                        CrevisInputNodeId,
                        32,
                        0x5AA5A55Au,
                        304)))
            {
                InspectRequest = request => AssertDigitalInputRequest(
                    request,
                    14,
                    CrevisInputIoReference,
                    32)
            });
            steps.Add(new FakeRpcStep(
                0x7E22,
                TestFrame.Response(0, malformedInput))
            {
                InspectRequest = request => AssertDigitalInputRequest(
                    request,
                    15,
                    CrevisInputIoReference,
                    32)
            });
            steps.Add(new FakeRpcStep(
                0x7E13,
                TestFrame.Response(
                    0,
                    NodeHealthPayload(
                        16,
                        CrevisInputNodeId,
                        306,
                        false)))
            {
                InspectRequest = request => AssertNodeHealthRequest(
                    request,
                    16,
                    CrevisInputNodeId)
            });
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(journalDirectory, server.Port);
                    ((DispatcherTimer)GetPrivateField(
                        window,
                        "topologyIoLiveMonitorTimer")).Stop();
                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => window.GridEtherCATTopology.Items.Count
                                == TopologyNodeCount
                            && window.ButtonReadSelectedNodeHealth.IsEnabled,
                        "The topology did not become ready for channel-state smoke.");

                    var inputRow = FindTopologyRow(
                        window,
                        "GL_9086_1_Slot001");
                    window.GridEtherCATTopology.SelectedItem = inputRow;
                    WaitUntil(
                        () => window.ButtonReadSelectedDigitalInput.IsEnabled,
                        "The CREVIS input row did not become ready for channel-state smoke.");

                    var monitorPolicy = GetPrivateField(
                        window,
                        "topologyIoLiveMonitorPolicy");
                    SetPrivateField(monitorPolicy, "nextHealthIndex", 5);
                    SetPrivateField(
                        monitorPolicy,
                        "scheduleSelectedInputNext",
                        false);

                    InvokeTopologyIoMonitorTick(window);
                    WaitUntil(
                        () => string.Equals(
                            GetRowString(inputRow, "LiveOnline"),
                            "Yes",
                            StringComparison.Ordinal),
                        "The initial input-row health sample did not reach the UI.");
                    AssertEx.Equal(
                        "H=301; DI=-",
                        GetRowString(inputRow, "LiveCycle"));

                    InvokeTopologyIoMonitorTick(window);
                    WaitUntil(
                        () => string.Equals(
                            GetRowString(inputRow, "LiveDigitalInput"),
                            "0x0F0F00FF",
                            StringComparison.Ordinal),
                        "The initial selected-input sample did not reach the UI.");
                    AssertEx.Contains(
                        "Health=Configured",
                        GetRowString(inputRow, "LiveQuality"));
                    AssertEx.Contains(
                        "DI=Valid",
                        GetRowString(inputRow, "LiveQuality"));

                    SetPrivateField(monitorPolicy, "nextHealthIndex", 5);
                    SetPrivateField(
                        monitorPolicy,
                        "scheduleSelectedInputNext",
                        false);
                    InvokeTopologyIoMonitorTick(window);
                    WaitUntil(
                        () => GetRowString(inputRow, "LiveQuality").IndexOf(
                                "Health=ERROR:",
                                StringComparison.Ordinal) >= 0,
                        "The malformed health sample did not mark only the health channel erroneous.");
                    AssertEx.Equal(
                        "stale Yes",
                        GetRowString(inputRow, "LiveOnline"));
                    AssertEx.Equal(
                        "0x0F0F00FF",
                        GetRowString(inputRow, "LiveDigitalInput"));
                    AssertEx.Contains(
                        "DI=Valid",
                        GetRowString(inputRow, "LiveQuality"));
                    AssertEx.Contains(
                        "LATEST NODE HEALTH READ FAILED for GL_9086_1_Slot001",
                        window.TextSelectedNodeHealth.Text);
                    AssertEx.Contains(
                        "Any earlier value for this channel is stale.",
                        window.TextSelectedNodeHealth.Text);

                    SetPrivateField(
                        monitorPolicy,
                        "nextAllowedUtc",
                        DateTime.MinValue);
                    InvokeTopologyIoMonitorTick(window);
                    WaitUntil(
                        () => string.Equals(
                            GetRowString(inputRow, "LiveDigitalInput"),
                            "0x5AA5A55A",
                            StringComparison.Ordinal),
                        "The recovery digital-input sample did not reach the UI.");
                    AssertEx.Equal(
                        "stale Yes",
                        GetRowString(inputRow, "LiveOnline"));
                    AssertEx.Contains(
                        "Health=ERROR:",
                        GetRowString(inputRow, "LiveQuality"));
                    AssertEx.Contains(
                        "DI=Valid",
                        GetRowString(inputRow, "LiveQuality"));
                    AssertEx.Equal(
                        "H=301; DI=304",
                        GetRowString(inputRow, "LiveCycle"));
                    AssertEx.Contains(
                        "Direction=Input, BitWidth=32",
                        window.TextSelectedDigitalIO.Text);
                    AssertEx.Contains(
                        "Value=0x000000005AA5A55A",
                        window.TextSelectedDigitalIO.Text);

                    SetPrivateField(
                        monitorPolicy,
                        "scheduleSelectedInputNext",
                        true);
                    InvokeTopologyIoMonitorTick(window);
                    WaitUntil(
                        () => GetRowString(inputRow, "LiveQuality").IndexOf(
                                "DI=ERROR:",
                                StringComparison.Ordinal) >= 0,
                        "The malformed input sample did not mark only the DI channel erroneous.");
                    AssertEx.Equal(
                        "stale 0x5AA5A55A",
                        GetRowString(inputRow, "LiveDigitalInput"));
                    AssertEx.Contains(
                        "Health=ERROR:",
                        GetRowString(inputRow, "LiveQuality"));
                    AssertEx.Contains(
                        "LATEST DIGITAL INPUT READ FAILED for GL_9086_1_Slot001",
                        window.TextSelectedDigitalIO.Text);
                    AssertEx.Contains(
                        "Any earlier value for this channel is stale.",
                        window.TextSelectedDigitalIO.Text);

                    SetPrivateField(
                        monitorPolicy,
                        "nextAllowedUtc",
                        DateTime.MinValue);
                    SetPrivateField(monitorPolicy, "nextHealthIndex", 5);
                    SetPrivateField(
                        monitorPolicy,
                        "scheduleSelectedInputNext",
                        false);
                    InvokeTopologyIoMonitorTick(window);
                    WaitUntil(
                        () => string.Equals(
                            GetRowString(inputRow, "LiveOnline"),
                            "Yes",
                            StringComparison.Ordinal),
                        "The recovery health sample did not clear only the health channel error.");
                    var finalQuality = GetRowString(
                        inputRow,
                        "LiveQuality");
                    AssertEx.Contains("Health=Configured", finalQuality);
                    AssertEx.Contains("DI=ERROR:", finalQuality);
                    AssertEx.True(
                        finalQuality.IndexOf(
                            "Health=ERROR:",
                            StringComparison.Ordinal) < 0,
                        "A successful health sample did not clear its own error.");
                    AssertEx.Equal(
                        "stale 0x5AA5A55A",
                        GetRowString(inputRow, "LiveDigitalInput"));
                    AssertEx.Equal(
                        "H=306; DI=304",
                        GetRowString(inputRow, "LiveCycle"));
                    AssertEx.Contains(
                        "Cycle=306",
                        window.TextSelectedNodeHealth.Text);
                    AssertEx.Contains(
                        "LATEST DIGITAL INPUT READ FAILED for GL_9086_1_Slot001",
                        window.TextSelectedDigitalIO.Text);

                    var failureEvidence =
                        CaptureTopologyIoLiveEvidence(window);
                    AssertEx.Equal(6, failureEvidence.Records.Count);
                    AssertEx.Equal(
                        TopologyIoLiveEvidenceKind.Health,
                        failureEvidence.Records[2].Context.Kind);
                    AssertEx.Equal(
                        TopologyIoLiveEvidenceOutcome.Failure,
                        failureEvidence.Records[2].Outcome);
                    AssertEx.False(
                        failureEvidence.Records[2].CycleCounter.HasValue);
                    AssertEx.False(
                        failureEvidence.Records[2].Online.HasValue);
                    AssertEx.Equal(
                        TopologyIoLiveEvidenceKind.DI,
                        failureEvidence.Records[4].Context.Kind);
                    AssertEx.Equal(
                        TopologyIoLiveEvidenceOutcome.Failure,
                        failureEvidence.Records[4].Outcome);
                    AssertEx.False(
                        failureEvidence.Records[4].Value.HasValue);
                    AssertEx.False(
                        failureEvidence.Records[4].ValidMask.HasValue);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                    AssertChannelIndependentMonitorRequestSequence(
                        server.ReceivedRequests);
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void
            CapabilityOffAutoLoadThenManualReloadRecoversCrevis()
        {
            var canonical = CreateTopologyCanonicalBytes();
            var steps = new List<FakeRpcStep>
            {
                InitStep(),
                CallbackStep(),
                CapabilitiesStep(1, LMCDiagnosticCapability.None),
                CapabilitiesStep(
                    2,
                    LMCDiagnosticCapability.EtherCATTopology),
                CapabilitiesStep(
                    3,
                    LMCDiagnosticCapability.EtherCATTopology),
                new FakeRpcStep(
                    0x7E11,
                    TestFrame.Response(0, TopologyInfoPayload(4)))
            };

            for (ushort startIndex = 0;
                startIndex < TopologyNodeCount;
                startIndex++)
            {
                steps.Add(new FakeRpcStep(
                    0x7E12,
                    TestFrame.Response(
                        0,
                        TopologyChunkPayload(
                            checked((uint)(5 + startIndex)),
                            startIndex,
                            canonical))));
            }

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
                        () => string.Equals(
                                window.TextConnectionState.Text,
                                "Connected",
                                StringComparison.Ordinal)
                            && window.ButtonLoadEtherCATTopology.IsEnabled
                            && window.TextEtherCATTopologySummary.Text.IndexOf(
                                "LOAD FAILED (automatic post-connect load)",
                                StringComparison.Ordinal) >= 0,
                        "Capability-off auto-load did not leave a connected manual-retry state.");

                    AssertEx.Equal(0, window.GridEtherCATTopology.Items.Count);
                    AssertRequestCommandSequence(
                        server.ReceivedRequests,
                        0x8080,
                        0x405C,
                        0x7E00);

                    Click(window.ButtonLoadEtherCATTopology);
                    WaitUntil(
                        () => window.GridEtherCATTopology.Items.Count
                                == TopologyNodeCount
                            && window.TextEtherCATTopologySummary.Text.IndexOf(
                                "Load=manual reload",
                                StringComparison.Ordinal) >= 0,
                        "Manual topology reload did not recover the configured CREVIS rows.");

                    AssertEx.Equal(
                        (int)TopologyNodeCount,
                        window.GridEtherCATTopology.Items.Count);
                    AssertEx.Equal(3, CountCrevisRows(window));
                    AssertEx.Contains(
                        "Configured CREVIS entries=3",
                        window.TextEtherCATTopologySummary.Text);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                    AssertRequestCommandSequence(
                        server.ReceivedRequests,
                        0x8080,
                        0x405C,
                        0x7E00,
                        0x7E00,
                        0x7E00,
                        0x7E11,
                        0x7E12,
                        0x7E12,
                        0x7E12,
                        0x7E12,
                        0x7E12,
                        0x7E12,
                        0x7E12,
                        0x405D);
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void InlineReadOneClickRendersTypedTerminalResult()
        {
            var capabilities = LMCDiagnosticCapability.SDORead
                | LMCDiagnosticCapability.SDOReadGeneralInline
                | LMCDiagnosticCapability.EtherCATTopology;
            const uint ticketId = 4101;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(D5SdoSubmitStep(
                13,
                ticketId,
                2001,
                1,
                0x6064,
                LMCSignalValueType.UInt16,
                2,
                10));
            steps.Add(D5SdoOperationStatusStep(
                14,
                ticketId,
                2001,
                2002,
                LMCOperationState.Completed,
                LMCOperationOutcome.Success,
                LMCSignalValueType.UInt16,
                new byte[] { 0x34, 0x12 }));
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
                        "Inline SDO success smoke setup did not complete connection/topology loading.");

                    window.ComboSdoOperation.SelectedItem =
                        GetSdoReadMode(window);
                    window.TextSdoSlaveReference.Text = "1";
                    window.TextSdoIndex.Text = "0x6064";
                    window.TextSdoSubIndex.Text = "0";
                    window.ComboSdoValueType.SelectedItem =
                        LMCSignalValueType.UInt16;
                    window.ComboSdoDataLength.SelectedItem = (ushort)2;
                    window.TextSdoTimeoutCycles.Text = "10";
                    PumpDispatcherOnce();

                    AssertEx.True(window.ButtonReadSdoInline.IsEnabled);
                    Click(window.ButtonReadSdoInline);
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Read SDO Inline completed",
                            StringComparison.Ordinal),
                        "One-click Inline SDO Read did not reach its terminal UI state.");

                    AssertEx.Contains(
                        "TicketId=" + ticketId.ToString(
                            CultureInfo.InvariantCulture),
                        window.TextDiagnosticOperationSummary.Text);
                    AssertEx.Contains(
                        "State=Completed, Outcome=Success",
                        window.TextDiagnosticOperationSummary.Text);
                    AssertEx.Contains(
                        "TypedValue=4660 (UInt16)",
                        window.TextDiagnosticOperationSummary.Text);
                    AssertEx.Contains(
                        "Raw=34 12",
                        window.TextDiagnosticOperationSummary.Text);
                    AssertEx.Contains(
                        "No manual Refresh was required",
                        window.TextDiagnosticOperationSummary.Text);
                    AssertEx.Equal(
                        "34-12",
                        BitConverter.ToString(
                            (byte[])GetPrivateField(
                                window,
                                "diagnosticOperationResult")));
                    AssertEx.True(
                        window.ButtonRefreshDiagnosticOperation.IsEnabled,
                        "The completed ticket must remain available to the existing low-level Refresh path.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E50));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E03));

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
            InlineReadAcceptedTimeoutPreservesTicketForManualCleanup()
        {
            var capabilities = LMCDiagnosticCapability.SDORead
                | LMCDiagnosticCapability.SDOReadGeneralInline
                | LMCDiagnosticCapability.EtherCATTopology;
            const uint ticketId = 4102;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(D5SdoSubmitStep(
                13,
                ticketId,
                2101,
                1,
                0x6061,
                LMCSignalValueType.Int8,
                1,
                1));
            for (var poll = 0; poll < 33; poll++)
            {
                steps.Add(D5SdoOperationStatusStep(
                    checked((uint)(14 + poll)),
                    ticketId,
                    2101,
                    0,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending,
                    LMCSignalValueType.Invalid,
                    new byte[0]));
            }

            steps.Add(D5SdoOperationStatusStep(
                47,
                ticketId,
                2101,
                2200,
                LMCOperationState.Completed,
                LMCOperationOutcome.Success,
                LMCSignalValueType.Int8,
                new byte[] { 8 }));
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
                        "Inline SDO timeout smoke setup did not complete connection/topology loading.");

                    window.ComboSdoOperation.SelectedItem =
                        GetSdoReadMode(window);
                    window.TextSdoSlaveReference.Text = "1";
                    window.TextSdoIndex.Text = "0x6061";
                    window.TextSdoSubIndex.Text = "0";
                    window.ComboSdoValueType.SelectedItem =
                        LMCSignalValueType.Int8;
                    window.ComboSdoDataLength.SelectedItem = (ushort)1;
                    window.TextSdoTimeoutCycles.Text = "1";
                    PumpDispatcherOnce();

                    AssertEx.True(window.ButtonReadSdoInline.IsEnabled);
                    Click(window.ButtonReadSdoInline);
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Read SDO Inline failed",
                            StringComparison.Ordinal),
                        "Accepted Inline SDO timeout did not reach the manual-cleanup UI state.");

                    AssertEx.Contains(
                        "TicketId=" + ticketId.ToString(
                            CultureInfo.InvariantCulture),
                        window.TextDiagnosticOperationSummary.Text);
                    AssertEx.Contains(
                        "ticket is preserved",
                        window.TextDiagnosticOperationSummary.Text);
                    AssertEx.Contains(
                        "do not resubmit",
                        window.TextDiagnosticOperationSummary.Text);
                    AssertEx.False(
                        window.ButtonReadSdoInline.IsEnabled,
                        "A preserved nonterminal ticket must block another Inline submission.");
                    AssertEx.True(
                        window.ButtonRefreshDiagnosticOperation.IsEnabled,
                        "The preserved ticket must be recoverable through the existing Refresh path.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E50));

                    Click(window.ButtonRefreshDiagnosticOperation);
                    WaitUntil(
                        () => window.TextDiagnosticOperationSummary.Text
                            .IndexOf(
                                "State=Completed, Outcome=Success",
                                StringComparison.Ordinal) >= 0,
                        "Manual Refresh did not resolve the preserved Inline SDO ticket.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E50));

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
            InlineReadGeneralCapabilityOffForcedAttemptIsZeroWire()
        {
            var capabilities = LMCDiagnosticCapability.SDORead
                | LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
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
                        "Inline SDO capability-off smoke setup did not complete connection/topology loading.");

                    var requestCountBeforeForcedAttempt =
                        server.ReceivedRequests.Count;
                    window.TextSdoIndex.Text = "0x6061";
                    window.ComboSdoValueType.SelectedItem =
                        LMCSignalValueType.Int8;
                    window.ComboSdoDataLength.SelectedItem = (ushort)1;
                    InvokePrivate(
                        window,
                        "ButtonReadSdoInline_Click",
                        window.ButtonReadSdoInline,
                        new RoutedEventArgs());
                    PumpDispatcherOnce();

                    AssertEx.Equal(
                        requestCountBeforeForcedAttempt,
                        server.ReceivedRequests.Count,
                        "A forced general-inline read must remain zero-wire when bit 13 is off.");
                    AssertEx.Contains(
                        "SDOReadGeneralInline capability",
                        window.TextDiagnosticOperationSummary.Text);
                    AssertEx.Contains(
                        "Not submitted",
                        window.TextDiagnosticOperationSummary.Text);

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

        private static void InlineReadPreAcceptanceWaitCancelIsZeroSubmit()
        {
            var capabilities = LMCDiagnosticCapability.SDORead
                | LMCDiagnosticCapability.SDOReadGeneralInline
                | LMCDiagnosticCapability.EtherCATTopology;
            var capabilityRequestReached = new ManualResetEventSlim(false);
            var releaseCapabilityResponse = new ManualResetEventSlim(false);
            var steps = CreateConnectAndTopologySteps(capabilities);
            var gatedCapabilities = CapabilitiesStep(11, capabilities);
            gatedCapabilities.InspectRequest = request =>
            {
                capabilityRequestReached.Set();
                if (!releaseCapabilityResponse.Wait(WaitTimeoutMilliseconds))
                {
                    throw new TimeoutException(
                        "The pre-accept Inline cancellation smoke did not release the capability response.");
                }
            };
            steps.Add(gatedCapabilities);
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (capabilityRequestReached)
                using (releaseCapabilityResponse)
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(journalDirectory, server.Port);
                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => window.GridEtherCATTopology.Items.Count
                            == TopologyNodeCount,
                        "Pre-accept Inline cancellation smoke setup did not complete connection/topology loading.");

                    AssertEx.True(window.ButtonReadSdoInline.IsEnabled);
                    Click(window.ButtonReadSdoInline);
                    WaitUntil(
                        () => capabilityRequestReached.IsSet,
                        "Inline Read did not reach its external capability preflight.");
                    AssertEx.True(
                        window.ButtonCancelSdoInlineWait.IsEnabled,
                        "The dedicated PC wait cancel button was not enabled during preflight.");

                    Click(window.ButtonCancelSdoInlineWait);
                    AssertEx.False(
                        window.ButtonCancelSdoInlineWait.IsEnabled,
                        "A consumed wait-cancel request must disable repeated cancellation.");
                    releaseCapabilityResponse.Set();
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Read SDO Inline failed",
                            StringComparison.Ordinal)
                            && window.ButtonReadSdoInline.IsEnabled,
                        "Pre-accept wait cancellation did not return the Inline UI to idle.");

                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E50),
                        "Pre-accept PC wait cancellation must not submit an SDO ticket.");
                    AssertEx.Contains(
                        "NOT_SUBMITTED_CANCELLED",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "no PLC CancelOperation or replay",
                        window.TextExecutionLog.Text);
                    AssertEx.True(
                        GetPrivateField(
                            window,
                            "diagnosticOperationTicket") == null);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                releaseCapabilityResponse.Set();
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void InlineReadAcceptedWaitCancelPreservesTicket()
        {
            var capabilities = LMCDiagnosticCapability.SDORead
                | LMCDiagnosticCapability.SDOReadGeneralInline
                | LMCDiagnosticCapability.EtherCATTopology;
            const uint ticketId = 4103;
            var statusRequestReached = new ManualResetEventSlim(false);
            var releaseStatusResponse = new ManualResetEventSlim(false);
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(D5SdoSubmitStep(
                13,
                ticketId,
                2301,
                1,
                0x6061,
                LMCSignalValueType.Int8,
                1,
                1000));
            var gatedStatus = D5SdoOperationStatusStep(
                14,
                ticketId,
                2301,
                0,
                LMCOperationState.Running,
                LMCOperationOutcome.NoneOrPending,
                LMCSignalValueType.Invalid,
                new byte[0]);
            var inspectStatusRequest = gatedStatus.InspectRequest;
            gatedStatus.InspectRequest = request =>
            {
                inspectStatusRequest(request);
                statusRequestReached.Set();
                if (!releaseStatusResponse.Wait(WaitTimeoutMilliseconds))
                {
                    throw new TimeoutException(
                        "The accepted Inline cancellation smoke did not release the status response.");
                }
            };
            steps.Add(gatedStatus);
            steps.Add(D5SdoOperationStatusStep(
                15,
                ticketId,
                2301,
                2400,
                LMCOperationState.Completed,
                LMCOperationOutcome.Success,
                LMCSignalValueType.Int8,
                new byte[] { 8 }));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (statusRequestReached)
                using (releaseStatusResponse)
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(journalDirectory, server.Port);
                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => window.GridEtherCATTopology.Items.Count
                            == TopologyNodeCount,
                        "Accepted Inline cancellation smoke setup did not complete connection/topology loading.");

                    window.TextSdoIndex.Text = "0x6061";
                    window.ComboSdoValueType.SelectedItem =
                        LMCSignalValueType.Int8;
                    window.ComboSdoDataLength.SelectedItem = (ushort)1;
                    window.TextSdoTimeoutCycles.Text = "1000";
                    PumpDispatcherOnce();

                    Click(window.ButtonReadSdoInline);
                    WaitUntil(
                        () => statusRequestReached.IsSet,
                        "Inline Read did not reach status polling after ticket acceptance.");
                    AssertEx.True(window.ButtonCancelSdoInlineWait.IsEnabled);
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E50));

                    InvokePrivate(
                        window,
                        "ButtonReadSdoInline_Click",
                        window.ButtonReadSdoInline,
                        new RoutedEventArgs());
                    PumpDispatcherOnce();
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E50),
                        "A forced double-click must not create a second Inline submission.");
                    AssertEx.Contains(
                        "another Inline wait already owns",
                        window.TextExecutionLog.Text);

                    Click(window.ButtonCancelSdoInlineWait);
                    releaseStatusResponse.Set();
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Read SDO Inline failed",
                            StringComparison.Ordinal),
                        "Accepted PC wait cancellation did not preserve the ticket for manual cleanup.");

                    AssertEx.Contains(
                        "TicketId=" + ticketId.ToString(
                            CultureInfo.InvariantCulture),
                        window.TextDiagnosticOperationSummary.Text);
                    AssertEx.Contains(
                        "State=Running",
                        window.TextDiagnosticOperationSummary.Text);
                    AssertEx.Contains(
                        "ticket is preserved",
                        window.TextDiagnosticOperationSummary.Text);
                    AssertEx.Contains(
                        "LMCSdoReadWaitCanceledException",
                        window.TextExecutionLog.Text);
                    AssertEx.False(window.ButtonCancelSdoInlineWait.IsEnabled);
                    AssertEx.False(
                        window.ButtonReadSdoInline.IsEnabled,
                        "An accepted cancelled wait must block a replacement submit until cleanup.");
                    AssertEx.True(
                        window.ButtonRefreshDiagnosticOperation.IsEnabled);

                    Click(window.ButtonRefreshDiagnosticOperation);
                    WaitUntil(
                        () => window.TextDiagnosticOperationSummary.Text
                            .IndexOf(
                                "State=Completed, Outcome=Success",
                                StringComparison.Ordinal) >= 0,
                        "Manual Refresh did not resolve the accepted cancelled-wait ticket.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x7E50),
                        "Manual cleanup must not replay the SDO submission.");

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                releaseStatusResponse.Set();
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void
            InlineReadTerminalFailureUsesExactTerminalResolution()
        {
            var capabilities = LMCDiagnosticCapability.SDORead
                | LMCDiagnosticCapability.SDOReadGeneralInline
                | LMCDiagnosticCapability.EtherCATTopology;
            const uint ticketId = 4104;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(D5SdoSubmitStep(
                13,
                ticketId,
                2501,
                1,
                0x6061,
                LMCSignalValueType.Int8,
                1,
                10));
            steps.Add(D5SdoOperationStatusStep(
                14,
                ticketId,
                2501,
                2502,
                LMCOperationState.Failed,
                LMCOperationOutcome.Failed,
                LMCSignalValueType.Invalid,
                new byte[0]));
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
                        "Inline terminal-failure smoke setup did not complete connection/topology loading.");

                    window.TextSdoIndex.Text = "0x6061";
                    window.ComboSdoValueType.SelectedItem =
                        LMCSignalValueType.Int8;
                    window.ComboSdoDataLength.SelectedItem = (ushort)1;
                    window.TextSdoTimeoutCycles.Text = "10";
                    PumpDispatcherOnce();

                    Click(window.ButtonReadSdoInline);
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Read SDO Inline failed",
                            StringComparison.Ordinal),
                        "Inline terminal failure did not reach the exact terminal UI state.");

                    AssertEx.Contains(
                        "TicketId=" + ticketId.ToString(
                            CultureInfo.InvariantCulture),
                        window.TextDiagnosticOperationSummary.Text);
                    AssertEx.Contains(
                        "State=Failed, Outcome=Failed",
                        window.TextDiagnosticOperationSummary.Text);
                    AssertEx.Contains(
                        "terminal failure",
                        window.TextDiagnosticOperationSummary.Text);
                    AssertEx.Contains(
                        "state=TERMINAL_OPERATION_FAILURE",
                        window.TextExecutionLog.Text);
                    AssertEx.False(
                        window.TextExecutionLog.Text.IndexOf(
                            "state=KNOWN_TICKET_PRESERVED",
                            StringComparison.Ordinal) >= 0,
                        "A terminal Inline failure must not be logged as a preserved nonterminal ticket.");
                    AssertEx.True(
                        window.ButtonReadSdoInline.IsEnabled,
                        "Exact terminal failure must clear the external guard for a later explicit request.");
                    AssertEx.True(
                        window.ButtonRefreshDiagnosticOperation.IsEnabled,
                        "The exact terminal ticket must remain visible to the low-level Refresh path.");

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

        private static object GetSdoReadMode(MainWindow window)
        {
            foreach (var item in window.ComboSdoOperation.Items)
            {
                if (string.Equals(
                    Convert.ToString(item, CultureInfo.InvariantCulture),
                    "Read",
                    StringComparison.Ordinal))
                {
                    return item;
                }
            }

            throw new InvalidOperationException(
                "The WPF SDO Read mode was not available.");
        }

        private static void
            WriteConfirmationRequiresExactSecondClickWithoutModal()
        {
            var state = new SdoWriteConfirmationState();
            var owner = new object();
            var otherOwner = new object();
            var first = LMCSdoRequest.CreateWrite(
                1,
                0x2F00,
                24,
                LMCSignalValueType.UInt32,
                new byte[] { 0x44, 0x33, 0x22, 0x11 },
                1000);
            var edited = LMCSdoRequest.CreateWrite(
                1,
                0x2F00,
                24,
                LMCSignalValueType.UInt32,
                new byte[] { 0x45, 0x33, 0x22, 0x11 },
                1000);

            AssertEx.False(
                state.TryConsumeOrArm(
                    owner,
                    1,
                    DiagnosticsBootId,
                    DiagnosticMapRevision,
                    first),
                "The first click must arm only and must not authorize a Write.");
            AssertEx.True(state.IsArmed);

            AssertEx.False(
                state.TryConsumeOrArm(
                    owner,
                    1,
                    DiagnosticsBootId,
                    DiagnosticMapRevision,
                    edited),
                "Editing the immutable request must re-arm the changed snapshot instead of consuming the old confirmation.");
            AssertEx.True(state.IsArmed);
            AssertEx.True(
                state.TryConsumeOrArm(
                    owner,
                    1,
                    DiagnosticsBootId,
                    DiagnosticMapRevision,
                    edited),
                "An exact second click must consume the armed snapshot.");
            AssertEx.False(state.IsArmed);

            AssertEx.False(
                state.TryConsumeOrArm(
                    owner,
                    1,
                    DiagnosticsBootId,
                    DiagnosticMapRevision,
                    first));
            AssertEx.False(
                state.TryConsumeOrArm(
                    otherOwner,
                    2,
                    DiagnosticsBootId,
                    DiagnosticMapRevision,
                    first),
                "A new connection/session must re-arm instead of consuming a stale confirmation.");
            AssertEx.True(
                state.TryConsumeOrArm(
                    otherOwner,
                    2,
                    DiagnosticsBootId,
                    DiagnosticMapRevision,
                    first));
            AssertEx.False(state.IsArmed);

            state.TryConsumeOrArm(
                owner,
                1,
                DiagnosticsBootId,
                DiagnosticMapRevision,
                first);
            state.Clear();
            AssertEx.False(state.IsArmed);

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                window = new MainWindow(journalDirectory)
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
                    "The SDO Write editor smoke window did not load.");
                window.ComboSdoOperation.SelectedIndex = 1;
                PumpDispatcherOnce();

                var uiState = (SdoWriteConfirmationState)GetPrivateField(
                    window,
                    "sdoWriteConfirmationState");
                Action armUi = () =>
                {
                    AssertEx.False(
                        uiState.TryConsumeOrArm(
                            owner,
                            1,
                            DiagnosticsBootId,
                            DiagnosticMapRevision,
                            first));
                    InvokePrivate(window, "UpdateUiState");
                    AssertEx.Equal(
                        "Confirm & Submit SDO Write",
                        Convert.ToString(
                            window.ButtonSubmitSdo.Content,
                            CultureInfo.InvariantCulture));
                };
                Action<Action> assertEditRearms = edit =>
                {
                    armUi();
                    edit();
                    PumpDispatcherOnce();
                    AssertEx.False(
                        uiState.IsArmed,
                        "Editing any SDO Write request field must invalidate the armed snapshot immediately.");
                    AssertEx.Equal(
                        "Arm SDO Write",
                        Convert.ToString(
                            window.ButtonSubmitSdo.Content,
                            CultureInfo.InvariantCulture));
                };

                assertEditRearms(
                    () => window.TextSdoSlaveReference.Text = "2");
                assertEditRearms(
                    () => window.TextSdoIndex.Text = "0x1001");
                assertEditRearms(
                    () => window.TextSdoSubIndex.Text = "1");
                assertEditRearms(
                    () => window.ComboSdoValueType.SelectedItem =
                        LMCSignalValueType.UInt16);
                assertEditRearms(
                    () => window.ComboSdoDataLength.SelectedItem = (ushort)4);
                assertEditRearms(
                    () => window.TextSdoTimeoutCycles.Text = "1001");
                assertEditRearms(
                    () => window.TextSdoWriteData.Text = "1");
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void OrdinaryInFlightKeepsWriteEditorEditable()
        {
            var capabilities = LMCDiagnosticCapability.SDORead
                | LMCDiagnosticCapability.SDOWrite
                | LMCDiagnosticCapability.SDOReadGeneralInline
                | LMCDiagnosticCapability.EtherCATTopology;
            var requestReached = new ManualResetEventSlim(false);
            var releaseResponse = new ManualResetEventSlim(false);
            var steps = CreateConnectAndTopologySteps(capabilities);
            var gatedCapabilities = CapabilitiesStep(11, capabilities);
            gatedCapabilities.InspectRequest = request =>
            {
                requestReached.Set();
                if (!releaseResponse.Wait(WaitTimeoutMilliseconds))
                {
                    throw new TimeoutException(
                        "The WPF smoke test did not release the gated capabilities response.");
                }
            };
            steps.Add(gatedCapabilities);
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (requestReached)
                using (releaseResponse)
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(journalDirectory, server.Port);
                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => window.GridEtherCATTopology.Items.Count
                            == TopologyNodeCount,
                        "Initial topology auto-load did not complete.");

                    window.ComboSdoOperation.SelectedIndex = 1;
                    PumpDispatcherOnce();
                    AssertEx.True(
                        window.ButtonRunD5SdoContentionQualification.IsEnabled,
                        "D5 contention qualification must be enabled only after the exact read capability preflight is available.");
                    AssertEx.True(
                        window.ButtonRunD5SdoTimeoutQualification.IsEnabled,
                        "D5 timeout qualification must be enabled only after the exact read capability preflight is available.");
                    AssertEx.True(
                        window.ButtonRunD5SdoQueuedCancelQualification.IsEnabled,
                        "D5 queued-cancel qualification must be enabled only after the exact read capability preflight is available.");
                    AssertEx.True(
                        window.ButtonRunD5SdoDisconnectRecoveryQualification.IsEnabled,
                        "D5 abrupt-disconnect application recovery must be enabled only after the exact read capability preflight is available.");
                    AssertWriteEditorEnabled(window);
                    AssertEx.False(
                        window.ComboSdoWriteTarget.IsEnabled,
                        "The empty SDK write allowlist must remain fail-closed.");
                    AssertEx.False(
                        window.ButtonSubmitSdo.IsEnabled,
                        "SDO Write submit must remain disabled without an approved target.");
                    AssertEx.False(
                        window.ButtonReadSdoInline.IsEnabled,
                        "Inline SDO Read must remain disabled while the editor is in Write mode.");

                    var refreshedTargets = new[]
                    {
                        CreateSdoWriteTarget(
                            "Test target axis 1",
                            1,
                            0x2F00,
                            24),
                        CreateSdoWriteTarget(
                            "Test target axis 2",
                            2,
                            0x2F00,
                            24)
                    };
                    window.TextSdoSlaveReference.Text = "4";
                    window.TextSdoIndex.Text = "0x607A";
                    window.TextSdoSubIndex.Text = "1";
                    InvokePrivate(
                        window,
                        "RefreshSdoWriteTargetItems",
                        (object)refreshedTargets);
                    PumpDispatcherOnce();
                    AssertEx.True(
                        ReferenceEquals(
                            refreshedTargets[0],
                            window.ComboSdoWriteTarget.SelectedItem),
                        "Programmatic target refresh did not retain/select a visible target.");
                    AssertEx.Equal("4", window.TextSdoSlaveReference.Text);
                    AssertEx.Equal("0x607A", window.TextSdoIndex.Text);
                    AssertEx.Equal("1", window.TextSdoSubIndex.Text);
                    window.ComboSdoWriteTarget.SelectedItem =
                        refreshedTargets[1];
                    PumpDispatcherOnce();
                    AssertEx.Equal("2", window.TextSdoSlaveReference.Text);
                    AssertEx.Equal("0x2F00", window.TextSdoIndex.Text);
                    AssertEx.Equal("24", window.TextSdoSubIndex.Text);

                    Click(window.ButtonDiagnosticsCapabilities);
                    WaitUntil(
                        () => requestReached.IsSet,
                        "The ordinary diagnostics request did not reach the fake PLC.");

                    AssertWriteEditorEnabled(window);
                    AssertEx.False(window.ButtonDiagnosticsCapabilities.IsEnabled);
                    AssertEx.False(window.ButtonSubmitSdo.IsEnabled);
                    AssertEx.False(
                        window.ButtonRunD5SdoContentionQualification.IsEnabled,
                        "An ordinary in-flight RPC must serialize the contention runner start.");
                    AssertEx.False(
                        window.ButtonRunD5SdoTimeoutQualification.IsEnabled,
                        "An ordinary in-flight RPC must serialize the timeout runner start.");
                    AssertEx.False(
                        window.ButtonRunD5SdoQueuedCancelQualification.IsEnabled,
                        "An ordinary in-flight RPC must serialize the queued-cancel runner start.");
                    AssertEx.False(
                        window.ButtonRunD5SdoDisconnectRecoveryQualification.IsEnabled,
                        "An ordinary in-flight RPC must serialize the abrupt-disconnect runner start.");

                    window.TextSdoSlaveReference.Text = "4";
                    window.TextSdoIndex.Text = "0x607A";
                    window.TextSdoSubIndex.Text = "1";
                    window.TextSdoTimeoutCycles.Text = "2345";
                    window.TextSdoWriteData.Text = "0x1234";
                    window.ComboSdoValueType.SelectedItem =
                        LMCSignalValueType.UInt16;
                    window.ComboSdoDataLength.SelectedItem = (ushort)2;
                    PumpDispatcherOnce();

                    AssertEx.Equal("4", window.TextSdoSlaveReference.Text);
                    AssertEx.Equal("0x607A", window.TextSdoIndex.Text);
                    AssertEx.Equal("1", window.TextSdoSubIndex.Text);
                    AssertEx.Equal("2345", window.TextSdoTimeoutCycles.Text);
                    AssertEx.Equal("0x1234", window.TextSdoWriteData.Text);
                    AssertEx.Equal(
                        LMCSignalValueType.UInt16,
                        (LMCSignalValueType)window.ComboSdoValueType.SelectedItem);
                    AssertEx.Equal(
                        (ushort)2,
                        (ushort)window.ComboSdoDataLength.SelectedItem);

                    releaseResponse.Set();
                    WaitUntil(
                        () => window.ButtonDiagnosticsCapabilities.IsEnabled,
                        "The gated diagnostics operation did not complete.");
                    AssertEx.Equal("4", window.TextSdoSlaveReference.Text);
                    AssertEx.Equal("0x607A", window.TextSdoIndex.Text);
                    AssertEx.Equal("0x1234", window.TextSdoWriteData.Text);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                releaseResponse.Set();
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void
            WriteSameValueEmptyAllowlistForcedAttemptIsZeroWire()
        {
            var capabilities = LMCDiagnosticCapability.SDORead
                | LMCDiagnosticCapability.SDOWrite
                | LMCDiagnosticCapability.SDOReadGeneralInline
                | LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
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
                        "Same-value Write gate test did not complete connection/topology setup.");

                    AssertEx.Equal(
                        0,
                        window.ComboD5SdoWriteQualificationTarget.Items.Count);
                    AssertEx.False(
                        window.ButtonRunD5SdoWriteSameValueQualification
                            .IsEnabled,
                        "The same-value Write runner must remain disabled when the SDK allowlist is empty even if PLC bit 9 is advertised.");
                    AssertEx.Contains(
                        "no approved SDO Write target",
                        window.TextD5SdoWriteQualificationGateStatus.Text);
                    AssertEx.Contains(
                        "No same-value SDO Write qualification has run yet",
                        window.TextD5SdoWriteQualificationSummary.Text);

                    var requestCountBeforeForcedAttempt =
                        server.ReceivedRequests.Count;
                    InvokePrivate(
                        window,
                        "ButtonRunD5SdoWriteSameValueQualification_Click",
                        null,
                        new RoutedEventArgs());
                    PumpDispatcherOnce();

                    AssertEx.Equal(
                        requestCountBeforeForcedAttempt,
                        server.ReceivedRequests.Count,
                        "A forced same-value Write handler invocation with an empty SDK allowlist sent a PLC request.");
                    AssertEx.Contains(
                        "NOT STARTED",
                        window.TextD5SdoWriteQualificationSummary.Text);

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
            WriteSameValueTerminalEvidenceSurvivesUiRefresh()
        {
            var capabilities = LMCDiagnosticCapability.SDORead
                | LMCDiagnosticCapability.SDOWrite
                | LMCDiagnosticCapability.SDOReadGeneralInline
                | LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
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
                        "Same-value terminal-evidence smoke setup did not complete connection/topology loading.");

                    var target = CreateSdoWriteTarget(
                        "Test target axis 1",
                        1,
                        0x2F00,
                        24);
                    InvokePrivate(
                        window,
                        "RefreshSdoWriteTargetItems",
                        (object)new[] { target });
                    window.CheckConfirmD5SdoWriteUi24Unused.IsChecked = true;
                    window.CheckConfirmD5SdoWriteOriginalRecorded.IsChecked =
                        true;
                    window.CheckConfirmD5SdoWriteCaptureRunning.IsChecked =
                        true;
                    window.CheckConfirmD5SdoWriteSingleWriter.IsChecked = true;
                    PumpDispatcherOnce();
                    var requestCountBeforeMatrixRefresh =
                        server.ReceivedRequests.Count;
                    InvokePrivate(window, "UpdateUiState");
                    PumpDispatcherOnce();

                    AssertEx.False(
                        window.ButtonRunD5SdoWriteSameValueQualification
                            .IsEnabled,
                        "A GUI-only injected target bypassed the central empty SDK SDO Write policy.");
                    AssertEx.Contains(
                        "EVALUATION_WIRE=NONE",
                        window.TextD5SdoWriteQualificationGateStatus.Text);
                    AssertEx.Contains(
                        "NoApprovedTarget",
                        window.TextD5SdoWriteQualificationGateStatus.Text);
                    AssertEx.Contains(
                        "SDK POLICY    FAIL",
                        window.TextD5SdoWriteQualificationGateStatus.Text);
                    AssertEx.Contains(
                        "bit8/read=1 bit9/write=1 bit13/general=1",
                        window.TextD5SdoWriteQualificationGateStatus.Text);
                    AssertEx.Equal(
                        requestCountBeforeMatrixRefresh,
                        server.ReceivedRequests.Count,
                        "Refreshing the cached SDO Write readiness matrix sent an RPC request.");

                    const string passEvidence =
                        "PASS | Target=axis1 | Baseline=2A-00-00-00 | Tickets=1/2/3/4";
                    window.TextD5SdoWriteQualificationSummary.Text =
                        passEvidence;
                    InvokePrivate(
                        window,
                        "ResetD5SdoWriteSameValueOperatorConfirmations");
                    InvokePrivate(window, "UpdateUiState");
                    PumpDispatcherOnce();

                    AssertEx.Equal(
                        passEvidence,
                        window.TextD5SdoWriteQualificationSummary.Text,
                        "PASS evidence was overwritten by readiness refresh or checkbox reset.");
                    AssertEx.Contains(
                        "OVERALL       CLOSED",
                        window.TextD5SdoWriteQualificationGateStatus.Text);
                    AssertEx.Contains(
                        "CONFIRMATIONS FAIL | 0/4",
                        window.TextD5SdoWriteQualificationGateStatus.Text);
                    AssertEx.False(
                        window.TextD5SdoWriteQualificationSummary.Text
                            .IndexOf(
                                "CONFIRMATIONS FAIL",
                                StringComparison.Ordinal) >= 0,
                        "The last-attempt result was replaced with a preflight-only claim.");

                    window.CheckConfirmD5SdoWriteUi24Unused.IsChecked = true;
                    window.CheckConfirmD5SdoWriteOriginalRecorded.IsChecked =
                        true;
                    window.CheckConfirmD5SdoWriteCaptureRunning.IsChecked =
                        true;
                    window.CheckConfirmD5SdoWriteSingleWriter.IsChecked = true;
                    const string recoveryEvidence =
                        "RECOVERY REQUIRED | Stage=Readback | WriteAttempted=True | JournalActive=True";
                    window.TextD5SdoWriteQualificationSummary.Text =
                        recoveryEvidence;
                    InvokePrivate(
                        window,
                        "ResetD5SdoWriteSameValueOperatorConfirmations");
                    InvokePrivate(window, "UpdateUiState");
                    PumpDispatcherOnce();

                    AssertEx.Equal(
                        recoveryEvidence,
                        window.TextD5SdoWriteQualificationSummary.Text,
                        "RECOVERY REQUIRED evidence was overwritten by readiness refresh or checkbox reset.");
                    AssertEx.Contains(
                        "OVERALL       CLOSED",
                        window.TextD5SdoWriteQualificationGateStatus.Text);

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
            PendingReadbackPreservesDraftAndExplicitLoadRestoresExactRequest()
        {
            var capabilities = LMCDiagnosticCapability.SDORead
                | LMCDiagnosticCapability.SDOWrite
                | LMCDiagnosticCapability.SDOReadGeneralInline
                | LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
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
                        "Pending SDO readback smoke setup did not complete connection/topology loading.");

                    window.ComboSdoOperation.SelectedIndex = 1;
                    window.TextSdoSlaveReference.Text = "4";
                    window.TextSdoIndex.Text = "0x607A";
                    window.TextSdoSubIndex.Text = "1";
                    window.ComboSdoValueType.SelectedItem =
                        LMCSignalValueType.UInt16;
                    window.ComboSdoDataLength.SelectedItem = (ushort)2;
                    window.TextSdoTimeoutCycles.Text = "2345";
                    window.TextSdoWriteData.Text = "0x1234";
                    PumpDispatcherOnce();

                    var currentConnection =
                        (LMCConnection)GetPrivateField(
                            window,
                            "connection");
                    var stalePendingReadback =
                        CreatePendingSdoWriteReadback(window);
                    SetProperty(
                        stalePendingReadback,
                        "ConnectionSessionGeneration",
                        0L);
                    SetPrivateField(
                        window,
                        "d5SdoPendingWriteReadback",
                        stalePendingReadback);
                    InvokePrivate(window, "UpdateUiState");

                    AssertEx.False(
                        window.ButtonLoadRequiredSdoReadback.IsEnabled,
                        "A stale-session readback must not enable the editor load action.");
                    InvokePrivate(
                        window,
                        "ButtonLoadRequiredSdoReadback_Click",
                        null,
                        new RoutedEventArgs());
                    PumpDispatcherOnce();
                    AssertEx.Equal(
                        1,
                        window.ComboSdoOperation.SelectedIndex,
                        "A forced stale-session load changed the editor mode.");
                    AssertEx.Equal("4", window.TextSdoSlaveReference.Text);
                    AssertEx.Equal("0x607A", window.TextSdoIndex.Text);
                    AssertEx.Equal("1", window.TextSdoSubIndex.Text);
                    AssertEx.Equal("2345", window.TextSdoTimeoutCycles.Text);
                    AssertEx.Equal("0x1234", window.TextSdoWriteData.Text);

                    var pendingReadback =
                        CreatePendingSdoWriteReadback(window);
                    SetPrivateField(
                        window,
                        "d5SdoPendingWriteReadback",
                        pendingReadback);
                    InvokePrivate(window, "UpdateUiState");

                    AssertWriteEditorEnabled(window);
                    AssertEx.False(
                        window.ComboSdoWriteTarget.IsEnabled,
                        "The empty SDK write allowlist must remain fail-closed while readback is pending.");
                    AssertEx.True(
                        window.ButtonLoadRequiredSdoReadback.IsEnabled,
                        "Pending exact readback must expose the local restore action.");
                    AssertEx.False(
                        window.ButtonSubmitSdo.IsEnabled,
                        "A prepared Write draft must not be submittable while exact readback is pending.");
                    AssertEx.False(
                        window.ButtonReadSdoInline.IsEnabled,
                        "Inline SDO Read must not bypass the pending exact Write readback workflow.");
                    AssertEx.Equal("4", window.TextSdoSlaveReference.Text);
                    AssertEx.Equal("0x607A", window.TextSdoIndex.Text);
                    AssertEx.Equal("1", window.TextSdoSubIndex.Text);
                    AssertEx.Equal(
                        LMCSignalValueType.UInt16,
                        (LMCSignalValueType)window.ComboSdoValueType.SelectedItem);
                    AssertEx.Equal(
                        (ushort)2,
                        (ushort)window.ComboSdoDataLength.SelectedItem);
                    AssertEx.Equal("2345", window.TextSdoTimeoutCycles.Text);
                    AssertEx.Equal("0x1234", window.TextSdoWriteData.Text);

                    Click(window.ButtonLoadRequiredSdoReadback);
                    PumpDispatcherOnce();

                    AssertEx.Equal(
                        0,
                        window.ComboSdoOperation.SelectedIndex,
                        "Loading the required request must select SDO Read.");
                    AssertEx.True(window.ComboSdoOperation.IsEnabled);
                    AssertEx.True(window.TextSdoSlaveReference.IsEnabled);
                    AssertEx.True(window.TextSdoIndex.IsEnabled);
                    AssertEx.True(window.TextSdoSubIndex.IsEnabled);
                    AssertEx.True(window.ComboSdoValueType.IsEnabled);
                    AssertEx.True(window.ComboSdoDataLength.IsEnabled);
                    AssertEx.True(window.TextSdoTimeoutCycles.IsEnabled);
                    AssertEx.False(window.TextSdoWriteData.IsEnabled);
                    AssertEx.Equal("2", window.TextSdoSlaveReference.Text);
                    AssertEx.Equal("0x2F00", window.TextSdoIndex.Text);
                    AssertEx.Equal("24", window.TextSdoSubIndex.Text);
                    AssertEx.Equal(
                        LMCSignalValueType.Int32,
                        (LMCSignalValueType)window.ComboSdoValueType.SelectedItem);
                    AssertEx.Equal(
                        (ushort)4,
                        (ushort)window.ComboSdoDataLength.SelectedItem);
                    AssertEx.Equal("1000", window.TextSdoTimeoutCycles.Text);
                    AssertEx.True(
                        window.ButtonSubmitSdo.IsEnabled,
                        "The restored exact Read request must be eligible for submission in the owner session.");
                    AssertEx.False(
                        window.ButtonReadSdoInline.IsEnabled,
                        "The exact Write readback must remain on the existing Submit/Refresh workflow after loading its request.");

                    var writeRequest = LMCSdoRequest.CreateWrite(
                        2,
                        0x2F00,
                        24,
                        LMCSignalValueType.Int32,
                        new byte[] { 0x2A, 0, 0, 0 },
                        1000);
                    InvokePrivate(
                        window,
                        "ArmSdoWriteMutationJournal",
                        writeRequest,
                        currentConnection,
                        DiagnosticsBootId,
                        DiagnosticMapRevision);

                    var readRequest = pendingReadback.CreateReadRequest();
                    const uint readTicketId = 7801;
                    const uint readSubmitCycle = 901;
                    var readTicket = (LMCOperationTicket)Activator.CreateInstance(
                        typeof(LMCOperationTicket),
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        null,
                        new object[]
                        {
                            readTicketId,
                            LMCOperationKind.SDORead,
                            readSubmitCycle,
                            DiagnosticsBootId,
                            DiagnosticMapRevision,
                            GetConnectionSessionGeneration(
                                currentConnection),
                            currentConnection.Diagnostics,
                            true,
                            (ushort)4,
                            LMCSignalValueType.Int32,
                            false,
                            (ushort)0,
                            readRequest
                        },
                        CultureInfo.InvariantCulture);
                    var readStatus = (LMCOperationStatus)Activator.CreateInstance(
                        typeof(LMCOperationStatus),
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        null,
                        new object[]
                        {
                            null,
                            readTicketId,
                            LMCOperationKind.SDORead,
                            LMCOperationState.Completed,
                            readSubmitCycle,
                            readSubmitCycle + 1,
                            LMCOperationOutcome.Success,
                            (short)0,
                            0u,
                            (ushort)4,
                            LMCSignalValueType.Int32,
                            new byte[] { 0x2A, 0, 0, 0 },
                            DiagnosticsBootId
                        },
                        CultureInfo.InvariantCulture);
                    readStatus = BindOperationStatus(
                        readStatus,
                        currentConnection);
                    SetPrivateField(
                        window,
                        "d5SdoQualificationActiveTicket",
                        readTicket);
                    SetPrivateField(
                        window,
                        "d5SdoQualificationActiveRequest",
                        readRequest);
                    var handledVerifiedReadback = (bool)InvokePrivate(
                        window,
                        "HandleD5SdoWriteReadbackTerminal",
                        readTicket,
                        readStatus,
                        "wpf-smoke",
                        currentConnection,
                        GetPrivateField(
                            window,
                            "diagnosticCapabilities"));
                    PumpDispatcherOnce();

                    AssertEx.True(
                        handledVerifiedReadback,
                        "The actual terminal handler did not consume the exact VERIFIED readback.");
                    AssertEx.True(
                        GetPrivateField(
                            window,
                            "d5SdoPendingWriteReadback") == null,
                        "VERIFIED did not clear the pending readback context.");
                    AssertEx.Equal(
                        1,
                        window.ComboSdoOperation.SelectedIndex,
                        "Draft restore did not return the editor to SDO Write mode.");
                    AssertEx.Equal("4", window.TextSdoSlaveReference.Text);
                    AssertEx.Equal("0x607A", window.TextSdoIndex.Text);
                    AssertEx.Equal("1", window.TextSdoSubIndex.Text);
                    AssertEx.Equal(
                        LMCSignalValueType.UInt16,
                        (LMCSignalValueType)window.ComboSdoValueType.SelectedItem);
                    AssertEx.Equal(
                        (ushort)2,
                        (ushort)window.ComboSdoDataLength.SelectedItem);
                    AssertEx.Equal("2345", window.TextSdoTimeoutCycles.Text);
                    AssertEx.Equal("0x1234", window.TextSdoWriteData.Text);
                    AssertEx.Contains(
                        "restored the same-session editor draft",
                        window.TextExecutionLog.Text);

                    SetPrivateField(
                        window,
                        "d5SdoPendingWriteReadback",
                        pendingReadback);
                    InvokePrivate(window, "UpdateUiState");
                    Click(window.ButtonLoadRequiredSdoReadback);
                    PumpDispatcherOnce();
                    AssertEx.Equal("0x2F00", window.TextSdoIndex.Text);

                    var requestCountBeforeInvalidSubmit =
                        server.ReceivedRequests.Count;
                    window.TextSdoIndex.Text = "0x2F01";
                    var postLoadOperatorEditWasPreserved =
                        !(bool)InvokePrivate(
                            window,
                            "TryRestoreSdoEditorDraftAfterVerifiedReadback",
                            pendingReadback,
                            currentConnection);
                    AssertEx.True(
                        postLoadOperatorEditWasPreserved,
                        "A post-load operator edit was overwritten by the older captured draft.");
                    AssertEx.Equal(
                        "0x2F01",
                        window.TextSdoIndex.Text,
                        "The post-load operator edit was not preserved.");
                    Click(window.ButtonSubmitSdo);
                    PumpDispatcherOnce();

                    AssertEx.Equal(
                        requestCountBeforeInvalidSubmit,
                        server.ReceivedRequests.Count,
                        "An edited non-exact pending readback must fail before wire submission.");
                    AssertEx.Contains(
                        "pending SDO Write interlock accepts only the exact same",
                        window.TextDiagnosticOperationSummary.Text);
                    AssertEx.True(
                        ReferenceEquals(
                            pendingReadback,
                            GetPrivateField(
                                window,
                                "d5SdoPendingWriteReadback")),
                        "A rejected non-exact Read must preserve the pending verification context.");

                    SetPrivateField(
                        window,
                        "d5SdoPendingWriteReadback",
                        null);
                    InvokePrivate(window, "UpdateUiState");
                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                if (window != null)
                {
                    try
                    {
                        SetPrivateField(
                            window,
                            "d5SdoPendingWriteReadback",
                            null);
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
            RecoveredTypedWriteEmptyAllowlistForcedAttemptIsZeroWire()
        {
            var capabilities = LMCDiagnosticCapability.SDORead
                | LMCDiagnosticCapability.SDOReadGeneralInline
                | LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            var identity = Guid.NewGuid();
            var createdUtc = DateTime.UtcNow;
            using (var journal =
                DiagnosticsMutationJournal.Open(journalDirectory))
            {
                journal.Arm(
                    DiagnosticsMutationKind.SdoWrite,
                    identity,
                    createdUtc,
                    0x10203040u,
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
                journal.Transition(
                    identity,
                    DiagnosticsMutationState.AcceptedPendingTerminal,
                    createdUtc.AddMilliseconds(1),
                    77);
                journal.Transition(
                    identity,
                    DiagnosticsMutationState
                        .TerminalSuccessPendingReadback,
                    createdUtc.AddMilliseconds(2),
                    77);
            }

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
                        "Recovered typed SDO test did not complete connection/topology setup.");

                    AssertEx.Equal(
                        "Acknowledge Recovered Mutation",
                        window.ButtonAcknowledgePersistedMutation.Content
                            as string,
                        "An empty SDK allowlist must not expose protocol recovery.");
                    AssertEx.True(
                        window.CheckPersistedMutationPhysicallyVerified
                            .IsEnabled);
                    AssertEx.False(
                        window.ButtonAcknowledgePersistedMutation.IsEnabled);
                    AssertEx.Equal(0, window.ComboSdoWriteTarget.Items.Count);

                    var recoveryMethod = typeof(MainWindow).GetMethod(
                        "RecoverPersistedSdoWriteByExactReadbackAsync",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    AssertEx.NotNull(recoveryMethod);
                    var forcedTask = (Task)recoveryMethod.Invoke(
                        window,
                        new object[] { true });
                    var rejection = AssertEx.Throws<InvalidOperationException>(
                        () => forcedTask.GetAwaiter().GetResult());
                    AssertEx.Contains(
                        "no longer eligible",
                        rejection.Message);

                    AssertRequestCommandSequence(
                        server.ReceivedRequests,
                        0x8080,
                        0x405C,
                        0x7E00,
                        0x7E00,
                        0x7E11,
                        0x7E12,
                        0x7E12,
                        0x7E12,
                        0x7E12,
                        0x7E12,
                        0x7E12,
                        0x7E12);

                    InvokePrivate(
                        window,
                        "ResolveDiagnosticsMutationJournal",
                        DiagnosticsMutationKind.SdoWrite);
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

        private static void DoubleContractAdvertisedRemainsDormantAndZeroWire()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology
                | LMCDiagnosticCapability.SignalCatalog
                | LMCDiagnosticCapability.RecorderSingleBank
                | LMCDiagnosticCapability.RecorderTrigger
                | LMCDiagnosticCapability.RecorderDoubleBank;
            var steps = CreateConnectAndTopologySteps(capabilities, 2);
            steps.Add(CapabilitiesStep(11, capabilities, 2));
            steps.Add(new FakeRpcStep(
                0x7E01,
                TestFrame.Response(
                    0,
                    CatalogInfoPayload(12, RecorderCatalogEntryCount))));
            steps.Add(new FakeRpcStep(
                0x7E02,
                TestFrame.Response(
                    0,
                    CatalogChunkPayload(
                        13,
                        RecorderCatalogEntryCount))));
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
                                == TopologyNodeCount
                            && window.TextQualificationRecorderCapability.Text
                                .IndexOf(
                                    "Double=advertised",
                                    StringComparison.Ordinal) >= 0,
                        "Advertised Double-bank capability did not reach the dormant UI state.");

                    WaitUntil(
                        () => window.ButtonLoadSignalCatalog.IsEnabled,
                        "Load PI Catalog did not become enabled after the automatic topology load.");
                    Click(window.ButtonLoadSignalCatalog);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Load Signal Catalog completed",
                                StringComparison.Ordinal)
                            || string.Equals(
                                window.TextOperationState.Text,
                                "Load Signal Catalog failed",
                                StringComparison.Ordinal),
                        "Load PI Catalog did not reach a terminal UI state.");
                    AssertEx.Equal(
                        "Load Signal Catalog completed",
                        window.TextOperationState.Text,
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "DoubleContractReady=True",
                        window.TextQualificationRecorderCapability.Text);
                    AssertEx.Contains(
                        "Double recovery journal ready.",
                        window.TextRecorderDoubleRecoveryStatus.Text);

                    AssertEx.False(
                        window.ButtonRunRecorderDoubleQualification.IsEnabled,
                        "Double-bank live execution must stay dormant until external-session-loss recovery exists.");
                    AssertEx.False(
                        window.ButtonRecoverRecorderDoubleJournal.IsEnabled,
                        "Double-bank recovery must stay dormant until its proof gate opens and an exact durable record exists.");
                    AssertEx.False(
                        window.ButtonReleaseRecorderDoubleRetained.IsEnabled,
                        "Same-session Double cleanup must stay dormant without exact retained handles.");
                    AssertEx.False(
                        window.CheckConfirmRecorderDoubleRelease.IsEnabled,
                        "Double Release confirmation must stay unavailable without an active durable record.");
                    AssertEx.Contains(
                        "Buffers=2",
                        window.TextQualificationRecorderCapability.Text);
                    AssertEx.Contains(
                        "DoubleManualGate=CLOSED_MANUAL_PROOF",
                        window.TextQualificationRecorderCapability.Text);
                    AssertEx.Contains(
                        "DoubleQualificationGate=CLOSED_RUNNER_PROOF",
                        window.TextQualificationRecorderCapability.Text);
                    AssertEx.Contains(
                        "DoubleReconnectGate=CLOSED_RECOVERY_PROOF",
                        window.TextQualificationRecorderCapability.Text);
                    AssertEx.Contains(
                        "QualificationExecution proof gate is CLOSED",
                        Convert.ToString(
                            window.ButtonRunRecorderDoubleQualification.ToolTip,
                            CultureInfo.InvariantCulture));
                    AssertEx.False(
                        HasRecorderBufferMode(
                            window,
                            LMCRecorderBufferMode.Double),
                        "The manual Recorder UI must not expose Double mode while live recovery is blocked.");
                    AssertEx.True(
                        HasRecorderBufferMode(
                            window,
                            LMCRecorderBufferMode.Single),
                        "The Single mode must remain available while all Double proof gates are closed.");
                    AssertEx.True(
                        HasRecorderBufferMode(
                            window,
                            LMCRecorderBufferMode.Ring),
                        "The Ring mode must remain available while all Double proof gates are closed.");
                    AssertEx.False(
                        window.ButtonAdoptRecorder.IsEnabled,
                        "Manual Recorder adoption must stay disabled when the advertised capability makes the target mode ambiguous before wire.");

                    Click(window.ButtonRunRecorderDoubleQualification);
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "RecorderDoubleBank failed",
                            StringComparison.Ordinal),
                        "The Double qualification handler did not reject the closed proof gates.");
                    AssertEx.Contains(
                        "QualificationExecution proof gate is CLOSED",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "ReconnectRecovery proof gate is CLOSED",
                        window.TextExecutionLog.Text);

                    Click(window.ButtonRecoverRecorderDoubleJournal);
                    AssertEx.Equal(
                        "RecorderDoubleRecovery failed",
                        window.TextOperationState.Text);
                    AssertEx.Contains(
                        "ReconnectRecovery proof gate is CLOSED",
                        window.TextExecutionLog.Text);

                    window.TextRecorderAdoptBootId.Text = "0x10203040";
                    window.TextRecorderAdoptRecordId.Text = "1";
                    window.TextRecorderAdoptBufferId.Text = "1";
                    Click(window.ButtonAdoptRecorder);
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Adopt Recorder failed",
                            StringComparison.Ordinal),
                        "The manual Adopt handler did not reject the mode-ambiguous Double-capable target.");
                    AssertEx.Contains(
                        "ReconnectRecovery proof gate is CLOSED",
                        window.TextExecutionLog.Text);

                    window.ComboRecorderBufferMode.ItemsSource = new[]
                    {
                        LMCRecorderBufferMode.Single,
                        LMCRecorderBufferMode.Double
                    };
                    window.ComboRecorderBufferMode.SelectedItem =
                        LMCRecorderBufferMode.Double;
                    WaitUntil(
                        () => !window.ButtonConfigureRecorder.IsEnabled,
                        "An injected Double selection did not disable manual Configure.");
                    Click(window.ButtonConfigureRecorder);
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Configure Recorder failed",
                            StringComparison.Ordinal),
                        "The manual Configure handler did not reject an injected Double selection.");
                    AssertEx.Contains(
                        "ManualActions proof gate is CLOSED",
                        window.TextExecutionLog.Text);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                    AssertNoRecorderRequests(server.ReceivedRequests);
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void AssertWriteEditorEnabled(MainWindow window)
        {
            AssertEx.True(window.ComboSdoOperation.IsEnabled);
            AssertEx.True(window.TextSdoSlaveReference.IsEnabled);
            AssertEx.True(window.TextSdoIndex.IsEnabled);
            AssertEx.True(window.TextSdoSubIndex.IsEnabled);
            AssertEx.True(window.ComboSdoValueType.IsEnabled);
            AssertEx.True(window.ComboSdoDataLength.IsEnabled);
            AssertEx.True(window.TextSdoTimeoutCycles.IsEnabled);
            AssertEx.True(window.TextSdoWriteData.IsEnabled);
        }

        private static LMCSdoWriteVerificationContext
            CreatePendingSdoWriteReadback(MainWindow window)
        {
            var currentConnection =
                (LMCConnection)GetPrivateField(window, "connection");
            var sessionGeneration = GetConnectionSessionGeneration(
                currentConnection);
            var writeRequest = LMCSdoRequest.CreateWrite(
                2,
                0x2F00,
                24,
                LMCSignalValueType.Int32,
                new byte[] { 0x2A, 0, 0, 0 },
                1000);
            var writeTicket = (LMCOperationTicket)Activator.CreateInstance(
                typeof(LMCOperationTicket),
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new object[]
                {
                    77u,
                    LMCOperationKind.SDOWrite,
                    123u,
                    0x10203040u,
                    DiagnosticMapRevision,
                    sessionGeneration,
                    currentConnection.Diagnostics,
                    false,
                    (ushort)0,
                    LMCSignalValueType.Invalid,
                    false,
                    (ushort)0,
                    writeRequest
                },
                CultureInfo.InvariantCulture);

            return (LMCSdoWriteVerificationContext)Activator.CreateInstance(
                typeof(LMCSdoWriteVerificationContext),
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new object[]
                {
                    currentConnection.Diagnostics,
                    writeRequest,
                    writeTicket,
                    0L
                },
                CultureInfo.InvariantCulture);
        }

        private static long GetConnectionSessionGeneration(
            LMCConnection currentConnection)
        {
            AssertEx.NotNull(currentConnection);
            var sessionGenerationProperty = typeof(LMCConnection).GetProperty(
                "SessionGeneration",
                BindingFlags.Instance | BindingFlags.NonPublic);
            AssertEx.NotNull(sessionGenerationProperty);
            return (long)sessionGenerationProperty.GetValue(
                currentConnection,
                null);
        }

        private static LMCOperationStatus BindOperationStatus(
            LMCOperationStatus status,
            LMCConnection currentConnection)
        {
            AssertEx.NotNull(status);
            AssertEx.NotNull(currentConnection);
            var bindMethod = typeof(LMCOperationStatus).GetMethod(
                "BindProvenance",
                BindingFlags.Instance | BindingFlags.NonPublic);
            AssertEx.NotNull(bindMethod);
            return (LMCOperationStatus)bindMethod.Invoke(
                status,
                new object[]
                {
                    currentConnection.Diagnostics,
                    GetConnectionSessionGeneration(currentConnection)
                });
        }

        private static object InvokePrivate(
            MainWindow window,
            string methodName,
            params object[] arguments)
        {
            var method = typeof(MainWindow).GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            AssertEx.NotNull(method);
            return method.Invoke(window, arguments);
        }

        private static object GetPrivateField(
            MainWindow window,
            string fieldName)
        {
            var field = typeof(MainWindow).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            AssertEx.NotNull(field);
            return field.GetValue(window);
        }

        private static TopologyIoLiveEvidenceSnapshot
            CaptureTopologyIoLiveEvidence(MainWindow window)
        {
            return ((TopologyIoLiveEvidenceJournal)GetPrivateField(
                    window,
                    "topologyIoLiveEvidenceJournal"))
                .CaptureSnapshot();
        }

        private static void SetPrivateField(
            object target,
            string fieldName,
            object value)
        {
            AssertEx.NotNull(target);
            var field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            AssertEx.NotNull(field);
            field.SetValue(target, value);
        }

        private static void SetProperty(
            object target,
            string propertyName,
            object value)
        {
            AssertEx.NotNull(target);
            var property = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance
                    | BindingFlags.Public
                    | BindingFlags.NonPublic);
            AssertEx.NotNull(property);
            var setter = property.GetSetMethod(true);
            AssertEx.NotNull(setter);
            setter.Invoke(target, new[] { value });
        }

        private static void InvokeTopologyIoMonitorTick(MainWindow window)
        {
            InvokePrivate(
                window,
                "TopologyIoLiveMonitorTimer_Tick",
                null,
                EventArgs.Empty);
        }

        private static MainWindow CreateWindow(
            string journalDirectory,
            int rpcPort)
        {
            return CreateWindow(
                journalDirectory,
                Path.Combine(
                    journalDirectory,
                    "RecorderDoubleRecovery"),
                rpcPort);
        }

        private static MainWindow CreateWindow(
            string journalDirectory,
            string recorderDoubleJournalDirectory,
            int rpcPort)
        {
            var window = new MainWindow(
                journalDirectory,
                recorderDoubleJournalDirectory)
            {
                ShowActivated = false,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000
            };
            window.TextRemoteIp.Text = "127.0.0.1";
            window.TextRemotePort.Text = rpcPort.ToString(
                CultureInfo.InvariantCulture);
            window.TextLocalIp.Text = "127.0.0.1";
            window.TextCallbackPort.Text = "0";
            window.Show();
            WaitUntil(() => window.IsLoaded, "The WPF window did not load.");
            return window;
        }

        private static void CloseConnectedWindow(MainWindow window)
        {
            WaitUntil(
                () => window.ButtonCloseConnection.IsEnabled,
                "The WPF connection did not become closable.");
            Click(window.ButtonCloseConnection);
            WaitUntil(
                () => string.Equals(
                    window.TextConnectionState.Text,
                    "Disconnected",
                    StringComparison.Ordinal),
                "The WPF connection did not close.");
            window.Close();
            WaitUntil(() => !window.IsLoaded, "The WPF window did not close.");
        }

        private static void CloseWindowBestEffort(MainWindow window)
        {
            if (window == null || !window.IsLoaded)
            {
                return;
            }

            try
            {
                WaitUntil(
                    () => window.ButtonCloseConnection.IsEnabled
                        || string.Equals(
                            window.TextConnectionState.Text,
                            "Disconnected",
                            StringComparison.Ordinal),
                    "The WPF cleanup connection did not settle.",
                    2000);
                if (window.ButtonCloseConnection.IsEnabled)
                {
                    Click(window.ButtonCloseConnection);
                    WaitUntil(
                        () => string.Equals(
                            window.TextConnectionState.Text,
                            "Disconnected",
                            StringComparison.Ordinal),
                        "The WPF cleanup connection did not close.",
                        3000);
                }

                window.Close();
                WaitUntil(
                    () => !window.IsLoaded,
                    "The WPF cleanup window did not close.",
                    2000);
            }
            catch
            {
            }

            if (window.IsLoaded)
            {
                ForceCloseMotionRecoveryWindow(window);
            }
        }

        private static int CountCrevisRows(MainWindow window)
        {
            var count = 0;
            foreach (var item in window.GridEtherCATTopology.Items)
            {
                var nameProperty = item.GetType().GetProperty(
                    "Name",
                    BindingFlags.Instance | BindingFlags.Public);
                AssertEx.NotNull(nameProperty);
                var name = nameProperty.GetValue(item, null) as string;
                if (name != null
                    && name.StartsWith("GL_9086_1", StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }

        private static object FindTopologyRow(
            MainWindow window,
            string expectedName)
        {
            foreach (var item in window.GridEtherCATTopology.Items)
            {
                if (string.Equals(
                        GetRowString(item, "Name"),
                        expectedName,
                        StringComparison.Ordinal))
                {
                    return item;
                }
            }

            throw new InvalidOperationException(
                "Topology row was not rendered: " + expectedName + ".");
        }

        private static string GetRowString(object row, string propertyName)
        {
            AssertEx.NotNull(row);
            var property = row.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public);
            AssertEx.NotNull(property);
            return Convert.ToString(
                property.GetValue(row, null),
                CultureInfo.InvariantCulture);
        }

        private static bool HasRecorderBufferMode(
            MainWindow window,
            LMCRecorderBufferMode expected)
        {
            foreach (var item in window.ComboRecorderBufferMode.Items)
            {
                if (item is LMCRecorderBufferMode
                    && (LMCRecorderBufferMode)item == expected)
                {
                    return true;
                }
            }

            return false;
        }

        private static void Click(Button button)
        {
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, button));
        }

        private static void WaitUntil(
            Func<bool> condition,
            string message,
            int timeoutMilliseconds = WaitTimeoutMilliseconds)
        {
            var stopwatch = Stopwatch.StartNew();
            while (!condition())
            {
                if (stopwatch.ElapsedMilliseconds >= timeoutMilliseconds)
                {
                    throw new TimeoutException(message);
                }

                PumpDispatcherOnce();
            }
        }

        private static void PumpDispatcherOnce()
        {
            var frame = new DispatcherFrame();
            var timer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(5),
                DispatcherPriority.Background,
                (sender, args) =>
                {
                    ((DispatcherTimer)sender).Stop();
                    frame.Continue = false;
                },
                Dispatcher.CurrentDispatcher);
            timer.Start();
            Dispatcher.PushFrame(frame);
        }

        private static List<FakeRpcStep> CreateConnectAndTopologySteps(
            LMCDiagnosticCapability capabilities,
            ushort recorderBufferCount = 0)
        {
            var canonical = CreateTopologyCanonicalBytes();
            var steps = new List<FakeRpcStep>
            {
                InitStep(),
                CallbackStep(),
                CapabilitiesStep(1, capabilities, recorderBufferCount),
                CapabilitiesStep(2, capabilities, recorderBufferCount),
                new FakeRpcStep(
                    0x7E11,
                    TestFrame.Response(0, TopologyInfoPayload(3)))
            };

            for (ushort startIndex = 0;
                startIndex < TopologyNodeCount;
                startIndex++)
            {
                steps.Add(new FakeRpcStep(
                    0x7E12,
                    TestFrame.Response(
                        0,
                        TopologyChunkPayload(
                            checked((uint)(4 + startIndex)),
                            startIndex,
                            canonical))));
            }

            return steps;
        }

        private static List<FakeRpcStep>
            CreateD5DisconnectTwoSessionSteps(
                LMCDiagnosticCapability capabilities,
                byte[] recoveryTopologyCanonical,
                uint recoveryTopologyRevision,
                uint baselineTicketId,
                uint oldProbeTicketId,
                uint firstRecoveryTicketId,
                uint secondRecoveryTicketId)
        {
            var stableValue = new byte[] { 8 };
            var steps = CreateConnectAndTopologySteps(capabilities);

            steps.Add(D5AxisLookupStep(1));
            steps.Add(D5AxisInfoStep(1));
            for (var sample = 0; sample < 3; sample++)
            {
                steps.Add(D5SafeAxisStatusStep());
                steps.Add(D5StableAxisPositionStep(123456));
            }

            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(CapabilitiesStep(13, capabilities));
            steps.Add(D5SdoSubmitStep(
                14,
                baselineTicketId,
                1001,
                1,
                0x6061,
                LMCSignalValueType.Int8,
                1,
                1000));
            steps.Add(D5SdoOperationStatusStep(
                15,
                baselineTicketId,
                1001,
                1010,
                LMCOperationState.Completed,
                LMCOperationOutcome.Success,
                LMCSignalValueType.Int8,
                stableValue));
            steps.Add(CapabilitiesStep(16, capabilities));
            steps.Add(CapabilitiesStep(17, capabilities));
            steps.Add(D5SdoSubmitStep(
                18,
                oldProbeTicketId,
                1002,
                1,
                0xFFFF,
                LMCSignalValueType.UInt32,
                4,
                1000));
            steps.Add(D5SdoOperationStatusStep(
                19,
                oldProbeTicketId,
                1002,
                0,
                LMCOperationState.Running,
                LMCOperationOutcome.NoneOrPending,
                LMCSignalValueType.Invalid,
                new byte[0]));
            steps.Add(new FakeRpcStep(0, null)
            {
                RequireClientDisconnectBeforeRequest = true,
                ContinueWithNextClientAfterDisconnect = true
            });

            steps.Add(InitStep());
            steps.Add(CallbackStep());
            steps.Add(CapabilitiesStep(1, capabilities));
            steps.Add(CapabilitiesStep(2, capabilities));
            steps.Add(CapabilitiesStep(3, capabilities));
            steps.Add(D5SdoSubmitStep(
                4,
                firstRecoveryTicketId,
                2001,
                1,
                0x6061,
                LMCSignalValueType.Int8,
                1,
                1000));
            steps.Add(D5SdoOperationStatusStep(
                5,
                firstRecoveryTicketId,
                2001,
                2010,
                LMCOperationState.Completed,
                LMCOperationOutcome.Success,
                LMCSignalValueType.Int8,
                stableValue));
            steps.Add(CapabilitiesStep(6, capabilities));
            steps.Add(D5SdoSubmitStep(
                7,
                secondRecoveryTicketId,
                2002,
                1,
                0x6061,
                LMCSignalValueType.Int8,
                1,
                1000));
            steps.Add(D5SdoOperationStatusStep(
                8,
                secondRecoveryTicketId,
                2002,
                2011,
                LMCOperationState.Completed,
                LMCOperationOutcome.Success,
                LMCSignalValueType.Int8,
                stableValue));
            steps.Add(CapabilitiesStep(9, capabilities));
            steps.Add(CapabilitiesStep(10, capabilities));
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(new FakeRpcStep(
                0x7E11,
                TestFrame.Response(
                    0,
                    TopologyInfoPayload(
                        12,
                        recoveryTopologyRevision))));
            for (ushort startIndex = 0;
                startIndex < TopologyNodeCount;
                startIndex++)
            {
                steps.Add(new FakeRpcStep(
                    0x7E12,
                    TestFrame.Response(
                        0,
                        TopologyChunkPayload(
                            checked((uint)(13 + startIndex)),
                            startIndex,
                            recoveryTopologyCanonical,
                            recoveryTopologyRevision))));
            }

            steps.Add(CloseStep());
            return steps;
        }

        private static FakeRpcStep D5AxisLookupStep(
            ushort axisReference)
        {
            var requestPayload = new byte[80];
            var axisName = "_LMCAxis"
                + axisReference.ToString(CultureInfo.InvariantCulture);
            var axisNameBytes = Encoding.ASCII.GetBytes(axisName);
            Buffer.BlockCopy(
                axisNameBytes,
                0,
                requestPayload,
                0,
                axisNameBytes.Length);

            var payload = new byte[6];
            TestFrame.WriteUInt16(payload, 4, axisReference);
            return new FakeRpcStep(
                0x103C,
                TestFrame.Response(0, payload))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    TestFrame.Request(0x103C, 0, requestPayload),
                    request)
            };
        }

        private static FakeRpcStep D5AxisInfoStep(
            ushort axisReference)
        {
            var requestPayload = new byte[12];
            TestFrame.WriteInt32(requestPayload, 0, 5);
            TestFrame.WriteInt32(requestPayload, 8, 1);

            var payload = new byte[8];
            TestFrame.WriteUInt32(payload, 0, axisReference);
            return new FakeRpcStep(
                0x202B,
                TestFrame.Response(0, payload))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    TestFrame.Request(
                        0x202B,
                        axisReference,
                        requestPayload),
                    request)
            };
        }

        private static FakeRpcStep AdminCapabilitiesStep(uint requestId)
        {
            var payload = CommonPayload(40, requestId);
            TestFrame.WriteUInt32(
                payload,
                16,
                (uint)(LMCAdminFeature.AxisParameterRead
                    | LMCAdminFeature.GroupParameterRead));
            TestFrame.WriteUInt32(payload, 20, 0x0000003Fu);
            TestFrame.WriteUInt32(
                payload,
                24,
                (uint)LMCGroupParameterSelection.All);
            TestFrame.WriteUInt16(payload, 28, 4);
            TestFrame.WriteUInt16(payload, 30, 1);
            TestFrame.WriteUInt16(payload, 32, 0x0100);
            TestFrame.WriteUInt16(payload, 34, 3);
            TestFrame.WriteUInt16(payload, 36, 1);

            return new FakeRpcStep(
                0x7D00,
                TestFrame.Response(0, payload))
            {
                InspectRequest = request =>
                {
                    AssertEx.Equal(
                        requestId,
                        TestFrame.ReadUInt32(request, 12));
                }
            };
        }

        private static FakeRpcStep AdminAxisParameterStep(
            uint requestId,
            ushort axisReference,
            LMCAxisParameterKey key,
            LMCAdminUnit unit,
            int value)
        {
            var payload = CommonPayload(28, requestId);
            TestFrame.WriteUInt16(payload, 16, (ushort)key);
            TestFrame.WriteUInt16(
                payload,
                18,
                (ushort)LMCAdminValueType.Int32);
            TestFrame.WriteUInt16(payload, 20, (ushort)unit);
            TestFrame.WriteInt32(payload, 24, value);

            return new FakeRpcStep(
                0x7D10,
                TestFrame.Response(0, payload))
            {
                InspectRequest = request =>
                {
                    AssertEx.Equal(
                        axisReference,
                        TestFrame.ReadUInt16(request, 6));
                    AssertEx.Equal(
                        requestId,
                        TestFrame.ReadUInt32(request, 12));
                    AssertEx.Equal(
                        (ushort)key,
                        TestFrame.ReadUInt16(request, 16));
                }
            };
        }

        private static FakeRpcStep AdminGroupParametersStep(
            uint requestId,
            ushort groupReference,
            LMCGroupParameterSelection selection,
            int velocity,
            int acceleration,
            int jerkTime)
        {
            var payload = CommonPayload(32, requestId);
            TestFrame.WriteUInt32(payload, 16, (uint)selection);
            TestFrame.WriteInt32(payload, 20, velocity);
            TestFrame.WriteInt32(payload, 24, acceleration);
            TestFrame.WriteInt32(payload, 28, jerkTime);

            return new FakeRpcStep(
                0x7D20,
                TestFrame.Response(0, payload))
            {
                InspectRequest = request =>
                {
                    AssertEx.Equal(
                        groupReference,
                        TestFrame.ReadUInt16(request, 6));
                    AssertEx.Equal(
                        requestId,
                        TestFrame.ReadUInt32(request, 12));
                    AssertEx.Equal(
                        (uint)selection,
                        TestFrame.ReadUInt32(request, 16));
                }
            };
        }

        private static FakeRpcStep DriveStatusAxisStep(
            ushort axisReference)
        {
            var payload = new byte[12];
            TestFrame.WriteUInt32(payload, 0, 0x00000020u);
            TestFrame.WriteUInt16(payload, 8, 0x0012);

            return new FakeRpcStep(
                0x2028,
                TestFrame.Response(0, payload))
            {
                InspectRequest = request =>
                {
                    AssertEx.Equal(
                        axisReference,
                        TestFrame.ReadUInt16(request, 6));
                }
            };
        }

        private static FakeRpcStep D5SafeAxisStatusStep()
        {
            var payload = new byte[12];
            TestFrame.WriteUInt32(payload, 0, 0x02000000u);
            return new FakeRpcStep(
                0x2028,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep D5StableAxisPositionStep(
            int positionRaw)
        {
            var payload = new byte[8];
            TestFrame.WriteInt32(payload, 0, positionRaw);
            return new FakeRpcStep(
                0x202E,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep D5SdoSubmitStep(
            uint requestId,
            uint ticketId,
            uint submitCycle,
            ushort slaveReference,
            ushort objectIndex,
            LMCSignalValueType valueType,
            ushort dataLength,
            uint timeoutCycles)
        {
            var payload = CommonPayload(32, requestId);
            TestFrame.WriteUInt32(payload, 16, ticketId);
            TestFrame.WriteUInt16(
                payload,
                20,
                (ushort)LMCOperationKind.SDORead);
            TestFrame.WriteUInt16(
                payload,
                22,
                (ushort)LMCOperationState.Queued);
            TestFrame.WriteUInt32(payload, 24, submitCycle);
            TestFrame.WriteUInt32(payload, 28, DiagnosticsBootId);

            return new FakeRpcStep(
                0x7E50,
                TestFrame.Response(0, payload))
            {
                InspectRequest = request =>
                {
                    AssertEx.Equal(
                        requestId,
                        TestFrame.ReadUInt32(request, 12));
                    AssertEx.Equal(
                        DiagnosticMapRevision,
                        TestFrame.ReadUInt32(request, 16));
                    AssertEx.Equal(
                        slaveReference,
                        TestFrame.ReadUInt16(request, 20));
                    AssertEx.Equal(
                        objectIndex,
                        TestFrame.ReadUInt16(request, 24));
                    AssertEx.Equal((byte)0, request[26]);
                    AssertEx.Equal((byte)valueType, request[27]);
                    AssertEx.Equal(
                        timeoutCycles,
                        TestFrame.ReadUInt32(request, 28));
                    AssertEx.Equal(
                        dataLength,
                        TestFrame.ReadUInt16(request, 32));
                    AssertEx.Equal(
                        DiagnosticsBootId,
                        TestFrame.ReadUInt32(request, 36));
                }
            };
        }

        private static FakeRpcStep D5SdoOperationStatusStep(
            uint requestId,
            uint ticketId,
            uint submitCycle,
            uint completionCycle,
            LMCOperationState state,
            LMCOperationOutcome outcome,
            LMCSignalValueType resultType,
            byte[] resultData)
        {
            var safeResultData = resultData ?? new byte[0];
            var payload = CommonPayload(64, requestId);
            TestFrame.WriteUInt32(payload, 16, ticketId);
            TestFrame.WriteUInt16(
                payload,
                20,
                (ushort)LMCOperationKind.SDORead);
            TestFrame.WriteUInt16(payload, 22, (ushort)state);
            TestFrame.WriteUInt32(payload, 24, submitCycle);
            TestFrame.WriteUInt32(payload, 28, completionCycle);
            TestFrame.WriteUInt16(payload, 32, (ushort)outcome);
            TestFrame.WriteUInt32(
                payload,
                40,
                outcome == LMCOperationOutcome.Success
                    ? checked((uint)safeResultData.Length)
                    : 0u);
            payload[44] = (byte)resultType;
            payload[45] = checked((byte)safeResultData.Length);
            Buffer.BlockCopy(
                safeResultData,
                0,
                payload,
                48,
                safeResultData.Length);
            TestFrame.WriteUInt32(payload, 60, DiagnosticsBootId);

            return new FakeRpcStep(
                0x7E03,
                TestFrame.Response(0, payload))
            {
                InspectRequest = request =>
                {
                    AssertEx.Equal(
                        requestId,
                        TestFrame.ReadUInt32(request, 12));
                    AssertEx.Equal(
                        ticketId,
                        TestFrame.ReadUInt32(request, 16));
                    AssertEx.Equal(
                        DiagnosticsBootId,
                        TestFrame.ReadUInt32(request, 20));
                }
            };
        }

        private static List<FakeRpcStep>
            CreateConnectAndTopologyStepsForCanonical(
                LMCDiagnosticCapability capabilities,
                byte[] canonical,
                uint topologyRevision)
        {
            var steps = new List<FakeRpcStep>
            {
                InitStep(),
                CallbackStep(),
                CapabilitiesStep(1, capabilities),
                CapabilitiesStep(2, capabilities),
                new FakeRpcStep(
                    0x7E11,
                    TestFrame.Response(
                        0,
                        TopologyInfoPayload(3, topologyRevision)))
            };

            for (ushort startIndex = 0;
                startIndex < TopologyNodeCount;
                startIndex++)
            {
                steps.Add(new FakeRpcStep(
                    0x7E12,
                    TestFrame.Response(
                        0,
                        TopologyChunkPayload(
                            checked((uint)(4 + startIndex)),
                            startIndex,
                            canonical,
                            topologyRevision))));
            }

            return steps;
        }

        private static FakeRpcStep InitStep()
        {
            var payload = new byte[24];
            TestFrame.WriteUInt32(payload, 0, 64);
            return new FakeRpcStep(0x8080, TestFrame.Response(0, payload));
        }

        private static FakeRpcStep CallbackStep()
        {
            return new FakeRpcStep(
                0x405C,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")));
        }

        private static FakeRpcStep CloseStep()
        {
            return new FakeRpcStep(
                0x405D,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")));
        }

        private static FakeRpcStep CapabilitiesStep(
            uint requestId,
            LMCDiagnosticCapability capabilities,
            ushort recorderBufferCount = 0)
        {
            return new FakeRpcStep(
                0x7E00,
                TestFrame.Response(
                    0,
                    CapabilitiesPayload(
                        requestId,
                        capabilities,
                        recorderBufferCount)));
        }

        private static byte[] CapabilitiesPayload(
            uint requestId,
            LMCDiagnosticCapability capabilities,
            ushort recorderBufferCount)
        {
            var payload = CommonPayload(68, requestId);
            TestFrame.WriteUInt32(payload, 16, 1);
            TestFrame.WriteUInt32(payload, 20, (uint)capabilities);
            TestFrame.WriteUInt32(payload, 24, DiagnosticMapRevision);
            TestFrame.WriteUInt16(
                payload,
                28,
                (capabilities & LMCDiagnosticCapability.SignalCatalog) != 0
                    ? RecorderCatalogEntryCount
                    : (ushort)0);
            TestFrame.WriteUInt16(payload, 32, 4);
            TestFrame.WriteUInt16(payload, 34, recorderBufferCount);
            TestFrame.WriteUInt32(payload, 36, 1000);
            TestFrame.WriteUInt32(payload, 40, 1000);
            TestFrame.WriteUInt16(payload, 44, 1320);
            TestFrame.WriteUInt16(payload, 46, 2040);
            TestFrame.WriteUInt16(payload, 48, 1280);
            TestFrame.WriteUInt16(payload, 50, 80);
            TestFrame.WriteUInt16(payload, 52, 16);
            TestFrame.WriteUInt32(payload, 56, 16000);
            TestFrame.WriteUInt16(payload, 60, 4);
            TestFrame.WriteUInt32(payload, 64, DiagnosticsBootId);
            return payload;
        }

        private static byte[] CatalogInfoPayload(
            uint requestId,
            ushort totalCount)
        {
            var payload = CommonPayload(36, requestId);
            TestFrame.WriteUInt32(payload, 16, DiagnosticMapRevision);
            TestFrame.WriteUInt16(payload, 20, totalCount);
            TestFrame.WriteUInt16(payload, 22, 80);
            TestFrame.WriteUInt16(payload, 24, 40);
            TestFrame.WriteUInt16(payload, 26, RecorderCatalogEntryCount);
            TestFrame.WriteUInt32(payload, 28, 0x0000000Fu);
            TestFrame.WriteUInt32(payload, 32, 1);
            return payload;
        }

        private static byte[] CatalogChunkPayload(
            uint requestId,
            ushort totalCount)
        {
            var payload = CommonPayload(28 + totalCount * 80, requestId);
            TestFrame.WriteUInt16(payload, 2, 2);
            TestFrame.WriteUInt32(payload, 16, DiagnosticMapRevision);
            TestFrame.WriteUInt16(payload, 20, 0);
            TestFrame.WriteUInt16(payload, 22, totalCount);
            TestFrame.WriteUInt16(payload, 24, totalCount);
            TestFrame.WriteUInt16(payload, 26, 80);

            for (ushort index = 0; index < totalCount; index++)
            {
                WriteRecorderCatalogEntry(payload, 28 + index * 80, index);
            }

            return payload;
        }

        private static void WriteRecorderCatalogEntry(
            byte[] payload,
            int offset,
            ushort catalogIndex)
        {
            var physicalAxis = checked((byte)(catalogIndex + 1));
            TestFrame.WriteUInt32(
                payload,
                offset,
                0x00100004u | ((uint)physicalAxis << 8));
            TestFrame.WriteUInt16(payload, offset + 4, catalogIndex);
            payload[offset + 6] = (byte)LMCSignalSourceKind.PdoInput;
            payload[offset + 7] = physicalAxis;
            payload[offset + 8] = (byte)LMCSignalValueType.Int32;
            payload[offset + 9] = 4;
            TestFrame.WriteUInt16(payload, offset + 10, 1);
            TestFrame.WriteUInt16(
                payload,
                offset + 12,
                (ushort)(LMCSignalAccessFlags.Readable
                    | LMCSignalAccessFlags.Recordable
                    | LMCSignalAccessFlags.BulkReadable));
            TestFrame.WriteUInt16(
                payload,
                offset + 14,
                (ushort)(LMCSignalFlags.ActivePdo
                    | LMCSignalFlags.PhysicalAxis
                    | LMCSignalFlags.InputMappedPhase));
            TestFrame.WriteUInt16(payload, offset + 16, 0x6064);
            payload[offset + 18] = 0;
            payload[offset + 19] = (byte)LMCPdoDirection.DriveToMaster;
            TestFrame.WriteInt32(payload, offset + 20, 1);
            TestFrame.WriteInt32(payload, offset + 24, 1);
            TestFrame.WriteInt32(payload, offset + 28, int.MinValue);
            TestFrame.WriteInt32(payload, offset + 32, int.MaxValue);
            var alias = Encoding.ASCII.GetBytes(
                "axis"
                    + physicalAxis.ToString(CultureInfo.InvariantCulture)
                    + ".actual_position");
            Buffer.BlockCopy(alias, 0, payload, offset + 36, alias.Length);
        }

        private static byte[] TopologyInfoPayload(uint requestId)
        {
            return TopologyInfoPayload(requestId, TopologyRevision);
        }

        private static byte[] TopologyInfoPayload(
            uint requestId,
            uint topologyRevision)
        {
            var payload = CommonPayload(44, requestId);
            TestFrame.WriteUInt32(payload, 16, topologyRevision);
            TestFrame.WriteUInt16(payload, 20, TopologyNodeCount);
            TestFrame.WriteUInt16(payload, 22, 96);
            TestFrame.WriteUInt16(payload, 24, 1);
            TestFrame.WriteUInt16(payload, 26, 5);
            TestFrame.WriteUInt16(payload, 28, 2);
            TestFrame.WriteUInt16(payload, 30, 4);
            TestFrame.WriteUInt32(payload, 32, 0x0000000Fu);
            TestFrame.WriteUInt32(payload, 36, 1);
            return payload;
        }

        private static byte[] TopologyChunkPayload(
            uint requestId,
            ushort startIndex,
            byte[] canonical)
        {
            return TopologyChunkPayload(
                requestId,
                startIndex,
                canonical,
                TopologyRevision);
        }

        private static byte[] TopologyChunkPayload(
            uint requestId,
            ushort startIndex,
            byte[] canonical,
            uint topologyRevision)
        {
            var payload = CommonPayload(124, requestId);
            if (startIndex == TopologyNodeCount - 1)
            {
                TestFrame.WriteUInt16(payload, 2, 2);
            }

            TestFrame.WriteUInt32(payload, 16, topologyRevision);
            TestFrame.WriteUInt16(payload, 20, startIndex);
            TestFrame.WriteUInt16(payload, 22, 1);
            TestFrame.WriteUInt16(payload, 24, TopologyNodeCount);
            TestFrame.WriteUInt16(payload, 26, 96);
            Buffer.BlockCopy(canonical, startIndex * 96, payload, 28, 96);
            return payload;
        }

        private static byte[] NodeHealthPayload(
            uint requestId,
            uint nodeId,
            uint cycleCounter,
            bool hasDs402Data)
        {
            var payload = CommonPayload(72, requestId);
            TestFrame.WriteUInt32(payload, 16, TopologyRevision);
            TestFrame.WriteUInt32(payload, 20, nodeId);
            TestFrame.WriteUInt16(
                payload,
                24,
                (ushort)LMCCapturePhase.InputMapped);
            var flags = LMCEtherCATNodeHealthFlags.Configured
                | LMCEtherCATNodeHealthFlags.Detected
                | LMCEtherCATNodeHealthFlags.IdentityMatched
                | LMCEtherCATNodeHealthFlags.DataValid;
            if (hasDs402Data)
            {
                flags |= LMCEtherCATNodeHealthFlags.Ds402DataPresent;
            }

            TestFrame.WriteUInt16(payload, 26, (ushort)flags);
            TestFrame.WriteUInt32(payload, 28, cycleCounter);
            TestFrame.WriteUInt64(payload, 32, cycleCounter * 1000UL);
            TestFrame.WriteUInt32(payload, 40, 2);
            payload[44] = 1;
            payload[45] = 8;
            TestFrame.WriteUInt32(payload, 48, 7);
            TestFrame.WriteUInt32(payload, 52, 8);
            if (hasDs402Data)
            {
                TestFrame.WriteUInt32(payload, 56, 0x1234u);
                TestFrame.WriteUInt32(payload, 64, 8);
                TestFrame.WriteUInt32(payload, 68, 8);
            }

            return payload;
        }

        private static byte[] DigitalInputPayload(
            uint requestId,
            uint ioReference,
            uint nodeId,
            byte bitWidth,
            ulong value,
            uint cycleCounter)
        {
            return DigitalInputPayload(
                requestId,
                ioReference,
                nodeId,
                bitWidth,
                value,
                cycleCounter,
                TopologyRevision);
        }

        private static byte[] DigitalInputPayload(
            uint requestId,
            uint ioReference,
            uint nodeId,
            byte bitWidth,
            ulong value,
            uint cycleCounter,
            uint topologyRevision)
        {
            var payload = CommonPayload(56, requestId);
            TestFrame.WriteUInt32(payload, 16, topologyRevision);
            TestFrame.WriteUInt32(payload, 20, ioReference);
            TestFrame.WriteUInt32(payload, 24, nodeId);
            payload[28] = (byte)LMCDigitalIODirection.Input;
            payload[29] = bitWidth;
            TestFrame.WriteUInt16(
                payload,
                30,
                (ushort)LMCDigitalIOStatusFlags.Valid);
            TestFrame.WriteUInt64(payload, 32, value);
            TestFrame.WriteUInt64(
                payload,
                40,
                bitWidth == 64
                    ? ulong.MaxValue
                    : (1UL << bitWidth) - 1UL);
            TestFrame.WriteUInt32(payload, 48, cycleCounter);
            return payload;
        }

        private static byte[] DigitalOutputPayload(
            uint requestId,
            uint ioReference,
            uint nodeId,
            byte bitWidth,
            ulong value,
            uint cycleCounter,
            uint outputRevision)
        {
            return DigitalOutputPayload(
                requestId,
                ioReference,
                nodeId,
                bitWidth,
                value,
                cycleCounter,
                outputRevision,
                TopologyRevision);
        }

        private static byte[] DigitalOutputPayload(
            uint requestId,
            uint ioReference,
            uint nodeId,
            byte bitWidth,
            ulong value,
            uint cycleCounter,
            uint outputRevision,
            uint topologyRevision)
        {
            var payload = DigitalInputPayload(
                requestId,
                ioReference,
                nodeId,
                bitWidth,
                value,
                cycleCounter,
                topologyRevision);
            payload[28] = (byte)LMCDigitalIODirection.Output;
            TestFrame.WriteUInt32(payload, 52, outputRevision);
            return payload;
        }

        private static byte[] CommonPayload(int length, uint requestId)
        {
            var payload = new byte[length];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, requestId);
            return payload;
        }

        private static byte[] RecorderConfigurePayload(
            uint requestId,
            uint configId,
            ushort channelCount,
            uint acceptedCapacity)
        {
            var sampleStrideBytes = checked((ushort)(channelCount * 4));
            var payload = CommonPayload(56, requestId);
            TestFrame.WriteUInt32(payload, 16, configId);
            TestFrame.WriteUInt32(payload, 20, 1);
            TestFrame.WriteUInt32(payload, 24, DiagnosticMapRevision);
            TestFrame.WriteUInt32(payload, 28, acceptedCapacity);
            TestFrame.WriteUInt32(
                payload,
                32,
                checked(acceptedCapacity * sampleStrideBytes));
            TestFrame.WriteUInt16(
                payload,
                36,
                (ushort)LMCRecorderState.Configured);
            TestFrame.WriteUInt16(payload, 38, channelCount);
            TestFrame.WriteUInt16(payload, 40, sampleStrideBytes);
            TestFrame.WriteUInt16(payload, 42, 1);
            TestFrame.WriteUInt16(
                payload,
                44,
                (ushort)LMCCapturePhase.InputMapped);
            TestFrame.WriteUInt32(payload, 48, 1);
            TestFrame.WriteUInt32(payload, 52, DiagnosticsBootId);
            return payload;
        }

        private static byte[] CreateTopologyCanonicalBytes()
        {
            var canonical = new byte[TopologyNodeCount * 96];
            WriteTopologyEntry(
                canonical,
                0,
                0xEC000001u,
                0,
                0,
                0,
                1,
                65,
                0,
                0,
                ushort.MaxValue,
                669,
                1196200070,
                65536,
                0,
                0,
                0,
                "GL_9086_11",
                0);

            var driveNames = new[]
            {
                "Elmo_11",
                "Elmo_21",
                "Elmo_31",
                "Elmo_41"
            };
            for (ushort axis = 1; axis <= driveNames.Length; axis++)
            {
                WriteTopologyEntry(
                    canonical,
                    axis,
                    checked(0xEC000100u + axis),
                    0,
                    axis,
                    axis,
                    1,
                    39,
                    axis,
                    axis,
                    ushort.MaxValue,
                    154,
                    198948,
                    66592,
                    0,
                    0,
                    0,
                    driveNames[axis - 1],
                    0);
            }

            WriteTopologyEntry(
                canonical,
                5,
                0xEC010001u,
                0xEC000001u,
                5,
                ushort.MaxValue,
                2,
                136,
                0,
                0,
                0,
                669,
                1196692218,
                0,
                0,
                4,
                0,
                "GL_9086_1_Slot001",
                0x00010001u);
            WriteTopologyEntry(
                canonical,
                6,
                0xEC010002u,
                0xEC000001u,
                6,
                ushort.MaxValue,
                2,
                144,
                0,
                0,
                1,
                669,
                1196696250,
                0,
                0,
                0,
                4,
                "GL_9086_1_Slot011",
                0x00010002u);
            return canonical;
        }

        private static byte[] CreateMixedIoTopologyCanonicalBytes()
        {
            var canonical = CreateTopologyCanonicalBytes();
            var mixedEntryOffset = 5 * 96;
            TestFrame.WriteUInt16(
                canonical,
                mixedEntryOffset + 14,
                (ushort)(LMCEtherCATTopologyNodeFlags.HasInputs
                    | LMCEtherCATTopologyNodeFlags.HasOutputs
                    | LMCEtherCATTopologyNodeFlags.HasDigitalIO));
            TestFrame.WriteUInt16(canonical, mixedEntryOffset + 42, 4);
            return canonical;
        }

        private static uint ComputeTopologyRevision(byte[] canonical)
        {
            AssertEx.NotNull(canonical);
            var crc = 0xFFFFFFFFu;
            foreach (var octet in canonical)
            {
                crc ^= octet;
                for (var bit = 0; bit < 8; bit++)
                {
                    crc = (crc & 1u) != 0
                        ? (crc >> 1) ^ 0xEDB88320u
                        : crc >> 1;
                }
            }

            var result = crc ^ 0xFFFFFFFFu;
            return result == 0 ? 0xFFFFFFFFu : result;
        }

        private static void WriteTopologyEntry(
            byte[] canonical,
            int entryIndex,
            uint nodeId,
            uint parentNodeId,
            ushort topologyIndex,
            ushort masterSlaveIndex,
            byte nodeKind,
            ushort nodeFlags,
            ushort sdoSlaveReference,
            ushort physicalAxisReference,
            ushort slotIndex,
            uint vendorId,
            uint productCode,
            uint revisionNumber,
            uint serialNumber,
            ushort inputBytes,
            ushort outputBytes,
            string name,
            uint ioReference)
        {
            var offset = entryIndex * 96;
            TestFrame.WriteUInt32(canonical, offset, nodeId);
            TestFrame.WriteUInt32(canonical, offset + 4, parentNodeId);
            TestFrame.WriteUInt16(canonical, offset + 8, topologyIndex);
            TestFrame.WriteUInt16(canonical, offset + 10, masterSlaveIndex);
            canonical[offset + 12] = nodeKind;
            TestFrame.WriteUInt16(canonical, offset + 14, nodeFlags);
            TestFrame.WriteUInt16(canonical, offset + 16, sdoSlaveReference);
            TestFrame.WriteUInt16(canonical, offset + 18, physicalAxisReference);
            TestFrame.WriteUInt16(canonical, offset + 20, slotIndex);
            TestFrame.WriteUInt32(canonical, offset + 24, vendorId);
            TestFrame.WriteUInt32(canonical, offset + 28, productCode);
            TestFrame.WriteUInt32(canonical, offset + 32, revisionNumber);
            TestFrame.WriteUInt32(canonical, offset + 36, serialNumber);
            TestFrame.WriteUInt16(canonical, offset + 40, inputBytes);
            TestFrame.WriteUInt16(canonical, offset + 42, outputBytes);
            var nameBytes = Encoding.ASCII.GetBytes(name);
            Buffer.BlockCopy(
                nameBytes,
                0,
                canonical,
                offset + 44,
                nameBytes.Length);
            TestFrame.WriteUInt32(canonical, offset + 92, ioReference);
        }

        private static void AssertNoLiveIoRequests(
            IEnumerable<byte[]> requests)
        {
            foreach (var request in requests)
            {
                var command = TestFrame.ReadUInt16(request, 0);
                AssertEx.True(
                    command != 0x7E13
                        && command != 0x7E22
                        && command != 0x7E23,
                    "A live EtherCAT I/O request was sent while bits 15-17 were off: 0x"
                        + command.ToString("X4", CultureInfo.InvariantCulture));
            }
        }

        private static int CountRequestsInSession(
            FakeRpcServer server,
            int sessionOrdinal)
        {
            var count = 0;
            for (var index = 0;
                index < server.ReceivedRequests.Count;
                index++)
            {
                if (server.ReceivedRequestSessionOrdinals[index]
                    == sessionOrdinal)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountCommandInSession(
            FakeRpcServer server,
            int sessionOrdinal,
            ushort command)
        {
            var count = 0;
            for (var index = 0;
                index < server.ReceivedRequests.Count;
                index++)
            {
                if (server.ReceivedRequestSessionOrdinals[index]
                        == sessionOrdinal
                    && TestFrame.ReadUInt16(
                        server.ReceivedRequests[index],
                        0) == command)
                {
                    count++;
                }
            }

            return count;
        }

        private static void AssertNoRpcCloseInSession(
            FakeRpcServer server,
            int sessionOrdinal)
        {
            AssertEx.Equal(
                0,
                CountCommandInSession(server, sessionOrdinal, 0x405D));
        }

        private static void AssertNodeHealthRequest(
            byte[] request,
            uint expectedRequestId,
            uint expectedNodeId)
        {
            AssertEx.Equal(24, request.Length);
            AssertEx.Equal(expectedRequestId, TestFrame.ReadUInt32(request, 12));
            AssertEx.Equal(TopologyRevision, TestFrame.ReadUInt32(request, 16));
            AssertEx.Equal(expectedNodeId, TestFrame.ReadUInt32(request, 20));
        }

        private static void AssertDigitalInputRequest(
            byte[] request,
            uint expectedRequestId,
            uint expectedIoReference,
            byte expectedBitWidth)
        {
            AssertDigitalInputRequest(
                request,
                expectedRequestId,
                expectedIoReference,
                expectedBitWidth,
                TopologyRevision);
        }

        private static void AssertDigitalInputRequest(
            byte[] request,
            uint expectedRequestId,
            uint expectedIoReference,
            byte expectedBitWidth,
            uint expectedTopologyRevision)
        {
            AssertEx.Equal(28, request.Length);
            AssertEx.Equal(expectedRequestId, TestFrame.ReadUInt32(request, 12));
            AssertEx.Equal(
                expectedTopologyRevision,
                TestFrame.ReadUInt32(request, 16));
            AssertEx.Equal(expectedIoReference, TestFrame.ReadUInt32(request, 20));
            AssertEx.Equal(
                (byte)LMCDigitalIODirection.Input,
                request[24]);
            AssertEx.Equal(expectedBitWidth, request[25]);
        }

        private static void AssertDigitalOutputRequest(
            byte[] request,
            uint expectedRequestId,
            uint expectedIoReference,
            byte expectedBitWidth)
        {
            AssertDigitalOutputRequest(
                request,
                expectedRequestId,
                expectedIoReference,
                expectedBitWidth,
                TopologyRevision);
        }

        private static void AssertDigitalOutputRequest(
            byte[] request,
            uint expectedRequestId,
            uint expectedIoReference,
            byte expectedBitWidth,
            uint expectedTopologyRevision)
        {
            AssertEx.Equal(28, request.Length);
            AssertEx.Equal(expectedRequestId, TestFrame.ReadUInt32(request, 12));
            AssertEx.Equal(
                expectedTopologyRevision,
                TestFrame.ReadUInt32(request, 16));
            AssertEx.Equal(expectedIoReference, TestFrame.ReadUInt32(request, 20));
            AssertEx.Equal(
                (byte)LMCDigitalIODirection.Output,
                request[24]);
            AssertEx.Equal(expectedBitWidth, request[25]);
        }

        private static void AssertFullCapabilityMonitorRequestSequence(
            IList<byte[]> requests)
        {
            AssertRequestCommandSequence(
                requests,
                0x8080,
                0x405C,
                0x7E00,
                0x7E00,
                0x7E11,
                0x7E12,
                0x7E12,
                0x7E12,
                0x7E12,
                0x7E12,
                0x7E12,
                0x7E12,
                0x7E13,
                0x7E22,
                0x7E13,
                0x7E00,
                0x405D);

            var digitalIoRequests = 0;
            foreach (var request in requests)
            {
                if (TestFrame.ReadUInt16(request, 0) != 0x7E22)
                {
                    continue;
                }

                digitalIoRequests++;
                AssertEx.Equal(
                    (byte)LMCDigitalIODirection.Input,
                    request[24],
                    "The background monitor sent an output-shadow read.");
            }

            AssertEx.Equal(
                1,
                digitalIoRequests,
                "The deterministic monitor smoke must send only the selected-input read.");
        }

        private static void AssertLateManualReadRequestSequence(
            IList<byte[]> requests)
        {
            AssertRequestCommandSequence(
                requests,
                0x8080,
                0x405C,
                0x7E00,
                0x7E00,
                0x7E11,
                0x7E12,
                0x7E12,
                0x7E12,
                0x7E12,
                0x7E12,
                0x7E12,
                0x7E12,
                0x7E13,
                0x7E22,
                0x7E22,
                0x405D);
        }

        private static void AssertMixedIoShadowPreservationRequestSequence(
            IList<byte[]> requests)
        {
            AssertRequestCommandSequence(
                requests,
                0x8080,
                0x405C,
                0x7E00,
                0x7E00,
                0x7E11,
                0x7E12,
                0x7E12,
                0x7E12,
                0x7E12,
                0x7E12,
                0x7E12,
                0x7E12,
                0x7E22,
                0x7E22,
                0x7E22,
                0x405D);
        }

        private static void AssertInvalidatedManualFailureRequestSequence(
            IList<byte[]> requests)
        {
            AssertRequestCommandSequence(
                requests,
                0x8080,
                0x405C,
                0x7E00,
                0x7E00,
                0x7E11,
                0x7E12,
                0x7E12,
                0x7E12,
                0x7E12,
                0x7E12,
                0x7E12,
                0x7E12,
                0x7E13,
                0x405D);
        }

        private static void AssertChannelIndependentMonitorRequestSequence(
            IList<byte[]> requests)
        {
            AssertRequestCommandSequence(
                requests,
                0x8080,
                0x405C,
                0x7E00,
                0x7E00,
                0x7E11,
                0x7E12,
                0x7E12,
                0x7E12,
                0x7E12,
                0x7E12,
                0x7E12,
                0x7E12,
                0x7E13,
                0x7E22,
                0x7E13,
                0x7E22,
                0x7E22,
                0x7E13,
                0x405D);
        }

        private static void AssertNoRecorderRequests(
            IEnumerable<byte[]> requests)
        {
            foreach (var request in requests)
            {
                var command = TestFrame.ReadUInt16(request, 0);
                AssertEx.True(
                    !IsRecorderCommand(command),
                    "A Recorder command was sent by the dormant Double-bank UI: 0x"
                        + command.ToString("X4", CultureInfo.InvariantCulture));
            }
        }

        private static bool IsRecorderCommand(ushort command)
        {
            return command >= 0x7E40 && command <= 0x7E4F;
        }

        private static void AssertRequestCommandSequence(
            IList<byte[]> requests,
            params ushort[] expectedCommands)
        {
            AssertEx.Equal(
                expectedCommands.Length,
                requests.Count,
                "Fake RPC request count mismatch.");
            for (var index = 0; index < expectedCommands.Length; index++)
            {
                AssertEx.Equal(
                    expectedCommands[index],
                    TestFrame.ReadUInt16(requests[index], 0),
                    "Unexpected RPC command at index "
                        + index.ToString(CultureInfo.InvariantCulture)
                        + ".");
            }
        }

        private static int CountRequestCommand(
            IList<byte[]> requests,
            ushort expectedCommand)
        {
            var count = 0;
            for (var index = 0; index < requests.Count; index++)
            {
                if (TestFrame.ReadUInt16(requests[index], 0)
                    == expectedCommand)
                {
                    count++;
                }
            }

            return count;
        }

        private static string CreateJournalDirectory()
        {
            return Path.Combine(
                Path.GetTempPath(),
                "ElmoWpfSmoke-" + Guid.NewGuid().ToString("N"));
        }

        private static void DeleteJournalDirectory(string path)
        {
            var cleanup = Stopwatch.StartNew();
            while (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);
                    return;
                }
                catch (IOException)
                {
                    if (cleanup.ElapsedMilliseconds >= 2000)
                    {
                        throw;
                    }

                    // Window.IsLoaded can become false immediately before the
                    // OnClosed journal-disposal path releases journal.lock.
                    PumpDispatcherOnce();
                    Thread.Sleep(10);
                }
            }
        }
    }
}

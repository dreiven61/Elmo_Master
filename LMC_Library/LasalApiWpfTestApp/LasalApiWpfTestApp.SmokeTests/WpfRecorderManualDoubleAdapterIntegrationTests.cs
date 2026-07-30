using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlApiExample;
using LasalMotionControlLib;
using LasalMotionControlLib.Tests;

namespace LasalApiWpfTestApp.SmokeTests
{
    internal static partial class WpfMainWindowIntegrationTests
    {
        private const string RecorderManualDoubleOperation =
            "Recorder Manual Double Adapter";

        private static void RegisterRecorderManualDoubleAdapterIntegrationTests(
            ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.Recorder.ManualDoubleConfigureRetainsAndCleansConfigurationOnly",
                ManualDoubleConfigureRetainsAndCleansConfigurationOnly);
            tests.Add(
                "Wpf.Recorder.ManualDoubleAcceptedPreemptionRetainsAndCleansExactResult",
                ManualDoubleAcceptedPreemptionRetainsAndCleansExactResult);
            tests.Add(
                "Wpf.Recorder.ManualDoubleConfigureResponseLossRetainsUnknownScope",
                ManualDoubleConfigureResponseLossRetainsUnknownScope);
            tests.Add(
                "Wpf.Recorder.ManualDoubleCleanupResponseLossIsNotReplayed",
                ManualDoubleCleanupResponseLossIsNotReplayed);
            tests.Add(
                "Wpf.Recorder.ManualDoublePreArmValidationIsZeroConfigureAndNoJournal",
                ManualDoublePreArmValidationIsZeroConfigureAndNoJournal);
        }

        private static void
            ManualDoubleConfigureRetainsAndCleansConfigurationOnly()
        {
            var capabilities = RecorderManualDoubleCapabilities();
            var steps = CreateConnectAndTopologySteps(capabilities, 2);
            steps.Add(CapabilitiesStep(11, capabilities, 2));

            var configureResponse =
                RecorderManualDoubleConfigureResponse(12);
            var configure = new FakeRpcStep(
                0x7E4C,
                configureResponse)
            {
                InspectRequest = request =>
                    BindAndAssertRecorderManualDoubleConfigure(
                        request,
                        configureResponse,
                        12)
            };
            steps.Add(configure);

            RecorderDoubleRecoveryJournal observedJournal = null;
            RecorderDoubleBankConfigurationLease expectedLease = null;
            var release = new FakeRpcStep(
                0x7E48,
                TestFrame.Response(0, CommonPayload(16, 13)))
            {
                InspectRequest = request =>
                {
                    AssertEx.NotNull(observedJournal);
                    AssertEx.NotNull(expectedLease);
                    AssertEx.True(
                        observedJournal.CurrentRecord
                            .ConfigurationReleaseIntent,
                        "Configuration Release intent was not durable before wire.");
                    AssertRecorderManualDoubleReleaseRequest(
                        request,
                        13,
                        expectedLease);
                }
            };
            steps.Add(release);
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(journalDirectory, server.Port);
                    ConnectRecorderManualDoubleWindow(window);

                    var source = RecorderManualDoubleSourceConfiguration();
                    var operation = StartRecorderManualDoubleOperation(
                        window,
                        source);
                    AwaitRecorderManualDoubleTask(
                        operation,
                        "Manual Double Configure did not finish.");

                    AssertEx.Equal(
                        RecorderManualDoubleOperation + " completed",
                        window.TextOperationState.Text,
                        window.TextExecutionLog.Text);
                    AssertEx.Equal(0u, source.RequestedConfigId);

                    var scope =
                        (RecorderDoubleBankRecoveryScope)GetPrivateField(
                            window,
                            "recorderDoubleRetainedQualificationScope");
                    var operations =
                        (RecorderDoubleBankQualificationOperations)
                            GetPrivateField(
                                window,
                                "recorderDoubleRetainedQualificationOperations");
                    var coordinator =
                        (RecorderDoubleDurableReleaseCoordinator)
                            GetPrivateField(
                                window,
                                "recorderDoubleRetainedQualificationReleaseCoordinator");
                    observedJournal =
                        (RecorderDoubleRecoveryJournal)GetPrivateField(
                            window,
                            "recorderDoubleRecoveryJournal");
                    expectedLease = scope.Configuration;
                    var native =
                        (LMCRecorderConfigurationHandle)
                            expectedLease.NativeHandle;

                    AssertRecorderManualDoubleRetainedScope(
                        window,
                        source,
                        scope,
                        native,
                        observedJournal.CurrentRecord);
                    AssertEx.True(
                        GetPrivateField(
                            window,
                            "recorderDoubleRetainedQualificationResult")
                        == null);
                    AssertEx.True(
                        GetPrivateField(
                            window,
                            "recorderDoubleRetainedQualificationError")
                        == null);
                    AssertEx.False(
                        window.ButtonReleaseRecorderDoubleRetained.IsEnabled,
                        "The proof gate must remain closed after adapter implementation.");

                    var cleanup = coordinator
                        .ReleaseQualificationConfigurationAndResolveAsync(
                            scope,
                            operations,
                            true,
                            CancellationToken.None);
                    AwaitRecorderManualDoubleTask(
                        cleanup,
                        "Manual Double configuration cleanup did not finish.");

                    AssertEx.True(expectedLease.IsReleased);
                    AssertEx.True(native.IsReleased);
                    AssertEx.Equal(
                        RecorderDoubleRecoveryState.Resolved,
                        observedJournal.CurrentRecord.State);
                    AssertEx.True(
                        observedJournal.CurrentRecord
                            .ConfigurationReleaseIntent);
                    AssertEx.True(
                        observedJournal.CurrentRecord
                            .ConfigurationReleaseConfirmed);
                    AssertEx.Equal(
                        0,
                        observedJournal.CurrentRecord.Banks.Count);

                    InvokePrivate(
                        window,
                        "ClearRecorderDoubleRetainedQualification");
                    InvokePrivate(window, "UpdateUiState");
                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                    AssertEx.Equal(
                        0,
                        server.ReceivedRequests.Count(
                            request => TestFrame.ReadUInt16(request, 0)
                                == 0x7E49));
                }
            }
            finally
            {
                CloseRecorderManualDoubleWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void
            ManualDoubleAcceptedPreemptionRetainsAndCleansExactResult()
        {
            var capabilities = RecorderManualDoubleCapabilities();
            var steps = CreateConnectAndTopologySteps(capabilities, 2);
            steps.Add(CapabilitiesStep(11, capabilities, 2));

            using (var configureReceived = new ManualResetEventSlim(false))
            using (var releaseConfigure = new ManualResetEventSlim(false))
            {
                var configureResponse =
                    RecorderManualDoubleConfigureResponse(12);
                steps.Add(
                    new FakeRpcStep(0x7E4C, configureResponse)
                    {
                        InspectRequest = request =>
                        {
                            BindAndAssertRecorderManualDoubleConfigure(
                                request,
                                configureResponse,
                                12);
                            configureReceived.Set();
                            AssertEx.True(
                                releaseConfigure.Wait(5000),
                                "The delayed recoverable Configure response was not released.");
                        }
                    });
                steps.Add(
                    new FakeRpcStep(
                        0x7E48,
                        TestFrame.Response(
                            0,
                            CommonPayload(16, 13))));
                steps.Add(CloseStep());

                var journalDirectory = CreateJournalDirectory();
                MainWindow window = null;
                try
                {
                    using (var server = new FakeRpcServer(steps.ToArray()))
                    {
                        window = CreateWindow(journalDirectory, server.Port);
                        ConnectRecorderManualDoubleWindow(window);
                        var source =
                            RecorderManualDoubleSourceConfiguration();
                        var operation = StartRecorderManualDoubleOperation(
                            window,
                            source);
                        WaitUntil(
                            () => configureReceived.IsSet,
                            "Recoverable Configure did not reach the response barrier.",
                            3000);

                        var sendCoordinator =
                            (LMCSendPriorityCoordinator)GetPrivateField(
                                window,
                                "sendPriorityCoordinator");
                        sendCoordinator.ReservePrioritySend();
                        releaseConfigure.Set();
                        AwaitRecorderManualDoubleTask(
                            operation,
                            "Preempted manual Double Configure did not finish.");

                        AssertEx.Equal(
                            RecorderManualDoubleOperation + " failed",
                            window.TextOperationState.Text);
                        var retainedError =
                            (Exception)GetPrivateField(
                                window,
                                "recorderDoubleRetainedQualificationError");
                        var preempted = retainedError
                            as LMCSendPreemptedException;
                        AssertEx.NotNull(preempted);
                        AssertEx.Equal(
                            LMCSendPreemptionPhase.ResultDiscarded,
                            preempted.Phase);
                        AssertEx.Equal((ushort)0x7E4C, preempted.Command);

                        LMCRecorderAcceptedResultFailureContext context;
                        AssertEx.True(
                            LMCRecorderAcceptedResultFailureContext.TryGet(
                                retainedError,
                                out context));
                        AssertEx.Equal(
                            LMCRecorderAcceptedOperation
                                .ConfigureRecoverableDoubleRecorder,
                            context.Operation);

                        var scope =
                            (RecorderDoubleBankRecoveryScope)GetPrivateField(
                                window,
                                "recorderDoubleRetainedQualificationScope");
                        var operations =
                            (RecorderDoubleBankQualificationOperations)
                                GetPrivateField(
                                    window,
                                    "recorderDoubleRetainedQualificationOperations");
                        var coordinator =
                            (RecorderDoubleDurableReleaseCoordinator)
                                GetPrivateField(
                                    window,
                                    "recorderDoubleRetainedQualificationReleaseCoordinator");
                        var journal =
                            (RecorderDoubleRecoveryJournal)GetPrivateField(
                                window,
                                "recorderDoubleRecoveryJournal");
                        var native =
                            (LMCRecorderConfigurationHandle)
                                scope.Configuration.NativeHandle;

                        AssertEx.True(
                            ReferenceEquals(
                                context.ConfigurationHandle,
                                native));
                        AssertEx.True(native.IsAcceptedResultRecoveryOnly);
                        AssertEx.Equal(
                            RecorderDoubleRecoveryState
                                .ConfigurationIdentified,
                            journal.CurrentRecord.State);
                        AssertEx.Equal(
                            "CONFIGURATION_RETAINED",
                            scope.Stage);
                        AssertEx.True(
                            GetPrivateField(window, "recorderConfiguration")
                            == null);

                        var cleanup = coordinator
                            .ReleaseQualificationConfigurationAndResolveAsync(
                                scope,
                                operations,
                                true,
                                CancellationToken.None);
                        AwaitRecorderManualDoubleTask(
                            cleanup,
                            "Accepted manual Double result cleanup did not finish.");
                        AssertEx.True(native.IsReleased);
                        AssertEx.True(scope.Configuration.IsReleased);
                        AssertEx.Equal(
                            RecorderDoubleRecoveryState.Resolved,
                            journal.CurrentRecord.State);

                        InvokePrivate(
                            window,
                            "ClearRecorderDoubleRetainedQualification");
                        InvokePrivate(window, "UpdateUiState");
                        CloseConnectedWindow(window);
                        window = null;
                        server.Verify();
                    }
                }
                finally
                {
                    releaseConfigure.Set();
                    CloseRecorderManualDoubleWindowBestEffort(window);
                    DeleteJournalDirectory(journalDirectory);
                }
            }
        }

        private static void
            ManualDoubleConfigureResponseLossRetainsUnknownScope()
        {
            var capabilities = RecorderManualDoubleCapabilities();
            var steps = CreateConnectAndTopologySteps(capabilities, 2);
            steps.Add(CapabilitiesStep(11, capabilities, 2));

            var configureResponse =
                RecorderManualDoubleConfigureResponse(12);
            steps.Add(
                new FakeRpcStep(0x7E4C, configureResponse)
                {
                    InspectRequest = request =>
                        BindAndAssertRecorderManualDoubleConfigure(
                            request,
                            configureResponse,
                            12),
                    CloseClientBeforeResponse = true
                });

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(journalDirectory, server.Port);
                    ConnectRecorderManualDoubleWindow(window);
                    var operation = StartRecorderManualDoubleOperation(
                        window,
                        RecorderManualDoubleSourceConfiguration());
                    AwaitRecorderManualDoubleTask(
                        operation,
                        "Lost manual Double Configure response did not finish.");

                    AssertEx.Equal(
                        RecorderManualDoubleOperation + " failed",
                        window.TextOperationState.Text);
                    var scope =
                        (RecorderDoubleBankRecoveryScope)GetPrivateField(
                            window,
                            "recorderDoubleRetainedQualificationScope");
                    var journal =
                        (RecorderDoubleRecoveryJournal)GetPrivateField(
                            window,
                            "recorderDoubleRecoveryJournal");
                    var retainedError =
                        (Exception)GetPrivateField(
                            window,
                            "recorderDoubleRetainedQualificationError");

                    AssertEx.True(scope.ConfigurationAttempted);
                    AssertEx.True(scope.Configuration == null);
                    AssertEx.True(scope.HasAnyPossibleResource);
                    AssertEx.Equal("CONFIGURE", scope.Stage);
                    AssertEx.Equal(
                        RecorderDoubleRecoveryState
                            .ArmedBeforeConfigureDispatch,
                        journal.CurrentRecord.State);
                    LMCRecorderAcceptedResultFailureContext ignored;
                    AssertEx.False(
                        LMCRecorderAcceptedResultFailureContext.TryGet(
                            retainedError,
                            out ignored));
                    var denial = (string)InvokePrivate(
                        window,
                        "GetRecorderDoubleLifecycleAdmissionDenial",
                        true);
                    AssertEx.Contains(
                        "no exact returned handle",
                        denial);
                    AssertEx.Equal(
                        0,
                        server.ReceivedRequests.Count(
                            request => TestFrame.ReadUInt16(request, 0)
                                == 0x7E48));
                    server.Verify();
                }
            }
            finally
            {
                CloseRecorderManualDoubleWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void
            ManualDoubleCleanupResponseLossIsNotReplayed()
        {
            var capabilities = RecorderManualDoubleCapabilities();
            var steps = CreateConnectAndTopologySteps(capabilities, 2);
            steps.Add(CapabilitiesStep(11, capabilities, 2));

            var configureResponse =
                RecorderManualDoubleConfigureResponse(12);
            steps.Add(
                new FakeRpcStep(0x7E4C, configureResponse)
                {
                    InspectRequest = request =>
                        BindAndAssertRecorderManualDoubleConfigure(
                            request,
                            configureResponse,
                            12)
                });
            steps.Add(
                new FakeRpcStep(
                    0x7E48,
                    TestFrame.Response(0, CommonPayload(16, 13)))
                {
                    CloseClientBeforeResponse = true
                });

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(journalDirectory, server.Port);
                    ConnectRecorderManualDoubleWindow(window);
                    var operation = StartRecorderManualDoubleOperation(
                        window,
                        RecorderManualDoubleSourceConfiguration());
                    AwaitRecorderManualDoubleTask(
                        operation,
                        "Manual Double Configure did not finish before cleanup-loss test.");

                    var scope =
                        (RecorderDoubleBankRecoveryScope)GetPrivateField(
                            window,
                            "recorderDoubleRetainedQualificationScope");
                    var operations =
                        (RecorderDoubleBankQualificationOperations)
                            GetPrivateField(
                                window,
                                "recorderDoubleRetainedQualificationOperations");
                    var coordinator =
                        (RecorderDoubleDurableReleaseCoordinator)
                            GetPrivateField(
                                window,
                                "recorderDoubleRetainedQualificationReleaseCoordinator");
                    var journal =
                        (RecorderDoubleRecoveryJournal)GetPrivateField(
                            window,
                            "recorderDoubleRecoveryJournal");
                    var native =
                        (LMCRecorderConfigurationHandle)
                            scope.Configuration.NativeHandle;

                    var firstCleanup = coordinator
                        .ReleaseQualificationConfigurationAndResolveAsync(
                            scope,
                            operations,
                            true,
                            CancellationToken.None);
                    AwaitRecorderManualDoubleFailure(
                        firstCleanup,
                        "Lost manual Double cleanup response did not fail.");

                    AssertEx.True(
                        journal.CurrentRecord.ConfigurationReleaseIntent);
                    AssertEx.False(
                        journal.CurrentRecord
                            .ConfigurationReleaseConfirmed);
                    AssertEx.True(
                        journal.CurrentRecord
                            .HasConfigurationReleaseOutcomeUncertain);
                    AssertEx.True(native.IsReleaseOutcomeUnverified);
                    AssertEx.True(
                        scope.Configuration.IsReleaseOutcomeUnverified);

                    var secondCleanup = coordinator
                        .ReleaseQualificationConfigurationAndResolveAsync(
                            scope,
                            operations,
                            true,
                            CancellationToken.None);
                    AwaitRecorderManualDoubleFailure(
                        secondCleanup,
                        "Unverified manual Double cleanup was replayed.");
                    AssertEx.Equal(
                        1,
                        server.ReceivedRequests.Count(
                            request => TestFrame.ReadUInt16(request, 0)
                                == 0x7E48));
                    server.Verify();
                }
            }
            finally
            {
                CloseRecorderManualDoubleWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void
            ManualDoublePreArmValidationIsZeroConfigureAndNoJournal()
        {
            var capabilities = RecorderManualDoubleCapabilities();
            var steps = CreateConnectAndTopologySteps(capabilities, 2);
            steps.Add(CapabilitiesStep(11, capabilities, 2));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(journalDirectory, server.Port);
                    ConnectRecorderManualDoubleWindow(window);
                    var operation = StartRecorderManualDoubleOperation(
                        window,
                        RecorderManualDoubleInvalidCapacityConfiguration());
                    AwaitRecorderManualDoubleTask(
                        operation,
                        "Invalid manual Double pre-arm validation did not finish.");

                    AssertEx.Equal(
                        RecorderManualDoubleOperation + " failed",
                        window.TextOperationState.Text);
                    AssertEx.Contains(
                        "exceeds the connected PLC capability",
                        window.TextExecutionLog.Text);
                    var journal =
                        (RecorderDoubleRecoveryJournal)GetPrivateField(
                            window,
                            "recorderDoubleRecoveryJournal");
                    AssertEx.True(journal.CurrentRecord == null);
                    AssertEx.True(
                        GetPrivateField(
                            window,
                            "recorderDoubleRetainedQualificationScope")
                        == null);
                    AssertEx.Equal(
                        0,
                        server.ReceivedRequests.Count(
                            request => TestFrame.ReadUInt16(request, 0)
                                == 0x7E4C));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseRecorderManualDoubleWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static LMCDiagnosticCapability
            RecorderManualDoubleCapabilities()
        {
            return LMCDiagnosticCapability.EtherCATTopology
                | LMCDiagnosticCapability.RecorderSingleBank
                | LMCDiagnosticCapability.RecorderTrigger
                | LMCDiagnosticCapability.RecorderDoubleBank;
        }

        private static LMCRecorderConfiguration
            RecorderManualDoubleSourceConfiguration()
        {
            return new LMCRecorderConfiguration(
                new uint[]
                {
                    0x00100104u,
                    0x00100204u,
                    0x00100304u,
                    0x00100404u
                },
                1,
                1000,
                LMCRecorderBufferMode.Double,
                LMCRecorderTriggerType.Window,
                LMCSignalValueType.Int32,
                125,
                250,
                0x00100104u,
                LMCRecorderTriggerOperator.EnterWindow,
                unchecked((uint)-100),
                2000,
                0);
        }

        private static LMCRecorderConfiguration
            RecorderManualDoubleInvalidCapacityConfiguration()
        {
            var source = RecorderManualDoubleSourceConfiguration();
            return new LMCRecorderConfiguration(
                source.SignalIds,
                source.SamplePeriodCycles,
                1001,
                source.BufferMode,
                source.TriggerType,
                source.TriggerValueType,
                source.PreTriggerSamples,
                source.PostTriggerSamples,
                source.TriggerSignalId,
                source.TriggerOperator,
                source.TriggerValue,
                source.TriggerMask,
                source.RequestedConfigId);
        }

        private static byte[] RecorderManualDoubleConfigureResponse(
            uint requestId)
        {
            var payload = CommonPayload(72, requestId);
            TestFrame.WriteUInt32(payload, 16, 1);
            TestFrame.WriteUInt32(payload, 20, 1);
            TestFrame.WriteUInt32(
                payload,
                24,
                DiagnosticMapRevision);
            TestFrame.WriteUInt32(payload, 28, 1000);
            TestFrame.WriteUInt32(payload, 32, 32000);
            TestFrame.WriteUInt16(
                payload,
                36,
                (ushort)LMCRecorderState.Configured);
            TestFrame.WriteUInt16(payload, 38, 4);
            TestFrame.WriteUInt16(payload, 40, 16);
            TestFrame.WriteUInt16(payload, 42, 2);
            TestFrame.WriteUInt16(
                payload,
                44,
                (ushort)LMCCapturePhase.InputMapped);
            TestFrame.WriteUInt32(payload, 48, 1);
            TestFrame.WriteUInt32(payload, 52, DiagnosticsBootId);
            return TestFrame.Response(0, payload);
        }

        private static void BindAndAssertRecorderManualDoubleConfigure(
            byte[] request,
            byte[] response,
            uint requestId)
        {
            AssertEx.Equal(requestId, TestFrame.ReadUInt32(request, 12));
            AssertEx.Equal(
                DiagnosticMapRevision,
                TestFrame.ReadUInt32(request, 16));
            var requestedConfigId = TestFrame.ReadUInt32(request, 20);
            AssertEx.True(requestedConfigId != 0);
            AssertEx.Equal((ushort)1, TestFrame.ReadUInt16(request, 24));
            AssertEx.Equal((ushort)4, TestFrame.ReadUInt16(request, 26));
            AssertEx.Equal(1000u, TestFrame.ReadUInt32(request, 28));
            AssertEx.Equal(
                (byte)LMCRecorderBufferMode.Double,
                request[32]);
            AssertEx.Equal(
                (byte)LMCRecorderTriggerType.Window,
                request[33]);
            AssertEx.Equal(
                (byte)LMCSignalValueType.Int32,
                request[34]);
            AssertEx.Equal(125u, TestFrame.ReadUInt32(request, 36));
            AssertEx.Equal(250u, TestFrame.ReadUInt32(request, 40));
            AssertEx.Equal(0x00100104u, TestFrame.ReadUInt32(request, 44));
            AssertEx.Equal(
                (byte)LMCRecorderTriggerOperator.EnterWindow,
                request[48]);
            AssertEx.Equal(
                unchecked((uint)-100),
                TestFrame.ReadUInt32(request, 52));
            AssertEx.Equal(2000u, TestFrame.ReadUInt32(request, 56));
            AssertEx.Equal(
                DiagnosticsBootId,
                TestFrame.ReadUInt32(request, 60));
            AssertEx.Equal(0x00100104u, TestFrame.ReadUInt32(request, 80));
            AssertEx.Equal(0x00100204u, TestFrame.ReadUInt32(request, 84));
            AssertEx.Equal(0x00100304u, TestFrame.ReadUInt32(request, 88));
            AssertEx.Equal(0x00100404u, TestFrame.ReadUInt32(request, 92));

            var tokenBytes = new byte[16];
            Buffer.BlockCopy(request, 64, tokenBytes, 0, tokenBytes.Length);
            var token = new Guid(tokenBytes);
            AssertEx.True(token != Guid.Empty);
            AssertEx.Equal(
                requestedConfigId,
                MainWindow.CreateRecorderDoubleRequestedConfigId(token));

            TestFrame.WriteUInt32(response, 24, requestedConfigId);
            Buffer.BlockCopy(request, 64, response, 64, tokenBytes.Length);
        }

        private static void AssertRecorderManualDoubleReleaseRequest(
            byte[] request,
            uint requestId,
            RecorderDoubleBankConfigurationLease lease)
        {
            var native =
                (LMCRecorderConfigurationHandle)lease.NativeHandle;
            AssertEx.Equal(requestId, TestFrame.ReadUInt32(request, 12));
            AssertEx.Equal(lease.ConfigId, TestFrame.ReadUInt32(request, 16));
            AssertEx.Equal(
                lease.ConfigRevision,
                TestFrame.ReadUInt32(request, 20));
            AssertEx.Equal(
                DiagnosticMapRevision,
                TestFrame.ReadUInt32(request, 24));
            AssertEx.Equal(
                native.OwnerSessionEpoch,
                TestFrame.ReadUInt32(request, 28));
            AssertEx.Equal(
                DiagnosticsBootId,
                TestFrame.ReadUInt32(request, 32));
        }

        private static void AssertRecorderManualDoubleRetainedScope(
            MainWindow window,
            LMCRecorderConfiguration source,
            RecorderDoubleBankRecoveryScope scope,
            LMCRecorderConfigurationHandle native,
            RecorderDoubleRecoveryRecord record)
        {
            AssertEx.NotNull(scope);
            AssertEx.NotNull(scope.Configuration);
            AssertEx.True(scope.ConfigurationOnlyRetention);
            AssertEx.True(scope.ConfigurationAttempted);
            AssertEx.Equal("CONFIGURATION_RETAINED", scope.Stage);
            AssertEx.True(scope.BankA == null);
            AssertEx.True(scope.BankB == null);
            AssertEx.True(scope.UnexpectedThird == null);
            AssertEx.False(scope.BankAStartAttempted);
            AssertEx.False(scope.BankBStartAttempted);
            AssertEx.False(scope.ThirdStartAttempted);
            AssertEx.True(scope.HasValidConfigurationOnlyRetentionShape);
            AssertEx.True(scope.HasAnyPossibleResource);
            AssertEx.True(
                GetPrivateField(window, "recorderConfiguration") == null);
            AssertEx.True(
                MainWindow.IsRecorderDoubleSameSessionCleanupRouteReady(
                    scope,
                    true,
                    true,
                    false,
                    false));
            AssertEx.False(
                MainWindow.IsRecorderDoubleSameSessionCleanupRouteReady(
                    scope,
                    false,
                    false,
                    true,
                    true));

            var invalidConfigurationOnlyScope =
                new RecorderDoubleBankRecoveryScope(
                    scope.Request,
                    RecorderDoubleBankRecoveryScopeKind.ConfigurationOnly)
                {
                    BankAStartAttempted = true
                };
            AssertEx.False(
                invalidConfigurationOnlyScope
                    .HasValidConfigurationOnlyRetentionShape);
            AssertEx.False(
                MainWindow.IsRecorderDoubleSameSessionCleanupRouteReady(
                    invalidConfigurationOnlyScope,
                    true,
                    true,
                    false,
                    false));

            AssertEx.NotNull(native);
            AssertEx.False(native.IsReleased);
            AssertEx.True(native.IsRecoverable);
            AssertEx.Equal(scope.RecoveryToken, native.RecoveryToken);
            AssertEx.Equal((ushort)2, native.RecorderBufferCount);
            AssertEx.Equal((ushort)4, native.ChannelCount);
            AssertEx.Equal((ushort)16, native.SampleStrideBytes);
            AssertEx.Equal(1000u, native.AcceptedCapacity);
            AssertEx.Equal(32000u, native.ReservedDataBytes);
            AssertEx.Equal(1000u, native.SamplePeriodUs);
            AssertEx.Equal(0u, source.RequestedConfigId);
            AssertEx.True(native.Configuration.RequestedConfigId != 0);
            AssertEx.Equal(
                source.SamplePeriodCycles,
                native.Configuration.SamplePeriodCycles);
            AssertEx.Equal(
                source.SampleCapacity,
                native.Configuration.SampleCapacity);
            AssertEx.Equal(
                source.BufferMode,
                native.Configuration.BufferMode);
            AssertEx.Equal(
                source.TriggerType,
                native.Configuration.TriggerType);
            AssertEx.Equal(
                source.TriggerValueType,
                native.Configuration.TriggerValueType);
            AssertEx.Equal(
                source.PreTriggerSamples,
                native.Configuration.PreTriggerSamples);
            AssertEx.Equal(
                source.PostTriggerSamples,
                native.Configuration.PostTriggerSamples);
            AssertEx.Equal(
                source.TriggerSignalId,
                native.Configuration.TriggerSignalId);
            AssertEx.Equal(
                source.TriggerOperator,
                native.Configuration.TriggerOperator);
            AssertEx.Equal(
                source.TriggerValue,
                native.Configuration.TriggerValue);
            AssertEx.Equal(
                source.TriggerMask,
                native.Configuration.TriggerMask);
            AssertEx.Equal(
                source.SignalIds.Count,
                native.Configuration.SignalIds.Count);
            for (var index = 0; index < source.SignalIds.Count; index++)
            {
                AssertEx.Equal(
                    source.SignalIds[index],
                    native.Configuration.SignalIds[index]);
            }

            AssertEx.NotNull(record);
            AssertEx.Equal(
                RecorderDoubleRecoveryState.ConfigurationIdentified,
                record.State);
            AssertEx.Equal(scope.RecoveryToken, record.Identity);
            AssertEx.Equal(native.ConfigId, record.RequestedConfigId);
            AssertEx.Equal(native.ConfigRevision, record.ConfigRevision);
            AssertEx.Equal(0, record.Banks.Count);
            AssertEx.False(record.ConfigurationReleaseIntent);
            AssertEx.False(record.ConfigurationReleaseConfirmed);
        }

        private static void ConnectRecorderManualDoubleWindow(
            MainWindow window)
        {
            Click(window.ButtonConnect);
            WaitUntil(
                () => window.GridEtherCATTopology.Items.Count
                    == TopologyNodeCount,
                "Configured topology did not load before manual Double adapter test.");
        }

        private static Task StartRecorderManualDoubleOperation(
            MainWindow window,
            LMCRecorderConfiguration configuration)
        {
            var adapter = typeof(MainWindow).GetMethod(
                "ConfigureManualRecoverableDoubleRecorderAsync",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var runOperation = typeof(MainWindow).GetMethod(
                "RunOperationAsync",
                BindingFlags.Instance | BindingFlags.NonPublic);
            AssertEx.NotNull(adapter);
            AssertEx.NotNull(runOperation);
            Func<Task> action = () => (Task)adapter.Invoke(
                window,
                new object[] { configuration });
            return (Task)runOperation.Invoke(
                window,
                new object[]
                {
                    RecorderManualDoubleOperation,
                    action,
                    false
                });
        }

        private static void AwaitRecorderManualDoubleTask(
            Task task,
            string message)
        {
            AssertEx.NotNull(task);
            WaitUntil(() => task.IsCompleted, message);
            task.GetAwaiter().GetResult();
        }

        private static Exception AwaitRecorderManualDoubleFailure(
            Task task,
            string message)
        {
            AssertEx.NotNull(task);
            WaitUntil(() => task.IsCompleted, message);
            return AssertEx.Throws<Exception>(
                () => task.GetAwaiter().GetResult());
        }

        private static void CloseRecorderManualDoubleWindowBestEffort(
            MainWindow window)
        {
            if (window == null)
            {
                return;
            }

            CloseWindowBestEffort(window);
            if (window.IsLoaded)
            {
                try
                {
                    window.Close();
                    WaitUntil(
                        () => !window.IsLoaded,
                        "The manual Double test window did not close.",
                        2000);
                }
                catch
                {
                }
            }

            DisposeRecorderManualDoubleJournalBestEffort(
                window,
                "DisposeAxisPowerOnRecoveryJournal");
            DisposeRecorderManualDoubleJournalBestEffort(
                window,
                "DisposeGroupPowerRecoveryJournal");
            DisposeRecorderManualDoubleJournalBestEffort(
                window,
                "DisposeMotionUncertaintyJournal");
            DisposeRecorderManualDoubleJournalBestEffort(
                window,
                "DisposeGroupProfileLockRecoveryJournal");
            DisposeRecorderManualDoubleJournalBestEffort(
                window,
                "DisposeRecorderDoubleRecoveryJournal");
            DisposeRecorderManualDoubleJournalBestEffort(
                window,
                "DisposeDiagnosticsMutationJournal");
        }

        private static void DisposeRecorderManualDoubleJournalBestEffort(
            MainWindow window,
            string methodName)
        {
            try
            {
                InvokePrivate(window, methodName);
            }
            catch
            {
            }
        }
    }
}

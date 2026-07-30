using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlApiExample;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class DiagnosticsRecorderContractTests
    {
        private const uint GoldenRequestId = 0x11223344u;
        private const uint MapRevision = 0x957F101Eu;
        private const uint DiagnosticsBootId = 0x89ABCDEFu;
        private const uint ConfigId = 0x10203040u;
        private const uint ConfigRevision = 0x01020304u;
        private const uint OwnerSessionEpoch = 0x55667788u;
        private const uint ReconnectedOwnerSessionEpoch = 0x66778899u;
        private const uint RecordId = 0xA1B2C3D4u;
        private const uint Signal1 = 0x00100104u;
        private const uint Signal2 = 0x00100105u;

        private static readonly uint[] Signals = { Signal1, Signal2 };
        private static readonly Guid RecoveryToken = new Guid(
            "00112233-4455-6677-8899-aabbccddeeff");
        private static readonly Guid OtherRecoveryToken = new Guid(
            "10213243-5465-7687-98a9-bacbdcedfe0f");

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Recorder.Configuration.D3AndD4Validation",
                RecorderConfigurationD3AndD4Validation);
            tests.Add(
                "Recorder.Capability.ChunkCarriesWholeSample",
                RecorderCapabilityChunkCarriesWholeSample);
            tests.Add(
                "Recorder.Request.GoldenAndBounds",
                RecorderRequestGoldenAndBounds);
            tests.Add(
                "Recorder.Recoverable.RequestParserAndLegacyDoubleReject",
                RecorderRecoverableRequestParserAndLegacyDoubleReject);
            tests.Add(
                "Recorder.Recoverable.ParserFailClosed",
                RecorderRecoverableParserFailClosed);
            tests.Add(
                "Rpc.Recorder.Recoverable.SyncAndAsync",
                RecorderRecoverableSyncAndAsync);
            tests.Add(
                "Rpc.Recorder.Recoverable.PinnedCapabilities",
                RecorderRecoverablePinnedCapabilities);
            tests.Add(
                "Rpc.Recorder.Recoverable.TypedAbsenceSyncAndAsync",
                RecorderRecoverableTypedAbsenceSyncAndAsync);
            tests.Add(
                "Rpc.Recorder.Recoverable.CapabilityZeroWire",
                RecorderRecoverableCapabilityZeroWire);
            tests.Add(
                "Recorder.Inventory.RequestAndParser",
                RecorderBankInventoryRequestAndParser);
            tests.Add(
                "Recorder.Inventory.ParserFailClosed",
                RecorderBankInventoryParserFailClosed);
            tests.Add(
                "Recorder.Inventory.TypedConfigurationAbsence",
                RecorderBankInventoryTypedConfigurationAbsence);
            tests.Add(
                "Rpc.Recorder.Inventory.SyncAndAsync",
                RecorderBankInventorySyncAndAsync);
            tests.Add(
                "Rpc.Recorder.Inventory.TypedAbsenceSyncAndAsync",
                RecorderBankInventoryTypedAbsenceSyncAndAsync);
            tests.Add(
                "Recorder.EmptyConfigurationAdoption.RequestAndParser",
                RecorderEmptyConfigurationAdoptionRequestAndParser);
            tests.Add(
                "Rpc.Recorder.EmptyConfigurationAdoption.RecoverableInventoryZeroWire",
                RecorderRecoverableInventoryAdoptionZeroWire);
            tests.Add(
                "Rpc.Recorder.EmptyConfigurationAdoption.SyncAsyncRelease",
                RecorderEmptyConfigurationAdoptionSyncAsyncRelease);
            tests.Add(
                "Rpc.Recorder.EmptyConfigurationAdoption.ResponseLossReconnectReadopt",
                RecorderEmptyConfigurationAdoptionResponseLossReconnectReadopt);
            tests.Add(
                "Recorder.Response.ConfigureStartStatus",
                RecorderConfigureStartStatus);
            tests.Add(
                "Recorder.Response.HeaderTriggerAndMalformed",
                RecorderHeaderTriggerAndMalformed);
            tests.Add(
                "Recorder.Response.ChunkCrcSequenceAndFlags",
                RecorderChunkCrcSequenceAndFlags);
            tests.Add(
                "Recorder.Response.ReleaseAndAdopt",
                RecorderReleaseAndAdopt);
            tests.Add(
                "Rpc.Recorder.BufferRelease.ResponseLossFailClosed",
                RecorderBufferReleaseResponseLossFailClosed);
            tests.Add(
                "Rpc.Recorder.ConfigurationRelease.ResponseLossFailClosed",
                RecorderConfigurationReleaseResponseLossFailClosed);
            tests.Add(
                "Rpc.Recorder.AdoptedRelease.ResponseLossFailClosed",
                RecorderAdoptedReleaseResponseLossFailClosed);
            tests.Add(
                "Rpc.Recorder.RecoveredRelease.ResponseLossFailClosed",
                RecorderRecoveredReleaseResponseLossFailClosed);
            tests.Add(
                "Rpc.Recorder.BufferRelease.CommandRejectionRetry",
                RecorderBufferReleaseCommandRejectionRetry);
            tests.Add(
                "Rpc.Recorder.SyncAndAsync",
                RecorderSyncAndAsync);
            tests.Add(
                "Rpc.Recorder.SingleWorkerDownload",
                RecorderSingleWorkerDownload);
            tests.Add(
                "Rpc.Recorder.AdoptCleanup",
                RecorderAdoptCleanup);
            tests.Add(
                "Rpc.Recorder.AdoptActive",
                RecorderAdoptActive);
            tests.Add(
                "Rpc.Recorder.ReconnectExactAndDiscovery",
                RecorderReconnectExactAndDiscovery);
            tests.Add(
                "Rpc.Recorder.PreCloseTransportFaultExactRecovery",
                RecorderPreCloseTransportFaultExactRecovery);
            tests.Add(
                "Rpc.Recorder.FaultPreservesWithoutMutation",
                RecorderFaultPreservesWithoutMutation);
            tests.Add(
                "Qualification.Recorder.PreStartCancellationCleanup",
                RecorderPreStartCancellationCleanup);
            tests.Add(
                "Qualification.Recorder.ActiveCancellationCleanup",
                RecorderActiveCancellationCleanup);
            tests.Add(
                "Qualification.Recorder.UploadingCancellationCleanup",
                RecorderUploadingCancellationCleanup);
            tests.Add(
                "Qualification.Recorder.StopInvalidStateReadyRace",
                RecorderStopInvalidStateReadyRace);
            tests.Add(
                "Qualification.Recorder.StopInvalidStateFaultPreserves",
                RecorderStopInvalidStateFaultPreserves);
            tests.Add(
                "Qualification.Recorder.BufferReleaseFailureBlocksConfig",
                RecorderBufferReleaseFailureBlocksConfig);
            tests.Add(
                "Qualification.Recorder.ConfigReleaseFailureRetry",
                RecorderConfigReleaseFailureRetry);
            tests.Add(
                "Qualification.Recorder.FaultNoMutationProductionPath",
                RecorderFaultNoMutationProductionPath);
            tests.Add(
                "Qualification.Recorder.ReconnectStatusMismatchNoMutation",
                RecorderReconnectStatusMismatchNoMutation);
            tests.Add(
                "Qualification.Recorder.UnvalidatedAdoptionQuarantined",
                RecorderUnvalidatedAdoptionQuarantined);
            tests.Add(
                "Qualification.Recorder.AdoptedConfigReleaseFailureRetry",
                RecorderAdoptedConfigReleaseFailureRetry);
            tests.Add(
                "Qualification.Recorder.PostRpcCancellationAggregatesCleanupFailure",
                RecorderPostRpcCancellationAggregatesCleanupFailure);
            tests.Add(
                "Rpc.Recorder.StatefulCancellationBoundary",
                RecorderStatefulCancellationBoundary);
            tests.Add(
                "Rpc.Recorder.BootIdMismatchInvalidatesHandles",
                RecorderBootIdMismatchInvalidatesHandles);
        }

        private static void RecorderConfigurationD3AndD4Validation()
        {
            var callerSignals = (uint[])Signals.Clone();
            var manual = new LMCRecorderConfiguration(
                callerSignals,
                2,
                100);
            callerSignals[0] = 0;
            AssertEx.Equal(Signal1, manual.SignalIds[0]);
            AssertEx.Equal((ushort)2, manual.ChannelCount);
            AssertEx.Equal(LMCRecorderBufferMode.Single, manual.BufferMode);
            AssertEx.Equal(LMCRecorderTriggerType.Manual, manual.TriggerType);
            AssertEx.False(manual.RequiresTriggerCapability);
            AssertEx.False(manual.RequiresDoubleBankCapability);

            var edge = TriggerConfiguration(
                LMCRecorderBufferMode.Ring,
                LMCRecorderTriggerType.Edge,
                LMCRecorderTriggerOperator.RisingEdge,
                0);
            AssertEx.True(edge.RequiresTriggerCapability);

            var window = TriggerConfiguration(
                LMCRecorderBufferMode.Double,
                LMCRecorderTriggerType.Window,
                LMCRecorderTriggerOperator.EnterWindow,
                200);
            AssertEx.Equal(100u, window.TriggerLowerBound);
            AssertEx.Equal(200u, window.TriggerUpperBound);

            var mask = TriggerConfiguration(
                LMCRecorderBufferMode.Double,
                LMCRecorderTriggerType.Mask,
                LMCRecorderTriggerOperator.MaskAnySet,
                0x0000000Fu);
            AssertEx.Equal(0u, mask.TriggerValue);
            AssertEx.Equal(0x0000000Fu, mask.TriggerMask);

            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new LMCRecorderConfiguration(Signals, 0, 10));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new LMCRecorderConfiguration(Signals, 1, 0));
            AssertEx.Throws<ArgumentException>(
                () => new LMCRecorderConfiguration(
                    new[] { Signal1, Signal1 },
                    1,
                    10));
            AssertEx.Throws<ArgumentException>(
                () => new LMCRecorderConfiguration(
                    Signals,
                    1,
                    10,
                    LMCRecorderBufferMode.Single,
                    LMCRecorderTriggerType.Manual,
                    LMCSignalValueType.Invalid,
                    0,
                    0,
                    0,
                    LMCRecorderTriggerOperator.None,
                    1,
                    0));
            AssertEx.Throws<ArgumentException>(
                () => new LMCRecorderConfiguration(
                    Signals,
                    1,
                    10,
                    LMCRecorderBufferMode.Ring,
                    LMCRecorderTriggerType.Manual,
                    LMCSignalValueType.Invalid,
                    0,
                    0,
                    0,
                    LMCRecorderTriggerOperator.None,
                    0,
                    0));
            AssertEx.Throws<ArgumentException>(
                () => TriggerConfiguration(
                    LMCRecorderBufferMode.Single,
                    LMCRecorderTriggerType.Edge,
                    LMCRecorderTriggerOperator.RisingEdge,
                    0));
            AssertEx.Throws<ArgumentException>(
                () => TriggerConfiguration(
                    LMCRecorderBufferMode.Ring,
                    LMCRecorderTriggerType.Edge,
                    LMCRecorderTriggerOperator.EnterWindow,
                    0));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => TriggerConfiguration(
                    LMCRecorderBufferMode.Ring,
                    LMCRecorderTriggerType.Mask,
                    LMCRecorderTriggerOperator.MaskAllSet,
                    0));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new LMCRecorderConfiguration(
                    Signals,
                    1,
                    10,
                    LMCRecorderBufferMode.Ring,
                    LMCRecorderTriggerType.Mask,
                    LMCSignalValueType.BitField32,
                    4,
                    5,
                    Signal1,
                    LMCRecorderTriggerOperator.MaskAnySet,
                    1,
                    0x0F));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new LMCRecorderConfiguration(
                    Signals,
                    1,
                    10,
                    LMCRecorderBufferMode.Ring,
                    LMCRecorderTriggerType.Edge,
                    LMCSignalValueType.Int32,
                    5,
                    5,
                    Signal1,
                    LMCRecorderTriggerOperator.RisingEdge,
                    0,
                    0));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new LMCRecorderConfiguration(
                    Signals,
                    1,
                    10,
                    LMCRecorderBufferMode.Ring,
                    LMCRecorderTriggerType.Edge,
                    LMCSignalValueType.Int8,
                    1,
                    1,
                    Signal1,
                    LMCRecorderTriggerOperator.RisingEdge,
                    0,
                    0));
        }

        private static void RecorderCapabilityChunkCarriesWholeSample()
        {
            var capabilities = CapabilitiesPayload(1);
            TestFrame.WriteUInt16(capabilities, 48, 4);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, capabilities)),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                AssertEx.Throws<InvalidDataException>(
                    () => connection.Diagnostics.ConfigureRecorder(
                        ManualConfiguration()));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RecorderRequestGoldenAndBounds()
        {
            var configuration = ManualConfiguration();
            var configure = LMC_DiagnosticsFrame.ConfigureRecorder(
                GoldenRequestId,
                MapRevision,
                configuration,
                DiagnosticsBootId);
            AssertRequestHeader(configure, 0x7E40, 64, GoldenRequestId);
            AssertEx.Equal(MapRevision, TestFrame.ReadUInt32(configure, 16));
            AssertEx.Equal((ushort)1, TestFrame.ReadUInt16(configure, 24));
            AssertEx.Equal((ushort)2, TestFrame.ReadUInt16(configure, 26));
            AssertEx.Equal(3u, TestFrame.ReadUInt32(configure, 28));
            AssertEx.Equal((byte)0, configure[32]);
            AssertEx.Equal((byte)0, configure[33]);
            AssertEx.Equal(DiagnosticsBootId, TestFrame.ReadUInt32(configure, 60));
            AssertEx.Equal(Signal1, TestFrame.ReadUInt32(configure, 64));
            AssertEx.Equal(Signal2, TestFrame.ReadUInt32(configure, 68));

            var handle = ConfigurationHandle(configuration);
            var start = LMC_DiagnosticsFrame.StartRecorder(
                GoldenRequestId,
                handle);
            AssertRequestHeader(start, 0x7E41, 28, GoldenRequestId);
            AssertEx.Equal(ConfigId, TestFrame.ReadUInt32(start, 16));
            AssertEx.Equal(ConfigRevision, TestFrame.ReadUInt32(start, 20));
            AssertEx.Equal(MapRevision, TestFrame.ReadUInt32(start, 24));
            AssertEx.Equal(OwnerSessionEpoch, TestFrame.ReadUInt32(start, 28));
            AssertEx.Equal(DiagnosticsBootId, TestFrame.ReadUInt32(start, 32));

            var identity = RecorderIdentity();
            AssertRecorderIdentityRequest(
                0x7E43,
                LMC_DiagnosticsFrame.StopRecorder(GoldenRequestId, identity));
            AssertRecorderIdentityRequest(
                0x7E44,
                LMC_DiagnosticsFrame.ReadRecorderStatus(
                    GoldenRequestId,
                    identity));
            AssertRecorderIdentityRequest(
                0x7E45,
                LMC_DiagnosticsFrame.ReadRecorderHeader(
                    GoldenRequestId,
                    identity));
            AssertRecorderIdentityRequest(
                0x7E47,
                LMC_DiagnosticsFrame.ReleaseRecorderBuffer(
                    GoldenRequestId,
                    identity));

            var chunk = LMC_DiagnosticsFrame.ReadRecorderChunk(
                GoldenRequestId,
                new LMCRecorderChunkRequest(identity, 4, 2, 99));
            AssertRequestHeader(chunk, 0x7E46, 32, GoldenRequestId);
            AssertEx.Equal(RecordId, TestFrame.ReadUInt32(chunk, 16));
            AssertEx.Equal(0u, TestFrame.ReadUInt32(chunk, 20));
            AssertEx.Equal(4u, TestFrame.ReadUInt32(chunk, 24));
            AssertEx.Equal((ushort)2, TestFrame.ReadUInt16(chunk, 28));
            AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(chunk, 30));
            AssertEx.Equal(99u, TestFrame.ReadUInt32(chunk, 32));
            AssertEx.Equal(DiagnosticsBootId, TestFrame.ReadUInt32(chunk, 36));

            var release = LMC_DiagnosticsFrame.ReleaseRecorder(
                GoldenRequestId,
                handle);
            AssertRequestHeader(release, 0x7E48, 28, GoldenRequestId);

            var adopt = LMC_DiagnosticsFrame.AdoptRecorder(
                GoldenRequestId,
                DiagnosticsBootId,
                RecordId,
                0);
            AssertRequestHeader(adopt, 0x7E49, 20, GoldenRequestId);
            AssertEx.Equal(RecordId, TestFrame.ReadUInt32(adopt, 16));
            AssertEx.Equal(DiagnosticsBootId, TestFrame.ReadUInt32(adopt, 24));

            var adoptActive = LMC_DiagnosticsFrame.AdoptActiveRecorder(
                GoldenRequestId,
                DiagnosticsBootId);
            AssertRequestHeader(adoptActive, 0x7E49, 20, GoldenRequestId);
            AssertEx.Equal(0u, TestFrame.ReadUInt32(adoptActive, 16));
            AssertEx.Equal(0u, TestFrame.ReadUInt32(adoptActive, 20));
            AssertEx.Equal(
                DiagnosticsBootId,
                TestFrame.ReadUInt32(adoptActive, 24));

            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.ConfigureRecorder(
                    GoldenRequestId,
                    0,
                    configuration,
                    DiagnosticsBootId));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.AdoptRecorder(
                    GoldenRequestId,
                    0,
                    RecordId,
                    0));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.AdoptRecorder(
                    GoldenRequestId,
                    DiagnosticsBootId,
                    0,
                    0));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.AdoptActiveRecorder(
                    GoldenRequestId,
                    0));
        }

        private static void
            RecorderRecoverableRequestParserAndLegacyDoubleReject()
        {
            var configuration = RecoverableDoubleConfiguration();
            AssertEx.Equal(
                (ushort)0x7E4C,
                LMC_CommandId.ConfigureRecoverableDoubleRecorder);
            AssertEx.Equal(
                (ushort)0x7E4D,
                LMC_CommandId.ReadRecoverableRecorderBankInventory);

            AssertEx.Throws<NotSupportedException>(
                () => LMC_DiagnosticsFrame.ConfigureRecorder(
                    GoldenRequestId,
                    MapRevision,
                    configuration,
                    DiagnosticsBootId));

            var configure = LMC_DiagnosticsFrame
                .ConfigureRecoverableDoubleRecorder(
                    GoldenRequestId,
                    MapRevision,
                    configuration,
                    DiagnosticsBootId,
                    RecoveryToken);
            AssertRequestHeader(configure, 0x7E4C, 80, GoldenRequestId);
            AssertEx.Equal(MapRevision, TestFrame.ReadUInt32(configure, 16));
            AssertEx.Equal(ConfigId, TestFrame.ReadUInt32(configure, 20));
            AssertEx.Equal((ushort)1, TestFrame.ReadUInt16(configure, 24));
            AssertEx.Equal((ushort)2, TestFrame.ReadUInt16(configure, 26));
            AssertEx.Equal((byte)LMCRecorderBufferMode.Double, configure[32]);
            AssertEx.Equal(DiagnosticsBootId, TestFrame.ReadUInt32(configure, 60));
            AssertRecoveryTokenBytes(configure, 64, RecoveryToken);
            AssertEx.Equal(Signal1, TestFrame.ReadUInt32(configure, 80));
            AssertEx.Equal(Signal2, TestFrame.ReadUInt32(configure, 84));

            var inventoryRequest = LMC_DiagnosticsFrame
                .ReadRecoverableRecorderBankInventory(
                    GoldenRequestId,
                    DiagnosticsBootId,
                    ConfigId,
                    MapRevision,
                    RecoveryToken);
            AssertRequestHeader(
                inventoryRequest,
                0x7E4D,
                36,
                GoldenRequestId);
            AssertEx.Equal(
                DiagnosticsBootId,
                TestFrame.ReadUInt32(inventoryRequest, 16));
            AssertEx.Equal(
                ConfigId,
                TestFrame.ReadUInt32(inventoryRequest, 20));
            AssertEx.Equal(
                MapRevision,
                TestFrame.ReadUInt32(inventoryRequest, 24));
            AssertRecoveryTokenBytes(
                inventoryRequest,
                28,
                RecoveryToken);

            var handle = LMC_DiagnosticsParser
                .ParseConfigureRecoverableDoubleRecorder(
                    TestFrame.Response(
                        0,
                        RecoverableConfigurePayload(
                            GoldenRequestId,
                            RecoveryToken)),
                    GoldenRequestId,
                    configuration,
                    RecoveryToken,
                    Capabilities(7),
                    7,
                    null);
            AssertEx.True(handle.IsRecoverable);
            AssertEx.Equal(RecoveryToken, handle.RecoveryToken);
            AssertEx.Equal((ushort)2, handle.RecorderBufferCount);
            AssertEx.Equal(ConfigRevision, handle.ConfigRevision);

            var inventory = LMC_DiagnosticsParser
                .ParseRecoverableRecorderBankInventory(
                    TestFrame.Response(
                        0,
                        RecoverableRecorderBankInventoryPayload(
                            GoldenRequestId,
                            RecoveryToken)),
                    GoldenRequestId,
                    DiagnosticsBootId,
                    ConfigId,
                    MapRevision,
                    RecoveryToken);
            AssertEx.True(inventory.IsRecoverable);
            AssertEx.Equal(RecoveryToken, inventory.RecoveryToken);
            AssertEx.Equal(ConfigRevision, inventory.ConfigRevision);
            AssertEx.Equal(0, inventory.OccupiedBanks.Count);
            AssertEx.Throws<ArgumentException>(
                () => LMC_DiagnosticsFrame
                    .AdoptEmptyRecorderConfiguration(
                        GoldenRequestId,
                        inventory));
            AssertEx.Throws<ArgumentException>(
                () => LMC_DiagnosticsParser
                    .ParseAdoptEmptyRecorderConfiguration(
                        TestFrame.Response(
                            0,
                            AdoptEmptyRecorderConfigurationPayload(
                                GoldenRequestId)),
                        GoldenRequestId,
                        inventory,
                        7,
                        null));

            AssertEx.Throws<ArgumentException>(
                () => LMC_DiagnosticsFrame
                    .ConfigureRecoverableDoubleRecorder(
                        GoldenRequestId,
                        MapRevision,
                        ManualConfiguration(),
                        DiagnosticsBootId,
                        RecoveryToken));
            AssertEx.Throws<ArgumentException>(
                () => LMC_DiagnosticsFrame
                    .ConfigureRecoverableDoubleRecorder(
                        GoldenRequestId,
                        MapRevision,
                        TriggerConfiguration(
                            LMCRecorderBufferMode.Double,
                            LMCRecorderTriggerType.Edge,
                            LMCRecorderTriggerOperator.RisingEdge,
                            0),
                        DiagnosticsBootId,
                        RecoveryToken));
            AssertEx.Throws<ArgumentException>(
                () => LMC_DiagnosticsFrame
                    .ConfigureRecoverableDoubleRecorder(
                        GoldenRequestId,
                        MapRevision,
                        configuration,
                        DiagnosticsBootId,
                        Guid.Empty));
            AssertEx.Throws<ArgumentException>(
                () => LMC_DiagnosticsFrame
                    .ReadRecoverableRecorderBankInventory(
                        GoldenRequestId,
                        DiagnosticsBootId,
                        ConfigId,
                        MapRevision,
                        Guid.Empty));
        }

        private static void RecorderRecoverableParserFailClosed()
        {
            var configuration = RecoverableDoubleConfiguration();
            var wrongConfigureToken = RecoverableConfigurePayload(
                GoldenRequestId,
                OtherRecoveryToken);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser
                    .ParseConfigureRecoverableDoubleRecorder(
                        TestFrame.Response(0, wrongConfigureToken),
                        GoldenRequestId,
                        configuration,
                        RecoveryToken,
                        Capabilities(7),
                        7,
                        null));
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser
                    .ParseConfigureRecoverableDoubleRecorder(
                        TestFrame.Response(
                            0,
                            CommonPayload(71, GoldenRequestId)),
                        GoldenRequestId,
                        configuration,
                        RecoveryToken,
                        Capabilities(7),
                        7,
                        null));
            AssertEx.Throws<ArgumentException>(
                () => LMC_DiagnosticsParser
                    .ParseConfigureRecoverableDoubleRecorder(
                        TestFrame.Response(
                            0,
                            RecoverableConfigurePayload(
                                GoldenRequestId,
                                RecoveryToken)),
                        GoldenRequestId,
                        configuration,
                        Guid.Empty,
                        Capabilities(7),
                        7,
                        null));
            var wrongInventoryToken =
                RecoverableRecorderBankInventoryPayload(
                    GoldenRequestId,
                    OtherRecoveryToken);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser
                    .ParseRecoverableRecorderBankInventory(
                        TestFrame.Response(0, wrongInventoryToken),
                        GoldenRequestId,
                        DiagnosticsBootId,
                        ConfigId,
                        MapRevision,
                        RecoveryToken));
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser
                    .ParseRecoverableRecorderBankInventory(
                        TestFrame.Response(
                            0,
                            RecorderBankInventoryPayload(
                                GoldenRequestId,
                                0,
                                LMCRecorderState.Configured,
                                OwnerSessionEpoch)),
                        GoldenRequestId,
                        DiagnosticsBootId,
                        ConfigId,
                        MapRevision,
                        RecoveryToken));
            AssertEx.Throws<ArgumentException>(
                () => LMC_DiagnosticsParser
                    .ParseRecoverableRecorderBankInventory(
                        TestFrame.Response(
                            0,
                            RecoverableRecorderBankInventoryPayload(
                                GoldenRequestId,
                                RecoveryToken)),
                        GoldenRequestId,
                        DiagnosticsBootId,
                        ConfigId,
                        MapRevision,
                        Guid.Empty));

            var absence = AssertEx.Throws<
                LMCRecoverableRecorderConfigurationAbsentException>(
                    () => LMC_DiagnosticsParser
                        .ParseRecoverableRecorderBankInventory(
                            TestFrame.Response(
                                0,
                                DomainErrorPayload(
                                    GoldenRequestId,
                                    LMCDiagnosticsDetailCode
                                        .RecorderConfigurationAbsent)),
                            GoldenRequestId,
                            DiagnosticsBootId,
                            ConfigId,
                            MapRevision,
                            RecoveryToken));
            AssertEx.Equal(DiagnosticsBootId, absence.DiagnosticsBootId);
            AssertEx.Equal(ConfigId, absence.ConfigId);
            AssertEx.Equal(MapRevision, absence.MapRevision);
            AssertEx.Equal(RecoveryToken, absence.RecoveryToken);
            AssertEx.Equal(
                LMCDiagnosticsDetailCode.RecorderConfigurationAbsent,
                absence.Response.Detail);
        }

        private static void RecorderRecoverableSyncAndAsync()
        {
            RunRecoverableConfigure(false);
            RunRecoverableConfigure(true);
            RunRecoverableInventory(false);
            RunRecoverableInventory(true);
        }

        private static void RunRecoverableConfigure(bool useAsync)
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E4C,
                    TestFrame.Response(
                        0,
                        RecoverableConfigurePayload(2, RecoveryToken))),
                new FakeRpcStep(
                    0x7E48,
                    TestFrame.Response(0, CommonPayload(16, 3))),
                CloseStep()))
            using (var connection = ConnectRecorder(server))
            {
                LMCRecorderConfigurationHandle handle;
                if (useAsync)
                {
                    handle = connection.Diagnostics
                        .ConfigureRecoverableDoubleRecorderAsync(
                            RecoverableDoubleConfiguration(),
                            RecoveryToken,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }
                else
                {
                    handle = connection.Diagnostics
                        .ConfigureRecoverableDoubleRecorder(
                            RecoverableDoubleConfiguration(),
                            RecoveryToken);
                }

                AssertEx.True(handle.IsRecoverable);
                AssertEx.Equal(RecoveryToken, handle.RecoveryToken);
                AssertEx.Equal((ushort)2, handle.RecorderBufferCount);
                if (useAsync)
                {
                    connection.Diagnostics.ReleaseRecorderAsync(
                            handle,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }
                else
                {
                    connection.Diagnostics.ReleaseRecorder(handle);
                }

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RecorderRecoverablePinnedCapabilities()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E4C,
                    TestFrame.Response(
                        0,
                        RecoverableConfigurePayload(2, RecoveryToken))),
                new FakeRpcStep(
                    0x7E48,
                    TestFrame.Response(0, CommonPayload(16, 3))),
                CloseStep()))
            using (var connection = ConnectRecorder(server))
            {
                var configuration = RecoverableDoubleConfiguration();
                var capabilities = connection.Diagnostics.GetCapabilities();
                connection.Diagnostics
                    .ValidateRecoverableDoubleRecorderConfiguration(
                        configuration,
                        RecoveryToken,
                        capabilities);
                var handle = connection.Diagnostics
                    .ConfigureRecoverableDoubleRecorderAsync(
                        configuration,
                        RecoveryToken,
                        capabilities,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.True(handle.IsRecoverable);
                AssertEx.Equal(RecoveryToken, handle.RecoveryToken);
                AssertEx.Equal(
                    1,
                    CountReceivedCommand(server, 0x7E00));
                AssertEx.Equal(
                    1,
                    CountReceivedCommand(server, 0x7E4C));

                connection.Diagnostics.ReleaseRecorder(handle);
                connection.CloseConnection();
                server.Verify();
            }

            AssertRecoverablePinnedForeignCapabilitiesAreZeroWire();
        }

        private static void
            AssertRecoverablePinnedForeignCapabilitiesAreZeroWire()
        {
            using (var ownerServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                CloseStep()))
            using (var foreignServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var owner = ConnectRecorder(ownerServer))
            using (var foreign = ConnectRecorder(foreignServer))
            {
                var configuration = RecoverableDoubleConfiguration();
                var capabilities = owner.Diagnostics.GetCapabilities();
                AssertEx.Throws<InvalidOperationException>(
                    () => foreign.Diagnostics
                        .ValidateRecoverableDoubleRecorderConfiguration(
                            configuration,
                            RecoveryToken,
                            capabilities));
                AssertEx.Throws<InvalidOperationException>(
                    () => foreign.Diagnostics
                        .ConfigureRecoverableDoubleRecorderAsync(
                            configuration,
                            RecoveryToken,
                            capabilities,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(
                    0,
                    CountReceivedCommand(foreignServer, 0x7E00));
                AssertEx.Equal(
                    0,
                    CountReceivedCommand(foreignServer, 0x7E4C));

                foreign.CloseConnection();
                owner.CloseConnection();
                foreignServer.Verify();
                ownerServer.Verify();
            }
        }

        private static void RunRecoverableInventory(bool useAsync)
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E4D,
                    TestFrame.Response(
                        0,
                        RecoverableRecorderBankInventoryPayload(
                            2,
                            RecoveryToken))),
                CloseStep()))
            using (var connection = ConnectRecorder(server))
            {
                LMCRecorderBankInventory inventory;
                if (useAsync)
                {
                    inventory = connection.Diagnostics
                        .ReadRecoverableRecorderBankInventoryAsync(
                            DiagnosticsBootId,
                            ConfigId,
                            MapRevision,
                            RecoveryToken,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }
                else
                {
                    inventory = connection.Diagnostics
                        .ReadRecoverableRecorderBankInventory(
                            DiagnosticsBootId,
                            ConfigId,
                            MapRevision,
                            RecoveryToken);
                }

                AssertEx.True(inventory.IsRecoverable);
                AssertEx.Equal(RecoveryToken, inventory.RecoveryToken);
                AssertEx.Equal(ConfigRevision, inventory.ConfigRevision);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RecorderRecoverableTypedAbsenceSyncAndAsync()
        {
            RunRecoverableTypedAbsence(false);
            RunRecoverableTypedAbsence(true);
        }

        private static void RunRecoverableTypedAbsence(bool useAsync)
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E4D,
                    TestFrame.Response(
                        0,
                        DomainErrorPayload(
                            2,
                            LMCDiagnosticsDetailCode
                                .RecorderConfigurationAbsent))),
                CloseStep()))
            using (var connection = ConnectRecorder(server))
            {
                Action read = () =>
                {
                    if (useAsync)
                    {
                        connection.Diagnostics
                            .ReadRecoverableRecorderBankInventoryAsync(
                                DiagnosticsBootId,
                                ConfigId,
                                MapRevision,
                                RecoveryToken,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult();
                    }
                    else
                    {
                        connection.Diagnostics
                            .ReadRecoverableRecorderBankInventory(
                                DiagnosticsBootId,
                                ConfigId,
                                MapRevision,
                                RecoveryToken);
                    }
                };

                var absence = AssertEx.Throws<
                    LMCRecoverableRecorderConfigurationAbsentException>(read);
                AssertEx.Equal(RecoveryToken, absence.RecoveryToken);
                AssertEx.Equal(DiagnosticsBootId, absence.DiagnosticsBootId);
                AssertEx.Equal(ConfigId, absence.ConfigId);
                AssertEx.Equal(MapRevision, absence.MapRevision);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RecorderRecoverableCapabilityZeroWire()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        SingleBankCapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        SingleBankCapabilitiesPayload(2))),
                CloseStep()))
            using (var connection = ConnectRecorder(server))
            {
                var configuration = RecoverableDoubleConfiguration();
                AssertEx.Throws<NotSupportedException>(
                    () => connection.Diagnostics.ConfigureRecorder(
                        configuration));
                AssertEx.Throws<ArgumentException>(
                    () => connection.Diagnostics
                        .ConfigureRecoverableDoubleRecorder(
                            configuration,
                            Guid.Empty));
                AssertEx.Throws<ArgumentException>(
                    () => connection.Diagnostics
                        .ConfigureRecoverableDoubleRecorder(
                            ManualConfiguration(),
                            RecoveryToken));
                AssertEx.Throws<NotSupportedException>(
                    () => connection.Diagnostics
                        .ConfigureRecoverableDoubleRecorder(
                            configuration,
                            RecoveryToken));
                AssertEx.Throws<NotSupportedException>(
                    () => connection.Diagnostics
                        .ReadRecoverableRecorderBankInventory(
                            DiagnosticsBootId,
                            ConfigId,
                            MapRevision,
                            RecoveryToken));

                AssertEx.Equal(0, CountReceivedCommand(server, 0x7E40));
                AssertEx.Equal(0, CountReceivedCommand(server, 0x7E4C));
                AssertEx.Equal(0, CountReceivedCommand(server, 0x7E4D));
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RecorderBankInventoryRequestAndParser()
        {
            var request = LMC_DiagnosticsFrame.ReadRecorderBankInventory(
                GoldenRequestId,
                DiagnosticsBootId,
                ConfigId,
                MapRevision,
                0);
            AssertRequestHeader(request, 0x7E4A, 24, GoldenRequestId);
            AssertEx.Equal(
                DiagnosticsBootId,
                TestFrame.ReadUInt32(request, 16));
            AssertEx.Equal(ConfigId, TestFrame.ReadUInt32(request, 20));
            AssertEx.Equal(MapRevision, TestFrame.ReadUInt32(request, 24));
            AssertEx.Equal(0u, TestFrame.ReadUInt32(request, 28));

            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.ReadRecorderBankInventory(
                    GoldenRequestId,
                    0,
                    ConfigId,
                    MapRevision,
                    0));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.ReadRecorderBankInventory(
                    GoldenRequestId,
                    DiagnosticsBootId,
                    0,
                    MapRevision,
                    0));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.ReadRecorderBankInventory(
                    GoldenRequestId,
                    DiagnosticsBootId,
                    ConfigId,
                    0,
                    0));

            var inventory = LMC_DiagnosticsParser.ParseRecorderBankInventory(
                TestFrame.Response(
                    0,
                    RecorderBankInventoryPayload(
                        GoldenRequestId,
                        2,
                        LMCRecorderState.Ready,
                        0)),
                GoldenRequestId,
                DiagnosticsBootId,
                ConfigId,
                MapRevision,
                0);
            AssertEx.Equal(DiagnosticsBootId, inventory.DiagnosticsBootId);
            AssertEx.Equal(ConfigId, inventory.ConfigId);
            AssertEx.Equal(ConfigRevision, inventory.ConfigRevision);
            AssertEx.Equal(MapRevision, inventory.MapRevision);
            AssertEx.Equal(
                OwnerSessionEpoch,
                inventory.ConfigurationOwnerSessionEpoch);
            AssertEx.Equal(
                0u,
                inventory.ConfigurationClosedSessionEpoch);
            AssertEx.Equal(LMCRecorderState.Ready, inventory.ConfigurationState);
            AssertEx.Equal(LMCRecorderBufferMode.Double, inventory.BufferMode);
            AssertEx.Equal((byte)2, inventory.RecorderBufferCount);
            AssertEx.Equal(2, inventory.OccupiedBanks.Count);
            AssertEx.Equal(RecordId, inventory.OccupiedBanks[0].RecordId);
            AssertEx.Equal(0u, inventory.OccupiedBanks[0].BufferId);
            AssertEx.Equal(RecordId + 1, inventory.OccupiedBanks[1].RecordId);
            AssertEx.Equal(1u, inventory.OccupiedBanks[1].BufferId);
            AssertEx.False(inventory.IsConfigurationOwnerSessionClosed);

            var empty = LMC_DiagnosticsParser.ParseRecorderBankInventory(
                TestFrame.Response(
                    0,
                    RecorderBankInventoryPayload(
                        GoldenRequestId,
                        0,
                        LMCRecorderState.Configured,
                        OwnerSessionEpoch)),
                GoldenRequestId,
                DiagnosticsBootId,
                ConfigId,
                MapRevision,
                ConfigRevision);
            AssertEx.Equal(0, empty.OccupiedBanks.Count);
            AssertEx.Equal(
                LMCRecorderState.Configured,
                empty.ConfigurationState);
            AssertEx.Equal(ConfigRevision, empty.ConfigRevision);
            AssertEx.True(empty.IsConfigurationOwnerSessionClosed);
        }

        private static void RecorderBankInventoryParserFailClosed()
        {
            var payload = RecorderBankInventoryPayload(
                GoldenRequestId,
                2,
                LMCRecorderState.Ready,
                0);
            payload[44] = 1;
            AssertInvalidRecorderBankInventory(payload);

            payload = RecorderBankInventoryPayload(
                GoldenRequestId,
                2,
                LMCRecorderState.Ready,
                0);
            TestFrame.WriteUInt32(payload, 72, 0);
            AssertInvalidRecorderBankInventory(payload);

            payload = RecorderBankInventoryPayload(
                GoldenRequestId,
                2,
                LMCRecorderState.Ready,
                0);
            TestFrame.WriteUInt32(payload, 68, RecordId);
            AssertInvalidRecorderBankInventory(payload);

            payload = RecorderBankInventoryPayload(
                GoldenRequestId,
                2,
                LMCRecorderState.Ready,
                0);
            TestFrame.WriteUInt32(payload, 76, ReconnectedOwnerSessionEpoch);
            AssertInvalidRecorderBankInventory(payload);

            payload = RecorderBankInventoryPayload(
                GoldenRequestId,
                2,
                LMCRecorderState.Ready,
                0);
            TestFrame.WriteUInt32(payload, 16, DiagnosticsBootId + 1);
            AssertInvalidRecorderBankInventory(payload);

            payload = RecorderBankInventoryPayload(
                GoldenRequestId,
                2,
                LMCRecorderState.Ready,
                0);
            TestFrame.WriteUInt32(payload, 20, ConfigId + 1);
            AssertInvalidRecorderBankInventory(payload);

            payload = RecorderBankInventoryPayload(
                GoldenRequestId,
                2,
                LMCRecorderState.Ready,
                0);
            TestFrame.WriteUInt32(payload, 24, ConfigRevision + 1);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseRecorderBankInventory(
                    TestFrame.Response(0, payload),
                    GoldenRequestId,
                    DiagnosticsBootId,
                    ConfigId,
                    MapRevision,
                    ConfigRevision));

            payload = RecorderBankInventoryPayload(
                GoldenRequestId,
                2,
                LMCRecorderState.Ready,
                0);
            TestFrame.WriteUInt32(payload, 28, MapRevision + 1);
            AssertInvalidRecorderBankInventory(payload);

            payload = RecorderBankInventoryPayload(
                GoldenRequestId,
                2,
                LMCRecorderState.Ready,
                0);
            TestFrame.WriteUInt16(
                payload,
                40,
                (ushort)LMCRecorderState.Fault);
            AssertInvalidRecorderBankInventory(payload);

            payload = RecorderBankInventoryPayload(
                GoldenRequestId,
                2,
                LMCRecorderState.Ready,
                0);
            payload[43] = 1;
            AssertInvalidRecorderBankInventory(payload);

            payload = RecorderBankInventoryPayload(
                GoldenRequestId,
                2,
                LMCRecorderState.Ready,
                0);
            payload[45] = 1;
            AssertInvalidRecorderBankInventory(payload);

            payload = RecorderBankInventoryPayload(
                GoldenRequestId,
                0,
                LMCRecorderState.Ready,
                OwnerSessionEpoch);
            AssertInvalidRecorderBankInventory(payload);

            payload = RecorderBankInventoryPayload(
                GoldenRequestId,
                0,
                LMCRecorderState.Configured,
                OwnerSessionEpoch);
            payload[42] = (byte)LMCRecorderBufferMode.Single;
            AssertInvalidRecorderBankInventory(payload);

            payload = RecorderBankInventoryPayload(
                GoldenRequestId,
                1,
                LMCRecorderState.Ready,
                0);
            Array.Resize(ref payload, payload.Length - 1);
            AssertInvalidRecorderBankInventory(payload);

            payload = RecorderBankInventoryPayload(
                GoldenRequestId,
                1,
                LMCRecorderState.Ready,
                0);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseRecorderBankInventory(
                    TestFrame.Response(0, payload),
                    GoldenRequestId,
                    DiagnosticsBootId,
                    ConfigId,
                    MapRevision,
                    ConfigRevision + 1));
        }

        private static void RecorderBankInventorySyncAndAsync()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E4A,
                    TestFrame.Response(
                        0,
                        RecorderBankInventoryPayload(
                            2,
                            0,
                            LMCRecorderState.Configured,
                            OwnerSessionEpoch))),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(3))),
                new FakeRpcStep(
                    0x7E4A,
                    TestFrame.Response(
                        0,
                        RecorderBankInventoryPayload(
                            4,
                            1,
                            LMCRecorderState.Ready,
                            0))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var empty = connection.Diagnostics.ReadRecorderBankInventory(
                    DiagnosticsBootId,
                    ConfigId,
                    MapRevision);
                AssertEx.Equal(0, empty.OccupiedBanks.Count);
                AssertEx.Equal(ConfigRevision, empty.ConfigRevision);

                var occupied = connection.Diagnostics
                    .ReadRecorderBankInventoryAsync(
                        DiagnosticsBootId,
                        ConfigId,
                        MapRevision,
                        ConfigRevision,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(1, occupied.OccupiedBanks.Count);
                AssertEx.Equal(RecordId, occupied.OccupiedBanks[0].RecordId);

                connection.CloseConnection();
                server.Verify();
            }

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        SingleBankCapabilitiesPayload(1))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                AssertEx.Throws<NotSupportedException>(
                    () => connection.Diagnostics.ReadRecorderBankInventory(
                        DiagnosticsBootId,
                        ConfigId,
                        MapRevision));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RecorderBankInventoryTypedConfigurationAbsence()
        {
            var raw = TestFrame.Response(
                0,
                DomainErrorPayload(
                    GoldenRequestId,
                    LMCDiagnosticsDetailCode
                        .RecorderConfigurationAbsent));
            var absence = AssertEx.Throws<
                LMCRecorderConfigurationAbsentException>(
                    () => LMC_DiagnosticsParser
                        .ParseRecorderBankInventory(
                            raw,
                            GoldenRequestId,
                            DiagnosticsBootId,
                            ConfigId,
                            MapRevision,
                            ConfigRevision));
            AssertEx.Equal(DiagnosticsBootId, absence.DiagnosticsBootId);
            AssertEx.Equal(ConfigId, absence.ConfigId);
            AssertEx.Equal(ConfigRevision, absence.ConfigRevision);
            AssertEx.Equal(MapRevision, absence.MapRevision);
            AssertEx.Equal(
                LMCDiagnosticsDetailCode.RecorderConfigurationAbsent,
                absence.Response.Detail);

            var unknownRevision = AssertEx.Throws<
                LMCDiagnosticsCommandException>(
                    () => LMC_DiagnosticsParser
                        .ParseRecorderBankInventory(
                            raw,
                            GoldenRequestId,
                            DiagnosticsBootId,
                            ConfigId,
                            MapRevision,
                            0));
            AssertEx.False(
                unknownRevision
                    is LMCRecorderConfigurationAbsentException,
                "ConfigRevision=0 must remain ineligible for typed absence recovery.");
        }

        private static void RecorderBankInventoryTypedAbsenceSyncAndAsync()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E4A,
                    TestFrame.Response(
                        0,
                        DomainErrorPayload(
                            2,
                            LMCDiagnosticsDetailCode
                                .RecorderConfigurationAbsent)))
                {
                    InspectRequest = request =>
                    {
                        AssertEx.Equal(
                            DiagnosticsBootId,
                            TestFrame.ReadUInt32(request, 16));
                        AssertEx.Equal(
                            ConfigId,
                            TestFrame.ReadUInt32(request, 20));
                        AssertEx.Equal(
                            MapRevision,
                            TestFrame.ReadUInt32(request, 24));
                        AssertEx.Equal(
                            ConfigRevision,
                            TestFrame.ReadUInt32(request, 28));
                    }
                },
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(3))),
                new FakeRpcStep(
                    0x7E4A,
                    TestFrame.Response(
                        0,
                        DomainErrorPayload(
                            4,
                            LMCDiagnosticsDetailCode
                                .RecorderConfigurationAbsent))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var sync = AssertEx.Throws<
                    LMCRecorderConfigurationAbsentException>(
                        () => connection.Diagnostics
                            .ReadRecorderBankInventory(
                                DiagnosticsBootId,
                                ConfigId,
                                MapRevision,
                                ConfigRevision));
                AssertEx.Equal(ConfigRevision, sync.ConfigRevision);

                var async = AssertEx.Throws<
                    LMCRecorderConfigurationAbsentException>(
                        () => connection.Diagnostics
                            .ReadRecorderBankInventoryAsync(
                                DiagnosticsBootId,
                                ConfigId,
                                MapRevision,
                                ConfigRevision,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                AssertEx.Equal(ConfigId, async.ConfigId);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RecorderEmptyConfigurationAdoptionRequestAndParser()
        {
            var inventory = EmptyClosedRecorderBankInventory(GoldenRequestId);
            var request = LMC_DiagnosticsFrame
                .AdoptEmptyRecorderConfiguration(
                    GoldenRequestId,
                    inventory);
            AssertRequestHeader(request, 0x7E4B, 28, GoldenRequestId);
            AssertEx.Equal(
                DiagnosticsBootId,
                TestFrame.ReadUInt32(request, 16));
            AssertEx.Equal(ConfigId, TestFrame.ReadUInt32(request, 20));
            AssertEx.Equal(ConfigRevision, TestFrame.ReadUInt32(request, 24));
            AssertEx.Equal(MapRevision, TestFrame.ReadUInt32(request, 28));
            AssertEx.Equal(
                OwnerSessionEpoch,
                TestFrame.ReadUInt32(request, 32));

            var lease = LMC_DiagnosticsParser
                .ParseAdoptEmptyRecorderConfiguration(
                    TestFrame.Response(
                        0,
                        AdoptEmptyRecorderConfigurationPayload(
                            GoldenRequestId)),
                    GoldenRequestId,
                    inventory,
                    7,
                    null);
            AssertEx.Equal(DiagnosticsBootId, lease.DiagnosticsBootId);
            AssertEx.Equal(ConfigId, lease.ConfigId);
            AssertEx.Equal(ConfigRevision, lease.ConfigRevision);
            AssertEx.Equal(MapRevision, lease.MapRevision);
            AssertEx.Equal(
                OwnerSessionEpoch,
                lease.PreviousOwnerSessionEpoch);
            AssertEx.Equal(
                ReconnectedOwnerSessionEpoch,
                lease.OwnerSessionEpoch);
            AssertEx.Equal(LMCRecorderState.Configured, lease.InitialState);
            AssertEx.Equal(LMCRecorderBufferMode.Double, lease.BufferMode);
            AssertEx.Equal((byte)2, lease.RecorderBufferCount);
            AssertEx.False(lease.IsReleased);

            var release = LMC_DiagnosticsFrame.ReleaseRecorder(
                GoldenRequestId,
                lease);
            AssertRequestHeader(release, 0x7E48, 28, GoldenRequestId);
            AssertEx.Equal(ConfigId, TestFrame.ReadUInt32(release, 16));
            AssertEx.Equal(ConfigRevision, TestFrame.ReadUInt32(release, 20));
            AssertEx.Equal(MapRevision, TestFrame.ReadUInt32(release, 24));
            AssertEx.Equal(
                ReconnectedOwnerSessionEpoch,
                TestFrame.ReadUInt32(release, 28));
            AssertEx.Equal(
                DiagnosticsBootId,
                TestFrame.ReadUInt32(release, 32));

            var invalid = AdoptEmptyRecorderConfigurationPayload(
                GoldenRequestId);
            TestFrame.WriteUInt32(invalid, 32, OwnerSessionEpoch);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser
                    .ParseAdoptEmptyRecorderConfiguration(
                        TestFrame.Response(0, invalid),
                        GoldenRequestId,
                        inventory,
                        7,
                        null));

            invalid = AdoptEmptyRecorderConfigurationPayload(
                GoldenRequestId);
            invalid[38] = (byte)LMCRecorderBufferMode.Single;
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser
                    .ParseAdoptEmptyRecorderConfiguration(
                        TestFrame.Response(0, invalid),
                        GoldenRequestId,
                        inventory,
                        7,
                        null));

            invalid = AdoptEmptyRecorderConfigurationPayload(
                GoldenRequestId);
            TestFrame.WriteUInt16(
                invalid,
                2,
                (ushort)LMCDiagnosticsResponseFlags.Partial);
            AssertInvalidAdoptEmptyRecorderConfiguration(invalid, inventory);

            invalid = AdoptEmptyRecorderConfigurationPayload(
                GoldenRequestId);
            TestFrame.WriteUInt32(invalid, 16, DiagnosticsBootId + 1);
            AssertInvalidAdoptEmptyRecorderConfiguration(invalid, inventory);

            invalid = AdoptEmptyRecorderConfigurationPayload(
                GoldenRequestId);
            TestFrame.WriteUInt32(invalid, 20, ConfigId + 1);
            AssertInvalidAdoptEmptyRecorderConfiguration(invalid, inventory);

            invalid = AdoptEmptyRecorderConfigurationPayload(
                GoldenRequestId);
            TestFrame.WriteUInt32(invalid, 24, ConfigRevision + 1);
            AssertInvalidAdoptEmptyRecorderConfiguration(invalid, inventory);

            invalid = AdoptEmptyRecorderConfigurationPayload(
                GoldenRequestId);
            TestFrame.WriteUInt32(invalid, 28, MapRevision + 1);
            AssertInvalidAdoptEmptyRecorderConfiguration(invalid, inventory);

            invalid = AdoptEmptyRecorderConfigurationPayload(
                GoldenRequestId);
            TestFrame.WriteUInt16(
                invalid,
                36,
                (ushort)LMCRecorderState.Ready);
            AssertInvalidAdoptEmptyRecorderConfiguration(invalid, inventory);

            invalid = AdoptEmptyRecorderConfigurationPayload(
                GoldenRequestId);
            invalid[39] = 1;
            AssertInvalidAdoptEmptyRecorderConfiguration(invalid, inventory);

            var occupiedInventory = LMC_DiagnosticsParser
                .ParseRecorderBankInventory(
                    TestFrame.Response(
                        0,
                        RecorderBankInventoryPayload(
                            GoldenRequestId,
                            1,
                            LMCRecorderState.Ready,
                            OwnerSessionEpoch)),
                    GoldenRequestId,
                    DiagnosticsBootId,
                    ConfigId,
                    MapRevision,
                    ConfigRevision);
            AssertEx.Throws<ArgumentException>(
                () => LMC_DiagnosticsFrame
                    .AdoptEmptyRecorderConfiguration(
                        GoldenRequestId,
                        occupiedInventory));
            AssertEx.Throws<ArgumentException>(
                () => LMC_DiagnosticsParser
                    .ParseAdoptEmptyRecorderConfiguration(
                        TestFrame.Response(
                            0,
                            AdoptEmptyRecorderConfigurationPayload(
                                GoldenRequestId)),
                        GoldenRequestId,
                        occupiedInventory,
                        7,
                        null));

            foreach (var method in typeof(LMCDiagnostics).GetMethods())
            {
                if (method.Name != "StartRecorder"
                    && method.Name != "StartRecorderAsync")
                {
                    continue;
                }

                foreach (var parameter in method.GetParameters())
                {
                    AssertEx.False(
                        parameter.ParameterType
                            == typeof(
                                LMCRecoveredRecorderConfigurationLease),
                        "Recovered empty Recorder configurations must remain release-only.");
                }
            }
        }

        private static void RecorderRecoverableInventoryAdoptionZeroWire()
        {
            var inventory = LMC_DiagnosticsParser
                .ParseRecoverableRecorderBankInventory(
                    TestFrame.Response(
                        0,
                        RecoverableRecorderBankInventoryPayload(
                            GoldenRequestId,
                            RecoveryToken)),
                    GoldenRequestId,
                    DiagnosticsBootId,
                    ConfigId,
                    MapRevision,
                    RecoveryToken);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = ConnectRecorder(server))
            {
                AssertEx.Throws<ArgumentException>(
                    () => connection.Diagnostics
                        .AdoptEmptyRecorderConfiguration(inventory));
                AssertEx.Throws<ArgumentException>(
                    () => connection.Diagnostics
                        .AdoptEmptyRecorderConfigurationAsync(
                            inventory,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RecorderEmptyConfigurationAdoptionSyncAsyncRelease()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E4B,
                    TestFrame.Response(
                        0,
                        AdoptEmptyRecorderConfigurationPayload(2))),
                new FakeRpcStep(
                    0x7E48,
                    TestFrame.Response(
                        0,
                        DomainErrorPayload(
                            3,
                            LMCDiagnosticsDetailCode.InvalidState))),
                new FakeRpcStep(
                    0x7E48,
                    TestFrame.Response(0, CommonPayload(16, 4))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var inventory = EmptyClosedRecorderBankInventory(99);
                var lease = connection.Diagnostics
                    .AdoptEmptyRecorderConfiguration(inventory);
                AssertEx.Equal(
                    ReconnectedOwnerSessionEpoch,
                    lease.OwnerSessionEpoch);
                AssertEx.False(lease.IsReleased);

                AssertEx.Throws<LMCDiagnosticsCommandException>(
                    () => connection.Diagnostics.ReleaseRecorder(lease));
                AssertEx.False(lease.IsReleased);
                AssertEx.False(lease.IsReleaseOutcomeUnverified);
                connection.Diagnostics.ReleaseRecorder(lease);
                AssertEx.True(lease.IsReleased);
                AssertEx.False(lease.IsReleaseOutcomeUnverified);
                AssertEx.Throws<InvalidOperationException>(
                    () => connection.Diagnostics.ReleaseRecorder(lease));

                connection.CloseConnection();
                server.Verify();
            }

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E4B,
                    TestFrame.Response(
                        0,
                        AdoptEmptyRecorderConfigurationPayload(2))),
                new FakeRpcStep(
                    0x7E48,
                    TestFrame.Response(0, CommonPayload(16, 3))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var lease = connection.Diagnostics
                    .AdoptEmptyRecorderConfigurationAsync(
                        EmptyClosedRecorderBankInventory(100),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                connection.Diagnostics.ReleaseRecorderAsync(
                        lease,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(lease.IsReleased);

                connection.CloseConnection();
                server.Verify();
            }

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        SingleBankCapabilitiesPayload(1))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);
                AssertEx.Throws<NotSupportedException>(
                    () => connection.Diagnostics
                        .AdoptEmptyRecorderConfiguration(
                            EmptyClosedRecorderBankInventory(101)));
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void
            RecorderEmptyConfigurationAdoptionResponseLossReconnectReadopt()
        {
            const uint secondAdoptedOwnerSessionEpoch = 0x778899AAu;
            var emulatedOwnerSessionEpoch = OwnerSessionEpoch;
            var firstAdoptionApplied = new ManualResetEventSlim(false);
            var firstInventory = EmptyClosedRecorderBankInventoryForOwner(
                99,
                OwnerSessionEpoch);
            var firstAdopt = new FakeRpcStep(0x7E4B, null)
            {
                InspectRequest = request =>
                {
                    AssertEx.Equal(
                        OwnerSessionEpoch,
                        TestFrame.ReadUInt32(request, 32));
                },
                AfterResponse = request =>
                {
                    emulatedOwnerSessionEpoch =
                        ReconnectedOwnerSessionEpoch;
                    firstAdoptionApplied.Set();
                },
                CloseAfterResponse = true
            };

            using (firstAdoptionApplied)
            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                firstAdopt))
            using (var secondServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(3))),
                new FakeRpcStep(
                    0x7E4A,
                    TestFrame.Response(
                        0,
                        RecorderBankInventoryPayloadForOwner(
                            4,
                            ReconnectedOwnerSessionEpoch,
                            ReconnectedOwnerSessionEpoch)))
                {
                    InspectRequest = request =>
                    {
                        AssertEx.Equal(
                            ReconnectedOwnerSessionEpoch,
                            emulatedOwnerSessionEpoch);
                        AssertEx.True(firstAdoptionApplied.IsSet);
                        AssertEx.Equal(
                            DiagnosticsBootId,
                            TestFrame.ReadUInt32(request, 16));
                        AssertEx.Equal(
                            ConfigId,
                            TestFrame.ReadUInt32(request, 20));
                        AssertEx.Equal(
                            MapRevision,
                            TestFrame.ReadUInt32(request, 24));
                        AssertEx.Equal(
                            ConfigRevision,
                            TestFrame.ReadUInt32(request, 28));
                    }
                },
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(5))),
                new FakeRpcStep(
                    0x7E4B,
                    TestFrame.Response(
                        0,
                        AdoptEmptyRecorderConfigurationPayloadForOwner(
                            6,
                            secondAdoptedOwnerSessionEpoch)))
                {
                    InspectRequest = request =>
                    {
                        var previousOwner =
                            TestFrame.ReadUInt32(request, 32);
                        AssertEx.Equal(
                            ReconnectedOwnerSessionEpoch,
                            previousOwner);
                        AssertEx.False(
                            previousOwner == OwnerSessionEpoch,
                            "Reconnect adoption must use only the exact previous owner returned by the new inventory.");
                    }
                },
                new FakeRpcStep(
                    0x7E48,
                    TestFrame.Response(0, CommonPayload(16, 7))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    firstServer.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                AssertEx.Throws<EndOfStreamException>(
                    () => connection.Diagnostics
                        .AdoptEmptyRecorderConfiguration(firstInventory));
                AssertEx.True(firstAdoptionApplied.Wait(1000));
                AssertEx.Equal(
                    ReconnectedOwnerSessionEpoch,
                    emulatedOwnerSessionEpoch);
                AssertEx.Equal(
                    LMCConnectionState.Faulted,
                    connection.State);

                var requestsBeforeSameSessionRetry =
                    firstServer.ReceivedRequests.Count;
                AssertEx.Throws<InvalidOperationException>(
                    () => connection.Diagnostics
                        .AdoptEmptyRecorderConfiguration(firstInventory));
                AssertEx.Equal(
                    requestsBeforeSameSessionRetry,
                    firstServer.ReceivedRequests.Count,
                    "A destructive 0x7E4B retry in the faulted session must send zero wire requests.");
                firstServer.Verify();

                connection.RpcInitConnection(
                    "127.0.0.1",
                    secondServer.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var newInventory = connection.Diagnostics
                    .ReadRecorderBankInventory(
                        DiagnosticsBootId,
                        ConfigId,
                        MapRevision,
                        ConfigRevision);
                AssertEx.Equal(
                    ReconnectedOwnerSessionEpoch,
                    newInventory.ConfigurationOwnerSessionEpoch);
                AssertEx.True(
                    newInventory.IsConfigurationOwnerSessionClosed);
                AssertEx.Equal(0, newInventory.OccupiedBanks.Count);

                var recovered = connection.Diagnostics
                    .AdoptEmptyRecorderConfiguration(newInventory);
                AssertEx.Equal(
                    ReconnectedOwnerSessionEpoch,
                    recovered.PreviousOwnerSessionEpoch);
                AssertEx.Equal(
                    secondAdoptedOwnerSessionEpoch,
                    recovered.OwnerSessionEpoch);

                connection.Diagnostics.ReleaseRecorder(recovered);
                AssertEx.True(recovered.IsReleased);
                connection.CloseConnection();
                secondServer.Verify();
            }
        }

        private static void AssertInvalidRecorderBankInventory(byte[] payload)
        {
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseRecorderBankInventory(
                    TestFrame.Response(0, payload),
                    GoldenRequestId,
                    DiagnosticsBootId,
                    ConfigId,
                    MapRevision,
                    0));
        }

        private static void AssertInvalidAdoptEmptyRecorderConfiguration(
            byte[] payload,
            LMCRecorderBankInventory inventory)
        {
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser
                    .ParseAdoptEmptyRecorderConfiguration(
                        TestFrame.Response(0, payload),
                        GoldenRequestId,
                        inventory,
                        7,
                        null));
        }

        private static void RecorderConfigureStartStatus()
        {
            var configuration = ManualConfiguration();
            var capabilities = Capabilities(0);
            var handle = LMC_DiagnosticsParser.ParseConfigureRecorder(
                TestFrame.Response(0, ConfigurePayload(GoldenRequestId)),
                GoldenRequestId,
                configuration,
                capabilities,
                7,
                null);
            AssertEx.Equal(ConfigId, handle.ConfigId);
            AssertEx.Equal(ConfigRevision, handle.ConfigRevision);
            AssertEx.Equal(3u, handle.AcceptedCapacity);
            AssertEx.Equal(1000u, handle.SamplePeriodUs);
            AssertEx.Equal(24u, handle.ReservedDataBytes);
            AssertEx.Equal((ushort)8, handle.SampleStrideBytes);
            AssertEx.Equal((ushort)1, handle.RecorderBufferCount);
            AssertEx.Equal(LMCCapturePhase.InputMapped, handle.CapturePhase);

            var doubleConfiguration = TriggerConfiguration(
                LMCRecorderBufferMode.Double,
                LMCRecorderTriggerType.Edge,
                LMCRecorderTriggerOperator.RisingEdge,
                0);
            var doublePayload = ConfigurePayload(GoldenRequestId);
            TestFrame.WriteUInt32(doublePayload, 28, 10);
            TestFrame.WriteUInt32(doublePayload, 32, 160);
            TestFrame.WriteUInt16(doublePayload, 42, 2);
            var doubleHandle = LMC_DiagnosticsParser.ParseConfigureRecorder(
                TestFrame.Response(0, doublePayload),
                GoldenRequestId,
                doubleConfiguration,
                capabilities,
                7,
                null);
            AssertEx.Equal((ushort)2, doubleHandle.RecorderBufferCount);
            AssertEx.Equal(160u, doubleHandle.ReservedDataBytes);

            var identity = LMC_DiagnosticsParser.ParseStartRecorder(
                TestFrame.Response(0, StartPayload(GoldenRequestId)),
                GoldenRequestId,
                handle,
                7,
                null);
            AssertEx.Equal(RecordId, identity.RecordId);
            AssertEx.Equal(0u, identity.BufferId);
            AssertEx.Equal(LMCRecorderState.Armed, identity.InitialState);
            AssertEx.True(identity.HasConfigurationShape);

            var status = LMC_DiagnosticsParser.ParseRecorderStatus(
                TestFrame.Response(0, StatusPayload(GoldenRequestId)),
                GoldenRequestId,
                identity);
            AssertEx.Equal(LMCRecorderState.Ready, status.State);
            AssertEx.Equal(LMCRecorderStopReason.SampleCountComplete, status.StopReason);
            AssertEx.Equal(3u, status.SampleCount);
            AssertEx.True(status.IsFrozen);
            AssertEx.False(status.HasTrigger);

            var rollingIdentity = RecorderIdentity(
                bufferMode: LMCRecorderBufferMode.Ring,
                triggerType: LMCRecorderTriggerType.Edge);
            var rollingStatusPayload = StatusPayload(GoldenRequestId);
            TestFrame.WriteUInt16(
                rollingStatusPayload,
                36,
                (ushort)LMCRecorderState.Recording);
            rollingStatusPayload[39] = (byte)LMCRecorderStopReason.None;
            TestFrame.WriteUInt32(rollingStatusPayload, 40, 1);
            TestFrame.WriteUInt32(rollingStatusPayload, 52, 90);
            var rollingStatus = LMC_DiagnosticsParser.ParseRecorderStatus(
                TestFrame.Response(0, rollingStatusPayload),
                GoldenRequestId,
                rollingIdentity);
            rollingIdentity.ApplyStatusMetadata(rollingStatus);
            AssertEx.Equal(0u, rollingIdentity.AcceptedStartCycle);

            TestFrame.WriteUInt32(rollingStatusPayload, 52, 91);
            rollingStatus = LMC_DiagnosticsParser.ParseRecorderStatus(
                TestFrame.Response(0, rollingStatusPayload),
                GoldenRequestId,
                rollingIdentity);
            rollingIdentity.ApplyStatusMetadata(rollingStatus);
            AssertEx.Equal(0u, rollingIdentity.AcceptedStartCycle);

            var frozenRollingPayload = StatusPayload(GoldenRequestId);
            frozenRollingPayload[39] =
                (byte)LMCRecorderStopReason.TriggerComplete;
            TestFrame.WriteUInt32(frozenRollingPayload, 48, 1);
            TestFrame.WriteUInt32(frozenRollingPayload, 52, 92);
            var frozenRollingStatus = LMC_DiagnosticsParser.ParseRecorderStatus(
                TestFrame.Response(0, frozenRollingPayload),
                GoldenRequestId,
                rollingIdentity);
            rollingIdentity.ApplyStatusMetadata(frozenRollingStatus);
            AssertEx.Equal(92u, rollingIdentity.AcceptedStartCycle);

            TestFrame.WriteUInt32(frozenRollingPayload, 52, 93);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseRecorderStatus(
                    TestFrame.Response(0, frozenRollingPayload),
                    GoldenRequestId,
                    rollingIdentity));

            var manualRecordingIdentity = RecorderIdentity();
            var manualRecordingPayload = StatusPayload(GoldenRequestId);
            TestFrame.WriteUInt16(
                manualRecordingPayload,
                36,
                (ushort)LMCRecorderState.Recording);
            manualRecordingPayload[39] =
                (byte)LMCRecorderStopReason.None;
            TestFrame.WriteUInt32(manualRecordingPayload, 40, 1);
            var manualRecordingStatus =
                LMC_DiagnosticsParser.ParseRecorderStatus(
                    TestFrame.Response(0, manualRecordingPayload),
                    GoldenRequestId,
                    manualRecordingIdentity);
            manualRecordingIdentity.ApplyStatusMetadata(
                manualRecordingStatus);
            AssertEx.Equal(
                100u,
                manualRecordingIdentity.AcceptedStartCycle);

            TestFrame.WriteUInt32(manualRecordingPayload, 52, 101);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseRecorderStatus(
                    TestFrame.Response(0, manualRecordingPayload),
                    GoldenRequestId,
                    manualRecordingIdentity));

            var wrongTriggerIndex = StatusPayload(GoldenRequestId);
            wrongTriggerIndex[39] =
                (byte)LMCRecorderStopReason.TriggerComplete;
            TestFrame.WriteUInt32(wrongTriggerIndex, 48, 0);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseRecorderStatus(
                    TestFrame.Response(0, wrongTriggerIndex),
                    GoldenRequestId,
                    RecorderIdentity(
                        bufferMode: LMCRecorderBufferMode.Ring,
                        triggerType: LMCRecorderTriggerType.Edge)));

            var wrongTriggerSampleCount = StatusPayload(GoldenRequestId);
            wrongTriggerSampleCount[39] =
                (byte)LMCRecorderStopReason.TriggerComplete;
            TestFrame.WriteUInt32(wrongTriggerSampleCount, 40, 2);
            TestFrame.WriteUInt32(wrongTriggerSampleCount, 48, 1);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseRecorderStatus(
                    TestFrame.Response(0, wrongTriggerSampleCount),
                    GoldenRequestId,
                    RecorderIdentity(
                        bufferMode: LMCRecorderBufferMode.Ring,
                        triggerType: LMCRecorderTriggerType.Edge)));

            var overPostLimit = StatusPayload(GoldenRequestId);
            overPostLimit[39] = (byte)LMCRecorderStopReason.UserStop;
            TestFrame.WriteUInt32(overPostLimit, 48, 1);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseRecorderStatus(
                    TestFrame.Response(0, overPostLimit),
                    GoldenRequestId,
                    RecorderIdentity(
                        triggerType: LMCRecorderTriggerType.Edge,
                        bufferMode: LMCRecorderBufferMode.Ring,
                        preTriggerSamples: 1,
                        postTriggerSamples: 0)));

            var triggeredCountComplete = StatusPayload(GoldenRequestId);
            TestFrame.WriteUInt32(triggeredCountComplete, 48, 1);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseRecorderStatus(
                    TestFrame.Response(0, triggeredCountComplete),
                    GoldenRequestId,
                    RecorderIdentity(
                        bufferMode: LMCRecorderBufferMode.Ring,
                        triggerType: LMCRecorderTriggerType.Edge)));

            var wrongBoot = StatusPayload(GoldenRequestId);
            TestFrame.WriteUInt32(wrongBoot, 72, DiagnosticsBootId + 1);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseRecorderStatus(
                    TestFrame.Response(0, wrongBoot),
                    GoldenRequestId,
                    identity));

            var wrongPhase = StatusPayload(GoldenRequestId);
            wrongPhase[38] = (byte)LMCCapturePhase.PreOutput;
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseRecorderStatus(
                    TestFrame.Response(0, wrongPhase),
                    GoldenRequestId,
                    identity));

            var incompleteCompleted = StatusPayload(GoldenRequestId);
            TestFrame.WriteUInt32(incompleteCompleted, 40, 2);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseRecorderStatus(
                    TestFrame.Response(0, incompleteCompleted),
                    GoldenRequestId,
                    identity));

            var missingTrigger = StatusPayload(GoldenRequestId);
            missingTrigger[39] = (byte)LMCRecorderStopReason.TriggerComplete;
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseRecorderStatus(
                    TestFrame.Response(0, missingTrigger),
                    GoldenRequestId,
                    identity));

            var fixedStartIdentity = RecorderIdentity(100);
            var wrongStartCycle = StatusPayload(GoldenRequestId);
            TestFrame.WriteUInt32(wrongStartCycle, 52, 101);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseRecorderStatus(
                    TestFrame.Response(0, wrongStartCycle),
                    GoldenRequestId,
                    fixedStartIdentity));

            var badConfigure = ConfigurePayload(GoldenRequestId);
            TestFrame.WriteUInt16(badConfigure, 46, 1);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseConfigureRecorder(
                    TestFrame.Response(0, badConfigure),
                    GoldenRequestId,
                    configuration,
                    capabilities,
                    7,
                    null));

            var badStart = StartPayload(GoldenRequestId);
            TestFrame.WriteUInt32(badStart, 20, 1);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseStartRecorder(
                    TestFrame.Response(0, badStart),
                    GoldenRequestId,
                    handle,
                    7,
                    null));
        }

        private static void RecorderHeaderTriggerAndMalformed()
        {
            var identity = RecorderIdentity(
                triggerType: LMCRecorderTriggerType.Edge);
            var header = LMC_DiagnosticsParser.ParseRecorderHeader(
                TestFrame.Response(
                    0,
                    HeaderPayload(GoldenRequestId, true)),
                GoldenRequestId,
                identity);
            AssertEx.Equal(DiagnosticsBootId, header.DiagnosticsBootId);
            AssertEx.Equal((ushort)2, header.ChannelCount);
            AssertEx.Equal((ushort)8, header.SampleStrideBytes);
            AssertEx.Equal(1000u, header.SamplePeriodUs);
            AssertEx.True(header.HasTrigger);
            AssertEx.Equal(1u, header.TriggerIndex);
            AssertEx.Equal(0x0000000200000001ul, header.StartTimestampUs);
            AssertEx.Equal(0x0000000400000003ul, header.TriggerTimestampUs);
            AssertEx.Equal(0x0000000600000005ul, header.EndTimestampUs);
            AssertEx.Equal(Signal1, header.SignalIds[0]);

            var wrongTriggerIndex = HeaderPayload(GoldenRequestId, true);
            TestFrame.WriteUInt32(wrongTriggerIndex, 64, 0);
            AssertHeaderMalformed(wrongTriggerIndex, identity);

            var wrongTriggerSampleCount = HeaderPayload(GoldenRequestId, true);
            TestFrame.WriteUInt32(wrongTriggerSampleCount, 44, 2);
            AssertHeaderMalformed(wrongTriggerSampleCount, identity);

            var overPostLimit = HeaderPayload(GoldenRequestId, true);
            overPostLimit[41] = (byte)LMCRecorderStopReason.UserStop;
            TestFrame.WriteUInt16(
                overPostLimit,
                42,
                (ushort)(LMCRecorderHeaderFlags.CaptureComplete
                    | LMCRecorderHeaderFlags.TriggerPresent
                    | LMCRecorderHeaderFlags.UserStopped
                    | LMCRecorderHeaderFlags.DataCrcPresent));
            AssertHeaderMalformed(
                overPostLimit,
                RecorderIdentity(
                    triggerType: LMCRecorderTriggerType.Edge,
                    bufferMode: LMCRecorderBufferMode.Ring,
                    preTriggerSamples: 1,
                    postTriggerSamples: 0));

            var triggeredCountComplete = HeaderPayload(GoldenRequestId, true);
            triggeredCountComplete[41] =
                (byte)LMCRecorderStopReason.SampleCountComplete;
            AssertHeaderMalformed(triggeredCountComplete, identity);

            var immutableIdentity = RecorderIdentity(
                triggerType: LMCRecorderTriggerType.Edge);
            var immutableHeader = LMC_DiagnosticsParser.ParseRecorderHeader(
                TestFrame.Response(
                    0,
                    HeaderPayload(GoldenRequestId, true)),
                GoldenRequestId,
                immutableIdentity);
            immutableIdentity.ApplyHeaderMetadata(immutableHeader);
            var changedFrozenPayload = HeaderPayload(GoldenRequestId, true);
            TestFrame.WriteUInt32(changedFrozenPayload, 108, 1);
            var changedFrozenHeader = LMC_DiagnosticsParser.ParseRecorderHeader(
                TestFrame.Response(0, changedFrozenPayload),
                GoldenRequestId,
                immutableIdentity);
            AssertEx.Throws<InvalidOperationException>(
                () => immutableIdentity.ApplyHeaderMetadata(changedFrozenHeader));

            var manualIdentity = RecorderIdentity();
            AssertHeaderMalformed(
                HeaderPayload(GoldenRequestId, true),
                manualIdentity);

            var triggerFlagMissing = HeaderPayload(GoldenRequestId, true);
            TestFrame.WriteUInt16(
                triggerFlagMissing,
                42,
                (ushort)(LMCRecorderHeaderFlags.CaptureComplete
                    | LMCRecorderHeaderFlags.DataCrcPresent));
            AssertHeaderMalformed(triggerFlagMissing, identity);

            var crcFlagMissing = HeaderPayload(GoldenRequestId, true);
            TestFrame.WriteUInt16(
                crcFlagMissing,
                42,
                (ushort)(LMCRecorderHeaderFlags.CaptureComplete
                    | LMCRecorderHeaderFlags.TriggerPresent));
            AssertHeaderMalformed(crcFlagMissing, identity);

            var duplicateSignal = HeaderPayload(GoldenRequestId, true);
            TestFrame.WriteUInt32(duplicateSignal, 116, Signal1);
            AssertHeaderMalformed(duplicateSignal, identity);

            var wrongOrder = HeaderPayload(GoldenRequestId, true);
            TestFrame.WriteUInt32(wrongOrder, 112, Signal2);
            AssertHeaderMalformed(wrongOrder, identity);

            var noTriggerWithMetadata = HeaderPayload(GoldenRequestId, false);
            TestFrame.WriteUInt32(noTriggerWithMetadata, 72, 123);
            AssertHeaderMalformed(noTriggerWithMetadata, manualIdentity);

            var wrongPeriod = HeaderPayload(GoldenRequestId, true);
            TestFrame.WriteUInt32(wrongPeriod, 56, 2000);
            AssertHeaderMalformed(wrongPeriod, identity);

            var wrongPhase = HeaderPayload(GoldenRequestId, true);
            wrongPhase[40] = (byte)LMCCapturePhase.PreOutput;
            AssertHeaderMalformed(wrongPhase, identity);

            var incompleteCompleted = HeaderPayload(GoldenRequestId, false);
            TestFrame.WriteUInt32(incompleteCompleted, 44, 2);
            AssertHeaderMalformed(incompleteCompleted, manualIdentity);

            var fixedStartIdentity = RecorderIdentity(
                100,
                LMCRecorderTriggerType.Edge);
            var wrongStartCycle = HeaderPayload(GoldenRequestId, true);
            TestFrame.WriteUInt32(wrongStartCycle, 68, 101);
            AssertHeaderMalformed(wrongStartCycle, fixedStartIdentity);
        }

        private static void RecorderChunkCrcSequenceAndFlags()
        {
            var identity = RecorderIdentity();
            var frozenHeader = LMC_DiagnosticsParser.ParseRecorderHeader(
                TestFrame.Response(
                    0,
                    HeaderPayload(GoldenRequestId, false)),
                GoldenRequestId,
                identity);
            identity.ApplyHeaderMetadata(frozenHeader);
            var request = new LMCRecorderChunkRequest(identity, 0, 2, 77);
            var chunk = LMC_DiagnosticsParser.ParseRecorderChunk(
                TestFrame.Response(
                    0,
                    ChunkPayload(GoldenRequestId, 0, 2, 77, false)),
                GoldenRequestId,
                request);
            AssertEx.Equal((ushort)2, chunk.ReturnedSampleCount);
            AssertEx.Equal(16, chunk.DataByteCount);
            AssertEx.False(chunk.IsLastChunk);
            AssertEx.Equal((byte)1, chunk.Data[0]);

            var finalRequest = new LMCRecorderChunkRequest(identity, 2, 1, 78);
            var final = LMC_DiagnosticsParser.ParseRecorderChunk(
                TestFrame.Response(
                    0,
                    ChunkPayload(GoldenRequestId, 2, 1, 78, true)),
                GoldenRequestId,
                finalRequest);
            AssertEx.True(final.IsLastChunk);

            var badCrc = ChunkPayload(GoldenRequestId, 0, 2, 77, false);
            TestFrame.WriteUInt32(badCrc, 44, 0);
            AssertChunkMalformed(badCrc, request);

            var wrongSequence = ChunkPayload(
                GoldenRequestId,
                0,
                2,
                77,
                false);
            TestFrame.WriteUInt32(wrongSequence, 32, 78);
            AssertChunkMalformed(wrongSequence, request);

            var incorrectLast = ChunkPayload(
                GoldenRequestId,
                0,
                2,
                77,
                false);
            TestFrame.WriteUInt16(
                incorrectLast,
                2,
                (ushort)LMCDiagnosticsResponseFlags.LastChunk);
            AssertChunkMalformed(incorrectLast, request);

            var partial = ChunkPayload(GoldenRequestId, 0, 2, 77, false);
            TestFrame.WriteUInt16(
                partial,
                2,
                (ushort)LMCDiagnosticsResponseFlags.Partial);
            AssertChunkMalformed(partial, request);

            var totalExceedsCapacity = ChunkPayload(
                GoldenRequestId,
                0,
                2,
                77,
                false);
            TestFrame.WriteUInt32(totalExceedsCapacity, 36, 4);
            AssertChunkMalformed(totalExceedsCapacity, request);

            var noCrcIdentity = RecorderIdentity();
            var noCrcHeaderPayload = HeaderPayload(GoldenRequestId, false);
            TestFrame.WriteUInt16(
                noCrcHeaderPayload,
                42,
                (ushort)LMCRecorderHeaderFlags.CaptureComplete);
            noCrcHeaderPayload[61] = (byte)LMCRecorderDataCrcPolicy.None;
            var noCrcHeader = LMC_DiagnosticsParser.ParseRecorderHeader(
                TestFrame.Response(0, noCrcHeaderPayload),
                GoldenRequestId,
                noCrcIdentity);
            noCrcIdentity.ApplyHeaderMetadata(noCrcHeader);
            var noCrcRequest = new LMCRecorderChunkRequest(
                noCrcIdentity,
                0,
                2,
                77);
            var noCrc = ChunkPayload(GoldenRequestId, 0, 2, 77, false);
            TestFrame.WriteUInt32(noCrc, 44, 0);
            var parsedNoCrc = LMC_DiagnosticsParser.ParseRecorderChunk(
                TestFrame.Response(0, noCrc),
                GoldenRequestId,
                noCrcRequest);
            AssertEx.Equal(0u, parsedNoCrc.DataCrc32);
        }

        private static void RecorderReleaseAndAdopt()
        {
            var common = CommonPayload(16, GoldenRequestId);
            AssertEx.True(LMC_DiagnosticsParser.ParseStopRecorder(
                TestFrame.Response(0, common),
                GoldenRequestId).IsSuccess);
            AssertEx.True(LMC_DiagnosticsParser.ParseReleaseRecorderBuffer(
                TestFrame.Response(0, common),
                GoldenRequestId).IsSuccess);
            AssertEx.True(LMC_DiagnosticsParser.ParseReleaseRecorder(
                TestFrame.Response(0, common),
                GoldenRequestId).IsSuccess);

            var adoption = LMC_DiagnosticsParser.ParseAdoptRecorder(
                TestFrame.Response(0, AdoptPayload(GoldenRequestId)),
                GoldenRequestId,
                DiagnosticsBootId,
                RecordId,
                0);
            AssertEx.Equal(OwnerSessionEpoch, adoption.OwnerSessionEpoch);
            AssertEx.Equal(LMCRecorderState.Ready, adoption.State);

            var discovered = LMC_DiagnosticsParser.ParseAdoptRecorder(
                TestFrame.Response(0, AdoptPayload(GoldenRequestId)),
                GoldenRequestId,
                DiagnosticsBootId,
                0,
                0);
            AssertEx.Equal(RecordId, discovered.RecordId);
            AssertEx.Equal(0u, discovered.BufferId);

            var missingActiveRecord = AdoptPayload(GoldenRequestId);
            TestFrame.WriteUInt32(missingActiveRecord, 20, 0);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseAdoptRecorder(
                    TestFrame.Response(0, missingActiveRecord),
                    GoldenRequestId,
                    DiagnosticsBootId,
                    0,
                    0));

            var invalidActiveBuffer = AdoptPayload(GoldenRequestId);
            TestFrame.WriteUInt32(invalidActiveBuffer, 24, 1);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseAdoptRecorder(
                    TestFrame.Response(0, invalidActiveBuffer),
                    GoldenRequestId,
                    DiagnosticsBootId,
                    0,
                    0));

            var faulted = AdoptPayload(GoldenRequestId);
            TestFrame.WriteUInt16(
                faulted,
                32,
                (ushort)LMCRecorderState.Fault);
            AssertEx.Equal(
                LMCRecorderState.Fault,
                LMC_DiagnosticsParser.ParseAdoptRecorder(
                    TestFrame.Response(0, faulted),
                    GoldenRequestId,
                    DiagnosticsBootId,
                    RecordId,
                    0).State);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseAdoptRecorder(
                    TestFrame.Response(0, faulted),
                    GoldenRequestId,
                    DiagnosticsBootId,
                    0,
                    0));

            var badReserved = AdoptPayload(GoldenRequestId);
            TestFrame.WriteUInt16(badReserved, 34, 1);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseAdoptRecorder(
                    TestFrame.Response(0, badReserved),
                    GoldenRequestId,
                    DiagnosticsBootId,
                    RecordId,
                    0));

            var flagged = CommonPayload(
                16,
                GoldenRequestId,
                (ushort)LMCDiagnosticsResponseFlags.Partial);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseReleaseRecorder(
                    TestFrame.Response(0, flagged),
                    GoldenRequestId));
        }

        private static void RecorderBufferReleaseResponseLossFailClosed()
        {
            RunRecorderBufferReleaseResponseLoss(false);
            RunRecorderBufferReleaseResponseLoss(true);
        }

        private static void RecorderConfigurationReleaseResponseLossFailClosed()
        {
            RunRecorderConfigurationReleaseResponseLoss(false);
            RunRecorderConfigurationReleaseResponseLoss(true);
        }

        private static void RecorderAdoptedReleaseResponseLossFailClosed()
        {
            RunRecorderAdoptedReleaseResponseLoss(false);
            RunRecorderAdoptedReleaseResponseLoss(true);
        }

        private static void RecorderRecoveredReleaseResponseLossFailClosed()
        {
            RunRecorderRecoveredReleaseResponseLoss(false);
            RunRecorderRecoveredReleaseResponseLoss(true);
        }

        private static void RecorderBufferReleaseCommandRejectionRetry()
        {
            RunRecorderBufferReleaseCommandRejectionRetry(false);
            RunRecorderBufferReleaseCommandRejectionRetry(true);
        }

        private static void RunRecorderBufferReleaseResponseLoss(bool useAsync)
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E40,
                    TestFrame.Response(0, ConfigurePayload(2))),
                new FakeRpcStep(
                    0x7E41,
                    TestFrame.Response(0, StartPayload(3))),
                ResponseLossStep(0x7E47)))
            using (var connection = ConnectRecorder(server))
            {
                LMCRecorderConfigurationHandle handle;
                LMCRecorderIdentity identity;
                StartRecorder(connection, out handle, out identity);
                Action release = () =>
                {
                    if (useAsync)
                    {
                        connection.Diagnostics.ReleaseRecorderBufferAsync(
                                identity,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult();
                    }
                    else
                    {
                        connection.Diagnostics.ReleaseRecorderBuffer(identity);
                    }
                };

                AssertEx.Throws<EndOfStreamException>(release);
                AssertEx.False(identity.IsBufferReleased);
                AssertEx.True(identity.IsBufferReleaseOutcomeUnverified);

                var retry = AssertEx.Throws<InvalidOperationException>(release);
                AssertEx.Contains("outcome is unverified", retry.Message);
                var reuse = AssertEx.Throws<InvalidOperationException>(
                    () => connection.Diagnostics.GetRecorderStatus(identity));
                AssertEx.Contains("outcome is unverified", reuse.Message);
                server.Verify();
            }
        }

        private static void RunRecorderConfigurationReleaseResponseLoss(
            bool useAsync)
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E40,
                    TestFrame.Response(0, ConfigurePayload(2))),
                ResponseLossStep(0x7E48)))
            using (var connection = ConnectRecorder(server))
            {
                var handle = connection.Diagnostics.ConfigureRecorder(
                    ManualConfiguration());
                Action release = () =>
                {
                    if (useAsync)
                    {
                        connection.Diagnostics.ReleaseRecorderAsync(
                                handle,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult();
                    }
                    else
                    {
                        connection.Diagnostics.ReleaseRecorder(handle);
                    }
                };

                AssertEx.Throws<EndOfStreamException>(release);
                AssertEx.False(handle.IsReleased);
                AssertEx.True(handle.IsReleaseOutcomeUnverified);

                var retry = AssertEx.Throws<InvalidOperationException>(release);
                AssertEx.Contains("outcome is unverified", retry.Message);
                var reuse = AssertEx.Throws<InvalidOperationException>(
                    () => connection.Diagnostics.StartRecorder(handle));
                AssertEx.Contains("outcome is unverified", reuse.Message);
                server.Verify();
            }
        }

        private static void RunRecorderAdoptedReleaseResponseLoss(bool useAsync)
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E49,
                    TestFrame.Response(0, AdoptPayload(2))),
                new FakeRpcStep(
                    0x7E44,
                    TestFrame.Response(0, StatusPayload(3))),
                new FakeRpcStep(
                    0x7E47,
                    TestFrame.Response(0, CommonPayload(16, 4))),
                ResponseLossStep(0x7E48)))
            using (var connection = ConnectRecorder(server))
            {
                var identity = connection.Diagnostics.AdoptRecorder(
                    DiagnosticsBootId,
                    RecordId,
                    0);
                connection.Diagnostics.GetRecorderStatus(identity);
                connection.Diagnostics.ReleaseRecorderBuffer(identity);
                Action release = () =>
                {
                    if (useAsync)
                    {
                        connection.Diagnostics.ReleaseRecorderAsync(
                                identity,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult();
                    }
                    else
                    {
                        connection.Diagnostics.ReleaseRecorder(identity);
                    }
                };

                AssertEx.Throws<EndOfStreamException>(release);
                AssertEx.True(identity.IsBufferReleased);
                AssertEx.False(identity.IsRecorderReleased);
                AssertEx.True(identity.IsRecorderReleaseOutcomeUnverified);

                AssertEx.Throws<InvalidOperationException>(release);
                var reuse = AssertEx.Throws<InvalidOperationException>(
                    () => connection.Diagnostics.GetRecorderStatus(identity));
                AssertEx.Contains("outcome is unverified", reuse.Message);
                server.Verify();
            }
        }

        private static void RunRecorderRecoveredReleaseResponseLoss(bool useAsync)
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E4B,
                    TestFrame.Response(
                        0,
                        AdoptEmptyRecorderConfigurationPayload(2))),
                ResponseLossStep(0x7E48)))
            using (var connection = ConnectRecorder(server))
            {
                var lease = connection.Diagnostics
                    .AdoptEmptyRecorderConfiguration(
                        EmptyClosedRecorderBankInventory(99));
                Action release = () =>
                {
                    if (useAsync)
                    {
                        connection.Diagnostics.ReleaseRecorderAsync(
                                lease,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult();
                    }
                    else
                    {
                        connection.Diagnostics.ReleaseRecorder(lease);
                    }
                };

                AssertEx.Throws<EndOfStreamException>(release);
                AssertEx.False(lease.IsReleased);
                AssertEx.True(lease.IsReleaseOutcomeUnverified);

                var retry = AssertEx.Throws<InvalidOperationException>(release);
                AssertEx.Contains("outcome is unverified", retry.Message);
                server.Verify();
            }
        }

        private static void RunRecorderBufferReleaseCommandRejectionRetry(
            bool useAsync)
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E40,
                    TestFrame.Response(0, ConfigurePayload(2))),
                new FakeRpcStep(
                    0x7E41,
                    TestFrame.Response(0, StartPayload(3))),
                new FakeRpcStep(
                    0x7E47,
                    TestFrame.Response(
                        0,
                        DomainErrorPayload(
                            4,
                            LMCDiagnosticsDetailCode.ResourceBusy))),
                new FakeRpcStep(
                    0x7E47,
                    TestFrame.Response(0, CommonPayload(16, 5))),
                new FakeRpcStep(
                    0x7E48,
                    TestFrame.Response(0, CommonPayload(16, 6))),
                CloseStep()))
            using (var connection = ConnectRecorder(server))
            {
                LMCRecorderConfigurationHandle handle;
                LMCRecorderIdentity identity;
                StartRecorder(connection, out handle, out identity);
                Action releaseBuffer = () =>
                {
                    if (useAsync)
                    {
                        connection.Diagnostics.ReleaseRecorderBufferAsync(
                                identity,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult();
                    }
                    else
                    {
                        connection.Diagnostics.ReleaseRecorderBuffer(identity);
                    }
                };

                var rejection = AssertEx.Throws<LMCDiagnosticsCommandException>(
                    releaseBuffer);
                AssertEx.Equal(
                    LMCDiagnosticsDetailCode.ResourceBusy,
                    rejection.Response.Detail);
                AssertEx.False(identity.IsBufferReleased);
                AssertEx.False(identity.IsBufferReleaseOutcomeUnverified);

                releaseBuffer();
                AssertEx.True(identity.IsBufferReleased);
                if (useAsync)
                {
                    connection.Diagnostics.ReleaseRecorderAsync(
                            handle,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }
                else
                {
                    connection.Diagnostics.ReleaseRecorder(handle);
                }

                AssertEx.True(handle.IsReleased);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RecorderSyncAndAsync()
        {
            RunRecorderIntegration(false);
            RunRecorderIntegration(true);
        }

        private static void RecorderSingleWorkerDownload()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E40,
                    TestFrame.Response(0, ConfigurePayload(2))),
                new FakeRpcStep(
                    0x7E41,
                    TestFrame.Response(0, StartPayload(3))),
                new FakeRpcStep(
                    0x7E45,
                    TestFrame.Response(0, HeaderPayload(4, false))),
                new FakeRpcStep(
                    0x7E46,
                    TestFrame.Response(0, ChunkPayload(5, 0, 2, 1, false))),
                new FakeRpcStep(
                    0x7E46,
                    TestFrame.Response(0, ChunkPayload(6, 2, 1, 2, true))),
                new FakeRpcStep(
                    0x7E47,
                    TestFrame.Response(0, CommonPayload(16, 7))),
                new FakeRpcStep(
                    0x7E48,
                    TestFrame.Response(0, CommonPayload(16, 8))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);
                var handle = connection.Diagnostics.ConfigureRecorder(
                    ManualConfiguration());
                var identity = connection.Diagnostics.StartRecorder(handle);
                LMCRecorderDownloadProgress lastProgress = null;
                var progress = new InlineProgress<LMCRecorderDownloadProgress>(
                    value => lastProgress = value);
                var data = connection.Diagnostics.DownloadRecorderAsync(
                        identity,
                        progress,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(24, data.Data.Count);
                AssertEx.Equal(0x04030201u, data.GetRawUInt32(0, 0));
                AssertEx.Equal(0x0C0B0A09u, data.GetRawUInt32(1, 0));
                AssertEx.Equal(0x14131211u, data.GetRawUInt32(2, 0));
                AssertEx.NotNull(lastProgress);
                AssertEx.Equal(3u, lastProgress.DownloadedSamples);
                AssertEx.Equal(1.0, lastProgress.Fraction);

                connection.Diagnostics.ReleaseRecorderBuffer(identity);
                connection.Diagnostics.ReleaseRecorder(handle);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RecorderAdoptCleanup()
        {
            RunRecorderAdoptCleanup(false);
            RunRecorderAdoptCleanup(true);
        }

        private static void RecorderAdoptActive()
        {
            RunRecorderAdoptActive(false);
            RunRecorderAdoptActive(true);
            RunRecorderAdoptActiveDoubleBankRejected();
            RunRecorderAdoptActivePreCanceled();

            using (var connection = new LMCConnection())
            {
                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => connection.Diagnostics.AdoptActiveRecorder(0));
                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => connection.Diagnostics.AdoptActiveRecorderAsync(
                            0,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
            }
        }

        private static void RunRecorderAdoptActive(bool useAsync)
        {
            var adoptStep = new FakeRpcStep(
                0x7E49,
                TestFrame.Response(0, AdoptPayload(2)));
            adoptStep.InspectRequest = request =>
            {
                AssertRequestHeader(request, 0x7E49, 20, 2);
                AssertEx.Equal(0u, TestFrame.ReadUInt32(request, 16));
                AssertEx.Equal(0u, TestFrame.ReadUInt32(request, 20));
                AssertEx.Equal(
                    DiagnosticsBootId,
                    TestFrame.ReadUInt32(request, 24));
            };

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        SingleBankCapabilitiesPayload(1))),
                adoptStep,
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var identity = useAsync
                    ? connection.Diagnostics.AdoptActiveRecorderAsync(
                            DiagnosticsBootId,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult()
                    : connection.Diagnostics.AdoptActiveRecorder(
                        DiagnosticsBootId);

                AssertEx.Equal(RecordId, identity.RecordId);
                AssertEx.Equal(0u, identity.BufferId);
                AssertEx.Equal(
                    OwnerSessionEpoch,
                    identity.OwnerSessionEpoch);
                AssertEx.True(identity.IsAdopted);
                AssertEx.False(identity.HasConfigurationShape);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RunRecorderAdoptActiveDoubleBankRejected()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);
                AssertEx.Throws<NotSupportedException>(
                    () => connection.Diagnostics.AdoptActiveRecorder(
                        DiagnosticsBootId));
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RunRecorderAdoptActivePreCanceled()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);
                cancellation.Cancel();
                AssertEx.Throws<OperationCanceledException>(
                    () => connection.Diagnostics.AdoptActiveRecorderAsync(
                            DiagnosticsBootId,
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RunRecorderAdoptCleanup(bool useAsync)
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E49,
                    TestFrame.Response(0, AdoptPayload(2))),
                new FakeRpcStep(
                    0x7E44,
                    TestFrame.Response(0, StatusPayload(3))),
                new FakeRpcStep(
                    0x7E47,
                    TestFrame.Response(0, CommonPayload(16, 4))),
                new FakeRpcStep(
                    0x7E48,
                    TestFrame.Response(0, CommonPayload(16, 5))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                LMCRecorderIdentity identity;
                if (useAsync)
                {
                    identity = connection.Diagnostics.AdoptRecorderAsync(
                            DiagnosticsBootId,
                            RecordId,
                            0,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }
                else
                {
                    identity = connection.Diagnostics.AdoptRecorder(
                        DiagnosticsBootId,
                        RecordId,
                        0);
                }

                AssertEx.False(identity.HasConfigurationMetadata);
                AssertEx.False(identity.HasConfigurationShape);
                AssertEx.Throws<InvalidOperationException>(
                    () => connection.Diagnostics.ReleaseRecorderBuffer(identity));

                if (useAsync)
                {
                    connection.Diagnostics.GetRecorderStatusAsync(
                            identity,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    connection.Diagnostics.ReleaseRecorderBufferAsync(
                            identity,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    connection.Diagnostics.ReleaseRecorderAsync(
                            identity,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }
                else
                {
                    connection.Diagnostics.GetRecorderStatus(identity);
                    connection.Diagnostics.ReleaseRecorderBuffer(identity);
                    connection.Diagnostics.ReleaseRecorder(identity);
                }

                AssertEx.True(identity.HasConfigurationMetadata);
                AssertEx.True(identity.IsBufferReleased);
                AssertEx.True(identity.IsRecorderReleased);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RecorderReconnectExactAndDiscovery()
        {
            RunRecorderReconnect(false);
            RunRecorderReconnect(true);
        }

        private static void RecorderPreCloseTransportFaultExactRecovery()
        {
            LMCRecorderIdentity originalIdentity;
            var truncatedStatusResponse = new byte[10];
            TestFrame.WriteUInt16(truncatedStatusResponse, 2, 76);
            truncatedStatusResponse[8] = 0xAA;
            truncatedStatusResponse[9] = 0xBB;

            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        SingleBankCapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E40,
                    TestFrame.Response(0, ConfigurePayload(2))),
                new FakeRpcStep(
                    0x7E41,
                    TestFrame.Response(0, StartPayload(3))),
                new FakeRpcStep(0x7E44, truncatedStatusResponse)
                {
                    CloseAfterResponse = true
                }))
            using (var firstConnection = new LMCConnection())
            {
                firstConnection.RpcInitConnection(
                    "127.0.0.1",
                    firstServer.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);
                var handle = firstConnection.Diagnostics.ConfigureRecorder(
                    ManualConfiguration());
                originalIdentity = firstConnection.Diagnostics.StartRecorder(
                    handle);

                AssertEx.Throws<EndOfStreamException>(
                    () => firstConnection.Diagnostics.GetRecorderStatus(
                        originalIdentity));
                AssertEx.False(firstConnection.IsConnected);
                AssertEx.Equal(
                    RecorderReconnectCleanupRoute.ExactReconnect,
                    RecorderReconnectQualificationPolicy.SelectCleanupRoute(
                        firstConnection.IsConnected,
                        true,
                        false,
                        false));
                firstServer.Verify();
            }

            var adoptPayload = AdoptPayload(
                3,
                ReconnectedOwnerSessionEpoch);
            TestFrame.WriteUInt16(
                adoptPayload,
                32,
                (ushort)LMCRecorderState.Recording);
            var recordingStatus = StatusPayload(
                4,
                ReconnectedOwnerSessionEpoch);
            TestFrame.WriteUInt16(
                recordingStatus,
                36,
                (ushort)LMCRecorderState.Recording);
            recordingStatus[39] = (byte)LMCRecorderStopReason.None;

            using (var secondServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        SingleBankCapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        SingleBankCapabilitiesPayload(2))),
                new FakeRpcStep(
                    0x7E49,
                    TestFrame.Response(0, adoptPayload)),
                new FakeRpcStep(
                    0x7E44,
                    TestFrame.Response(0, recordingStatus)),
                new FakeRpcStep(
                    0x7E43,
                    TestFrame.Response(0, CommonPayload(16, 5))),
                new FakeRpcStep(
                    0x7E44,
                    TestFrame.Response(
                        0,
                        StatusPayload(6, ReconnectedOwnerSessionEpoch))),
                new FakeRpcStep(
                    0x7E47,
                    TestFrame.Response(0, CommonPayload(16, 7))),
                new FakeRpcStep(
                    0x7E48,
                    TestFrame.Response(0, CommonPayload(16, 8))),
                CloseStep()))
            using (var secondConnection = new LMCConnection())
            {
                secondConnection.RpcInitConnection(
                    "127.0.0.1",
                    secondServer.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var capabilities =
                    secondConnection.Diagnostics.GetCapabilities();
                AssertEx.Equal(
                    originalIdentity.DiagnosticsBootId,
                    capabilities.DiagnosticsBootId);
                var recovered = secondConnection.Diagnostics.AdoptRecorder(
                    originalIdentity.DiagnosticsBootId,
                    originalIdentity.RecordId,
                    originalIdentity.BufferId);
                AssertEx.Equal(originalIdentity.RecordId, recovered.RecordId);
                AssertEx.Equal(
                    ReconnectedOwnerSessionEpoch,
                    recovered.OwnerSessionEpoch);

                var status = secondConnection.Diagnostics.GetRecorderStatus(
                    recovered);
                AssertEx.Equal(LMCRecorderState.Recording, status.State);
                secondConnection.Diagnostics.StopRecorder(recovered);
                status = secondConnection.Diagnostics.GetRecorderStatus(
                    recovered);
                AssertEx.True(status.IsFrozen);
                secondConnection.Diagnostics.ReleaseRecorderBuffer(recovered);
                secondConnection.Diagnostics.ReleaseRecorder(recovered);
                AssertEx.True(recovered.IsBufferReleased);
                AssertEx.True(recovered.IsRecorderReleased);

                secondConnection.CloseConnection();
                secondServer.Verify();
            }
        }

        private static void RecorderFaultPreservesWithoutMutation()
        {
            var adoptPayload = AdoptPayload(
                3,
                ReconnectedOwnerSessionEpoch);
            TestFrame.WriteUInt16(
                adoptPayload,
                32,
                (ushort)LMCRecorderState.Fault);
            var faultStatus = StatusPayload(
                4,
                ReconnectedOwnerSessionEpoch);
            TestFrame.WriteUInt16(
                faultStatus,
                36,
                (ushort)LMCRecorderState.Fault);
            faultStatus[39] = (byte)LMCRecorderStopReason.Error;

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        SingleBankCapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        SingleBankCapabilitiesPayload(2))),
                new FakeRpcStep(
                    0x7E49,
                    TestFrame.Response(0, adoptPayload)),
                new FakeRpcStep(
                    0x7E44,
                    TestFrame.Response(0, faultStatus)),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);
                connection.Diagnostics.GetCapabilities();
                var identity = connection.Diagnostics.AdoptRecorder(
                    DiagnosticsBootId,
                    RecordId,
                    0);
                var status = connection.Diagnostics.GetRecorderStatus(identity);
                AssertEx.Equal(LMCRecorderState.Fault, status.State);
                AssertEx.Equal(
                    RecorderQualificationCleanupAction.Preserve,
                    RecorderReconnectQualificationPolicy.SelectCleanupAction(
                        status.State));
                AssertEx.False(identity.IsBufferReleased);
                AssertEx.False(identity.IsRecorderReleased);

                connection.CloseConnection();
                server.Verify();
                foreach (var request in server.ReceivedRequests)
                {
                    var command = TestFrame.ReadUInt16(request, 0);
                    AssertEx.False(command == 0x7E43);
                    AssertEx.False(command == 0x7E47);
                    AssertEx.False(command == 0x7E48);
                }
            }
        }

        private static void RecorderPreStartCancellationCleanup()
        {
            using (var server = CreateConfiguredCleanupServer(
                new FakeRpcStep(
                    0x7E48,
                    TestFrame.Response(0, CommonPayload(16, 3)))))
            using (var connection = ConnectRecorder(server))
            {
                var handle = connection.Diagnostics.ConfigureRecorder(
                    ManualConfiguration());
                var operations = CreateOwnedCleanupOperations(
                    connection.Diagnostics,
                    handle,
                    null);
                var primary = new OperationCanceledException(
                    "canceled after Configure ACK");
                var observed = AssertEx.Throws<OperationCanceledException>(
                    () => RecorderQualificationCleanupOrchestrator
                        .CleanupAndRethrowPrimaryAsync(
                            primary,
                            () => CleanupOwnedResourcesAsync(operations),
                            CreateQualificationAggregate)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.True(ReferenceEquals(primary, observed));
                AssertEx.True(handle.IsReleased);
                AssertRecorderCleanupCommandCounts(server, 0, 0, 0, 1);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RecorderActiveCancellationCleanup()
        {
            using (var server = CreateStartedCleanupServer(
                StatusStep(4, LMCRecorderState.Recording),
                new FakeRpcStep(
                    0x7E43,
                    TestFrame.Response(0, CommonPayload(16, 5))),
                StatusStep(6, LMCRecorderState.Ready),
                new FakeRpcStep(
                    0x7E47,
                    TestFrame.Response(0, CommonPayload(16, 7))),
                new FakeRpcStep(
                    0x7E48,
                    TestFrame.Response(0, CommonPayload(16, 8)))))
            using (var connection = ConnectRecorder(server))
            {
                LMCRecorderConfigurationHandle handle;
                LMCRecorderIdentity identity;
                StartRecorder(connection, out handle, out identity);
                var operations = CreateOwnedCleanupOperations(
                    connection.Diagnostics,
                    handle,
                    identity);
                var primary = new OperationCanceledException(
                    "canceled while Recorder was active");
                var observed = AssertEx.Throws<OperationCanceledException>(
                    () => RecorderQualificationCleanupOrchestrator
                        .CleanupAndRethrowPrimaryAsync(
                            primary,
                            () => CleanupOwnedResourcesAsync(operations),
                            CreateQualificationAggregate)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.True(ReferenceEquals(primary, observed));
                AssertEx.True(identity.IsBufferReleased);
                AssertEx.True(handle.IsReleased);
                AssertRecorderCleanupCommandCounts(server, 2, 1, 1, 1);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RecorderUploadingCancellationCleanup()
        {
            using (var server = CreateStartedCleanupServer(
                StatusStep(4, LMCRecorderState.Uploading),
                new FakeRpcStep(
                    0x7E47,
                    TestFrame.Response(0, CommonPayload(16, 5))),
                new FakeRpcStep(
                    0x7E48,
                    TestFrame.Response(0, CommonPayload(16, 6)))))
            using (var connection = ConnectRecorder(server))
            {
                LMCRecorderConfigurationHandle handle;
                LMCRecorderIdentity identity;
                StartRecorder(connection, out handle, out identity);
                var operations = CreateOwnedCleanupOperations(
                    connection.Diagnostics,
                    handle,
                    identity);
                var primary = new OperationCanceledException(
                    "canceled between Recorder chunks");
                var observed = AssertEx.Throws<OperationCanceledException>(
                    () => RecorderQualificationCleanupOrchestrator
                        .CleanupAndRethrowPrimaryAsync(
                            primary,
                            () => CleanupOwnedResourcesAsync(operations),
                            CreateQualificationAggregate)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.True(ReferenceEquals(primary, observed));
                AssertEx.True(identity.IsBufferReleased);
                AssertEx.True(handle.IsReleased);
                AssertRecorderCleanupCommandCounts(server, 1, 0, 1, 1);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RecorderStopInvalidStateReadyRace()
        {
            using (var server = CreateStartedCleanupServer(
                StatusStep(4, LMCRecorderState.Recording),
                new FakeRpcStep(
                    0x7E43,
                    TestFrame.Response(
                        0,
                        DomainErrorPayload(
                            5,
                            LMCDiagnosticsDetailCode.InvalidState))),
                StatusStep(6, LMCRecorderState.Ready),
                new FakeRpcStep(
                    0x7E47,
                    TestFrame.Response(0, CommonPayload(16, 7))),
                new FakeRpcStep(
                    0x7E48,
                    TestFrame.Response(0, CommonPayload(16, 8)))))
            using (var connection = ConnectRecorder(server))
            {
                LMCRecorderConfigurationHandle handle;
                LMCRecorderIdentity identity;
                StartRecorder(connection, out handle, out identity);
                var result = CleanupOwnedResourcesAsync(
                        CreateOwnedCleanupOperations(
                            connection.Diagnostics,
                            handle,
                            identity))
                    .GetAwaiter()
                    .GetResult();

                AssertEx.True(result.StopAttempted);
                AssertEx.True(result.StopRaceResolved);
                AssertEx.Equal(2, result.StatusReadCount);
                AssertEx.True(identity.IsBufferReleased);
                AssertEx.True(handle.IsReleased);
                AssertRecorderCleanupCommandCounts(server, 2, 1, 1, 1);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RecorderStopInvalidStateFaultPreserves()
        {
            using (var server = CreateStartedCleanupServer(
                StatusStep(4, LMCRecorderState.Recording),
                new FakeRpcStep(
                    0x7E43,
                    TestFrame.Response(
                        0,
                        DomainErrorPayload(
                            5,
                            LMCDiagnosticsDetailCode.InvalidState))),
                StatusStep(6, LMCRecorderState.Fault)))
            using (var connection = ConnectRecorder(server))
            {
                LMCRecorderConfigurationHandle handle;
                LMCRecorderIdentity identity;
                StartRecorder(connection, out handle, out identity);
                var recoveryRequiredCount = 0;
                LMCRecorderState recoveryState = LMCRecorderState.Empty;
                var operations = CreateOwnedCleanupOperations(
                    connection.Diagnostics,
                    handle,
                    identity);
                operations.RecoveryRequired = status =>
                {
                    recoveryRequiredCount++;
                    recoveryState = status.State;
                };
                var observed = AssertEx.Throws<LMCDiagnosticsCommandException>(
                    () => CleanupOwnedResourcesAsync(operations)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCDiagnosticsDetailCode.InvalidState,
                    observed.Response.Detail);
                AssertEx.False(identity.IsBufferReleased);
                AssertEx.False(handle.IsReleased);
                AssertEx.Equal(1, recoveryRequiredCount);
                AssertEx.Equal(LMCRecorderState.Fault, recoveryState);
                AssertRecorderCleanupCommandCounts(server, 2, 1, 0, 0);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RecorderBufferReleaseFailureBlocksConfig()
        {
            using (var server = CreateStartedCleanupServer(
                StatusStep(4, LMCRecorderState.Ready),
                new FakeRpcStep(
                    0x7E47,
                    TestFrame.Response(
                        0,
                        DomainErrorPayload(
                            5,
                            LMCDiagnosticsDetailCode.ResourceBusy)))))
            using (var connection = ConnectRecorder(server))
            {
                LMCRecorderConfigurationHandle handle;
                LMCRecorderIdentity identity;
                StartRecorder(connection, out handle, out identity);
                var observed = AssertEx.Throws<LMCDiagnosticsCommandException>(
                    () => CleanupOwnedResourcesAsync(
                            CreateOwnedCleanupOperations(
                                connection.Diagnostics,
                                handle,
                                identity))
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCDiagnosticsDetailCode.ResourceBusy,
                    observed.Response.Detail);
                AssertEx.False(identity.IsBufferReleased);
                AssertEx.False(handle.IsReleased);
                AssertRecorderCleanupCommandCounts(server, 1, 0, 1, 0);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RecorderConfigReleaseFailureRetry()
        {
            using (var server = CreateStartedCleanupServer(
                StatusStep(4, LMCRecorderState.Ready),
                new FakeRpcStep(
                    0x7E47,
                    TestFrame.Response(0, CommonPayload(16, 5))),
                new FakeRpcStep(
                    0x7E48,
                    TestFrame.Response(
                        0,
                        DomainErrorPayload(
                            6,
                            LMCDiagnosticsDetailCode.ResourceBusy))),
                new FakeRpcStep(
                    0x7E48,
                    TestFrame.Response(0, CommonPayload(16, 7)))))
            using (var connection = ConnectRecorder(server))
            {
                LMCRecorderConfigurationHandle handle;
                LMCRecorderIdentity identity;
                StartRecorder(connection, out handle, out identity);
                var operations = CreateOwnedCleanupOperations(
                    connection.Diagnostics,
                    handle,
                    identity);
                var primary = new InvalidOperationException(
                    "qualification body failed");
                var observed = AssertEx.Throws<InvalidOperationException>(
                    () => RecorderQualificationCleanupOrchestrator
                        .CleanupAndRethrowPrimaryAsync(
                            primary,
                            () => CleanupOwnedResourcesAsync(operations),
                            CreateQualificationAggregate)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.True(observed.InnerException is AggregateException);
                var aggregate = (AggregateException)observed.InnerException;
                AssertEx.Equal(2, aggregate.InnerExceptions.Count);
                AssertEx.True(ReferenceEquals(
                    primary,
                    aggregate.InnerExceptions[0]));
                AssertEx.True(identity.IsBufferReleased);
                AssertEx.False(handle.IsReleased);
                AssertEx.False(handle.IsReleaseOutcomeUnverified);

                CleanupOwnedResourcesAsync(operations)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(identity.IsBufferReleased);
                AssertEx.True(handle.IsReleased);
                AssertEx.False(handle.IsReleaseOutcomeUnverified);
                AssertRecorderCleanupCommandCounts(server, 1, 0, 1, 2);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RecorderFaultNoMutationProductionPath()
        {
            using (var server = CreateStartedCleanupServer(
                StatusStep(4, LMCRecorderState.Fault)))
            using (var connection = ConnectRecorder(server))
            {
                LMCRecorderConfigurationHandle handle;
                LMCRecorderIdentity identity;
                StartRecorder(connection, out handle, out identity);
                AssertEx.Throws<InvalidOperationException>(
                    () => CleanupOwnedResourcesAsync(
                            CreateOwnedCleanupOperations(
                                connection.Diagnostics,
                                handle,
                                identity))
                        .GetAwaiter()
                        .GetResult());

                AssertEx.False(identity.IsBufferReleased);
                AssertEx.False(handle.IsReleased);
                AssertRecorderCleanupCommandCounts(server, 1, 0, 0, 0);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RecorderReconnectStatusMismatchNoMutation()
        {
            var mismatchedStatus = StatusPayload(3);
            TestFrame.WriteUInt32(
                mismatchedStatus,
                24,
                unchecked(ConfigId + 1));
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E49,
                    TestFrame.Response(0, AdoptPayload(2))),
                new FakeRpcStep(
                    0x7E44,
                    TestFrame.Response(0, mismatchedStatus)),
                CloseStep()))
            using (var connection = ConnectRecorder(server))
            {
                var identity = connection.Diagnostics.AdoptRecorder(
                    DiagnosticsBootId,
                    RecordId,
                    0);
                RecorderQualificationCleanupOrchestrator
                    .ValidateReconnectAdoption(
                        identity,
                        DiagnosticsBootId,
                        RecordId,
                        0,
                        MapRevision,
                        ReconnectedOwnerSessionEpoch);
                var operations =
                    new RecorderQualificationCleanupOperations
                    {
                        ReadStatusAsync = () =>
                            connection.Diagnostics.GetRecorderStatusAsync(
                                identity,
                                CancellationToken.None),
                        StopAsync = () =>
                            connection.Diagnostics.StopRecorderAsync(
                                identity,
                                CancellationToken.None),
                        ValidateStatus = status =>
                            RecorderQualificationCleanupOrchestrator
                                .ValidateReconnectStatus(
                                    status,
                                    identity,
                                    DiagnosticsBootId,
                                    RecordId,
                                    0,
                                    ConfigId,
                                    ConfigRevision,
                                    MapRevision,
                                    3),
                        IsBufferReleasePending = () =>
                            !identity.IsBufferReleased,
                        IsConfigurationReleasePending = () =>
                            !identity.IsRecorderReleased,
                        ReleaseBufferAsync = () =>
                            connection.Diagnostics.ReleaseRecorderBufferAsync(
                                identity,
                                CancellationToken.None),
                        ReleaseConfigurationAsync = () =>
                            connection.Diagnostics.ReleaseRecorderAsync(
                                identity,
                                CancellationToken.None),
                        DelayAsync = milliseconds => Task.FromResult(0)
                    };

                AssertEx.Throws<InvalidOperationException>(
                    () => CleanupOwnedResourcesAsync(operations)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.False(identity.IsBufferReleased);
                AssertEx.False(identity.IsRecorderReleased);
                AssertRecorderCleanupCommandCounts(server, 1, 0, 0, 0);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RecorderUnvalidatedAdoptionQuarantined()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E49,
                    TestFrame.Response(0, AdoptPayload(2))),
                CloseStep()))
            using (var connection = ConnectRecorder(server))
            {
                var identity = connection.Diagnostics.AdoptRecorder(
                    DiagnosticsBootId,
                    RecordId,
                    0);
                var validationError = AssertEx.Throws<InvalidOperationException>(
                    () => RecorderQualificationCleanupOrchestrator
                        .ValidateReconnectAdoption(
                            identity,
                            DiagnosticsBootId,
                            RecordId,
                            0,
                            MapRevision,
                            OwnerSessionEpoch));
                AssertEx.True(validationError != null);

                LMCRecorderIdentity retainedIdentity = null;
                RecorderReconnectQualificationPolicy
                    .QuarantineUnvalidatedAdoption(
                        identity,
                        false,
                        value => retainedIdentity = value);

                AssertEx.True(ReferenceEquals(identity, retainedIdentity));
                AssertEx.False(retainedIdentity.IsBufferReleased);
                AssertEx.False(retainedIdentity.IsRecorderReleased);
                AssertRecorderCleanupCommandCounts(server, 0, 0, 0, 0);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RecorderAdoptedConfigReleaseFailureRetry()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E49,
                    TestFrame.Response(0, AdoptPayload(2))),
                new FakeRpcStep(
                    0x7E44,
                    TestFrame.Response(0, StatusPayload(3))),
                new FakeRpcStep(
                    0x7E47,
                    TestFrame.Response(0, CommonPayload(16, 4))),
                new FakeRpcStep(
                    0x7E48,
                    TestFrame.Response(
                        0,
                        DomainErrorPayload(
                            5,
                            LMCDiagnosticsDetailCode.ResourceBusy))),
                new FakeRpcStep(
                    0x7E48,
                    TestFrame.Response(0, CommonPayload(16, 6))),
                CloseStep()))
            using (var connection = ConnectRecorder(server))
            {
                var identity = connection.Diagnostics.AdoptRecorder(
                    DiagnosticsBootId,
                    RecordId,
                    0);
                RecorderQualificationCleanupOrchestrator
                    .ValidateReconnectAdoption(
                        identity,
                        DiagnosticsBootId,
                        RecordId,
                        0,
                        MapRevision,
                        ReconnectedOwnerSessionEpoch);
                var operations = CreateAdoptedCleanupOperations(
                    connection.Diagnostics,
                    identity);

                var observed = AssertEx.Throws<LMCDiagnosticsCommandException>(
                    () => CleanupOwnedResourcesAsync(operations)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(
                    LMCDiagnosticsDetailCode.ResourceBusy,
                    observed.Response.Detail);
                AssertEx.True(identity.IsBufferReleased);
                AssertEx.False(identity.IsRecorderReleased);
                AssertEx.False(
                    identity.IsRecorderReleaseOutcomeUnverified);

                CleanupOwnedResourcesAsync(operations)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(identity.IsBufferReleased);
                AssertEx.True(identity.IsRecorderReleased);
                AssertEx.False(
                    identity.IsRecorderReleaseOutcomeUnverified);
                AssertRecorderCleanupCommandCounts(server, 1, 0, 1, 2);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RecorderPostRpcCancellationAggregatesCleanupFailure()
        {
            using (var cancellation = new CancellationTokenSource())
            {
                var capabilitiesStep = new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1)));
                capabilitiesStep.InspectRequest = request =>
                {
                    cancellation.Cancel();
                };
                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    capabilitiesStep,
                    CloseStep()))
                using (var connection = ConnectRecorder(server))
                {
                    var capabilities = connection.Diagnostics.GetCapabilities();
                    AssertEx.True(capabilities != null);
                    var primary = AssertEx.Throws<OperationCanceledException>(
                        () => RecorderQualificationCleanupOrchestrator
                            .ThrowIfCancellationRequestedAfterRpc(
                                cancellation.Token));
                    var cleanup = new InvalidOperationException(
                        "simulated Recorder Release failure");
                    var observed = AssertEx.Throws<InvalidOperationException>(
                        () => RecorderQualificationCleanupOrchestrator
                            .CleanupAndRethrowPrimaryAsync(
                                primary,
                                () => Task.FromException(cleanup),
                                CreateQualificationAggregate)
                            .GetAwaiter()
                            .GetResult());

                    AssertEx.True(observed.InnerException is AggregateException);
                    var aggregate = (AggregateException)observed.InnerException;
                    AssertEx.Equal(2, aggregate.InnerExceptions.Count);
                    AssertEx.True(ReferenceEquals(
                        primary,
                        aggregate.InnerExceptions[0]));
                    AssertEx.True(ReferenceEquals(
                        cleanup,
                        aggregate.InnerExceptions[1]));
                    connection.CloseConnection();
                    server.Verify();
                }
            }
        }

        private static void RunRecorderReconnect(bool discoverActive)
        {
            LMCRecorderIdentity originalIdentity;
            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        SingleBankCapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E40,
                    TestFrame.Response(0, ConfigurePayload(2))),
                new FakeRpcStep(
                    0x7E41,
                    TestFrame.Response(0, StartPayload(3))),
                CloseStep()))
            using (var firstConnection = new LMCConnection())
            {
                firstConnection.RpcInitConnection(
                    "127.0.0.1",
                    firstServer.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);
                var handle = firstConnection.Diagnostics.ConfigureRecorder(
                    ManualConfiguration());
                originalIdentity = firstConnection.Diagnostics.StartRecorder(
                    handle);
                firstConnection.CloseConnection();
                firstServer.Verify();
            }

            var adoptStep = new FakeRpcStep(
                0x7E49,
                TestFrame.Response(
                    0,
                    AdoptPayload(3, ReconnectedOwnerSessionEpoch)));
            adoptStep.InspectRequest = request =>
            {
                AssertRequestHeader(request, 0x7E49, 20, 3);
                AssertEx.Equal(
                    discoverActive ? 0u : originalIdentity.RecordId,
                    TestFrame.ReadUInt32(request, 16));
                AssertEx.Equal(0u, TestFrame.ReadUInt32(request, 20));
                AssertEx.Equal(
                    originalIdentity.DiagnosticsBootId,
                    TestFrame.ReadUInt32(request, 24));
            };

            using (var secondServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        SingleBankCapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        SingleBankCapabilitiesPayload(2))),
                adoptStep,
                new FakeRpcStep(
                    0x7E44,
                    TestFrame.Response(
                        0,
                        StatusPayload(4, ReconnectedOwnerSessionEpoch))),
                new FakeRpcStep(
                    0x7E45,
                    TestFrame.Response(0, HeaderPayload(5, false))),
                new FakeRpcStep(
                    0x7E46,
                    TestFrame.Response(
                        0,
                        ChunkPayload(6, 0, 2, 1, false))),
                new FakeRpcStep(
                    0x7E46,
                    TestFrame.Response(
                        0,
                        ChunkPayload(7, 2, 1, 2, true))),
                new FakeRpcStep(
                    0x7E47,
                    TestFrame.Response(0, CommonPayload(16, 8))),
                new FakeRpcStep(
                    0x7E48,
                    TestFrame.Response(0, CommonPayload(16, 9))),
                CloseStep()))
            using (var secondConnection = new LMCConnection())
            {
                secondConnection.RpcInitConnection(
                    "127.0.0.1",
                    secondServer.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var refreshedCapabilities =
                    secondConnection.Diagnostics.GetCapabilities();
                AssertEx.Equal(
                    originalIdentity.DiagnosticsBootId,
                    refreshedCapabilities.DiagnosticsBootId);

                var adoptedIdentity = discoverActive
                    ? secondConnection.Diagnostics.AdoptActiveRecorder(
                        originalIdentity.DiagnosticsBootId)
                    : secondConnection.Diagnostics.AdoptRecorder(
                        originalIdentity.DiagnosticsBootId,
                        originalIdentity.RecordId,
                        originalIdentity.BufferId);
                AssertEx.Equal(originalIdentity.RecordId, adoptedIdentity.RecordId);
                AssertEx.Equal(originalIdentity.BufferId, adoptedIdentity.BufferId);
                AssertEx.Equal(
                    ReconnectedOwnerSessionEpoch,
                    adoptedIdentity.OwnerSessionEpoch);
                AssertEx.True(
                    adoptedIdentity.OwnerSessionEpoch
                        != originalIdentity.OwnerSessionEpoch);

                var status = secondConnection.Diagnostics.GetRecorderStatus(
                    adoptedIdentity);
                AssertEx.Equal(ConfigId, status.ConfigId);
                AssertEx.Equal(ConfigRevision, status.ConfigRevision);
                AssertEx.True(adoptedIdentity.HasConfigurationMetadata);

                var data = secondConnection.Diagnostics.DownloadRecorderAsync(
                        adoptedIdentity,
                        null,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(24, data.Data.Count);
                secondConnection.Diagnostics.ReleaseRecorderBuffer(
                    adoptedIdentity);
                secondConnection.Diagnostics.ReleaseRecorder(adoptedIdentity);
                AssertEx.True(adoptedIdentity.IsBufferReleased);
                AssertEx.True(adoptedIdentity.IsRecorderReleased);
                secondConnection.CloseConnection();
                secondServer.Verify();
            }
        }

        private static void RecorderStatefulCancellationBoundary()
        {
            using (var cancellation = new CancellationTokenSource())
            {
                var configureStep = new FakeRpcStep(
                    0x7E40,
                    TestFrame.Response(0, ConfigurePayload(2)));
                configureStep.InspectRequest = request => cancellation.Cancel();

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    new FakeRpcStep(
                        0x7E00,
                        TestFrame.Response(0, CapabilitiesPayload(1))),
                    configureStep,
                    new FakeRpcStep(
                        0x7E48,
                        TestFrame.Response(0, CommonPayload(16, 3))),
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    connection.RpcInitConnection(
                        "127.0.0.1",
                        server.Port,
                        "127.0.0.1",
                        0,
                        LMCConnection.DefaultEventMask);

                    var handle = connection.Diagnostics.ConfigureRecorderAsync(
                            ManualConfiguration(),
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult();
                    AssertEx.True(cancellation.IsCancellationRequested);
                    AssertEx.Equal(ConfigId, handle.ConfigId);
                    connection.Diagnostics.ReleaseRecorder(handle);
                    connection.CloseConnection();
                    server.Verify();
                }
            }
        }

        private static void RecorderBootIdMismatchInvalidatesHandles()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E40,
                    TestFrame.Response(0, ConfigurePayload(2))),
                new FakeRpcStep(
                    0x7E41,
                    TestFrame.Response(
                        0,
                        DomainErrorPayload(
                            3,
                            LMCDiagnosticsDetailCode.BootIdMismatch))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);
                var handle = connection.Diagnostics.ConfigureRecorder(
                    ManualConfiguration());

                var exception = AssertEx.Throws<LMCDiagnosticsCommandException>(
                    () => connection.Diagnostics.StartRecorder(handle));
                AssertEx.Equal(
                    LMCDiagnosticsDetailCode.BootIdMismatch,
                    exception.Response.Detail);
                AssertEx.Throws<InvalidOperationException>(
                    () => connection.Diagnostics.StartRecorder(handle));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RunRecorderIntegration(bool useAsync)
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E40,
                    TestFrame.Response(0, ConfigurePayload(2))),
                new FakeRpcStep(
                    0x7E41,
                    TestFrame.Response(0, StartPayload(3))),
                new FakeRpcStep(
                    0x7E43,
                    TestFrame.Response(0, CommonPayload(16, 4))),
                new FakeRpcStep(
                    0x7E44,
                    TestFrame.Response(0, StatusPayload(5))),
                new FakeRpcStep(
                    0x7E45,
                    TestFrame.Response(0, HeaderPayload(6, false))),
                new FakeRpcStep(
                    0x7E46,
                    TestFrame.Response(0, ChunkPayload(7, 0, 2, 1, false))),
                new FakeRpcStep(
                    0x7E46,
                    TestFrame.Response(0, ChunkPayload(8, 2, 1, 2, true))),
                new FakeRpcStep(
                    0x7E47,
                    TestFrame.Response(0, CommonPayload(16, 9))),
                new FakeRpcStep(
                    0x7E48,
                    TestFrame.Response(0, CommonPayload(16, 10))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var configuration = ManualConfiguration();
                LMCRecorderConfigurationHandle handle;
                LMCRecorderIdentity identity;
                LMCRecorderStatus status;
                LMCRecorderHeader header;
                LMCRecorderChunk first;
                LMCRecorderChunk last;
                if (useAsync)
                {
                    handle = connection.Diagnostics.ConfigureRecorderAsync(
                            configuration,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    identity = connection.Diagnostics.StartRecorderAsync(
                            handle,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    connection.Diagnostics.StopRecorderAsync(
                            identity,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    status = connection.Diagnostics.GetRecorderStatusAsync(
                            identity,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    header = connection.Diagnostics.GetRecorderHeaderAsync(
                            identity,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    first = connection.Diagnostics.ReadRecorderChunkAsync(
                            new LMCRecorderChunkRequest(identity, 0, 2, 1),
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    last = connection.Diagnostics.ReadRecorderChunkAsync(
                            new LMCRecorderChunkRequest(identity, 2, 1, 2),
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }
                else
                {
                    handle = connection.Diagnostics.ConfigureRecorder(configuration);
                    identity = connection.Diagnostics.StartRecorder(handle);
                    connection.Diagnostics.StopRecorder(identity);
                    status = connection.Diagnostics.GetRecorderStatus(identity);
                    header = connection.Diagnostics.GetRecorderHeader(identity);
                    first = connection.Diagnostics.ReadRecorderChunk(
                        new LMCRecorderChunkRequest(identity, 0, 2, 1));
                    last = connection.Diagnostics.ReadRecorderChunk(
                        new LMCRecorderChunkRequest(identity, 2, 1, 2));
                }

                AssertEx.Equal(ConfigId, handle.ConfigId);
                AssertEx.Equal(RecordId, identity.RecordId);
                AssertEx.True(status.IsFrozen);
                AssertEx.Equal((ushort)2, header.ChannelCount);
                AssertEx.Equal(16, first.DataByteCount);
                AssertEx.True(last.IsLastChunk);

                using (var otherConnection = new LMCConnection())
                {
                    AssertEx.Throws<InvalidOperationException>(
                        () => otherConnection.Diagnostics.GetRecorderStatus(identity));
                }

                if (useAsync)
                {
                    connection.Diagnostics.ReleaseRecorderBufferAsync(
                            identity,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    connection.Diagnostics.ReleaseRecorderAsync(
                            handle,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }
                else
                {
                    connection.Diagnostics.ReleaseRecorderBuffer(identity);
                    connection.Diagnostics.ReleaseRecorder(handle);
                }

                AssertEx.True(identity.IsBufferReleased);
                AssertEx.True(handle.IsReleased);
                AssertEx.Throws<InvalidOperationException>(
                    () => connection.Diagnostics.GetRecorderStatus(identity));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static LMCRecorderConfiguration ManualConfiguration()
        {
            return new LMCRecorderConfiguration(Signals, 1, 3);
        }

        private static LMCRecorderConfiguration
            RecoverableDoubleConfiguration()
        {
            return new LMCRecorderConfiguration(
                Signals,
                1,
                10,
                LMCRecorderBufferMode.Double,
                LMCRecorderTriggerType.Edge,
                LMCSignalValueType.Int32,
                4,
                5,
                Signal1,
                LMCRecorderTriggerOperator.RisingEdge,
                100,
                0,
                ConfigId);
        }

        private static LMCRecorderConfiguration TriggerConfiguration(
            LMCRecorderBufferMode bufferMode,
            LMCRecorderTriggerType triggerType,
            LMCRecorderTriggerOperator triggerOperator,
            uint triggerMask)
        {
            return new LMCRecorderConfiguration(
                Signals,
                1,
                10,
                bufferMode,
                triggerType,
                triggerType == LMCRecorderTriggerType.Mask
                    ? LMCSignalValueType.BitField32
                    : LMCSignalValueType.Int32,
                4,
                5,
                Signal1,
                triggerOperator,
                triggerType == LMCRecorderTriggerType.Mask ? 0u : 100u,
                triggerMask);
        }

        private static LMCDiagnosticCapabilities Capabilities(long session)
        {
            return new LMCDiagnosticCapabilities(
                null,
                session,
                3,
                (uint)(LMCDiagnosticCapability.SignalCatalog
                    | LMCDiagnosticCapability.RecorderSingleBank
                    | LMCDiagnosticCapability.RecorderTrigger
                    | LMCDiagnosticCapability.RecorderDoubleBank),
                MapRevision,
                24,
                32,
                32,
                2,
                100,
                1000,
                1320,
                2040,
                16,
                80,
                16,
                800,
                0,
                DiagnosticsBootId);
        }

        private static LMCRecorderConfigurationHandle ConfigurationHandle(
            LMCRecorderConfiguration configuration)
        {
            return new LMCRecorderConfigurationHandle(
                null,
                configuration,
                DiagnosticsBootId,
                ConfigId,
                ConfigRevision,
                MapRevision,
                3,
                1000,
                24,
                LMCRecorderState.Configured,
                8,
                1,
                LMCCapturePhase.InputMapped,
                OwnerSessionEpoch,
                16,
                7,
                null);
        }

        private static LMCRecorderIdentity RecorderIdentity(
            uint acceptedStartCycle = 0,
            LMCRecorderTriggerType triggerType = LMCRecorderTriggerType.Manual,
            LMCRecorderBufferMode bufferMode = LMCRecorderBufferMode.Single,
            uint preTriggerSamples = 1,
            uint postTriggerSamples = 1)
        {
            return new LMCRecorderIdentity(
                null,
                DiagnosticsBootId,
                RecordId,
                0,
                ConfigId,
                ConfigRevision,
                MapRevision,
                OwnerSessionEpoch,
                LMCRecorderState.Armed,
                acceptedStartCycle,
                3,
                LMCCapturePhase.InputMapped,
                1000,
                bufferMode,
                triggerType,
                triggerType == LMCRecorderTriggerType.Manual
                    ? 0u
                    : preTriggerSamples,
                triggerType == LMCRecorderTriggerType.Manual
                    ? 0u
                    : postTriggerSamples,
                true,
                16,
                Signals,
                7,
                null,
                false);
        }

        private static void AssertRequestHeader(
            byte[] request,
            ushort commandId,
            ushort payloadLength,
            uint requestId)
        {
            AssertEx.Equal(commandId, TestFrame.ReadUInt16(request, 0));
            AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(request, 2));
            AssertEx.Equal(payloadLength, TestFrame.ReadUInt16(request, 4));
            AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(request, 6));
            AssertEx.Equal((ushort)1, TestFrame.ReadUInt16(request, 8));
            AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(request, 10));
            AssertEx.Equal(requestId, TestFrame.ReadUInt32(request, 12));
        }

        private static void AssertRecorderIdentityRequest(
            ushort commandId,
            byte[] request)
        {
            AssertRequestHeader(request, commandId, 28, GoldenRequestId);
            AssertEx.Equal(RecordId, TestFrame.ReadUInt32(request, 16));
            AssertEx.Equal(0u, TestFrame.ReadUInt32(request, 20));
            AssertEx.Equal(MapRevision, TestFrame.ReadUInt32(request, 24));
            AssertEx.Equal(OwnerSessionEpoch, TestFrame.ReadUInt32(request, 28));
            AssertEx.Equal(DiagnosticsBootId, TestFrame.ReadUInt32(request, 32));
        }

        private static void AssertHeaderMalformed(
            byte[] payload,
            LMCRecorderIdentity identity)
        {
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseRecorderHeader(
                    TestFrame.Response(0, payload),
                    GoldenRequestId,
                    identity));
        }

        private static void AssertChunkMalformed(
            byte[] payload,
            LMCRecorderChunkRequest request)
        {
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseRecorderChunk(
                    TestFrame.Response(0, payload),
                    GoldenRequestId,
                    request));
        }

        private static FakeRpcServer CreateConfiguredCleanupServer(
            params FakeRpcStep[] tail)
        {
            var steps = new List<FakeRpcStep>
            {
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E40,
                    TestFrame.Response(0, ConfigurePayload(2)))
            };
            steps.AddRange(tail);
            steps.Add(CloseStep());
            return new FakeRpcServer(steps.ToArray());
        }

        private static FakeRpcServer CreateStartedCleanupServer(
            params FakeRpcStep[] tail)
        {
            var steps = new List<FakeRpcStep>
            {
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E40,
                    TestFrame.Response(0, ConfigurePayload(2))),
                new FakeRpcStep(
                    0x7E41,
                    TestFrame.Response(0, StartPayload(3)))
            };
            steps.AddRange(tail);
            steps.Add(CloseStep());
            return new FakeRpcServer(steps.ToArray());
        }

        private static LMCConnection ConnectRecorder(FakeRpcServer server)
        {
            var connection = new LMCConnection();
            connection.RpcInitConnection(
                "127.0.0.1",
                server.Port,
                "127.0.0.1",
                0,
                LMCConnection.DefaultEventMask);
            return connection;
        }

        private static void StartRecorder(
            LMCConnection connection,
            out LMCRecorderConfigurationHandle handle,
            out LMCRecorderIdentity identity)
        {
            handle = connection.Diagnostics.ConfigureRecorder(
                ManualConfiguration());
            identity = connection.Diagnostics.StartRecorder(handle);
        }

        private static FakeRpcStep StatusStep(
            uint requestId,
            LMCRecorderState state)
        {
            var payload = StatusPayload(requestId);
            TestFrame.WriteUInt16(payload, 36, (ushort)state);
            if (state == LMCRecorderState.Armed
                || state == LMCRecorderState.Recording)
            {
                payload[39] = (byte)LMCRecorderStopReason.None;
            }
            else if (state == LMCRecorderState.Fault)
            {
                payload[39] = (byte)LMCRecorderStopReason.Error;
            }

            return new FakeRpcStep(
                0x7E44,
                TestFrame.Response(0, payload));
        }

        private static RecorderQualificationCleanupOperations
            CreateOwnedCleanupOperations(
                LMCDiagnostics diagnostics,
                LMCRecorderConfigurationHandle handle,
                LMCRecorderIdentity identity)
        {
            return new RecorderQualificationCleanupOperations
            {
                ReadStatusAsync = () => diagnostics.GetRecorderStatusAsync(
                    identity,
                    CancellationToken.None),
                StopAsync = () => diagnostics.StopRecorderAsync(
                    identity,
                    CancellationToken.None),
                ValidateStatus = status =>
                {
                    if (status == null
                        || identity == null
                        || status.DiagnosticsBootId
                            != identity.DiagnosticsBootId
                        || status.RecordId != identity.RecordId
                        || status.BufferId != identity.BufferId
                        || status.ConfigId != handle.ConfigId
                        || status.ConfigRevision != handle.ConfigRevision
                        || status.MapRevision != handle.MapRevision)
                    {
                        throw new InvalidOperationException(
                            "Recorder cleanup status identity mismatch.");
                    }
                },
                IsBufferReleasePending = () => identity != null
                    && !identity.IsBufferReleased,
                IsConfigurationReleasePending = () => handle != null
                    && !handle.IsReleased,
                ReleaseBufferAsync = () =>
                    diagnostics.ReleaseRecorderBufferAsync(
                        identity,
                        CancellationToken.None),
                ReleaseConfigurationAsync = () =>
                    diagnostics.ReleaseRecorderAsync(
                        handle,
                        CancellationToken.None),
                DelayAsync = milliseconds => Task.FromResult(0)
            };
        }

        private static RecorderQualificationCleanupOperations
            CreateAdoptedCleanupOperations(
                LMCDiagnostics diagnostics,
                LMCRecorderIdentity identity)
        {
            return new RecorderQualificationCleanupOperations
            {
                ReadStatusAsync = () => diagnostics.GetRecorderStatusAsync(
                    identity,
                    CancellationToken.None),
                StopAsync = () => diagnostics.StopRecorderAsync(
                    identity,
                    CancellationToken.None),
                ValidateStatus = status =>
                    RecorderQualificationCleanupOrchestrator
                        .ValidateReconnectStatus(
                            status,
                            identity,
                            DiagnosticsBootId,
                            RecordId,
                            0,
                            ConfigId,
                            ConfigRevision,
                            MapRevision,
                            3),
                IsBufferReleasePending = () =>
                    !identity.IsBufferReleased,
                IsConfigurationReleasePending = () =>
                    !identity.IsRecorderReleased,
                ReleaseBufferAsync = () =>
                    diagnostics.ReleaseRecorderBufferAsync(
                        identity,
                        CancellationToken.None),
                ReleaseConfigurationAsync = () =>
                    diagnostics.ReleaseRecorderAsync(
                        identity,
                        CancellationToken.None),
                DelayAsync = milliseconds => Task.FromResult(0)
            };
        }

        private static Task<RecorderReleasableStateResult>
            CleanupOwnedResourcesAsync(
                RecorderQualificationCleanupOperations operations)
        {
            return RecorderQualificationCleanupOrchestrator
                .CleanupOwnedResourcesAsync(
                    operations,
                    1000,
                    0);
        }

        private static Exception CreateQualificationAggregate(
            Exception primary,
            Exception cleanup)
        {
            return new InvalidOperationException(
                "Recorder primary and cleanup both failed.",
                new AggregateException(primary, cleanup));
        }

        private static int CountReceivedCommand(
            FakeRpcServer server,
            ushort commandId)
        {
            var count = 0;
            foreach (var request in server.ReceivedRequests)
            {
                if (TestFrame.ReadUInt16(request, 0) == commandId)
                {
                    count++;
                }
            }

            return count;
        }

        private static void AssertRecorderCleanupCommandCounts(
            FakeRpcServer server,
            int expectedStatus,
            int expectedStop,
            int expectedBufferRelease,
            int expectedConfigurationRelease)
        {
            var status = 0;
            var stop = 0;
            var bufferRelease = 0;
            var configurationRelease = 0;
            foreach (var request in server.ReceivedRequests)
            {
                switch (TestFrame.ReadUInt16(request, 0))
                {
                    case 0x7E44:
                        status++;
                        break;
                    case 0x7E43:
                        stop++;
                        break;
                    case 0x7E47:
                        bufferRelease++;
                        break;
                    case 0x7E48:
                        configurationRelease++;
                        break;
                }
            }

            AssertEx.Equal(expectedStatus, status);
            AssertEx.Equal(expectedStop, stop);
            AssertEx.Equal(expectedBufferRelease, bufferRelease);
            AssertEx.Equal(
                expectedConfigurationRelease,
                configurationRelease);
        }

        private static byte[] ConfigurePayload(uint requestId)
        {
            var payload = CommonPayload(56, requestId);
            TestFrame.WriteUInt32(payload, 16, ConfigId);
            TestFrame.WriteUInt32(payload, 20, ConfigRevision);
            TestFrame.WriteUInt32(payload, 24, MapRevision);
            TestFrame.WriteUInt32(payload, 28, 3);
            TestFrame.WriteUInt32(payload, 32, 24);
            TestFrame.WriteUInt16(payload, 36, (ushort)LMCRecorderState.Configured);
            TestFrame.WriteUInt16(payload, 38, 2);
            TestFrame.WriteUInt16(payload, 40, 8);
            TestFrame.WriteUInt16(payload, 42, 1);
            TestFrame.WriteUInt16(payload, 44, (ushort)LMCCapturePhase.InputMapped);
            TestFrame.WriteUInt32(payload, 48, OwnerSessionEpoch);
            TestFrame.WriteUInt32(payload, 52, DiagnosticsBootId);
            return payload;
        }

        private static byte[] RecoverableConfigurePayload(
            uint requestId,
            Guid recoveryToken)
        {
            var payload = CommonPayload(72, requestId);
            TestFrame.WriteUInt32(payload, 16, ConfigId);
            TestFrame.WriteUInt32(payload, 20, ConfigRevision);
            TestFrame.WriteUInt32(payload, 24, MapRevision);
            TestFrame.WriteUInt32(payload, 28, 10);
            TestFrame.WriteUInt32(payload, 32, 160);
            TestFrame.WriteUInt16(
                payload,
                36,
                (ushort)LMCRecorderState.Configured);
            TestFrame.WriteUInt16(payload, 38, 2);
            TestFrame.WriteUInt16(payload, 40, 8);
            TestFrame.WriteUInt16(payload, 42, 2);
            TestFrame.WriteUInt16(
                payload,
                44,
                (ushort)LMCCapturePhase.InputMapped);
            TestFrame.WriteUInt32(payload, 48, OwnerSessionEpoch);
            TestFrame.WriteUInt32(payload, 52, DiagnosticsBootId);
            WriteRecoveryTokenBytes(payload, 56, recoveryToken);
            return payload;
        }

        private static byte[] RecoverableRecorderBankInventoryPayload(
            uint requestId,
            Guid recoveryToken)
        {
            var source = RecorderBankInventoryPayload(
                requestId,
                0,
                LMCRecorderState.Configured,
                OwnerSessionEpoch);
            var payload = new byte[104];
            Buffer.BlockCopy(source, 0, payload, 0, source.Length);
            WriteRecoveryTokenBytes(payload, 88, recoveryToken);
            return payload;
        }

        private static void AssertRecoveryTokenBytes(
            byte[] buffer,
            int offset,
            Guid expectedRecoveryToken)
        {
            var expected = expectedRecoveryToken.ToByteArray();
            for (var index = 0; index < expected.Length; index++)
            {
                AssertEx.Equal(expected[index], buffer[offset + index]);
            }
        }

        private static void WriteRecoveryTokenBytes(
            byte[] buffer,
            int offset,
            Guid recoveryToken)
        {
            var bytes = recoveryToken.ToByteArray();
            Buffer.BlockCopy(bytes, 0, buffer, offset, bytes.Length);
        }

        private static byte[] StartPayload(uint requestId)
        {
            var payload = CommonPayload(40, requestId);
            TestFrame.WriteUInt32(payload, 16, RecordId);
            TestFrame.WriteUInt32(payload, 20, 0);
            TestFrame.WriteUInt16(payload, 24, (ushort)LMCRecorderState.Armed);
            TestFrame.WriteUInt32(payload, 28, OwnerSessionEpoch);
            TestFrame.WriteUInt32(payload, 32, 0);
            TestFrame.WriteUInt32(payload, 36, DiagnosticsBootId);
            return payload;
        }

        private static byte[] StatusPayload(
            uint requestId,
            uint ownerSessionEpoch = OwnerSessionEpoch)
        {
            var payload = CommonPayload(76, requestId);
            TestFrame.WriteUInt32(payload, 16, RecordId);
            TestFrame.WriteUInt32(payload, 20, 0);
            TestFrame.WriteUInt32(payload, 24, ConfigId);
            TestFrame.WriteUInt32(payload, 28, ConfigRevision);
            TestFrame.WriteUInt32(payload, 32, MapRevision);
            TestFrame.WriteUInt16(payload, 36, (ushort)LMCRecorderState.Ready);
            payload[38] = (byte)LMCCapturePhase.InputMapped;
            payload[39] = (byte)LMCRecorderStopReason.SampleCountComplete;
            TestFrame.WriteUInt32(payload, 40, 3);
            TestFrame.WriteUInt32(payload, 44, 3);
            TestFrame.WriteUInt32(payload, 48, uint.MaxValue);
            TestFrame.WriteUInt32(payload, 52, 100);
            TestFrame.WriteUInt32(payload, 56, 102);
            TestFrame.WriteUInt32(payload, 68, ownerSessionEpoch);
            TestFrame.WriteUInt32(payload, 72, DiagnosticsBootId);
            return payload;
        }

        private static byte[] HeaderPayload(uint requestId, bool triggered)
        {
            var flags = LMCRecorderHeaderFlags.CaptureComplete
                | LMCRecorderHeaderFlags.DataCrcPresent;
            if (triggered)
            {
                flags |= LMCRecorderHeaderFlags.TriggerPresent;
            }

            var payload = CommonPayload(120, requestId);
            TestFrame.WriteUInt32(payload, 16, DiagnosticsBootId);
            TestFrame.WriteUInt32(payload, 20, RecordId);
            TestFrame.WriteUInt32(payload, 24, 0);
            TestFrame.WriteUInt32(payload, 28, ConfigId);
            TestFrame.WriteUInt32(payload, 32, ConfigRevision);
            TestFrame.WriteUInt32(payload, 36, MapRevision);
            payload[40] = (byte)LMCCapturePhase.InputMapped;
            payload[41] = triggered
                ? (byte)LMCRecorderStopReason.TriggerComplete
                : (byte)LMCRecorderStopReason.SampleCountComplete;
            TestFrame.WriteUInt16(payload, 42, (ushort)flags);
            TestFrame.WriteUInt32(payload, 44, 3);
            TestFrame.WriteUInt32(payload, 48, 3);
            TestFrame.WriteUInt16(payload, 52, 2);
            TestFrame.WriteUInt16(payload, 54, 8);
            TestFrame.WriteUInt32(payload, 56, 1000);
            payload[60] = (byte)LMCRecorderDataEncoding.SampleMajorRaw32LittleEndian;
            payload[61] = (byte)LMCRecorderDataCrcPolicy.Crc32IsoHdlc;
            TestFrame.WriteUInt32(payload, 64, triggered ? 1u : uint.MaxValue);
            TestFrame.WriteUInt32(payload, 68, 100);
            TestFrame.WriteUInt32(payload, 72, triggered ? 101u : 0u);
            TestFrame.WriteUInt32(payload, 76, 102);
            TestFrame.WriteUInt32(payload, 80, 1);
            TestFrame.WriteUInt32(payload, 84, 2);
            TestFrame.WriteUInt32(payload, 88, triggered ? 3u : 0u);
            TestFrame.WriteUInt32(payload, 92, triggered ? 4u : 0u);
            TestFrame.WriteUInt32(payload, 96, 5);
            TestFrame.WriteUInt32(payload, 100, 6);
            TestFrame.WriteUInt32(payload, 112, Signal1);
            TestFrame.WriteUInt32(payload, 116, Signal2);
            return payload;
        }

        private static byte[] ChunkPayload(
            uint requestId,
            uint offsetSample,
            ushort returnedSampleCount,
            uint sequence,
            bool lastChunk)
        {
            var dataLength = returnedSampleCount * 8;
            var payload = CommonPayload(
                52 + dataLength,
                requestId,
                lastChunk
                    ? (ushort)LMCDiagnosticsResponseFlags.LastChunk
                    : (ushort)0);
            TestFrame.WriteUInt32(payload, 16, RecordId);
            TestFrame.WriteUInt32(payload, 20, 0);
            TestFrame.WriteUInt32(payload, 24, offsetSample);
            TestFrame.WriteUInt16(payload, 28, returnedSampleCount);
            TestFrame.WriteUInt16(payload, 30, 2);
            TestFrame.WriteUInt32(payload, 32, sequence);
            TestFrame.WriteUInt32(payload, 36, 3);
            TestFrame.WriteUInt16(payload, 40, 8);
            TestFrame.WriteUInt16(payload, 42, checked((ushort)dataLength));
            TestFrame.WriteUInt32(payload, 48, DiagnosticsBootId);
            for (var index = 0; index < dataLength; index++)
            {
                payload[52 + index] = checked((byte)(1 + offsetSample * 8 + index));
            }

            TestFrame.WriteUInt32(
                payload,
                44,
                LMC_DiagnosticsParser.ComputeRecorderDataCrc32(
                    payload,
                    52,
                    dataLength));
            return payload;
        }

        private static byte[] AdoptPayload(
            uint requestId,
            uint ownerSessionEpoch = OwnerSessionEpoch)
        {
            var payload = CommonPayload(36, requestId);
            TestFrame.WriteUInt32(payload, 16, DiagnosticsBootId);
            TestFrame.WriteUInt32(payload, 20, RecordId);
            TestFrame.WriteUInt32(payload, 24, 0);
            TestFrame.WriteUInt32(payload, 28, ownerSessionEpoch);
            TestFrame.WriteUInt16(payload, 32, (ushort)LMCRecorderState.Ready);
            return payload;
        }

        private static byte[] RecorderBankInventoryPayload(
            uint requestId,
            byte occupiedBankCount,
            LMCRecorderState configurationState,
            uint closedSessionEpoch)
        {
            var payload = CommonPayload(88, requestId);
            TestFrame.WriteUInt32(payload, 16, DiagnosticsBootId);
            TestFrame.WriteUInt32(payload, 20, ConfigId);
            TestFrame.WriteUInt32(payload, 24, ConfigRevision);
            TestFrame.WriteUInt32(payload, 28, MapRevision);
            TestFrame.WriteUInt32(payload, 32, OwnerSessionEpoch);
            TestFrame.WriteUInt32(payload, 36, closedSessionEpoch);
            TestFrame.WriteUInt16(
                payload,
                40,
                (ushort)configurationState);
            payload[42] = (byte)LMCRecorderBufferMode.Double;
            payload[43] = 2;
            payload[44] = occupiedBankCount;

            for (var index = 0; index < occupiedBankCount; index++)
            {
                var offset = 48
                    + index
                    * LMC_DiagnosticsParser.RecorderBankInventoryEntryStride;
                TestFrame.WriteUInt32(
                    payload,
                    offset,
                    RecordId + checked((uint)index));
                TestFrame.WriteUInt32(payload, offset + 4, (uint)index);
                TestFrame.WriteUInt32(
                    payload,
                    offset + 8,
                    OwnerSessionEpoch);
                TestFrame.WriteUInt32(
                    payload,
                    offset + 12,
                    closedSessionEpoch);
                TestFrame.WriteUInt16(
                    payload,
                    offset + 16,
                    (ushort)LMCRecorderState.Ready);
            }

            return payload;
        }

        private static LMCRecorderBankInventory
            EmptyClosedRecorderBankInventory(uint requestId)
        {
            return LMC_DiagnosticsParser.ParseRecorderBankInventory(
                TestFrame.Response(
                    0,
                    RecorderBankInventoryPayload(
                        requestId,
                        0,
                        LMCRecorderState.Configured,
                        OwnerSessionEpoch)),
                requestId,
                DiagnosticsBootId,
                ConfigId,
                MapRevision,
                ConfigRevision);
        }

        private static LMCRecorderBankInventory
            EmptyClosedRecorderBankInventoryForOwner(
                uint requestId,
                uint ownerSessionEpoch)
        {
            return LMC_DiagnosticsParser.ParseRecorderBankInventory(
                TestFrame.Response(
                    0,
                    RecorderBankInventoryPayloadForOwner(
                        requestId,
                        ownerSessionEpoch,
                        ownerSessionEpoch)),
                requestId,
                DiagnosticsBootId,
                ConfigId,
                MapRevision,
                ConfigRevision);
        }

        private static byte[] RecorderBankInventoryPayloadForOwner(
            uint requestId,
            uint ownerSessionEpoch,
            uint closedSessionEpoch)
        {
            var payload = CommonPayload(88, requestId);
            TestFrame.WriteUInt32(payload, 16, DiagnosticsBootId);
            TestFrame.WriteUInt32(payload, 20, ConfigId);
            TestFrame.WriteUInt32(payload, 24, ConfigRevision);
            TestFrame.WriteUInt32(payload, 28, MapRevision);
            TestFrame.WriteUInt32(payload, 32, ownerSessionEpoch);
            TestFrame.WriteUInt32(payload, 36, closedSessionEpoch);
            TestFrame.WriteUInt16(
                payload,
                40,
                (ushort)LMCRecorderState.Configured);
            payload[42] = (byte)LMCRecorderBufferMode.Double;
            payload[43] = 2;
            payload[44] = 0;
            return payload;
        }

        private static byte[] AdoptEmptyRecorderConfigurationPayload(
            uint requestId)
        {
            var payload = CommonPayload(40, requestId);
            TestFrame.WriteUInt32(payload, 16, DiagnosticsBootId);
            TestFrame.WriteUInt32(payload, 20, ConfigId);
            TestFrame.WriteUInt32(payload, 24, ConfigRevision);
            TestFrame.WriteUInt32(payload, 28, MapRevision);
            TestFrame.WriteUInt32(
                payload,
                32,
                ReconnectedOwnerSessionEpoch);
            TestFrame.WriteUInt16(
                payload,
                36,
                (ushort)LMCRecorderState.Configured);
            payload[38] = (byte)LMCRecorderBufferMode.Double;
            payload[39] = 2;
            return payload;
        }

        private static byte[]
            AdoptEmptyRecorderConfigurationPayloadForOwner(
                uint requestId,
                uint ownerSessionEpoch)
        {
            var payload = AdoptEmptyRecorderConfigurationPayload(requestId);
            TestFrame.WriteUInt32(payload, 32, ownerSessionEpoch);
            return payload;
        }

        private static byte[] CapabilitiesPayload(uint requestId)
        {
            var payload = CommonPayload(68, requestId);
            TestFrame.WriteUInt32(payload, 16, 3);
            TestFrame.WriteUInt32(
                payload,
                20,
                (uint)(LMCDiagnosticCapability.SignalCatalog
                    | LMCDiagnosticCapability.RecorderSingleBank
                    | LMCDiagnosticCapability.RecorderTrigger
                    | LMCDiagnosticCapability.RecorderDoubleBank));
            TestFrame.WriteUInt32(payload, 24, MapRevision);
            TestFrame.WriteUInt16(payload, 28, 24);
            TestFrame.WriteUInt16(payload, 30, 32);
            TestFrame.WriteUInt16(payload, 32, 32);
            TestFrame.WriteUInt16(payload, 34, 2);
            TestFrame.WriteUInt32(payload, 36, 100);
            TestFrame.WriteUInt32(payload, 40, 1000);
            TestFrame.WriteUInt16(payload, 44, 1320);
            TestFrame.WriteUInt16(payload, 46, 2040);
            TestFrame.WriteUInt16(payload, 48, 16);
            TestFrame.WriteUInt16(payload, 50, 80);
            TestFrame.WriteUInt16(payload, 52, 16);
            TestFrame.WriteUInt32(payload, 56, 800);
            TestFrame.WriteUInt32(payload, 64, DiagnosticsBootId);
            return payload;
        }

        private static byte[] SingleBankCapabilitiesPayload(uint requestId)
        {
            var payload = CapabilitiesPayload(requestId);
            var capabilityBits = TestFrame.ReadUInt32(payload, 20);
            capabilityBits &= ~(uint)LMCDiagnosticCapability.RecorderDoubleBank;
            TestFrame.WriteUInt32(payload, 20, capabilityBits);
            TestFrame.WriteUInt16(payload, 34, 1);
            return payload;
        }

        private static byte[] CommonPayload(
            int length,
            uint requestId,
            ushort responseFlags = 0)
        {
            var payload = new byte[length];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt16(payload, 2, responseFlags);
            TestFrame.WriteUInt32(payload, 8, requestId);
            return payload;
        }

        private static byte[] DomainErrorPayload(
            uint requestId,
            LMCDiagnosticsDetailCode detail)
        {
            var payload = CommonPayload(16, requestId);
            TestFrame.WriteUInt16(payload, 4, 1);
            TestFrame.WriteInt16(payload, 6, -32000);
            TestFrame.WriteUInt32(payload, 12, (uint)detail);
            return payload;
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

        private static FakeRpcStep ResponseLossStep(ushort commandId)
        {
            return new FakeRpcStep(commandId, null)
            {
                CloseAfterResponse = true
            };
        }

        private sealed class InlineProgress<T> : IProgress<T>
        {
            private readonly Action<T> callback;

            internal InlineProgress(Action<T> callback)
            {
                this.callback = callback;
            }

            public void Report(T value)
            {
                callback(value);
            }
        }
    }
}

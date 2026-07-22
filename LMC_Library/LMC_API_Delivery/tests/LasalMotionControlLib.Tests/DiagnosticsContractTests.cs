using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class DiagnosticsContractTests
    {
        private const uint GoldenRequestId = 0x11223344u;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Request.DiagnosticsCapabilities.GoldenBytes",
                DiagnosticsCapabilitiesRequestGoldenBytes);
            tests.Add(
                "Response.DiagnosticsCapabilities.GoldenFields",
                DiagnosticsCapabilitiesResponseGoldenFields);
            tests.Add(
                "Response.DiagnosticsCapabilities.BootIdSentinel",
                DiagnosticsCapabilitiesBootIdSentinel);
            tests.Add(
                "Response.DiagnosticsCapabilities.MalformedRejected",
                DiagnosticsCapabilitiesMalformedRejected);
            tests.Add(
                "Response.DiagnosticsCapabilities.DomainErrorPreserved",
                DiagnosticsCapabilitiesDomainErrorPreserved);
            tests.Add(
                "Contract.Diagnostics.ReservedCommandIds",
                DiagnosticsReservedCommandIds);
            tests.Add(
                "Rpc.DiagnosticsCapabilities.SyncAndAsync",
                DiagnosticsCapabilitiesSyncAndAsync);
            tests.Add(
                "Rpc.DiagnosticsCapabilities.UnsupportedServer",
                DiagnosticsCapabilitiesUnsupportedServer);
        }

        private static void DiagnosticsCapabilitiesRequestGoldenBytes()
        {
            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "00 7E 00 00 08 00 00 00 "
                    + "01 00 00 00 44 33 22 11"),
                LMC_DiagnosticsFrame.GetDiagnosticsCapabilities(
                    GoldenRequestId));

            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.GetDiagnosticsCapabilities(0));
        }

        private static void DiagnosticsCapabilitiesResponseGoldenFields()
        {
            var capabilityBits =
                (uint)(LMCDiagnosticCapability.EtherCATHealth
                    | LMCDiagnosticCapability.SignalCatalog
                    | LMCDiagnosticCapability.BulkSnapshot
                    | LMCDiagnosticCapability.RecorderSingleBank
                    | LMCDiagnosticCapability.RecorderDoubleBank
                    | LMCDiagnosticCapability.SDORead);
            var payload = CapabilitiesPayload(
                GoldenRequestId,
                capabilityBits,
                0x89ABCDEFu);

            var capabilities = LMC_DiagnosticsParser.ParseCapabilities(
                TestFrame.Response(0, payload),
                GoldenRequestId,
                27);

            AssertEx.True(capabilities.Response.IsSuccess);
            AssertEx.Equal((ushort)1, capabilities.Response.SchemaVersion);
            AssertEx.Equal(GoldenRequestId, capabilities.Response.RequestId);
            AssertEx.Equal(1u, capabilities.DiagnosticsBuild);
            AssertEx.Equal(capabilityBits, capabilities.CapabilityBits);
            AssertEx.Equal(0x01020304u, capabilities.MapRevision);
            AssertEx.Equal((ushort)24, capabilities.CatalogEntryCount);
            AssertEx.Equal((ushort)32, capabilities.MaxBulkSignals);
            AssertEx.Equal((ushort)32, capabilities.MaxRecorderChannels);
            AssertEx.Equal((ushort)2, capabilities.RecorderBufferCount);
            AssertEx.Equal(31250u, capabilities.MaxRecorderSamples);
            AssertEx.Equal(1000u, capabilities.BaseCycleTimeUs);
            AssertEx.Equal((ushort)1320, capabilities.MaxRequestPayloadBytes);
            AssertEx.Equal((ushort)2040, capabilities.MaxResponsePayloadBytes);
            AssertEx.Equal((ushort)1280, capabilities.MaxChunkDataBytes);
            AssertEx.Equal((ushort)80, capabilities.CatalogEntryStride);
            AssertEx.Equal((ushort)16, capabilities.SignalValueEntryStride);
            AssertEx.Equal(4000000u, capabilities.RecorderBytesPerBank);
            AssertEx.Equal((ushort)12, capabilities.MaxSdoDataBytes);
            AssertEx.Equal(0x89ABCDEFu, capabilities.DiagnosticsBootId);
            AssertEx.True(capabilities.HasStableDiagnosticsBootId);
            AssertEx.True(
                capabilities.Supports(
                    LMCDiagnosticCapability.EtherCATHealth
                    | LMCDiagnosticCapability.BulkSnapshot));
            AssertEx.False(
                capabilities.Supports(LMCDiagnosticCapability.PIWrite));
            AssertEx.Equal(27L, capabilities.ConnectionSessionGeneration);
        }

        private static void DiagnosticsCapabilitiesBootIdSentinel()
        {
            var statelessBits =
                (uint)(LMCDiagnosticCapability.EtherCATHealth
                    | LMCDiagnosticCapability.SignalCatalog
                    | LMCDiagnosticCapability.PIRead);
            var capabilities = LMC_DiagnosticsParser.ParseCapabilities(
                TestFrame.Response(
                    0,
                    CapabilitiesPayload(
                        GoldenRequestId,
                        statelessBits,
                        0)),
                GoldenRequestId,
                1);

            AssertEx.False(capabilities.HasStableDiagnosticsBootId);
            AssertEx.Equal(0u, capabilities.DiagnosticsBootId);

            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseCapabilities(
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            GoldenRequestId,
                            (uint)LMCDiagnosticCapability.BulkSnapshot,
                            0)),
                    GoldenRequestId,
                    1));

            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseCapabilities(
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            GoldenRequestId,
                            (uint)LMCDiagnosticCapability.ExtendedSdoResultChunk,
                            0)),
                    GoldenRequestId,
                    1));
        }

        private static void DiagnosticsCapabilitiesMalformedRejected()
        {
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseCapabilities(
                    null,
                    GoldenRequestId,
                    1));

            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseCapabilities(
                    TestFrame.Response(0, new byte[67]),
                    GoldenRequestId,
                    1));

            var wrongSchema = CapabilitiesPayload(GoldenRequestId, 0, 0);
            TestFrame.WriteUInt16(wrongSchema, 0, 2);
            AssertMalformed(wrongSchema, GoldenRequestId);

            var unknownFlags = CapabilitiesPayload(GoldenRequestId, 0, 0);
            TestFrame.WriteUInt16(unknownFlags, 2, 0x0004);
            AssertMalformed(unknownFlags, GoldenRequestId);

            var chunkFlag = CapabilitiesPayload(GoldenRequestId, 0, 0);
            TestFrame.WriteUInt16(
                chunkFlag,
                2,
                (ushort)LMCDiagnosticsResponseFlags.LastChunk);
            AssertMalformed(chunkFlag, GoldenRequestId);

            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseCapabilities(
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(GoldenRequestId, 0, 0),
                        1),
                    GoldenRequestId,
                    1));

            var invalidStatus = CapabilitiesPayload(GoldenRequestId, 0, 0);
            TestFrame.WriteInt16(invalidStatus, 6, -32000);
            AssertMalformed(invalidStatus, GoldenRequestId);

            var successWithDetail = CapabilitiesPayload(GoldenRequestId, 0, 0);
            TestFrame.WriteUInt32(successWithDetail, 12, 1);
            AssertMalformed(successWithDetail, GoldenRequestId);

            var errorWithoutDetail = CapabilitiesPayload(
                GoldenRequestId,
                0,
                0);
            TestFrame.WriteUInt16(errorWithoutDetail, 4, 1);
            TestFrame.WriteInt16(errorWithoutDetail, 6, -32000);
            AssertMalformed(errorWithoutDetail, GoldenRequestId);

            var errorWithUnknownDetail = (byte[])errorWithoutDetail.Clone();
            TestFrame.WriteUInt32(errorWithUnknownDetail, 12, 26);
            AssertMalformed(errorWithUnknownDetail, GoldenRequestId);

            AssertMalformed(
                CapabilitiesPayload(GoldenRequestId + 1, 0, 0),
                GoldenRequestId);

            var piReadWithoutCatalog = CapabilitiesPayload(
                GoldenRequestId,
                (uint)LMCDiagnosticCapability.PIRead,
                0);
            AssertMalformed(piReadWithoutCatalog, GoldenRequestId);

            var recorderTriggerWithoutBase = CapabilitiesPayload(
                GoldenRequestId,
                (uint)LMCDiagnosticCapability.RecorderTrigger,
                1);
            AssertMalformed(recorderTriggerWithoutBase, GoldenRequestId);

            var recorderDoubleWithoutBase = CapabilitiesPayload(
                GoldenRequestId,
                (uint)LMCDiagnosticCapability.RecorderDoubleBank,
                1);
            AssertMalformed(recorderDoubleWithoutBase, GoldenRequestId);

            var zeroMapRevision = CapabilitiesPayload(
                GoldenRequestId,
                (uint)LMCDiagnosticCapability.SignalCatalog,
                0);
            TestFrame.WriteUInt32(zeroMapRevision, 24, 0);
            AssertMalformed(zeroMapRevision, GoldenRequestId);

            var zeroCatalogCount = CapabilitiesPayload(
                GoldenRequestId,
                (uint)LMCDiagnosticCapability.SignalCatalog,
                0);
            TestFrame.WriteUInt16(zeroCatalogCount, 28, 0);
            AssertMalformed(zeroCatalogCount, GoldenRequestId);

            var wrongCatalogStride = CapabilitiesPayload(
                GoldenRequestId,
                (uint)LMCDiagnosticCapability.SignalCatalog,
                0);
            TestFrame.WriteUInt16(wrongCatalogStride, 50, 79);
            AssertMalformed(wrongCatalogStride, GoldenRequestId);

            var wrongValueStride = CapabilitiesPayload(
                GoldenRequestId,
                (uint)LMCDiagnosticCapability.SignalCatalog,
                0);
            TestFrame.WriteUInt16(wrongValueStride, 52, 15);
            AssertMalformed(wrongValueStride, GoldenRequestId);

            var requestLimitTooSmall = CapabilitiesPayload(
                GoldenRequestId,
                0,
                0);
            TestFrame.WriteUInt16(requestLimitTooSmall, 44, 7);
            AssertMalformed(requestLimitTooSmall, GoldenRequestId);

            var responseLimitTooSmall = CapabilitiesPayload(
                GoldenRequestId,
                0,
                0);
            TestFrame.WriteUInt16(responseLimitTooSmall, 46, 67);
            AssertMalformed(responseLimitTooSmall, GoldenRequestId);

            var misalignedChunkLimit = CapabilitiesPayload(
                GoldenRequestId,
                0,
                0);
            TestFrame.WriteUInt16(misalignedChunkLimit, 48, 1279);
            AssertMalformed(misalignedChunkLimit, GoldenRequestId);

            var chunkExceedsResponseLimit = CapabilitiesPayload(
                GoldenRequestId,
                0,
                0);
            TestFrame.WriteUInt16(chunkExceedsResponseLimit, 48, 2000);
            AssertMalformed(chunkExceedsResponseLimit, GoldenRequestId);

            var generalSdoWithoutBase = CapabilitiesPayload(
                GoldenRequestId,
                (uint)LMCDiagnosticCapability.SDOReadGeneralInline,
                1);
            TestFrame.WriteUInt16(generalSdoWithoutBase, 60, 4);
            AssertMalformed(generalSdoWithoutBase, GoldenRequestId);

            var generalSdoWrongLimit = CapabilitiesPayload(
                GoldenRequestId,
                (uint)(LMCDiagnosticCapability.SDORead
                    | LMCDiagnosticCapability.SDOReadGeneralInline),
                1);
            AssertMalformed(generalSdoWrongLimit, GoldenRequestId);

            var generalSdoWithoutBoot = CapabilitiesPayload(
                GoldenRequestId,
                (uint)(LMCDiagnosticCapability.SDORead
                    | LMCDiagnosticCapability.SDOReadGeneralInline),
                0);
            TestFrame.WriteUInt16(generalSdoWithoutBoot, 60, 4);
            AssertMalformed(generalSdoWithoutBoot, GoldenRequestId);

            var generalSdo = CapabilitiesPayload(
                GoldenRequestId,
                (uint)(LMCDiagnosticCapability.SDORead
                    | LMCDiagnosticCapability.SDOReadGeneralInline),
                1);
            TestFrame.WriteUInt16(generalSdo, 60, 4);
            var generalCapabilities = LMC_DiagnosticsParser.ParseCapabilities(
                TestFrame.Response(0, generalSdo),
                GoldenRequestId,
                1);
            AssertEx.True(
                generalCapabilities.Supports(
                    LMCDiagnosticCapability.SDOReadGeneralInline));
        }

        private static void DiagnosticsReservedCommandIds()
        {
            AssertEx.Equal((ushort)0x7E21, LMC_CommandId.SubmitPIWrite);
            AssertEx.Equal((ushort)0x7E42, LMC_CommandId.TriggerRecorder);
            AssertEx.Equal((ushort)0x7E51, LMC_CommandId.ReadSdoResultChunk);
        }

        private static void DiagnosticsCapabilitiesDomainErrorPreserved()
        {
            var payload = CapabilitiesPayload(GoldenRequestId, 0, 0);
            TestFrame.WriteUInt16(payload, 4, 1);
            TestFrame.WriteInt16(payload, 6, -32000);
            TestFrame.WriteUInt32(
                payload,
                12,
                (uint)LMCDiagnosticsDetailCode.UnsupportedFeature);

            var exception = AssertEx.Throws<LMCDiagnosticsCommandException>(
                () => LMC_DiagnosticsParser.ParseCapabilities(
                    TestFrame.Response(0, payload),
                    GoldenRequestId,
                    1));

            AssertEx.Equal((short)-32000, exception.Response.ErrorId);
            AssertEx.Equal(
                LMCDiagnosticsDetailCode.UnsupportedFeature,
                exception.Response.Detail);
            AssertEx.Equal(GoldenRequestId, exception.Response.RequestId);

            var boundsPayload = CapabilitiesPayload(GoldenRequestId, 0, 0);
            TestFrame.WriteUInt16(boundsPayload, 4, 1);
            TestFrame.WriteInt16(boundsPayload, 6, -32000);
            TestFrame.WriteUInt32(
                boundsPayload,
                12,
                (uint)LMCDiagnosticsDetailCode.BoundsInvalid);

            var boundsException = AssertEx.Throws<LMCDiagnosticsCommandException>(
                () => LMC_DiagnosticsParser.ParseCapabilities(
                    TestFrame.Response(0, boundsPayload),
                    GoldenRequestId,
                    1));

            AssertEx.Equal(
                LMCDiagnosticsDetailCode.BoundsInvalid,
                boundsException.Response.Detail);
            AssertEx.Equal(
                GoldenRequestId,
                boundsException.Response.RequestId);
        }

        private static void DiagnosticsCapabilitiesSyncAndAsync()
        {
            RunCapabilitiesIntegration(false);
            RunCapabilitiesIntegration(true);
        }

        private static void DiagnosticsCapabilitiesUnsupportedServer()
        {
            var shortError = TestFrame.Response(
                1,
                TestFrame.Hex("01 00 FC FF"));

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(0x7E00, shortError),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var exception = AssertEx.Throws<LMCDiagnosticsNotSupportedException>(
                    () => connection.Diagnostics.GetCapabilities());

                AssertEx.Equal((ushort)1, exception.Response.HeaderStatus);
                AssertEx.Equal((short)-4, exception.Response.ErrorId);
                AssertEx.True(connection.IsConnected);

                connection.CloseConnection();
                server.Verify();
            }

            var preconditionError = TestFrame.Response(
                1,
                TestFrame.Hex("01 00 FF FF"));

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(0x7E00, preconditionError),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var exception = AssertEx.Throws<InvalidOperationException>(
                    () => connection.Diagnostics.GetCapabilities());

                AssertEx.Contains("ErrorId=-1", exception.Message);
                AssertEx.True(connection.IsConnected);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RunCapabilitiesIntegration(bool useAsync)
        {
            var capabilitiesResponse = TestFrame.Response(
                0,
                D0CapabilitiesPayload(1));

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(0x7E00, capabilitiesResponse)
                {
                    ResponseChunks = new[] { 1, 2, 5, 7, 11 },
                    InspectRequest = request => AssertEx.SequenceEqual(
                        TestFrame.Request(
                            0x7E00,
                            0,
                            TestFrame.Hex(
                                "01 00 00 00 01 00 00 00")),
                        request)
                },
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                AssertEx.NotNull(connection.Diagnostics);
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var capabilities = useAsync
                    ? connection.Diagnostics.GetCapabilitiesAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult()
                    : connection.Diagnostics.GetCapabilities();

                AssertEx.Equal(1u, capabilities.DiagnosticsBuild);
                AssertEx.Equal(0u, capabilities.CapabilityBits);
                AssertEx.Equal(0u, capabilities.MapRevision);
                AssertEx.Equal((ushort)0, capabilities.CatalogEntryCount);
                AssertEx.Equal((ushort)0, capabilities.MaxBulkSignals);
                AssertEx.Equal((ushort)0, capabilities.MaxRecorderChannels);
                AssertEx.Equal((ushort)0, capabilities.RecorderBufferCount);
                AssertEx.Equal(0u, capabilities.MaxRecorderSamples);
                AssertEx.False(capabilities.HasStableDiagnosticsBootId);
                AssertEx.Equal((ushort)2040, capabilities.MaxResponsePayloadBytes);
                AssertEx.Equal((ushort)1280, capabilities.MaxChunkDataBytes);
                AssertEx.Equal(0u, capabilities.RecorderBytesPerBank);
                AssertEx.Equal((ushort)0, capabilities.MaxSdoDataBytes);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static byte[] CapabilitiesPayload(
            uint requestId,
            uint capabilityBits,
            uint diagnosticsBootId)
        {
            var payload = new byte[68];

            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, requestId);
            TestFrame.WriteUInt32(payload, 16, 1);
            TestFrame.WriteUInt32(payload, 20, capabilityBits);
            TestFrame.WriteUInt32(payload, 24, 0x01020304u);
            TestFrame.WriteUInt16(payload, 28, 24);
            TestFrame.WriteUInt16(payload, 30, 32);
            TestFrame.WriteUInt16(payload, 32, 32);
            TestFrame.WriteUInt16(payload, 34, 2);
            TestFrame.WriteUInt32(payload, 36, 31250);
            TestFrame.WriteUInt32(payload, 40, 1000);
            TestFrame.WriteUInt16(payload, 44, 1320);
            TestFrame.WriteUInt16(payload, 46, 2040);
            TestFrame.WriteUInt16(payload, 48, 1280);
            TestFrame.WriteUInt16(payload, 50, 80);
            TestFrame.WriteUInt16(payload, 52, 16);
            TestFrame.WriteUInt32(payload, 56, 4000000);
            TestFrame.WriteUInt16(payload, 60, 12);
            TestFrame.WriteUInt32(payload, 64, diagnosticsBootId);

            return payload;
        }

        private static byte[] D0CapabilitiesPayload(uint requestId)
        {
            var payload = new byte[68];

            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, requestId);
            TestFrame.WriteUInt32(payload, 16, 1);
            TestFrame.WriteUInt32(payload, 40, 1000);
            TestFrame.WriteUInt16(payload, 44, 1320);
            TestFrame.WriteUInt16(payload, 46, 2040);
            TestFrame.WriteUInt16(payload, 48, 1280);
            TestFrame.WriteUInt16(payload, 50, 80);
            TestFrame.WriteUInt16(payload, 52, 16);

            return payload;
        }

        private static void AssertMalformed(
            byte[] payload,
            uint expectedRequestId)
        {
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseCapabilities(
                    TestFrame.Response(0, payload),
                    expectedRequestId,
                    1));
        }

        private static FakeRpcStep InitStep()
        {
            var payload = new byte[24];
            TestFrame.WriteUInt32(payload, 0, 64);
            return new FakeRpcStep(
                0x8080,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep CallbackStep()
        {
            return new FakeRpcStep(
                0x405C,
                TestFrame.Response(
                    0,
                    TestFrame.Hex("00 00 00 00")));
        }

        private static FakeRpcStep CloseStep()
        {
            return new FakeRpcStep(
                0x405D,
                TestFrame.Response(
                    0,
                    TestFrame.Hex("00 00 00 00")));
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class DiagnosticsD45CompletionContractTests
    {
        private const uint RequestId = 0x11223344u;
        private const uint MapRevision = 0x957F101Eu;
        private const uint BootId = 0x89ABCDEFu;
        private const uint ConfigId = 0x10203040u;
        private const uint ConfigRevision = 0x01020304u;
        private const uint OwnerEpoch = 0x55667788u;
        private const uint RecordId = 0xA1B2C3D4u;
        private const uint SignalId = 0x00200001u;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Request.DiagnosticsD4.TriggerGoldenAndAck",
                TriggerRecorderGoldenAndAck);
            tests.Add(
                "Rpc.DiagnosticsD4.TriggerSyncAndAsync",
                TriggerRecorderSyncAndAsync);
            tests.Add(
                "Rpc.DiagnosticsD4.TriggerCancellationBoundary",
                TriggerRecorderCancellationBoundary);
            tests.Add(
                "Request.DiagnosticsD5.PIWriteAndResultChunkGolden",
                PiWriteAndResultChunkGolden);
            tests.Add(
                "Response.DiagnosticsD5.ExtendedResultChunk",
                ExtendedResultChunkContract);
            tests.Add(
                "Response.DiagnosticsD45.CapabilityDependencies",
                D45CapabilityDependencies);
            tests.Add(
                "Rpc.DiagnosticsD5.CompletedSurfaceSyncAndAsync",
                D5CompletedSurfaceSyncAndAsync);
            tests.Add(
                "Policy.DiagnosticsD5.WriteAllowlistFailClosed",
                D5WriteAllowlistFailClosed);
            tests.Add(
                "Policy.DiagnosticsD5.FirstSliceSdoReadMaxSdoBoundary",
                D5FirstSliceSdoReadMaxSdoBoundary);
            tests.Add(
                "Rpc.DiagnosticsD5.StatefulCancellationBoundary",
                D5StatefulCancellationBoundary);
        }

        private static void TriggerRecorderGoldenAndAck()
        {
            using (var connection = new LMCConnection())
            {
                var identity = RecorderIdentity(connection.Diagnostics, 0);
                AssertEx.SequenceEqual(
                    TestFrame.Hex(
                        "42 7E 00 00 1C 00 00 00 "
                        + "01 00 00 00 44 33 22 11 "
                        + "D4 C3 B2 A1 00 00 00 00 "
                        + "1E 10 7F 95 88 77 66 55 "
                        + "EF CD AB 89"),
                    LMC_DiagnosticsFrame.TriggerRecorder(RequestId, identity));

                var response = LMC_DiagnosticsParser.ParseTriggerRecorder(
                    TestFrame.Response(0, CommonPayload(16, RequestId)),
                    RequestId);
                AssertEx.True(response.IsSuccess);

                var flagged = CommonPayload(
                    16,
                    RequestId,
                    (ushort)LMCDiagnosticsResponseFlags.LastChunk);
                AssertEx.Throws<InvalidDataException>(
                    () => LMC_DiagnosticsParser.ParseTriggerRecorder(
                        TestFrame.Response(0, flagged),
                        RequestId));

                var domainError = DomainErrorPayload(
                    RequestId,
                    LMCDiagnosticsDetailCode.InvalidState);
                var exception = AssertEx.Throws<LMCDiagnosticsCommandException>(
                    () => LMC_DiagnosticsParser.ParseTriggerRecorder(
                        TestFrame.Response(0, domainError),
                        RequestId));
                AssertEx.Equal(
                    LMCDiagnosticsDetailCode.InvalidState,
                    exception.Response.Detail);
            }
        }

        private static void TriggerRecorderSyncAndAsync()
        {
            RunTriggerRecorderIntegration(false);
            RunTriggerRecorderIntegration(true);
        }

        private static void RunTriggerRecorderIntegration(bool useAsync)
        {
            var configuration = TriggeredConfiguration();
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            1,
                            LMCDiagnosticCapability.SignalCatalog
                                | LMCDiagnosticCapability.RecorderSingleBank
                                | LMCDiagnosticCapability.RecorderTrigger,
                            12,
                            1))),
                new FakeRpcStep(
                    0x7E40,
                    TestFrame.Response(0, ConfigurePayload(2))),
                new FakeRpcStep(
                    0x7E41,
                    TestFrame.Response(0, StartPayload(3))),
                new FakeRpcStep(
                    0x7E42,
                    TestFrame.Response(0, CommonPayload(16, 4)))
                {
                    InspectRequest = request =>
                    {
                        AssertEx.Equal(RecordId, TestFrame.ReadUInt32(request, 16));
                        AssertEx.Equal(BootId, TestFrame.ReadUInt32(request, 32));
                    }
                },
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                LMCRecorderConfigurationHandle handle;
                LMCRecorderIdentity identity;
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
                    connection.Diagnostics.TriggerRecorderAsync(
                            identity,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }
                else
                {
                    handle = connection.Diagnostics.ConfigureRecorder(configuration);
                    identity = connection.Diagnostics.StartRecorder(handle);
                    connection.Diagnostics.TriggerRecorder(identity);
                }

                AssertEx.Equal(LMCRecorderBufferMode.Ring, identity.BufferMode);
                AssertEx.Equal(LMCRecorderTriggerType.Edge, identity.TriggerType);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void TriggerRecorderCancellationBoundary()
        {
            using (var cancellation = new CancellationTokenSource())
            {
                var triggerStep = new FakeRpcStep(
                    0x7E42,
                    TestFrame.Response(0, CommonPayload(16, 4)))
                {
                    InspectRequest = request => cancellation.Cancel()
                };

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    new FakeRpcStep(
                        0x7E00,
                        TestFrame.Response(
                            0,
                            CapabilitiesPayload(
                                1,
                                LMCDiagnosticCapability.SignalCatalog
                                    | LMCDiagnosticCapability.RecorderSingleBank
                                    | LMCDiagnosticCapability.RecorderTrigger,
                                12,
                                1))),
                    new FakeRpcStep(
                        0x7E40,
                        TestFrame.Response(0, ConfigurePayload(2))),
                    new FakeRpcStep(
                        0x7E41,
                        TestFrame.Response(0, StartPayload(3))),
                    triggerStep,
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    Connect(connection, server.Port);
                    var handle = connection.Diagnostics.ConfigureRecorder(
                        TriggeredConfiguration());
                    var identity = connection.Diagnostics.StartRecorder(handle);

                    connection.Diagnostics.TriggerRecorderAsync(
                            identity,
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult();

                    AssertEx.True(cancellation.IsCancellationRequested);
                    AssertEx.Equal(RecordId, identity.RecordId);
                    connection.CloseConnection();
                    server.Verify();
                }
            }
        }

        private static void PiWriteAndResultChunkGolden()
        {
            var write = PiWriteRequest();
            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "21 7E 00 00 1C 00 00 00 "
                    + "01 00 00 00 44 33 22 11 "
                    + "1E 10 7F 95 01 00 20 00 "
                    + "03 00 00 00 32 00 00 00 "
                    + "EF CD AB 89"),
                LMC_DiagnosticsFrame.SubmitPIWrite(RequestId, write, BootId));

            using (var connection = new LMCConnection())
            {
                var ticket = ExtendedTicket(connection.Diagnostics, 20, 1280);
                var request = new LMCSdoResultChunkRequest(ticket, 4, 16, 7);
                AssertEx.SequenceEqual(
                    TestFrame.Hex(
                        "51 7E 00 00 1C 00 00 00 "
                        + "01 00 00 00 44 33 22 11 "
                        + "40 30 20 10 04 00 00 00 "
                        + "10 00 00 00 07 00 00 00 "
                        + "EF CD AB 89"),
                    LMC_DiagnosticsFrame.ReadSdoResultChunk(RequestId, request));

                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => new LMCSdoResultChunkRequest(ticket, 20, 1, 0));
                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => new LMCSdoResultChunkRequest(ticket, 0, 1281, 0));
            }
        }

        private static void ExtendedResultChunkContract()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = ExtendedTicket(connection.Diagnostics, 20, 1280);
                var request = new LMCSdoResultChunkRequest(ticket, 0, 20, 9);
                var data = TestFrame.Hex(
                    "00 01 02 03 04 05 06 07 08 09 "
                    + "0A 0B 0C 0D 0E 0F 10 11 12 13");
                var payload = SdoChunkPayload(1, ticket, request, data, true);
                var chunk = LMC_DiagnosticsParser.ParseSdoResultChunk(
                    TestFrame.Response(0, payload),
                    1,
                    request);
                AssertEx.True(chunk.IsLastChunk);
                AssertEx.Equal((ushort)20, chunk.ReturnedByteCount);
                AssertEx.SequenceEqual(data, chunk.Data);
                var copy = chunk.Data;
                copy[0] = 0xFF;
                AssertEx.Equal((byte)0, chunk.Data[0]);

                var badCrc = (byte[])payload.Clone();
                badCrc[36] ^= 1;
                AssertChunkMalformed(request, badCrc);

                var badFlag = (byte[])payload.Clone();
                TestFrame.WriteUInt16(badFlag, 2, 0);
                AssertChunkMalformed(request, badFlag);

                var dirtyReserved = (byte[])payload.Clone();
                dirtyReserved[45] = 1;
                AssertChunkMalformed(request, dirtyReserved);
            }
        }

        private static void D5CompletedSurfaceSyncAndAsync()
        {
            RunD5CompletedSurface(false);
            RunD5CompletedSurface(true);
        }

        private static void D45CapabilityDependencies()
        {
            AssertCapabilityMalformed(
                CapabilitiesPayload(
                    RequestId,
                    LMCDiagnosticCapability.PIWrite,
                    0,
                    0));
            AssertCapabilityMalformed(
                CapabilitiesPayload(
                    RequestId,
                    LMCDiagnosticCapability.ExtendedSdoResultChunk,
                    20,
                    0));
            AssertCapabilityMalformed(
                CapabilitiesPayload(
                    RequestId,
                    LMCDiagnosticCapability.SDORead,
                    20,
                    0));

            var capabilities = LMC_DiagnosticsParser.ParseCapabilities(
                TestFrame.Response(
                    0,
                    CapabilitiesPayload(
                        RequestId,
                        LMCDiagnosticCapability.SDORead
                            | LMCDiagnosticCapability.ExtendedSdoResultChunk,
                        20,
                        0)),
                RequestId,
                1);
            AssertEx.Equal((ushort)20, capabilities.MaxSdoDataBytes);
        }

        private static void RunD5CompletedSurface(bool useAsync)
        {
            var sdoRead = LMCSdoRequest.CreateRead(
                1,
                0x1000,
                0,
                LMCSignalValueType.UInt32,
                4,
                100);
            var inlineData = TestFrame.Hex("78 56 34 12");
            var resultData = TestFrame.Hex(
                "00 01 02 03 04 05 06 07 08 09 "
                + "0A 0B 0C 0D 0E 0F 10 11 12 13");

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            1,
                            LMCDiagnosticCapability.SDORead
                                | LMCDiagnosticCapability.ExtendedSdoResultChunk,
                            20,
                            0))),
                new FakeRpcStep(
                    0x7E50,
                    TestFrame.Response(
                        0,
                        SubmitPayload(2, 0x33333333u, LMCOperationKind.SDORead))),
                new FakeRpcStep(
                    0x7E03,
                    TestFrame.Response(
                        0,
                        InlineStatusPayload(
                            3,
                            0x33333333u,
                            inlineData))),
                new FakeRpcStep(
                    0x7E51,
                    TestFrame.Response(
                        0,
                        IntegrationChunkPayload(4, 0x33333333u, resultData))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                LMCOperationTicket readTicket;
                LMCOperationTicket extendedTicket;
                LMCOperationStatus status;
                LMCSdoResultChunk chunk;

                if (useAsync)
                {
                    readTicket = connection.Diagnostics.SubmitSdoAsync(
                            sdoRead,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    status = connection.Diagnostics.GetOperationStatusAsync(
                            readTicket,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    extendedTicket = ExtendedTicket(
                        connection,
                        readTicket,
                        20,
                        20);
                    chunk = connection.Diagnostics.ReadSdoResultChunkAsync(
                            new LMCSdoResultChunkRequest(
                                extendedTicket,
                                0,
                                20,
                                1),
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }
                else
                {
                    readTicket = connection.Diagnostics.SubmitSdo(sdoRead);
                    status = connection.Diagnostics.GetOperationStatus(readTicket);
                    extendedTicket = ExtendedTicket(
                        connection,
                        readTicket,
                        20,
                        20);
                    chunk = connection.Diagnostics.ReadSdoResultChunk(
                        new LMCSdoResultChunkRequest(
                            extendedTicket,
                            0,
                            20,
                            1));
                }

                AssertEx.False(readTicket.UsesExtendedResultChunks);
                AssertEx.Equal((ushort)4, readTicket.RequestedResultLength);
                AssertEx.True(status.IsSuccessful);
                AssertEx.Equal(4u, status.ResultLength);
                AssertEx.SequenceEqual(inlineData, status.ResultData);
                AssertEx.True(extendedTicket.UsesExtendedResultChunks);
                AssertEx.SequenceEqual(resultData, chunk.Data);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void D5WriteAllowlistFailClosed()
        {
            RunD5WriteAllowlistFailClosed(false);
            RunD5WriteAllowlistFailClosed(true);
        }

        private static void RunD5WriteAllowlistFailClosed(bool useAsync)
        {
            var piWrite = PiWriteRequest();
            var sdoWrite = LMCSdoRequest.CreateWrite(
                1,
                0x2000,
                0,
                LMCSignalValueType.UInt32,
                TestFrame.Hex("78 56 34 12"),
                100);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            1,
                            LMCDiagnosticCapability.SignalCatalog
                                | LMCDiagnosticCapability.PIWrite,
                            0,
                            0))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                if (useAsync)
                {
                    AssertEx.Throws<NotSupportedException>(
                        () => connection.Diagnostics.SubmitPIWriteAsync(
                                piWrite,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                    AssertEx.Throws<NotSupportedException>(
                        () => connection.Diagnostics.SubmitSdoAsync(
                                sdoWrite,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                }
                else
                {
                    AssertEx.Throws<NotSupportedException>(
                        () => connection.Diagnostics.SubmitPIWrite(piWrite));
                    AssertEx.Throws<NotSupportedException>(
                        () => connection.Diagnostics.SubmitSdo(sdoWrite));
                }

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void D5StatefulCancellationBoundary()
        {
            using (var submitCancellation = new CancellationTokenSource())
            using (var cancelCancellation = new CancellationTokenSource())
            {
                const uint ticketId = 0x33333333u;
                var submitStep = new FakeRpcStep(
                    0x7E50,
                    TestFrame.Response(
                        0,
                        SubmitPayload(2, ticketId, LMCOperationKind.SDORead)))
                {
                    InspectRequest = request => submitCancellation.Cancel()
                };
                var cancelStep = new FakeRpcStep(
                    0x7E04,
                    TestFrame.Response(
                        0,
                        CancelPayload(3, ticketId)))
                {
                    InspectRequest = request => cancelCancellation.Cancel()
                };

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    new FakeRpcStep(
                        0x7E00,
                        TestFrame.Response(
                            0,
                            CapabilitiesPayload(
                                1,
                                LMCDiagnosticCapability.SDORead,
                                12,
                                0))),
                    submitStep,
                    cancelStep,
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    Connect(connection, server.Port);
                    var request = LMCSdoRequest.CreateRead(
                        1,
                        0x1000,
                        0,
                        LMCSignalValueType.UInt32,
                        4,
                        100);
                    var ticket = connection.Diagnostics.SubmitSdoAsync(
                            request,
                            submitCancellation.Token)
                        .GetAwaiter()
                        .GetResult();

                    AssertEx.True(submitCancellation.IsCancellationRequested);
                    AssertEx.Equal(ticketId, ticket.TicketId);
                    connection.Diagnostics.CancelOperationAsync(
                            ticket,
                            cancelCancellation.Token)
                        .GetAwaiter()
                        .GetResult();
                    AssertEx.True(cancelCancellation.IsCancellationRequested);

                    connection.CloseConnection();
                    server.Verify();
                }
            }
        }

        private static void D5FirstSliceSdoReadMaxSdoBoundary()
        {
            RunD5FirstSliceSdoReadMaxSdoBoundary(false);
            RunD5FirstSliceSdoReadMaxSdoBoundary(true);
        }

        private static void RunD5FirstSliceSdoReadMaxSdoBoundary(
            bool useAsync)
        {
            const uint ticketId = 0x44444444u;
            var submitStep = new FakeRpcStep(
                0x7E50,
                TestFrame.Response(
                    0,
                    SubmitPayload(2, ticketId, LMCOperationKind.SDORead)))
            {
                InspectRequest = request =>
                {
                    AssertEx.Equal(40, request.Length);
                    AssertEx.Equal((ushort)32, TestFrame.ReadUInt16(request, 4));
                    AssertEx.Equal(MapRevision, TestFrame.ReadUInt32(request, 16));
                    AssertEx.Equal((ushort)1, TestFrame.ReadUInt16(request, 20));
                    AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(request, 22));
                    AssertEx.Equal((ushort)0x1000, TestFrame.ReadUInt16(request, 24));
                    AssertEx.Equal((byte)0, request[26]);
                    AssertEx.Equal((byte)LMCSignalValueType.UInt32, request[27]);
                    AssertEx.Equal(100u, TestFrame.ReadUInt32(request, 28));
                    AssertEx.Equal((ushort)4, TestFrame.ReadUInt16(request, 32));
                    AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(request, 34));
                    AssertEx.Equal(BootId, TestFrame.ReadUInt32(request, 36));
                }
            };

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            1,
                            LMCDiagnosticCapability.SDORead,
                            4,
                            0))),
                submitStep,
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var oversizedRequest = LMCSdoRequest.CreateRead(
                    1,
                    0x1000,
                    0,
                    LMCSignalValueType.UInt32,
                    8,
                    100);
                var firstSliceRequest = LMCSdoRequest.CreateRead(
                    1,
                    0x1000,
                    0,
                    LMCSignalValueType.UInt32,
                    4,
                    100);
                LMCOperationTicket ticket;
                if (useAsync)
                {
                    AssertEx.Throws<NotSupportedException>(
                        () => connection.Diagnostics.SubmitSdoAsync(
                                oversizedRequest,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                    ticket = connection.Diagnostics.SubmitSdoAsync(
                            firstSliceRequest,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }
                else
                {
                    AssertEx.Throws<NotSupportedException>(
                        () => connection.Diagnostics.SubmitSdo(
                            oversizedRequest));
                    ticket = connection.Diagnostics.SubmitSdo(
                        firstSliceRequest);
                }

                AssertEx.Equal(ticketId, ticket.TicketId);
                AssertEx.Equal((ushort)4, ticket.RequestedResultLength);
                AssertEx.False(ticket.UsesExtendedResultChunks);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static LMCPIWriteRequest PiWriteRequest()
        {
            var entry = new LMCSignalCatalogEntry(
                SignalId,
                0,
                LMCSignalSourceKind.PlcApplication,
                1,
                LMCSignalValueType.UInt16,
                2,
                0,
                LMCSignalAccessFlags.Readable
                    | LMCSignalAccessFlags.WritableByPolicy,
                LMCSignalFlags.PreOutputPhase,
                0x2000,
                0,
                LMCPdoDirection.None,
                1,
                1,
                0,
                100,
                "test.write");
            var info = new LMCSignalCatalogInfo(
                null,
                MapRevision,
                1,
                80,
                40,
                4,
                0x0F,
                1);
            var catalog = new LMCSignalCatalog(info, new[] { entry });
            return new LMCPIWriteRequest(
                catalog,
                entry,
                LMCSignalValueType.UInt16,
                50);
        }

        private static LMCRecorderConfiguration TriggeredConfiguration()
        {
            return new LMCRecorderConfiguration(
                new[] { SignalId },
                1,
                10,
                LMCRecorderBufferMode.Ring,
                LMCRecorderTriggerType.Edge,
                LMCSignalValueType.Int32,
                4,
                5,
                SignalId,
                LMCRecorderTriggerOperator.RisingEdge,
                100,
                0);
        }

        private static LMCRecorderIdentity RecorderIdentity(
            LMCDiagnostics owner,
            long sessionGeneration)
        {
            return new LMCRecorderIdentity(
                null,
                BootId,
                RecordId,
                0,
                ConfigId,
                ConfigRevision,
                MapRevision,
                OwnerEpoch,
                LMCRecorderState.Armed,
                0,
                10,
                LMCCapturePhase.InputMapped,
                1000,
                LMCRecorderBufferMode.Ring,
                LMCRecorderTriggerType.Edge,
                4,
                5,
                true,
                1280,
                new[] { SignalId },
                sessionGeneration,
                owner,
                false);
        }

        private static LMCOperationTicket ExtendedTicket(
            LMCDiagnostics owner,
            ushort resultLength,
            ushort maxChunk)
        {
            return new LMCOperationTicket(
                0x10203040u,
                LMCOperationKind.SDORead,
                100,
                BootId,
                0,
                owner,
                true,
                resultLength,
                LMCSignalValueType.UInt32,
                true,
                maxChunk);
        }

        // Keeps the generic future chunk surface covered without widening the
        // first-slice SubmitSdo policy.
        private static LMCOperationTicket ExtendedTicket(
            LMCConnection connection,
            LMCOperationTicket submittedTicket,
            ushort resultLength,
            ushort maxChunk)
        {
            return new LMCOperationTicket(
                submittedTicket.TicketId,
                LMCOperationKind.SDORead,
                submittedTicket.QueuedCycle,
                submittedTicket.DiagnosticsBootId,
                connection.SessionGeneration,
                connection.Diagnostics,
                true,
                resultLength,
                LMCSignalValueType.UInt32,
                true,
                maxChunk);
        }

        private static byte[] ConfigurePayload(uint requestId)
        {
            var payload = CommonPayload(56, requestId);
            TestFrame.WriteUInt32(payload, 16, ConfigId);
            TestFrame.WriteUInt32(payload, 20, ConfigRevision);
            TestFrame.WriteUInt32(payload, 24, MapRevision);
            TestFrame.WriteUInt32(payload, 28, 10);
            TestFrame.WriteUInt32(payload, 32, 40);
            TestFrame.WriteUInt16(payload, 36, (ushort)LMCRecorderState.Configured);
            TestFrame.WriteUInt16(payload, 38, 1);
            TestFrame.WriteUInt16(payload, 40, 4);
            TestFrame.WriteUInt16(payload, 42, 1);
            TestFrame.WriteUInt16(payload, 44, (ushort)LMCCapturePhase.InputMapped);
            TestFrame.WriteUInt32(payload, 48, OwnerEpoch);
            TestFrame.WriteUInt32(payload, 52, BootId);
            return payload;
        }

        private static byte[] StartPayload(uint requestId)
        {
            var payload = CommonPayload(40, requestId);
            TestFrame.WriteUInt32(payload, 16, RecordId);
            TestFrame.WriteUInt32(payload, 20, 0);
            TestFrame.WriteUInt16(payload, 24, (ushort)LMCRecorderState.Armed);
            TestFrame.WriteUInt32(payload, 28, OwnerEpoch);
            TestFrame.WriteUInt32(payload, 32, 100);
            TestFrame.WriteUInt32(payload, 36, BootId);
            return payload;
        }

        private static byte[] SubmitPayload(
            uint requestId,
            uint ticketId,
            LMCOperationKind kind)
        {
            var payload = CommonPayload(32, requestId);
            TestFrame.WriteUInt32(payload, 16, ticketId);
            TestFrame.WriteUInt16(payload, 20, (ushort)kind);
            TestFrame.WriteUInt16(payload, 22, (ushort)LMCOperationState.Queued);
            TestFrame.WriteUInt32(payload, 24, 100);
            TestFrame.WriteUInt32(payload, 28, BootId);
            return payload;
        }

        private static byte[] CancelPayload(uint requestId, uint ticketId)
        {
            var payload = CommonPayload(28, requestId);
            TestFrame.WriteUInt32(payload, 16, ticketId);
            TestFrame.WriteUInt16(
                payload,
                20,
                (ushort)LMCOperationState.Cancelled);
            TestFrame.WriteUInt16(
                payload,
                22,
                (ushort)LMCOperationOutcome.Cancelled);
            TestFrame.WriteUInt32(payload, 24, BootId);
            return payload;
        }

        private static byte[] InlineStatusPayload(
            uint requestId,
            uint ticketId,
            byte[] resultData)
        {
            var payload = CommonPayload(64, requestId);
            TestFrame.WriteUInt32(payload, 16, ticketId);
            TestFrame.WriteUInt16(payload, 20, (ushort)LMCOperationKind.SDORead);
            TestFrame.WriteUInt16(payload, 22, (ushort)LMCOperationState.Completed);
            TestFrame.WriteUInt32(payload, 24, 100);
            TestFrame.WriteUInt32(payload, 28, 200);
            TestFrame.WriteUInt16(payload, 32, (ushort)LMCOperationOutcome.Success);
            TestFrame.WriteUInt32(payload, 40, checked((uint)resultData.Length));
            payload[44] = (byte)LMCSignalValueType.UInt32;
            payload[45] = checked((byte)resultData.Length);
            Buffer.BlockCopy(resultData, 0, payload, 48, resultData.Length);
            TestFrame.WriteUInt32(payload, 60, BootId);
            return payload;
        }

        private static byte[] IntegrationChunkPayload(
            uint requestId,
            uint ticketId,
            byte[] data)
        {
            using (var connection = new LMCConnection())
            {
                var ticket = new LMCOperationTicket(
                    ticketId,
                    LMCOperationKind.SDORead,
                    100,
                    BootId,
                    0,
                    connection.Diagnostics,
                    true,
                    checked((ushort)data.Length),
                    LMCSignalValueType.UInt32,
                    true,
                    1280);
                var request = new LMCSdoResultChunkRequest(
                    ticket,
                    0,
                    checked((ushort)data.Length),
                    1);
                return SdoChunkPayload(requestId, ticket, request, data, true);
            }
        }

        private static byte[] SdoChunkPayload(
            uint requestId,
            LMCOperationTicket ticket,
            LMCSdoResultChunkRequest request,
            byte[] data,
            bool lastChunk)
        {
            var payload = CommonPayload(
                48 + data.Length,
                requestId,
                lastChunk
                    ? (ushort)LMCDiagnosticsResponseFlags.LastChunk
                    : (ushort)0);
            TestFrame.WriteUInt32(payload, 16, ticket.TicketId);
            TestFrame.WriteUInt32(payload, 20, request.OffsetBytes);
            TestFrame.WriteUInt16(payload, 24, checked((ushort)data.Length));
            TestFrame.WriteUInt32(payload, 28, request.Sequence);
            TestFrame.WriteUInt32(payload, 32, ticket.RequestedResultLength);
            TestFrame.WriteUInt32(payload, 40, ticket.DiagnosticsBootId);
            payload[44] = (byte)ticket.ResultValueType;
            Buffer.BlockCopy(data, 0, payload, 48, data.Length);
            TestFrame.WriteUInt32(
                payload,
                36,
                LMC_DiagnosticsParser.ComputeRecorderDataCrc32(
                    payload,
                    48,
                    data.Length));
            return payload;
        }

        private static byte[] CapabilitiesPayload(
            uint requestId,
            LMCDiagnosticCapability capabilities,
            ushort maxSdoDataBytes,
            ushort recorderBufferCount)
        {
            var payload = CommonPayload(68, requestId);
            TestFrame.WriteUInt32(payload, 16, 6);
            TestFrame.WriteUInt32(payload, 20, (uint)capabilities);
            TestFrame.WriteUInt32(payload, 24, MapRevision);
            TestFrame.WriteUInt16(payload, 28, 1);
            TestFrame.WriteUInt16(payload, 30, 32);
            TestFrame.WriteUInt16(payload, 32, 32);
            TestFrame.WriteUInt16(payload, 34, recorderBufferCount);
            TestFrame.WriteUInt32(payload, 36, 1000);
            TestFrame.WriteUInt32(payload, 40, 1000);
            TestFrame.WriteUInt16(payload, 44, 1320);
            TestFrame.WriteUInt16(payload, 46, 2040);
            TestFrame.WriteUInt16(payload, 48, 1280);
            TestFrame.WriteUInt16(payload, 50, 80);
            TestFrame.WriteUInt16(payload, 52, 16);
            TestFrame.WriteUInt32(payload, 56, 4000000);
            TestFrame.WriteUInt16(payload, 60, maxSdoDataBytes);
            TestFrame.WriteUInt32(payload, 64, BootId);
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

        private static void AssertChunkMalformed(
            LMCSdoResultChunkRequest request,
            byte[] payload)
        {
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseSdoResultChunk(
                    TestFrame.Response(0, payload),
                    1,
                    request));
        }

        private static void AssertCapabilityMalformed(byte[] payload)
        {
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseCapabilities(
                    TestFrame.Response(0, payload),
                    RequestId,
                    1));
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
    }
}

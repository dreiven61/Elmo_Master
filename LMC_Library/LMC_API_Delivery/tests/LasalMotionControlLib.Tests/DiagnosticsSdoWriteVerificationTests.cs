using System;
using System.Collections.Generic;
using System.Threading;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class DiagnosticsSdoWriteVerificationTests
    {
        private const uint DiagnosticsBootId = 0x1234ABCDu;
        private const uint MapRevision = 0x957F101Eu;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Contract.DiagnosticsSdoWriteVerification.ExactTicketProvenance",
                ExactTicketProvenance);
            tests.Add(
                "Contract.DiagnosticsSdoWriteVerification.ExactWriteTerminal",
                ExactWriteTerminal);
            tests.Add(
                "Contract.DiagnosticsSdoWriteVerification.ImmutableContext",
                ImmutableContext);
            tests.Add(
                "Rpc.DiagnosticsSdoWriteVerification.GuardedSyncAndAsync",
                GuardedSyncAndAsync);
        }

        private static void ExactTicketProvenance()
        {
            using (var server = CreateLifecycleServer())
            using (var connection = new LMCConnection())
            using (var foreignConnection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var request = WriteRequest(100);
                var ticket = WriteTicket(connection, request);
                var terminalStatus = WriteTerminalStatus(
                    connection,
                    ticket);
                var context = connection.Diagnostics
                    .CreateSdoWriteVerificationContext(
                        request,
                        ticket,
                        terminalStatus,
                        candidate => true);
                AssertEx.NotNull(context);

                AssertEx.NotNull(connection.Diagnostics
                    .CreateSdoWriteVerificationContext(
                        request,
                        ticket,
                        terminalStatus));
                AssertEx.Throws<InvalidOperationException>(
                    () => connection.Diagnostics
                        .CreateSdoWriteVerificationContext(
                            request,
                            ticket,
                            terminalStatus,
                            candidate => false));
                AssertEx.Throws<ArgumentException>(
                    () => connection.Diagnostics
                        .CreateSdoWriteVerificationContext(
                            WriteRequest(101),
                            ticket,
                            terminalStatus,
                            candidate => true));

                var unboundTicket = new LMCOperationTicket(
                    0x50505051u,
                    LMCOperationKind.SDOWrite,
                    100,
                    DiagnosticsBootId,
                    MapRevision,
                    connection.SessionGeneration,
                    connection.Diagnostics,
                    false,
                    0,
                    LMCSignalValueType.Invalid);
                AssertEx.Throws<ArgumentException>(
                    () => connection.Diagnostics
                        .CreateSdoWriteVerificationContext(
                            request,
                            unboundTicket,
                            terminalStatus,
                            candidate => true));

                var foreignTicket = WriteTicket(
                    foreignConnection,
                    request);
                AssertEx.Throws<ArgumentException>(
                    () => connection.Diagnostics
                        .CreateSdoWriteVerificationContext(
                            request,
                            foreignTicket,
                            terminalStatus,
                            candidate => true));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void ExactWriteTerminal()
        {
            using (var server = CreateLifecycleServer())
            using (var connection = new LMCConnection())
            using (var foreignConnection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var request = WriteRequest(100);
                var ticket = WriteTicket(connection, request);
                var exactStatus = WriteTerminalStatus(
                    connection,
                    ticket);
                AssertEx.NotNull(connection.Diagnostics
                    .CreateSdoWriteVerificationContext(
                        request,
                        ticket,
                        exactStatus,
                        candidate => true));

                AssertEx.Throws<ArgumentNullException>(
                    () => connection.Diagnostics
                        .CreateSdoWriteVerificationContext(
                            request,
                            ticket,
                            null,
                            candidate => true));
                AssertTerminalRejected(
                    connection,
                    request,
                    ticket,
                    WriteTerminalStatus(
                        connection,
                        ticket,
                        state: LMCOperationState.Queued,
                        outcome: LMCOperationOutcome.NoneOrPending));
                AssertTerminalRejected(
                    connection,
                    request,
                    ticket,
                    WriteTerminalStatus(
                        connection,
                        ticket,
                        state: LMCOperationState.Failed,
                        outcome: LMCOperationOutcome.Failed));
                AssertTerminalRejected(
                    connection,
                    request,
                    ticket,
                    WriteTerminalStatus(
                        connection,
                        ticket,
                        statusTicketId: ticket.TicketId + 1));
                AssertTerminalRejected(
                    connection,
                    request,
                    ticket,
                    WriteTerminalStatus(
                        connection,
                        ticket,
                        operationKind: LMCOperationKind.SDORead));
                AssertTerminalRejected(
                    connection,
                    request,
                    ticket,
                    WriteTerminalStatus(
                        connection,
                        ticket,
                        submitCycle: ticket.QueuedCycle + 1));
                AssertTerminalRejected(
                    connection,
                    request,
                    ticket,
                    WriteTerminalStatus(
                        connection,
                        ticket,
                        diagnosticsBootId:
                            ticket.DiagnosticsBootId + 1));
                AssertTerminalRejected(
                    connection,
                    request,
                    ticket,
                    WriteTerminalStatus(
                        connection,
                        ticket,
                        bindProvenance: false));
                AssertTerminalRejected(
                    connection,
                    request,
                    ticket,
                    WriteTerminalStatus(
                        foreignConnection,
                        ticket));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void AssertTerminalRejected(
            LMCConnection connection,
            LMCSdoRequest request,
            LMCOperationTicket ticket,
            LMCOperationStatus terminalStatus)
        {
            AssertEx.Throws<ArgumentException>(
                () => connection.Diagnostics
                    .CreateSdoWriteVerificationContext(
                        request,
                        ticket,
                        terminalStatus,
                        candidate => true));
        }

        private static void ImmutableContext()
        {
            using (var server = CreateLifecycleServer())
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var source = new byte[] { 0x78, 0x56, 0x34, 0x12 };
                var request = LMCSdoRequest.CreateWrite(
                    2,
                    0x2F00,
                    24,
                    LMCSignalValueType.Int32,
                    source,
                    100);
                var writeTicket = WriteTicket(connection, request);
                var context = connection.Diagnostics
                    .CreateSdoWriteVerificationContext(
                        request,
                        writeTicket,
                        WriteTerminalStatus(connection, writeTicket),
                        candidate => true);
                source[0] = 0;
                var exposed = context.ExpectedWriteData;
                exposed[1] = 0;

                AssertEx.SequenceEqual(
                    new byte[] { 0x78, 0x56, 0x34, 0x12 },
                    context.ExpectedWriteData);
                AssertEx.Equal(
                    connection.SessionGeneration,
                    context.ConnectionSessionGeneration);
                AssertEx.Equal(DiagnosticsBootId, context.DiagnosticsBootId);
                AssertEx.Equal(MapRevision, context.SubmissionMapRevision);
                AssertEx.True(context.MatchesOwnerCurrentSession(connection));

                var defaultRead = context.CreateReadRequest();
                AssertEx.Equal(100u, defaultRead.TimeoutCycles);
                AssertEx.True(context.MatchesReadRequest(defaultRead));
                var retryRead = context.CreateReadRequest(250);
                AssertEx.Equal(250u, retryRead.TimeoutCycles);
                AssertEx.True(context.MatchesReadRequest(retryRead));

                connection.CloseConnection();
                AssertEx.False(
                    context.MatchesOwnerCurrentSession(connection));
                server.Verify();
            }
        }

        private static void GuardedSyncAndAsync()
        {
            RunGuardedSubmit(false);
            RunGuardedSubmit(true);
        }

        private static void RunGuardedSubmit(bool useAsync)
        {
            const uint readTicketId = 0x51515151u;
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(2))),
                new FakeRpcStep(
                    0x7E50,
                    TestFrame.Response(
                        0,
                        SubmitPayload(3, readTicketId))),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(4))),
                new FakeRpcStep(
                    0x7E03,
                    TestFrame.Response(
                        0,
                        ReadStatusPayload(5, readTicketId))),
                CloseStep()))
            using (var connection = new LMCConnection())
            using (var foreignConnection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var writeRequest = WriteRequest(100);
                var writeTicket = WriteTicket(
                    connection,
                    writeRequest);
                var staleCapabilities = useAsync
                    ? connection.Diagnostics.GetCapabilitiesAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult()
                    : connection.Diagnostics.GetCapabilities();
                var context = connection.Diagnostics
                    .CreateSdoWriteVerificationContext(
                        writeRequest,
                        writeTicket,
                        WriteTerminalStatus(connection, writeTicket),
                        candidate => true);
                var readRequest = context.CreateReadRequest(200);

                AssertEx.Throws<ArgumentException>(
                    () => context.SubmitReadback(
                        LMCSdoRequest.CreateRead(
                            3,
                            readRequest.ObjectIndex,
                            readRequest.SubIndex,
                            readRequest.ValueType,
                            readRequest.DataLength,
                            readRequest.TimeoutCycles)));

                var readTicket = useAsync
                    ? context.SubmitReadbackAsync(
                            readRequest,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult()
                    : context.SubmitReadback(readRequest);
                AssertEx.Equal(readTicketId, readTicket.TicketId);
                AssertEx.True(
                    LMCSdoWriteVerificationContext.RequestsEqual(
                        readRequest,
                        readTicket.SubmittedSdoRequest));
                AssertEx.True(readTicket.BelongsToCurrentSession(connection));
                var freshCapabilities = useAsync
                    ? connection.Diagnostics.GetCapabilitiesAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult()
                    : connection.Diagnostics.GetCapabilities();
                var readStatus = useAsync
                    ? connection.Diagnostics.GetOperationStatusAsync(
                            readTicket,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult()
                    : connection.Diagnostics.GetOperationStatus(
                        readTicket);
                AssertEx.True(freshCapabilities.IsBoundTo(
                    connection.Diagnostics,
                    connection.SessionGeneration));
                AssertEx.True(readStatus.IsBoundTo(
                    connection.Diagnostics,
                    connection.SessionGeneration));
                AssertEx.Equal(
                    LMCSdoWriteVerificationVerdict.Pending,
                    context.Evaluate(
                        readRequest,
                        readTicket,
                        connection,
                        staleCapabilities,
                        readStatus));
                AssertEx.Equal(
                    LMCSdoWriteVerificationVerdict.Verified,
                    context.Evaluate(
                        readRequest,
                        readTicket,
                        connection,
                        freshCapabilities,
                        readStatus));

                var foreignCapabilities = ManualCapabilities(
                    foreignConnection.Diagnostics,
                    connection.SessionGeneration,
                    freshCapabilities.ObservationSequence + 1);
                AssertEx.Equal(
                    LMCSdoWriteVerificationVerdict.Pending,
                    context.Evaluate(
                        readRequest,
                        readTicket,
                        connection,
                        foreignCapabilities,
                        readStatus));
                var unboundCapabilities = CapabilitiesModel(
                    connection.SessionGeneration);
                AssertEx.Equal(
                    LMCSdoWriteVerificationVerdict.Pending,
                    context.Evaluate(
                        readRequest,
                        readTicket,
                        connection,
                        unboundCapabilities,
                        readStatus));
                var unboundStatus = new LMCOperationStatus(
                    null,
                    readStatus.TicketId,
                    readStatus.OperationKind,
                    readStatus.State,
                    readStatus.SubmitCycle,
                    readStatus.CompletionCycle,
                    readStatus.Outcome,
                    readStatus.OperationErrorId,
                    readStatus.OperationDetail,
                    readStatus.ResultLength,
                    readStatus.ResultValueType,
                    readStatus.ResultData,
                    readStatus.DiagnosticsBootId);
                AssertEx.Equal(
                    LMCSdoWriteVerificationVerdict.Pending,
                    context.Evaluate(
                        readRequest,
                        readTicket,
                        connection,
                        freshCapabilities,
                        unboundStatus));
                var foreignStatus = unboundStatus.BindProvenance(
                        foreignConnection.Diagnostics,
                        connection.SessionGeneration);
                AssertEx.Equal(
                    LMCSdoWriteVerificationVerdict.Pending,
                    context.Evaluate(
                        readRequest,
                        readTicket,
                        connection,
                        freshCapabilities,
                        foreignStatus));
                AssertEx.Equal(7, server.ReceivedRequests.Count);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static LMCSdoRequest WriteRequest(uint timeoutCycles)
        {
            return LMCSdoRequest.CreateWrite(
                2,
                0x2F00,
                24,
                LMCSignalValueType.Int32,
                new byte[] { 0x78, 0x56, 0x34, 0x12 },
                timeoutCycles);
        }

        private static LMCOperationTicket WriteTicket(
            LMCConnection connection,
            LMCSdoRequest request)
        {
            return new LMCOperationTicket(
                0x50505050u,
                LMCOperationKind.SDOWrite,
                100,
                DiagnosticsBootId,
                MapRevision,
                connection.SessionGeneration,
                connection.Diagnostics,
                false,
                0,
                LMCSignalValueType.Invalid,
                submittedSdoRequest: request);
        }

        private static LMCOperationStatus WriteTerminalStatus(
            LMCConnection bindingConnection,
            LMCOperationTicket ticket,
            uint? statusTicketId = null,
            LMCOperationKind operationKind = LMCOperationKind.SDOWrite,
            LMCOperationState state = LMCOperationState.Completed,
            LMCOperationOutcome outcome = LMCOperationOutcome.Success,
            uint? submitCycle = null,
            uint? diagnosticsBootId = null,
            bool bindProvenance = true)
        {
            var status = new LMCOperationStatus(
                null,
                statusTicketId ?? ticket.TicketId,
                operationKind,
                state,
                submitCycle ?? ticket.QueuedCycle,
                state == LMCOperationState.Queued
                    || state == LMCOperationState.Running
                    ? 0u
                    : ticket.QueuedCycle + 1,
                outcome,
                outcome == LMCOperationOutcome.Success
                    || outcome == LMCOperationOutcome.NoneOrPending
                    ? (short)0
                    : (short)-1,
                outcome == LMCOperationOutcome.Success
                    || outcome == LMCOperationOutcome.NoneOrPending
                    ? 0u
                    : 1u,
                0,
                LMCSignalValueType.Invalid,
                new byte[0],
                diagnosticsBootId ?? ticket.DiagnosticsBootId);
            return bindProvenance
                ? status.BindProvenance(
                    bindingConnection.Diagnostics,
                    ticket.ConnectionSessionGeneration)
                : status;
        }

        private static FakeRpcServer CreateLifecycleServer()
        {
            return new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep());
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

        private static byte[] CapabilitiesPayload(uint requestId)
        {
            var payload = CommonPayload(68, requestId);
            TestFrame.WriteUInt32(payload, 16, 5);
            TestFrame.WriteUInt32(
                payload,
                20,
                (uint)(LMCDiagnosticCapability.SDORead
                    | LMCDiagnosticCapability.SDOReadGeneralInline));
            TestFrame.WriteUInt32(payload, 24, MapRevision);
            TestFrame.WriteUInt32(payload, 40, 1000);
            TestFrame.WriteUInt16(payload, 44, 1320);
            TestFrame.WriteUInt16(payload, 46, 2040);
            TestFrame.WriteUInt16(payload, 60, 4);
            TestFrame.WriteUInt32(payload, 64, DiagnosticsBootId);
            return payload;
        }

        private static byte[] SubmitPayload(
            uint requestId,
            uint ticketId)
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
            TestFrame.WriteUInt32(payload, 24, 100);
            TestFrame.WriteUInt32(payload, 28, DiagnosticsBootId);
            return payload;
        }

        private static byte[] ReadStatusPayload(
            uint requestId,
            uint ticketId)
        {
            var payload = CommonPayload(64, requestId);
            TestFrame.WriteUInt32(payload, 16, ticketId);
            TestFrame.WriteUInt16(
                payload,
                20,
                (ushort)LMCOperationKind.SDORead);
            TestFrame.WriteUInt16(
                payload,
                22,
                (ushort)LMCOperationState.Completed);
            TestFrame.WriteUInt32(payload, 24, 100);
            TestFrame.WriteUInt32(payload, 28, 101);
            TestFrame.WriteUInt16(
                payload,
                32,
                (ushort)LMCOperationOutcome.Success);
            TestFrame.WriteUInt32(payload, 40, 4);
            payload[44] = (byte)LMCSignalValueType.Int32;
            payload[45] = 4;
            Buffer.BlockCopy(
                new byte[] { 0x78, 0x56, 0x34, 0x12 },
                0,
                payload,
                48,
                4);
            TestFrame.WriteUInt32(payload, 60, DiagnosticsBootId);
            return payload;
        }

        private static LMCDiagnosticCapabilities ManualCapabilities(
            LMCDiagnostics owner,
            long connectionSessionGeneration,
            long observationSequence)
        {
            return CapabilitiesModel(connectionSessionGeneration)
                .BindProvenance(
                    owner,
                    connectionSessionGeneration,
                    observationSequence);
        }

        private static LMCDiagnosticCapabilities CapabilitiesModel(
            long connectionSessionGeneration)
        {
            return new LMCDiagnosticCapabilities(
                null,
                connectionSessionGeneration,
                5,
                (uint)(LMCDiagnosticCapability.SDORead
                    | LMCDiagnosticCapability.SDOReadGeneralInline),
                MapRevision,
                0,
                0,
                0,
                0,
                0,
                1000,
                1320,
                2040,
                1280,
                80,
                16,
                0,
                4,
                DiagnosticsBootId);
        }

        private static byte[] CommonPayload(int length, uint requestId)
        {
            var payload = new byte[length];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, requestId);
            return payload;
        }
    }
}

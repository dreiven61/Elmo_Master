using System;
using System.Collections.Generic;
using System.Threading;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class DiagnosticsSdoReadFacadeTests
    {
        private const ushort SlaveReference = 2;
        private const ushort ObjectIndex = 0x6064;
        private const uint DiagnosticsBootId = 0x73A5C19Du;
        private const uint MapRevision = 0x957F101Eu;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Rpc.DiagnosticsSdoReadFacade.SyncAndAsyncTypedTerminal",
                SyncAndAsyncTypedTerminal);
            tests.Add(
                "Rpc.DiagnosticsSdoReadFacade.WriteAndExtendedPreWireRejected",
                WriteAndExtendedPreWireRejected);
            tests.Add(
                "Rpc.DiagnosticsSdoReadFacade.CapabilityOffStopsBeforeSubmit",
                CapabilityOffStopsBeforeSubmit);
            tests.Add(
                "Rpc.DiagnosticsSdoReadFacade.TerminalFailurePreservesEvidence",
                TerminalFailurePreservesEvidence);
            tests.Add(
                "Rpc.DiagnosticsSdoReadFacade.PollTimeoutPreservesLastStatus",
                PollTimeoutPreservesLastStatus);
            tests.Add(
                "Rpc.DiagnosticsSdoReadFacade.PreSubmitCancellationIsZeroWire",
                PreSubmitCancellationIsZeroWire);
            tests.Add(
                "Rpc.DiagnosticsSdoReadFacade.TerminalSuccessWinsCancellation",
                TerminalSuccessWinsCancellation);
            tests.Add(
                "Rpc.DiagnosticsSdoReadFacade.TerminalFailureWinsCancellation",
                TerminalFailureWinsCancellation);
            tests.Add(
                "Rpc.DiagnosticsSdoReadFacade.NonTerminalCancellationPreservesLastStatus",
                NonTerminalCancellationPreservesLastStatus);
        }

        private static void SyncAndAsyncTypedTerminal()
        {
            RunTypedTerminal(
                false,
                LMCSignalValueType.Int8,
                TestFrame.Hex("FE"),
                unchecked((sbyte)-2));
            RunTypedTerminal(
                true,
                LMCSignalValueType.UInt16,
                TestFrame.Hex("34 12"),
                (ushort)0x1234);
            RunTypedTerminal(
                false,
                LMCSignalValueType.Int32,
                TestFrame.Hex("78 56 34 92"),
                unchecked((int)0x92345678u));
            RunTypedTerminal(
                true,
                LMCSignalValueType.UInt32,
                TestFrame.Hex("FE DC BA 98"),
                0x98BADCFEu);
            RunTypedTerminal(
                false,
                LMCSignalValueType.Real32,
                TestFrame.Hex("00 00 A0 3F"),
                1.25f);
        }

        private static void RunTypedTerminal(
            bool useAsync,
            LMCSignalValueType valueType,
            byte[] resultData,
            object expectedValue)
        {
            const uint ticketId = 0x41424344u;
            var request = ReadRequest(
                valueType,
                checked((ushort)resultData.Length),
                100);
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CapabilitiesStep(1, ReadCapabilities),
                SubmitStep(2, ticketId, request),
                StatusStep(
                    3,
                    ticketId,
                    LMCOperationState.Queued,
                    LMCOperationOutcome.NoneOrPending,
                    LMCSignalValueType.Invalid,
                    new byte[0]),
                StatusStep(
                    4,
                    ticketId,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success,
                    valueType,
                    resultData),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server);
                var result = useAsync
                    ? connection.Diagnostics.ReadSdoInlineAsync(
                            request,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult()
                    : connection.Diagnostics.ReadSdoInline(request);

                AssertEx.NotNull(result);
                AssertEx.Equal(request, result.Request);
                AssertEx.Equal(ticketId, result.Ticket.TicketId);
                AssertEx.True(result.Ticket.BelongsToCurrentSession(connection));
                AssertEx.True(result.Status.IsSuccessful);
                AssertEx.Equal(valueType, result.ValueType);
                AssertEx.Equal(request.DataLength, result.DataLength);
                AssertEx.SequenceEqual(resultData, result.ResultData);
                AssertEx.Equal(expectedValue, result.Value);

                var exposed = result.ResultData;
                exposed[0] ^= 0xFF;
                AssertEx.SequenceEqual(resultData, result.ResultData);
                AssertEx.False(
                    ReferenceEquals(exposed, result.ResultData),
                    "ResultData must return a fresh immutable snapshot clone.");

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void WriteAndExtendedPreWireRejected()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server);
                var write = LMCSdoRequest.CreateWrite(
                    SlaveReference,
                    0x2F00,
                    24,
                    LMCSignalValueType.Int32,
                    TestFrame.Hex("01 00 00 00"),
                    100);
                var read8 = ReadRequest(
                    LMCSignalValueType.UInt32,
                    8,
                    100);
                var read12 = ReadRequest(
                    LMCSignalValueType.UInt32,
                    12,
                    100);

                AssertPreWireRejected(
                    connection,
                    () => connection.Diagnostics.ReadSdoInline(write));
                AssertPreWireRejected(
                    connection,
                    () => connection.Diagnostics.ReadSdoInlineAsync(
                            read8,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertPreWireRejected(
                    connection,
                    () => connection.Diagnostics.ReadSdoInline(read12));
                AssertEx.Equal(2, server.ReceivedRequests.Count);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void CapabilityOffStopsBeforeSubmit()
        {
            var request = ReadRequest(LMCSignalValueType.UInt32, 4, 100);
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CapabilitiesStep(1, 0),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server);
                var exception = AssertEx.Throws<NotSupportedException>(
                    () => connection.Diagnostics.ReadSdoInline(request));
                var context = RequireSubmissionContext(exception);
                AssertEx.Equal(
                    LMCSdoSubmissionPhase.CapabilityPreflight,
                    context.Phase);
                AssertEx.Equal(
                    LMCSdoSubmissionOutcome.NotAttempted,
                    context.SubmissionOutcome);
                AssertEx.Equal(DiagnosticsBootId, context.DiagnosticsBootId);
                AssertEx.Equal(MapRevision, context.MapRevision);
                AssertEx.Equal(3, server.ReceivedRequests.Count);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void TerminalFailurePreservesEvidence()
        {
            const uint ticketId = 0x51525354u;
            var request = ReadRequest(LMCSignalValueType.UInt32, 4, 100);
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CapabilitiesStep(1, ReadCapabilities),
                SubmitStep(2, ticketId, request),
                StatusStep(
                    3,
                    ticketId,
                    LMCOperationState.Failed,
                    LMCOperationOutcome.Failed,
                    LMCSignalValueType.Invalid,
                    new byte[0],
                    -9,
                    0x05040000u),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server);
                var exception = AssertEx.Throws<LMCSdoReadOperationException>(
                    () => connection.Diagnostics.ReadSdoInline(request));
                AssertEx.Equal(ticketId, exception.Ticket.TicketId);
                AssertEx.Equal(
                    LMCOperationState.Failed,
                    exception.OperationStatus.State);
                AssertEx.Equal(
                    LMCOperationOutcome.Failed,
                    exception.OperationStatus.Outcome);

                var context = RequireSubmissionContext(exception);
                AssertEx.Equal(
                    LMCSdoSubmissionOutcome.Accepted,
                    context.SubmissionOutcome);
                AssertEx.Equal(ticketId, context.Ticket.TicketId);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void PollTimeoutPreservesLastStatus()
        {
            const uint ticketId = 0x61626364u;
            var request = ReadRequest(LMCSignalValueType.UInt32, 4, 1);
            var steps = new List<FakeRpcStep>
            {
                InitStep(),
                CallbackStep(),
                CapabilitiesStep(1, ReadCapabilities),
                SubmitStep(2, ticketId, request)
            };
            var pollLimit = LMCDiagnostics.GetInlineSdoTerminalPollLimit(1);
            for (var poll = 0; poll < pollLimit; poll++)
            {
                steps.Add(StatusStep(
                    checked((uint)(3 + poll)),
                    ticketId,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending,
                    LMCSignalValueType.Invalid,
                    new byte[0]));
            }
            steps.Add(CloseStep());

            using (var server = new FakeRpcServer(steps.ToArray()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server);
                var exception = AssertEx.Throws<LMCSdoReadPollingTimeoutException>(
                    () => connection.Diagnostics.ReadSdoInline(request));
                AssertEx.Equal(ticketId, exception.Ticket.TicketId);
                AssertEx.Equal(pollLimit, exception.PollCount);
                AssertLastObservedStatus(
                    exception.LastObservedStatus,
                    ticketId,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending);

                var context = RequireSubmissionContext(exception);
                AssertEx.Equal(
                    LMCSdoSubmissionOutcome.Accepted,
                    context.SubmissionOutcome);
                AssertEx.Equal(ticketId, context.Ticket.TicketId);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void PreSubmitCancellationIsZeroWire()
        {
            var request = ReadRequest(LMCSignalValueType.UInt32, 4, 100);
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                Connect(connection, server);
                cancellation.Cancel();
                var exception = AssertEx.Throws<OperationCanceledException>(
                    () => connection.Diagnostics.ReadSdoInlineAsync(
                            request,
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());
                var context = RequireSubmissionContext(exception);
                AssertEx.Equal(
                    LMCSdoSubmissionPhase.SessionPreflight,
                    context.Phase);
                AssertEx.Equal(
                    LMCSdoSubmissionOutcome.NotAttempted,
                    context.SubmissionOutcome);
                AssertEx.Equal(2, server.ReceivedRequests.Count);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void TerminalSuccessWinsCancellation()
        {
            const uint ticketId = 0x71727374u;
            var request = ReadRequest(LMCSignalValueType.UInt32, 4, 100);
            using (var cancellation = new CancellationTokenSource())
            {
                var status = StatusStep(
                    3,
                    ticketId,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success,
                    LMCSignalValueType.UInt32,
                    TestFrame.Hex("78 56 34 12"));
                status.InspectRequest = ignored => cancellation.Cancel();

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    CapabilitiesStep(1, ReadCapabilities),
                    SubmitStep(2, ticketId, request),
                    status,
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    Connect(connection, server);
                    var result = connection.Diagnostics.ReadSdoInlineAsync(
                            request,
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult();
                    AssertEx.True(cancellation.IsCancellationRequested);
                    AssertEx.Equal(ticketId, result.Ticket.TicketId);
                    AssertEx.Equal(
                        LMCOperationState.Completed,
                        result.Status.State);
                    AssertEx.Equal(
                        LMCOperationOutcome.Success,
                        result.Status.Outcome);
                    AssertEx.Equal(0x12345678u, result.Value);
                    AssertEx.True(connection.IsConnected);
                    AssertEx.Equal(5, server.ReceivedRequests.Count);

                    connection.CloseConnection();
                    server.Verify();
                }
            }
        }

        private static void TerminalFailureWinsCancellation()
        {
            const uint ticketId = 0x81828384u;
            var request = ReadRequest(LMCSignalValueType.UInt32, 4, 100);
            using (var cancellation = new CancellationTokenSource())
            {
                var status = StatusStep(
                    3,
                    ticketId,
                    LMCOperationState.Failed,
                    LMCOperationOutcome.Failed,
                    LMCSignalValueType.Invalid,
                    new byte[0],
                    -9,
                    0x05040000u);
                status.InspectRequest = ignored => cancellation.Cancel();

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    CapabilitiesStep(1, ReadCapabilities),
                    SubmitStep(2, ticketId, request),
                    status,
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    Connect(connection, server);
                    var exception = AssertEx.Throws<
                        LMCSdoReadOperationException>(
                        () => connection.Diagnostics.ReadSdoInlineAsync(
                                request,
                                cancellation.Token)
                            .GetAwaiter()
                            .GetResult());
                    AssertEx.True(cancellation.IsCancellationRequested);
                    AssertEx.Equal(ticketId, exception.Ticket.TicketId);
                    AssertEx.Equal(
                        LMCOperationState.Failed,
                        exception.OperationStatus.State);
                    AssertEx.Equal(
                        LMCOperationOutcome.Failed,
                        exception.OperationStatus.Outcome);
                    AssertEx.Equal(
                        checked((short)-9),
                        exception.OperationStatus.OperationErrorId);
                    AssertEx.Equal(
                        0x05040000u,
                        exception.OperationStatus.OperationDetail);
                    AssertEx.True(connection.IsConnected);

                    var context = RequireSubmissionContext(exception);
                    AssertEx.Equal(
                        LMCSdoSubmissionOutcome.Accepted,
                        context.SubmissionOutcome);
                    AssertEx.Equal(ticketId, context.Ticket.TicketId);
                    AssertEx.Equal(5, server.ReceivedRequests.Count);

                    connection.CloseConnection();
                    server.Verify();
                }
            }
        }

        private static void NonTerminalCancellationPreservesLastStatus()
        {
            const uint ticketId = 0x91929394u;
            var request = ReadRequest(LMCSignalValueType.UInt32, 4, 100);
            using (var cancellation = new CancellationTokenSource())
            {
                var status = StatusStep(
                    3,
                    ticketId,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending,
                    LMCSignalValueType.Invalid,
                    new byte[0]);
                status.InspectRequest = ignored => cancellation.Cancel();

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    CapabilitiesStep(1, ReadCapabilities),
                    SubmitStep(2, ticketId, request),
                    status,
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    Connect(connection, server);
                    var exception = AssertEx.Throws<
                        LMCSdoReadWaitCanceledException>(
                        () => connection.Diagnostics.ReadSdoInlineAsync(
                                request,
                                cancellation.Token)
                            .GetAwaiter()
                            .GetResult());
                    AssertEx.True(cancellation.IsCancellationRequested);
                    AssertEx.Equal(ticketId, exception.Ticket.TicketId);
                    AssertLastObservedStatus(
                        exception.LastObservedStatus,
                        ticketId,
                        LMCOperationState.Running,
                        LMCOperationOutcome.NoneOrPending);
                    AssertEx.True(connection.IsConnected);

                    var context = RequireSubmissionContext(exception);
                    AssertEx.Equal(
                        LMCSdoSubmissionOutcome.Accepted,
                        context.SubmissionOutcome);
                    AssertEx.Equal(ticketId, context.Ticket.TicketId);
                    AssertEx.Equal(5, server.ReceivedRequests.Count);

                    connection.CloseConnection();
                    server.Verify();
                }
            }
        }

        private static void AssertLastObservedStatus(
            LMCOperationStatus status,
            uint ticketId,
            LMCOperationState expectedState,
            LMCOperationOutcome expectedOutcome)
        {
            AssertEx.NotNull(status);
            AssertEx.Equal(ticketId, status.TicketId);
            AssertEx.Equal(LMCOperationKind.SDORead, status.OperationKind);
            AssertEx.Equal(expectedState, status.State);
            AssertEx.Equal(expectedOutcome, status.Outcome);
            AssertEx.Equal(100u, status.SubmitCycle);
            AssertEx.Equal(DiagnosticsBootId, status.DiagnosticsBootId);
            AssertEx.False(status.IsTerminal);
        }

        private static void AssertPreWireRejected(
            LMCConnection connection,
            Action action)
        {
            var exception = AssertEx.Throws<NotSupportedException>(action);
            var context = RequireSubmissionContext(exception);
            AssertEx.Equal(
                LMCSdoSubmissionPhase.RequestValidation,
                context.Phase);
            AssertEx.Equal(
                LMCSdoSubmissionOutcome.NotAttempted,
                context.SubmissionOutcome);
            AssertEx.Equal(0u, context.DiagnosticsBootId);
            AssertEx.Equal(0u, context.MapRevision);
            AssertEx.Equal<LMCOperationTicket>(null, context.Ticket);
            AssertEx.True(connection.IsConnected);
        }

        private static LMCSdoSubmissionFailureContext
            RequireSubmissionContext(Exception exception)
        {
            LMCSdoSubmissionFailureContext context;
            AssertEx.True(
                LMCSdoSubmissionFailureContext.TryGet(
                    exception,
                    out context),
                "Expected an SDO submission failure context.");
            AssertEx.NotNull(context);
            return context;
        }

        private static LMCSdoRequest ReadRequest(
            LMCSignalValueType valueType,
            ushort dataLength,
            uint timeoutCycles)
        {
            return LMCSdoRequest.CreateRead(
                SlaveReference,
                ObjectIndex,
                0,
                valueType,
                dataLength,
                timeoutCycles);
        }

        private static void Connect(
            LMCConnection connection,
            FakeRpcServer server)
        {
            connection.RpcInitConnection(
                "127.0.0.1",
                server.Port,
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

        private static FakeRpcStep CapabilitiesStep(
            uint requestId,
            uint capabilityBits)
        {
            var payload = CommonPayload(68, requestId);
            TestFrame.WriteUInt32(payload, 16, 5);
            TestFrame.WriteUInt32(payload, 20, capabilityBits);
            TestFrame.WriteUInt32(payload, 24, MapRevision);
            TestFrame.WriteUInt32(payload, 40, 1000);
            TestFrame.WriteUInt16(payload, 44, 1320);
            TestFrame.WriteUInt16(payload, 46, 2040);
            TestFrame.WriteUInt16(payload, 60, 4);
            TestFrame.WriteUInt32(payload, 64, DiagnosticsBootId);
            return new FakeRpcStep(
                0x7E00,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep SubmitStep(
            uint requestId,
            uint ticketId,
            LMCSdoRequest expectedRequest)
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

            return new FakeRpcStep(
                0x7E50,
                TestFrame.Response(0, payload))
            {
                InspectRequest = raw =>
                {
                    AssertEx.Equal(
                        expectedRequest.SlaveReference,
                        TestFrame.ReadUInt16(raw, 20));
                    AssertEx.Equal(
                        expectedRequest.ObjectIndex,
                        TestFrame.ReadUInt16(raw, 24));
                    AssertEx.Equal(expectedRequest.SubIndex, raw[26]);
                    AssertEx.Equal((byte)expectedRequest.ValueType, raw[27]);
                    AssertEx.Equal(
                        expectedRequest.TimeoutCycles,
                        TestFrame.ReadUInt32(raw, 28));
                    AssertEx.Equal(
                        expectedRequest.DataLength,
                        TestFrame.ReadUInt16(raw, 32));
                }
            };
        }

        private static FakeRpcStep StatusStep(
            uint requestId,
            uint ticketId,
            LMCOperationState state,
            LMCOperationOutcome outcome,
            LMCSignalValueType resultType,
            byte[] resultData,
            short operationErrorId = 0,
            uint operationDetail = 0)
        {
            var safeData = resultData ?? new byte[0];
            var payload = CommonPayload(64, requestId);
            TestFrame.WriteUInt32(payload, 16, ticketId);
            TestFrame.WriteUInt16(
                payload,
                20,
                (ushort)LMCOperationKind.SDORead);
            TestFrame.WriteUInt16(payload, 22, (ushort)state);
            TestFrame.WriteUInt32(payload, 24, 100);
            TestFrame.WriteUInt32(
                payload,
                28,
                state == LMCOperationState.Queued
                        || state == LMCOperationState.Running
                    ? 0u
                    : 200u);
            TestFrame.WriteUInt16(payload, 32, (ushort)outcome);
            TestFrame.WriteInt16(payload, 34, operationErrorId);
            TestFrame.WriteUInt32(payload, 36, operationDetail);
            TestFrame.WriteUInt32(
                payload,
                40,
                outcome == LMCOperationOutcome.Success
                    ? checked((uint)safeData.Length)
                    : 0u);
            payload[44] = (byte)resultType;
            payload[45] = checked((byte)safeData.Length);
            Buffer.BlockCopy(safeData, 0, payload, 48, safeData.Length);
            TestFrame.WriteUInt32(payload, 60, DiagnosticsBootId);
            return new FakeRpcStep(
                0x7E03,
                TestFrame.Response(0, payload));
        }

        private static byte[] CommonPayload(int length, uint requestId)
        {
            var payload = new byte[length];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, requestId);
            return payload;
        }

        private static FakeRpcStep CloseStep()
        {
            return new FakeRpcStep(
                0x405D,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")));
        }

        private static uint ReadCapabilities
        {
            get
            {
                return (uint)(LMCDiagnosticCapability.SDORead
                    | LMCDiagnosticCapability.SDOReadGeneralInline);
            }
        }
    }
}

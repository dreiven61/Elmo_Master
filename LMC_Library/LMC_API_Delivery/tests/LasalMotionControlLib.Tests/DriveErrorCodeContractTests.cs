using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class DriveErrorCodeContractTests
    {
        private const uint MapRevision = 0x957F101Eu;
        private const uint DiagnosticsBootId = 0x89ABCDEFu;
        private const ushort AxisReference = 1;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Rpc.DriveErrorCode.SyncAsyncExactSingleRead",
                SyncAsyncExactSingleRead);
            tests.Add(
                "Rpc.DriveErrorCode.CapabilityGateZeroSubmit",
                CapabilityGateZeroSubmit);
            tests.Add(
                "Rpc.DriveErrorCode.IdentityGateZeroSubmit",
                IdentityGateZeroSubmit);
            tests.Add(
                "Rpc.DriveErrorCode.PayloadGateZeroSubmit",
                PayloadGateZeroSubmit);
            tests.Add(
                "Rpc.DriveErrorCode.PhysicalSlaveGateZeroDiagnostics",
                PhysicalSlaveGateZeroDiagnostics);
            tests.Add(
                "Rpc.DriveErrorCode.StaleSessionZeroWire",
                StaleSessionZeroWire);
            tests.Add(
                "Rpc.DriveErrorCode.FailureEvidenceNoRetry",
                FailureEvidenceNoRetry);
            tests.Add(
                "Rpc.DriveErrorCode.SubmitResponseLossUncertainNoRetry",
                SubmitResponseLossUncertainNoRetry);
        }

        private static void SyncAsyncExactSingleRead()
        {
            RunSuccessfulRead(false, 0x1234, 0x81010101u);
            RunSuccessfulRead(true, 0, 0x81010102u);
        }

        private static void RunSuccessfulRead(
            bool useAsync,
            ushort errorCode,
            uint ticketId)
        {
            var resultData = new[]
            {
                (byte)(errorCode & 0xFF),
                (byte)(errorCode >> 8)
            };

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(),
                AxisInfoStep(),
                CapabilitiesStep(1),
                SdoSubmitStep(2, ticketId, 37),
                OperationStatusStep(
                    3,
                    ticketId,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success,
                    LMCSignalValueType.UInt16,
                    resultData),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server);
                var axis = new LMCAxis(connection, "_LMCAxis1");

                var result = useAsync
                    ? axis.GetDriveErrorCodeAsync(
                            37,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult()
                    : axis.GetDriveErrorCode(37);

                AssertEx.Equal(AxisReference, result.AxisReference);
                AssertEx.Equal(errorCode, result.ErrorCode);
                AssertEx.Equal(errorCode != 0, result.HasError);
                AssertEx.True(result.IsSuccessful);
                AssertEx.Equal(ticketId, result.Ticket.TicketId);
                AssertEx.Equal(
                    LMCOperationKind.SDORead,
                    result.Ticket.OperationKind);
                AssertEx.Equal(
                    DiagnosticsBootId,
                    result.Ticket.DiagnosticsBootId);
                AssertEx.Equal(
                    MapRevision,
                    result.Ticket.SubmissionMapRevision);
                AssertEx.Equal((uint)200, result.OperationStatus.CompletionCycle);
                AssertEx.Equal(
                    LMCSignalValueType.UInt16,
                    result.OperationStatus.ResultValueType);
                AssertEx.Equal((ushort)2, result.OperationStatus.ResultLength);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(8, server.ReceivedRequests.Count);
            }
        }

        private static void CapabilityGateZeroSubmit()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(),
                AxisInfoStep(),
                CapabilitiesStep(
                    1,
                    LMCDiagnosticCapability.SDORead),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server);
                var axis = new LMCAxis(connection, "_LMCAxis1");

                var exception = AssertEx.Throws<NotSupportedException>(
                    () => axis.GetDriveErrorCode(100));
                var context = RequireFailureContext(exception);

                AssertEx.Equal(
                    LMCDriveReadOperationKind.DriveErrorCode,
                    context.OperationKind);
                AssertEx.Equal(
                    LMCDriveReadAttemptPhase.CapabilityPreflight,
                    context.Phase);
                AssertEx.Equal(1, context.SdoAttempts.Count);
                AssertEx.Equal(
                    LMCSdoSubmissionOutcome.NotAttempted,
                    context.CurrentSdoAttempt.GenericSubmissionOutcome);
                AssertEx.Equal(
                    DiagnosticsBootId,
                    context.CurrentSdoAttempt.DiagnosticsBootId);
                AssertEx.Equal(
                    MapRevision,
                    context.CurrentSdoAttempt.MapRevision);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(6, server.ReceivedRequests.Count);
            }
        }

        private static void IdentityGateZeroSubmit()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(),
                AxisInfoStep(),
                CapabilitiesStep(
                    1,
                    LMCDiagnosticCapability.SDORead
                        | LMCDiagnosticCapability.SDOReadGeneralInline,
                    0,
                    MapRevision),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server);
                var axis = new LMCAxis(connection, "_LMCAxis1");

                var exception = AssertEx.Throws<InvalidDataException>(
                    () => axis.GetDriveErrorCode(100));
                var context = RequireFailureContext(exception);

                AssertEx.Equal(
                    LMCDriveReadOperationKind.DriveErrorCode,
                    context.OperationKind);
                AssertEx.Equal(
                    LMCDriveReadAttemptPhase.CapabilityPreflight,
                    context.Phase);
                AssertEx.Equal(1, context.SdoAttempts.Count);
                AssertEx.Equal(
                    LMCSdoSubmissionOutcome.NotAttempted,
                    context.CurrentSdoAttempt.GenericSubmissionOutcome);
                AssertEx.Equal(
                    (uint)0,
                    context.CurrentSdoAttempt.DiagnosticsBootId);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(6, server.ReceivedRequests.Count);
            }
        }

        private static void PhysicalSlaveGateZeroDiagnostics()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(5),
                AxisInfoStep(5),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server);
                var axis = new LMCAxis(connection, "_LMCAxis5");

                var exception = AssertEx.Throws<NotSupportedException>(
                    () => axis.GetDriveErrorCode(100));
                var context = RequireFailureContext(exception);

                AssertEx.Equal(
                    LMCDriveReadOperationKind.DriveErrorCode,
                    context.OperationKind);
                AssertEx.Equal(
                    LMCDriveReadAttemptPhase.FacadePreflight,
                    context.Phase);
                AssertEx.Equal(0, context.SdoAttempts.Count);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(5, server.ReceivedRequests.Count);
            }
        }

        private static void PayloadGateZeroSubmit()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(),
                AxisInfoStep(),
                CapabilitiesStep(
                    1,
                    LMCDiagnosticCapability.SDORead
                        | LMCDiagnosticCapability.SDOReadGeneralInline,
                    DiagnosticsBootId,
                    MapRevision,
                    1),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server);
                var axis = new LMCAxis(connection, "_LMCAxis1");

                var exception = AssertEx.Throws<InvalidDataException>(
                    () => axis.GetDriveErrorCode(100));
                var context = RequireFailureContext(exception);

                AssertEx.Equal(
                    LMCDriveReadOperationKind.DriveErrorCode,
                    context.OperationKind);
                AssertEx.Equal(
                    LMCDriveReadAttemptPhase.CapabilityPreflight,
                    context.Phase);
                AssertEx.Equal(1, context.SdoAttempts.Count);
                AssertEx.Equal(
                    LMCSdoSubmissionOutcome.NotAttempted,
                    context.CurrentSdoAttempt.GenericSubmissionOutcome);
                AssertEx.Equal(
                    (ushort)2,
                    context.CurrentSdoAttempt.Request.DataLength);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(6, server.ReceivedRequests.Count);
            }
        }

        private static void StaleSessionZeroWire()
        {
            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(),
                AxisInfoStep(),
                CloseStep()))
            using (var secondServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, firstServer);
                var staleAxis = new LMCAxis(connection, "_LMCAxis1");

                connection.RpcInitConnection(
                    "127.0.0.1",
                    secondServer.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var exception = AssertEx.Throws<InvalidOperationException>(
                    () => staleAxis.GetDriveErrorCode(100));
                var context = RequireFailureContext(exception);

                AssertEx.Equal(
                    LMCDriveReadOperationKind.DriveErrorCode,
                    context.OperationKind);
                AssertEx.Equal(
                    LMCDriveReadAttemptPhase.FacadePreflight,
                    context.Phase);
                AssertEx.Equal(0, context.SdoAttempts.Count);

                connection.CloseConnection();
                firstServer.Verify();
                secondServer.Verify();
                AssertEx.Equal(5, firstServer.ReceivedRequests.Count);
                AssertEx.Equal(3, secondServer.ReceivedRequests.Count);
            }
        }

        private static void FailureEvidenceNoRetry()
        {
            const uint ticketId = 0x82020202u;
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(),
                AxisInfoStep(),
                CapabilitiesStep(1),
                SdoSubmitStep(2, ticketId, 100),
                OperationStatusStep(
                    3,
                    ticketId,
                    LMCOperationState.Failed,
                    LMCOperationOutcome.Failed,
                    LMCSignalValueType.Invalid,
                    new byte[0],
                    -55,
                    0x603F0001u),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server);
                var axis = new LMCAxis(connection, "_LMCAxis1");

                var exception = AssertEx.Throws<LMCSdoReadOperationException>(
                    () => axis.GetDriveErrorCode(100));
                var context = RequireFailureContext(exception);

                AssertEx.Equal(ticketId, exception.Ticket.TicketId);
                AssertEx.Equal(
                    LMCOperationState.Failed,
                    exception.OperationStatus.State);
                AssertEx.Equal((short)-55, exception.OperationStatus.OperationErrorId);
                AssertEx.Equal(
                    0x603F0001u,
                    exception.OperationStatus.OperationDetail);
                AssertEx.Equal(
                    LMCDriveReadOperationKind.DriveErrorCode,
                    context.OperationKind);
                AssertEx.Equal(
                    LMCDriveReadAttemptPhase.StatusPolling,
                    context.Phase);
                AssertEx.Equal(1, context.SdoAttempts.Count);
                AssertEx.Equal(
                    LMCSdoSubmissionOutcome.Accepted,
                    context.CurrentSdoAttempt.GenericSubmissionOutcome);
                AssertEx.True(context.CurrentSdoAttempt.IsTerminal);
                AssertEx.Equal(
                    (ushort)0x603F,
                    context.CurrentSdoAttempt.Request.ObjectIndex);
                AssertEx.Equal(
                    LMCSignalValueType.UInt16,
                    context.CurrentSdoAttempt.Request.ValueType);
                AssertEx.Equal(
                    (ushort)2,
                    context.CurrentSdoAttempt.Request.DataLength);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(8, server.ReceivedRequests.Count);
            }
        }

        private static void SubmitResponseLossUncertainNoRetry()
        {
            var lostSubmit = new FakeRpcStep(0x7E50, new byte[0])
            {
                CloseAfterResponse = true,
                InspectRequest = request =>
                    InspectDriveErrorCodeSubmit(request, 100)
            };

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(),
                AxisInfoStep(),
                CapabilitiesStep(1),
                lostSubmit))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server);
                var axis = new LMCAxis(connection, "_LMCAxis1");

                var exception = AssertEx.Throws<Exception>(
                    () => axis.GetDriveErrorCode(100));
                var context = RequireFailureContext(exception);

                AssertEx.False(exception is LMCSdoReadCommandException);
                AssertEx.Equal(
                    LMCDriveReadOperationKind.DriveErrorCode,
                    context.OperationKind);
                AssertEx.Equal(
                    LMCDriveReadAttemptPhase.Submission,
                    context.Phase);
                AssertEx.Equal(1, context.SdoAttempts.Count);
                AssertEx.Equal(
                    LMCSdoSubmissionOutcome.OutcomeUncertain,
                    context.CurrentSdoAttempt.GenericSubmissionOutcome);
                AssertEx.True(context.CurrentSdoAttempt.Ticket == null);
                AssertEx.Equal(
                    (ushort)0x603F,
                    context.CurrentSdoAttempt.Request.ObjectIndex);
                AssertEx.Equal(
                    LMCSignalValueType.UInt16,
                    context.CurrentSdoAttempt.Request.ValueType);
                AssertEx.Equal(
                    (ushort)2,
                    context.CurrentSdoAttempt.Request.DataLength);

                server.Verify();
                AssertEx.Equal(6, server.ReceivedRequests.Count);
            }
        }

        private static LMCDriveReadFailureContext RequireFailureContext(
            Exception exception)
        {
            LMCDriveReadFailureContext context;
            AssertEx.True(
                LMCDriveReadFailureContext.TryGet(exception, out context),
                "Expected typed drive-read failure context.");
            AssertEx.NotNull(context);
            return context;
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

        private static FakeRpcStep AxisLookupStep(
            ushort axisReference = AxisReference)
        {
            var payload = new byte[6];
            TestFrame.WriteUInt16(payload, 4, axisReference);
            return new FakeRpcStep(0x103C, TestFrame.Response(0, payload));
        }

        private static FakeRpcStep AxisInfoStep(
            ushort axisReference = AxisReference)
        {
            var payload = new byte[8];
            TestFrame.WriteUInt32(payload, 0, axisReference);
            return new FakeRpcStep(0x202B, TestFrame.Response(0, payload));
        }

        private static FakeRpcStep CapabilitiesStep(
            uint requestId,
            LMCDiagnosticCapability capabilities =
                LMCDiagnosticCapability.SDORead
                    | LMCDiagnosticCapability.SDOReadGeneralInline,
            uint diagnosticsBootId = DiagnosticsBootId,
            uint mapRevision = MapRevision,
            ushort maxSdoDataBytes = 4)
        {
            var payload = CommonPayload(68, requestId);
            TestFrame.WriteUInt32(payload, 16, 5);
            TestFrame.WriteUInt32(payload, 20, (uint)capabilities);
            TestFrame.WriteUInt32(payload, 24, mapRevision);
            TestFrame.WriteUInt32(payload, 40, 1000);
            TestFrame.WriteUInt16(payload, 44, 1320);
            TestFrame.WriteUInt16(payload, 46, 2040);
            TestFrame.WriteUInt16(payload, 60, maxSdoDataBytes);
            TestFrame.WriteUInt32(payload, 64, diagnosticsBootId);
            return new FakeRpcStep(0x7E00, TestFrame.Response(0, payload));
        }

        private static FakeRpcStep SdoSubmitStep(
            uint requestId,
            uint ticketId,
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
            TestFrame.WriteUInt32(payload, 24, 100);
            TestFrame.WriteUInt32(payload, 28, DiagnosticsBootId);

            return new FakeRpcStep(0x7E50, TestFrame.Response(0, payload))
            {
                InspectRequest = request =>
                    InspectDriveErrorCodeSubmit(request, timeoutCycles)
            };
        }

        private static void InspectDriveErrorCodeSubmit(
            byte[] request,
            uint timeoutCycles)
        {
            AssertEx.Equal(40, request.Length);
            AssertEx.Equal(MapRevision, TestFrame.ReadUInt32(request, 16));
            AssertEx.Equal(AxisReference, TestFrame.ReadUInt16(request, 20));
            AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(request, 22));
            AssertEx.Equal((ushort)0x603F, TestFrame.ReadUInt16(request, 24));
            AssertEx.Equal((byte)0, request[26]);
            AssertEx.Equal((byte)LMCSignalValueType.UInt16, request[27]);
            AssertEx.Equal(timeoutCycles, TestFrame.ReadUInt32(request, 28));
            AssertEx.Equal((ushort)2, TestFrame.ReadUInt16(request, 32));
            AssertEx.Equal(DiagnosticsBootId, TestFrame.ReadUInt32(request, 36));
        }

        private static FakeRpcStep OperationStatusStep(
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
            return new FakeRpcStep(0x7E03, TestFrame.Response(0, payload));
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
    }
}

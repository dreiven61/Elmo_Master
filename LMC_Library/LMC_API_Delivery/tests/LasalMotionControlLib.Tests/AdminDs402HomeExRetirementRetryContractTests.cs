using System;
using System.Collections.Generic;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class AdminDs402HomeExRetirementRetryContractTests
    {
        private const uint DiagnosticsBuild = 0x55667788u;
        private const uint DiagnosticsBootId = 0x99AABBCCu;
        private const uint MapRevision = 0xDDEEFF00u;
        private const uint RecordGeneration = 7u;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Rpc.Admin.Ds402HomeEx.RetireResponseLossReconnectExactRetry",
                RetireResponseLossReconnectExactRetry);
        }

        private static void RetireResponseLossReconnectExactRetry()
        {
            var key = RecoveryKey();
            var terminalOutcome = TerminalOutcome(key);
            var lostRetire = new FakeRpcStep(
                LMC_CommandId.RetireAxisDs402HomeExOutcome,
                new byte[0])
            {
                CloseClientBeforeResponseAndContinue = true
            };
            var retryRetire = new FakeRpcStep(
                LMC_CommandId.RetireAxisDs402HomeExOutcome,
                null)
            {
                ResponseFactory = request => TestFrame.Response(
                    0,
                    OutcomePayload(
                        TestFrame.ReadUInt32(request, 12),
                        key))
            };

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                AdminCapabilitiesStep(1),
                DiagnosticsCapabilitiesStep(1),
                lostRetire,
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                AdminCapabilitiesStep(1),
                DiagnosticsCapabilitiesStep(1),
                retryRetire,
                CloseStep()))
            using (var firstConnection = new LMCConnection())
            using (var recoveryConnection = new LMCConnection())
            {
                Connect(firstConnection, server.Port);
                var firstAxis = new LMCSingleAxis(
                    firstConnection,
                    "_LMCAxis1");
                var firstCapabilities = firstConnection.Admin.GetCapabilities();
                var firstDiagnostics =
                    firstConnection.Diagnostics.GetCapabilities();

                AssertEx.Throws<Exception>(() =>
                    firstAxis.RetireDs402HomeExOutcome(
                        terminalOutcome,
                        firstCapabilities,
                        firstDiagnostics));
                AssertEx.Equal(LMCConnectionState.Faulted, firstConnection.State);

                Connect(recoveryConnection, server.Port);
                var recoveryAxis = new LMCSingleAxis(
                    recoveryConnection,
                    "_LMCAxis1");
                var recoveryCapabilities =
                    recoveryConnection.Admin.GetCapabilities();
                var recoveryDiagnostics =
                    recoveryConnection.Diagnostics.GetCapabilities();
                var retirement = recoveryAxis.RetireDs402HomeExOutcome(
                    terminalOutcome,
                    recoveryCapabilities,
                    recoveryDiagnostics);

                AssertEx.True(retirement.RetirementConfirmed);
                AssertEx.Equal(RecordGeneration, retirement.RecordGeneration);
                AssertEx.True(retirement.HomingSucceeded);

                recoveryConnection.CloseConnection();
                server.Verify();

                var retireRequests = new List<byte[]>();
                foreach (var request in server.ReceivedRequests)
                {
                    if (TestFrame.ReadUInt16(request, 0)
                        == LMC_CommandId.RetireAxisDs402HomeExOutcome)
                    {
                        retireRequests.Add(request);
                    }
                }

                AssertEx.Equal(2, retireRequests.Count);
                AssertEx.Equal(
                    key.OriginalRequestId,
                    TestFrame.ReadUInt32(retireRequests[0], 28));
                AssertEx.Equal(
                    key.OriginalRequestId,
                    TestFrame.ReadUInt32(retireRequests[1], 28));
                AssertEx.Equal(
                    RecordGeneration,
                    TestFrame.ReadUInt32(retireRequests[0], 124));
                AssertEx.Equal(
                    RecordGeneration,
                    TestFrame.ReadUInt32(retireRequests[1], 124));
                AssertEx.Equal(
                    key.DiagnosticsBootId,
                    TestFrame.ReadUInt32(retireRequests[0], 20));
                AssertEx.Equal(
                    key.DiagnosticsBootId,
                    TestFrame.ReadUInt32(retireRequests[1], 20));
                AssertEx.Equal(
                    0,
                    CountCommand(server, LMC_CommandId.StartAxisDs402HomeEx));
            }
        }

        private static LMCAxisDs402HomeExOutcomeResult TerminalOutcome(
            LMCAxisDs402HomeExRecoveryKey key)
        {
            const uint queryRequestId = 0xA1B2C3D4u;
            var parsed = LMC_AdminParser.ParseAxisDs402HomeExOutcome(
                TestFrame.Response(
                    0,
                    OutcomePayload(queryRequestId, key)),
                queryRequestId,
                key);
            return new LMCAxisDs402HomeExOutcomeResult(
                parsed.Response,
                key,
                parsed.RecordState,
                parsed.OriginalCommandStatus,
                parsed.OriginalErrorId,
                parsed.OriginalDetailCode,
                parsed.Ds402StatusWord,
                parsed.ActualPosition,
                parsed.ExpectedFinalPosition,
                parsed.StartCycle,
                parsed.CompletionCycle,
                parsed.NativeCommandState,
                parsed.RecordGeneration,
                parsed.CleanupProofFlags,
                parsed.SdoExecutorToken);
        }

        private static LMCAxisDs402HomeExRecoveryKey RecoveryKey()
        {
            return LMCAxisDs402HomeExRecovery.Rehydrate(
                LMCAdmin.ProtocolSchemaVersion,
                0x11223344u,
                DiagnosticsBuild,
                DiagnosticsBootId,
                MapRevision,
                new LMCAxisDs402HomeExClientIntentId(
                    0x01020304u,
                    0x11121314u,
                    0x21222324u,
                    0x31323334u),
                1,
                1,
                -100,
                -4,
                250,
                500,
                25,
                0,
                0,
                LMCDs402HomeBufferMode.Aborting,
                60000,
                5000,
                new byte[LMCAxisDs402HomeExExecutionPlan.SpareLength]);
        }

        private static byte[] OutcomePayload(
            uint requestId,
            LMCAxisDs402HomeExRecoveryKey key)
        {
            var plan = key.ExecutionPlan;
            var expectedFinalPosition = checked(-plan.Position);
            var payload = CommonAdminPayload(requestId, 176);
            TestFrame.WriteUInt16(
                payload,
                16,
                (ushort)LMCAxisDs402HomeExOutcomeRecordState.Succeeded);
            TestFrame.WriteUInt32(payload, 20, key.DiagnosticsBuild);
            TestFrame.WriteUInt32(payload, 24, key.DiagnosticsBootId);
            TestFrame.WriteUInt32(payload, 28, key.MapRevision);
            TestFrame.WriteUInt32(payload, 32, key.OriginalRequestId);
            TestFrame.WriteUInt32(payload, 36, key.ClientIntentId.Word0);
            TestFrame.WriteUInt32(payload, 40, key.ClientIntentId.Word1);
            TestFrame.WriteUInt32(payload, 44, key.ClientIntentId.Word2);
            TestFrame.WriteUInt32(payload, 48, key.ClientIntentId.Word3);
            TestFrame.WriteUInt16(payload, 52, key.AxisReference);
            TestFrame.WriteInt32(payload, 56, plan.HomingMethod);
            TestFrame.WriteInt32(payload, 60, plan.Position);
            TestFrame.WriteInt32(payload, 64, plan.DetectionVelocityLimit);
            TestFrame.WriteInt32(payload, 68, plan.Acceleration);
            TestFrame.WriteInt32(payload, 72, plan.VelocityHigh);
            TestFrame.WriteInt32(payload, 76, plan.VelocityLow);
            TestFrame.WriteInt32(payload, 80, plan.DistanceLimit);
            TestFrame.WriteInt32(payload, 84, plan.TorqueLimit);
            TestFrame.WriteUInt16(payload, 88, (ushort)plan.BufferMode);
            TestFrame.WriteUInt32(payload, 92, plan.OverallTimeoutMilliseconds);
            TestFrame.WriteUInt32(payload, 96, plan.DetectionTimeoutMilliseconds);
            TestFrame.WriteUInt16(payload, 140, 0x1234);
            TestFrame.WriteInt32(payload, 144, expectedFinalPosition);
            TestFrame.WriteInt32(payload, 148, expectedFinalPosition);
            TestFrame.WriteUInt32(payload, 152, 10);
            TestFrame.WriteUInt32(payload, 156, 20);
            TestFrame.WriteUInt32(payload, 164, RecordGeneration);
            TestFrame.WriteUInt32(
                payload,
                168,
                (uint)LMCAxisDs402HomeExCleanupProofFlags
                    .RequiredForSafeTerminal);
            TestFrame.WriteUInt32(payload, 172, 0x1234u);
            return payload;
        }

        private static FakeRpcStep AdminCapabilitiesStep(uint requestId)
        {
            var payload = CommonAdminPayload(requestId, 40);
            TestFrame.WriteUInt32(
                payload,
                16,
                (uint)LMCAdminFeature.AxisDs402HomeEx);
            TestFrame.WriteUInt16(payload, 28, 4);
            TestFrame.WriteUInt16(payload, 36, 7);
            return new FakeRpcStep(
                LMC_CommandId.GetAdminCapabilities,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep DiagnosticsCapabilitiesStep(uint requestId)
        {
            var payload = new byte[68];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, requestId);
            TestFrame.WriteUInt32(payload, 16, DiagnosticsBuild);
            TestFrame.WriteUInt32(payload, 24, MapRevision);
            TestFrame.WriteUInt32(payload, 40, 1000);
            TestFrame.WriteUInt16(payload, 44, 1320);
            TestFrame.WriteUInt16(payload, 46, 2040);
            TestFrame.WriteUInt16(payload, 48, 1280);
            TestFrame.WriteUInt32(payload, 64, DiagnosticsBootId);
            return new FakeRpcStep(
                LMC_CommandId.GetDiagnosticsCapabilities,
                TestFrame.Response(0, payload));
        }

        private static byte[] CommonAdminPayload(uint requestId, int length)
        {
            var payload = new byte[length];
            TestFrame.WriteUInt16(payload, 0, LMCAdmin.ProtocolSchemaVersion);
            TestFrame.WriteUInt32(payload, 8, requestId);
            return payload;
        }

        private static FakeRpcStep AxisLookupStep(ushort axisReference)
        {
            var payload = new byte[6];
            TestFrame.WriteUInt16(payload, 4, axisReference);
            return new FakeRpcStep(
                LMC_CommandId.GetAxisByName,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep AxisInfoStep(ushort axisReference)
        {
            var payload = new byte[8];
            TestFrame.WriteUInt32(payload, 0, axisReference);
            return new FakeRpcStep(
                LMC_CommandId.AxisInfo,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep InitStep()
        {
            var payload = new byte[24];
            TestFrame.WriteUInt32(payload, 0, 64);
            return new FakeRpcStep(
                LMC_CommandId.RpcSessionInit,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep CallbackStep()
        {
            return new FakeRpcStep(
                LMC_CommandId.RpcCallbackRegistration,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")));
        }

        private static FakeRpcStep CloseStep()
        {
            return new FakeRpcStep(
                LMC_CommandId.CloseConnection,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")));
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

        private static int CountCommand(FakeRpcServer server, ushort command)
        {
            var count = 0;
            foreach (var request in server.ReceivedRequests)
            {
                if (TestFrame.ReadUInt16(request, 0) == command)
                {
                    count++;
                }
            }
            return count;
        }
    }
}

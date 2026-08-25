using System;
using System.Collections.Generic;
using System.Threading;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class AdminDs402HomeExLifecycleContractTests
    {
        private const uint DiagnosticsBuild = 0x55667788u;
        private const uint DiagnosticsBootId = 0x99AABBCCu;
        private const uint MapRevision = 0xDDEEFF00u;
        private const uint RecordGeneration = 7u;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Rpc.Admin.Ds402HomeEx.CapabilityOffStopsBeforeStart",
                CapabilityOffStopsBeforeStart);
            tests.Add(
                "Rpc.Admin.Ds402HomeEx.SyncLifecycleFacade",
                SyncLifecycleFacade);
            tests.Add(
                "Rpc.Admin.Ds402HomeEx.PreCanceledZeroStartReusable",
                PreCanceledZeroStartReusable);
            tests.Add(
                "Rpc.Admin.Ds402HomeEx.ResponseLossUncertainNoReplay",
                ResponseLossUncertainNoReplay);
            tests.Add(
                "Rpc.Admin.Ds402HomeEx.DefinitiveRejectNoReplay",
                DefinitiveRejectNoReplay);
            tests.Add(
                "Rpc.Admin.Ds402HomeEx.ReconnectReadOnlyRecoveryNoReplay",
                ReconnectReadOnlyRecoveryNoReplay);
        }

        private static void CapabilityOffStopsBeforeStart()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                AdminCapabilitiesStep(1, LMCAdminFeature.None),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var capabilities = connection.Admin.GetCapabilities();
                var token = LMCAxisDs402HomeExExecuteToken.Create();

                AssertEx.Throws<NotSupportedException>(() =>
                    axis.PrepareDs402HomeExApprovedPlan(
                        ExecutionPlan(),
                        capabilities,
                        null,
                        token));
                AssertEx.False(token.IsConsumed);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(
                    0,
                    CountCommand(server, LMC_CommandId.StartAxisDs402HomeEx));
            }
        }

        private static void SyncLifecycleFacade()
        {
            var start = StartAcceptedStep();
            var query = new FakeRpcStep(
                LMC_CommandId.ReadAxisDs402HomeExOutcome,
                null);
            var retire = new FakeRpcStep(
                LMC_CommandId.RetireAxisDs402HomeExOutcome,
                null);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                AdminCapabilitiesStep(1, LMCAdminFeature.AxisDs402HomeEx),
                DiagnosticsCapabilitiesStep(1),
                AdminCapabilitiesStep(3, LMCAdminFeature.AxisDs402HomeEx),
                DiagnosticsCapabilitiesStep(2),
                start,
                AdminCapabilitiesStep(4, LMCAdminFeature.AxisDs402HomeEx),
                DiagnosticsCapabilitiesStep(3),
                query,
                retire,
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var capabilities = connection.Admin.GetCapabilities();
                var diagnostics = connection.Diagnostics.GetCapabilities();
                var prepared = axis.PrepareDs402HomeExApprovedPlan(
                    ExecutionPlan(),
                    capabilities,
                    diagnostics,
                    LMCAxisDs402HomeExExecuteToken.Create());

                query.ResponseFactory = request => TestFrame.Response(
                    0,
                    OutcomePayload(
                        TestFrame.ReadUInt32(request, 12),
                        prepared.RecoveryKey));
                retire.ResponseFactory = request => TestFrame.Response(
                    0,
                    OutcomePayload(
                        TestFrame.ReadUInt32(request, 12),
                        prepared.RecoveryKey));

                var acknowledgement = axis.LMC_HomeDS402Ex(prepared);
                AssertEx.True(acknowledgement.IsAccepted);
                AssertEx.True(prepared.IsConsumed);
                AssertEx.Equal(
                    prepared.ExecutionPlan.HomingMethod,
                    acknowledgement.HomingMethod);

                // Start deliberately refreshes both capability observations.
                // Re-read them before the read-only outcome lifecycle so stale
                // capability snapshots can never authorize recovery traffic.
                capabilities = connection.Admin.GetCapabilities();
                diagnostics = connection.Diagnostics.GetCapabilities();

                var outcome = axis.ReadDs402HomeExOutcome(
                    prepared.RecoveryKey,
                    capabilities,
                    diagnostics);
                AssertEx.True(outcome.IsTerminal);
                AssertEx.True(outcome.HomingSucceeded);
                AssertEx.Equal(RecordGeneration, outcome.RecordGeneration);

                var retirement = axis.RetireDs402HomeExOutcome(
                    outcome,
                    capabilities,
                    diagnostics);
                AssertEx.True(retirement.RetirementConfirmed);
                AssertEx.True(retirement.HomingSucceeded);
                AssertEx.Equal(RecordGeneration, retirement.RecordGeneration);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(
                    1,
                    CountCommand(server, LMC_CommandId.StartAxisDs402HomeEx));
                AssertEx.Equal(
                    1,
                    CountCommand(
                        server,
                        LMC_CommandId.ReadAxisDs402HomeExOutcome));
                AssertEx.Equal(
                    1,
                    CountCommand(
                        server,
                        LMC_CommandId.RetireAxisDs402HomeExOutcome));
            }
        }

        private static void PreCanceledZeroStartReusable()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                AdminCapabilitiesStep(1, LMCAdminFeature.AxisDs402HomeEx),
                DiagnosticsCapabilitiesStep(1),
                AdminCapabilitiesStep(3, LMCAdminFeature.AxisDs402HomeEx),
                DiagnosticsCapabilitiesStep(2),
                StartAcceptedStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var capabilities = connection.Admin.GetCapabilities();
                var diagnostics = connection.Diagnostics.GetCapabilities();
                var prepared = axis.PrepareDs402HomeExApprovedPlan(
                    ExecutionPlan(),
                    capabilities,
                    diagnostics,
                    LMCAxisDs402HomeExExecuteToken.Create());
                cancellation.Cancel();

                AssertEx.Throws<OperationCanceledException>(() =>
                    axis.Ds402HomeExAsync(
                            prepared,
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.False(prepared.IsConsumed);
                AssertEx.Equal(
                    0,
                    CountCommand(server, LMC_CommandId.StartAxisDs402HomeEx));

                var acknowledgement = axis.Ds402HomeEx(prepared);
                AssertEx.True(acknowledgement.IsAccepted);
                AssertEx.True(prepared.IsConsumed);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(
                    1,
                    CountCommand(server, LMC_CommandId.StartAxisDs402HomeEx));
            }
        }

        private static void ResponseLossUncertainNoReplay()
        {
            var lostStart = new FakeRpcStep(
                LMC_CommandId.StartAxisDs402HomeEx,
                new byte[0])
            {
                CloseClientBeforeResponse = true
            };

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                AdminCapabilitiesStep(1, LMCAdminFeature.AxisDs402HomeEx),
                DiagnosticsCapabilitiesStep(1),
                AdminCapabilitiesStep(3, LMCAdminFeature.AxisDs402HomeEx),
                DiagnosticsCapabilitiesStep(2),
                lostStart))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var capabilities = connection.Admin.GetCapabilities();
                var diagnostics = connection.Diagnostics.GetCapabilities();
                var prepared = axis.PrepareDs402HomeExApprovedPlan(
                    ExecutionPlan(),
                    capabilities,
                    diagnostics,
                    LMCAxisDs402HomeExExecuteToken.Create());

                var error = AssertEx.Throws<
                    LMCAxisDs402HomeExOutcomeUncertainException>(() =>
                        axis.Ds402HomeEx(prepared));
                AssertEx.Equal(prepared, error.PreparedCommand);
                AssertEx.Equal(prepared.RecoveryKey, error.RecoveryKey);
                AssertEx.True(prepared.IsConsumed);
                AssertEx.NotNull(error.InnerException);
                AssertEx.Equal(LMCConnectionState.Faulted, connection.State);

                AssertEx.Throws<InvalidOperationException>(() =>
                    axis.Ds402HomeEx(prepared));
                server.Verify();
                AssertEx.Equal(
                    1,
                    CountCommand(server, LMC_CommandId.StartAxisDs402HomeEx));
            }
        }

        private static void DefinitiveRejectNoReplay()
        {
            var reject = new FakeRpcStep(
                LMC_CommandId.StartAxisDs402HomeEx,
                null)
            {
                ResponseFactory = request => TestFrame.Response(
                    0,
                    StartFailurePayload(
                        TestFrame.ReadUInt32(request, 12),
                        LMCAdminDetailCode.Ds402HomeExInvalidProfile))
            };

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                AdminCapabilitiesStep(1, LMCAdminFeature.AxisDs402HomeEx),
                DiagnosticsCapabilitiesStep(1),
                AdminCapabilitiesStep(3, LMCAdminFeature.AxisDs402HomeEx),
                DiagnosticsCapabilitiesStep(2),
                reject,
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var capabilities = connection.Admin.GetCapabilities();
                var diagnostics = connection.Diagnostics.GetCapabilities();
                var prepared = axis.PrepareDs402HomeExApprovedPlan(
                    ExecutionPlan(),
                    capabilities,
                    diagnostics,
                    LMCAxisDs402HomeExExecuteToken.Create());

                var error = AssertEx.Throws<
                    LMCAxisDs402HomeExRejectedException>(() =>
                        axis.Ds402HomeEx(prepared));
                AssertEx.Equal(
                    LMCAdminDetailCode.Ds402HomeExInvalidProfile,
                    error.Response.DetailCode);
                AssertEx.True(prepared.IsConsumed);
                AssertEx.Equal(LMCConnectionState.Connected, connection.State);

                AssertEx.Throws<InvalidOperationException>(() =>
                    axis.Ds402HomeEx(prepared));
                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(
                    1,
                    CountCommand(server, LMC_CommandId.StartAxisDs402HomeEx));
            }
        }

        private static void ReconnectReadOnlyRecoveryNoReplay()
        {
            var lostStart = new FakeRpcStep(
                LMC_CommandId.StartAxisDs402HomeEx,
                new byte[0])
            {
                CloseClientBeforeResponseAndContinue = true
            };
            var query = new FakeRpcStep(
                LMC_CommandId.ReadAxisDs402HomeExOutcome,
                null);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                AdminCapabilitiesStep(1, LMCAdminFeature.AxisDs402HomeEx),
                DiagnosticsCapabilitiesStep(1),
                AdminCapabilitiesStep(3, LMCAdminFeature.AxisDs402HomeEx),
                DiagnosticsCapabilitiesStep(2),
                lostStart,
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                AdminCapabilitiesStep(1, LMCAdminFeature.AxisDs402HomeEx),
                DiagnosticsCapabilitiesStep(1),
                query,
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
                var prepared = firstAxis.PrepareDs402HomeExApprovedPlan(
                    ExecutionPlan(),
                    firstCapabilities,
                    firstDiagnostics,
                    LMCAxisDs402HomeExExecuteToken.Create());

                AssertEx.Throws<LMCAxisDs402HomeExOutcomeUncertainException>(() =>
                    firstAxis.Ds402HomeEx(prepared));
                AssertEx.True(prepared.IsConsumed);

                var persistedKey = Rehydrate(prepared.RecoveryKey);
                query.ResponseFactory = request => TestFrame.Response(
                    0,
                    OutcomePayload(
                        TestFrame.ReadUInt32(request, 12),
                        persistedKey));

                Connect(recoveryConnection, server.Port);
                var recoveryAxis = new LMCSingleAxis(
                    recoveryConnection,
                    "_LMCAxis1");
                var recoveryCapabilities =
                    recoveryConnection.Admin.GetCapabilities();
                var recoveryDiagnostics =
                    recoveryConnection.Diagnostics.GetCapabilities();

                var outcome = recoveryAxis.ReadDs402HomeExOutcome(
                    persistedKey,
                    recoveryCapabilities,
                    recoveryDiagnostics);
                AssertEx.True(outcome.HomingSucceeded);
                AssertEx.Equal(RecordGeneration, outcome.RecordGeneration);

                recoveryConnection.CloseConnection();
                server.Verify();
                AssertEx.Equal(
                    1,
                    CountCommand(server, LMC_CommandId.StartAxisDs402HomeEx));
                AssertEx.Equal(
                    1,
                    CountCommand(
                        server,
                        LMC_CommandId.ReadAxisDs402HomeExOutcome));
            }
        }

        private static LMCAxisDs402HomeExRecoveryKey Rehydrate(
            LMCAxisDs402HomeExRecoveryKey key)
        {
            var plan = key.ExecutionPlan;
            return LMCAxisDs402HomeExRecovery.Rehydrate(
                key.SchemaVersion,
                key.OriginalRequestId,
                key.DiagnosticsBuild,
                key.DiagnosticsBootId,
                key.MapRevision,
                new LMCAxisDs402HomeExClientIntentId(
                    key.ClientIntentId.Word0,
                    key.ClientIntentId.Word1,
                    key.ClientIntentId.Word2,
                    key.ClientIntentId.Word3),
                key.AxisReference,
                plan.HomingMethod,
                plan.Position,
                plan.DetectionVelocityLimit,
                plan.Acceleration,
                plan.VelocityHigh,
                plan.VelocityLow,
                plan.DistanceLimit,
                plan.TorqueLimit,
                plan.BufferMode,
                plan.OverallTimeoutMilliseconds,
                plan.DetectionTimeoutMilliseconds,
                plan.Spare);
        }

        private static LMCAxisDs402HomeExExecutionPlan ExecutionPlan()
        {
            return new LMCAxisDs402HomeExExecutionPlan(
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

        private static FakeRpcStep StartAcceptedStep()
        {
            return new FakeRpcStep(
                LMC_CommandId.StartAxisDs402HomeEx,
                null)
            {
                InspectRequest = request =>
                {
                    AssertEx.Equal((ushort)116, TestFrame.ReadUInt16(request, 4));
                    AssertEx.Equal((ushort)1, TestFrame.ReadUInt16(request, 6));
                    AssertEx.Equal(0x58453448u, TestFrame.ReadUInt32(request, 120));
                },
                ResponseFactory = request => TestFrame.Response(
                    0,
                    StartSuccessPayload(TestFrame.ReadUInt32(request, 12)))
            };
        }

        private static byte[] StartSuccessPayload(uint requestId)
        {
            var payload = CommonAdminPayload(requestId, 24);
            TestFrame.WriteInt32(payload, 16, 1);
            return payload;
        }

        private static byte[] StartFailurePayload(
            uint requestId,
            LMCAdminDetailCode detailCode)
        {
            var payload = CommonAdminPayload(requestId, 24);
            TestFrame.WriteUInt16(payload, 4, 1);
            TestFrame.WriteInt16(payload, 6, -31000);
            TestFrame.WriteUInt32(payload, 12, (uint)detailCode);
            TestFrame.WriteInt32(payload, 16, 1);
            return payload;
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

        private static FakeRpcStep AdminCapabilitiesStep(
            uint requestId,
            LMCAdminFeature features)
        {
            return new FakeRpcStep(
                LMC_CommandId.GetAdminCapabilities,
                TestFrame.Response(
                    0,
                    AdminCapabilitiesPayload(requestId, features)));
        }

        private static byte[] AdminCapabilitiesPayload(
            uint requestId,
            LMCAdminFeature features)
        {
            var payload = CommonAdminPayload(requestId, 40);
            TestFrame.WriteUInt32(payload, 16, (uint)features);
            TestFrame.WriteUInt16(payload, 28, 4);
            TestFrame.WriteUInt16(
                payload,
                36,
                (ushort)(features == LMCAdminFeature.None ? 1 : 7));
            return payload;
        }

        private static FakeRpcStep DiagnosticsCapabilitiesStep(uint requestId)
        {
            return new FakeRpcStep(
                LMC_CommandId.GetDiagnosticsCapabilities,
                TestFrame.Response(
                    0,
                    DiagnosticsCapabilitiesPayload(requestId)));
        }

        private static byte[] DiagnosticsCapabilitiesPayload(uint requestId)
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
            return payload;
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

        private static int CountCommand(
            FakeRpcServer server,
            ushort command)
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

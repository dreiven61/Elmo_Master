using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class AdminSetAxisPositionOutcomeContractTests
    {
        private const uint OriginalRequestId = 0x11223344u;
        private const uint QueryRequestId = 0xA1B2C3D4u;
        private const uint DiagnosticsBuild = 0x55667788u;
        private const uint DiagnosticsBootId = 0x99AABBCCu;
        private const uint MapRevision = 0xDDEEFF00u;
        private const uint Intent0 = 0x01234567u;
        private const uint Intent1 = 0x89ABCDEFu;
        private const uint Intent2 = 0x10203040u;
        private const uint Intent3 = 0x50607080u;
        private const int TargetPosition = -12345;
        private const int ExpectedActualPosition = 6789;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Request.Admin.ReadAxisSetPositionOutcome.GoldenBytes",
                RequestGoldenBytes);
            tests.Add(
                "Contract.Admin.ReadAxisSetPositionOutcome.RecoveryKeySurface",
                RecoveryKeySurface);
            tests.Add(
                "Response.Admin.ReadAxisSetPositionOutcome.StrictTerminalSchema",
                StrictTerminalSchema);
            tests.Add(
                "Response.Admin.ReadAxisSetPositionOutcome.QueryFailureTyped",
                QueryFailureTyped);
            tests.Add(
                "Rpc.Admin.ReadAxisSetPositionOutcome.SyncRepeatableRead",
                SyncRepeatableRead);
            tests.Add(
                "Rpc.Admin.ReadAxisSetPositionOutcome.AsyncRejectedRecord",
                AsyncRejectedRecord);
            tests.Add(
                "Rpc.Admin.ReadAxisSetPositionOutcome.PreWireGuards",
                PreWireGuards);
            tests.Add(
                "Rpc.Admin.ReadAxisSetPositionOutcome.DomainFailureKeepsSession",
                DomainFailureKeepsSession);
            tests.Add(
                "Rpc.Admin.ReadAxisSetPositionOutcome.MalformedFaultsSyncSession",
                MalformedFaultsSyncSession);
            tests.Add(
                "Rpc.Admin.ReadAxisSetPositionOutcome.MalformedFaultsAsyncSession",
                MalformedFaultsAsyncSession);
        }

        private static void RequestGoldenBytes()
        {
            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "14 7D 00 00 30 00 02 00 "
                    + "01 00 00 00 D4 C3 B2 A1 "
                    + "88 77 66 55 CC BB AA 99 "
                    + "00 FF EE DD 44 33 22 11 "
                    + "67 45 23 01 EF CD AB 89 "
                    + "40 30 20 10 80 70 60 50 "
                    + "C7 CF FF FF 85 1A 00 00"),
                LMC_AdminFrame.ReadAxisSetPositionOutcome(
                    QueryRequestId,
                    RecoveryKey()));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_AdminFrame.ReadAxisSetPositionOutcome(
                    0,
                    RecoveryKey()));
            AssertEx.Throws<ArgumentNullException>(
                () => LMC_AdminFrame.ReadAxisSetPositionOutcome(
                    QueryRequestId,
                    null));
        }

        private static void RecoveryKeySurface()
        {
            var key = RecoveryKey();
            var restored = new LMCAxisSetPositionRecoveryKey(
                key.SchemaVersion,
                key.OriginalRequestId,
                key.DiagnosticsBuild,
                key.DiagnosticsBootId,
                key.MapRevision,
                key.ClientIntentId0,
                key.ClientIntentId1,
                key.ClientIntentId2,
                key.ClientIntentId3,
                key.AxisReference,
                key.TargetPosition,
                key.ExpectedActualPosition,
                key.SemanticMode);

            AssertEx.True(key.Equals(restored));
            AssertEx.Equal(key.GetHashCode(), restored.GetHashCode());
            AssertReadOnly(typeof(LMCAxisSetPositionRecoveryKey), "OriginalRequestId");
            AssertReadOnly(typeof(LMCAxisSetPositionRecoveryKey), "DiagnosticsBootId");
            AssertReadOnly(typeof(LMCAxisSetPositionRecoveryKey), "ClientIntentId0");
            AssertReadOnly(typeof(LMCAxisSetPositionOutcomeResult), "RecordState");
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => RecoveryKey(diagnosticsBootId: 0));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => RecoveryKey(mapRevision: 0));
        }

        private static void StrictTerminalSchema()
        {
            var key = RecoveryKey();
            var success = LMC_AdminParser.ParseAxisSetPositionOutcome(
                TestFrame.Response(
                    0,
                    TerminalPayload(
                        QueryRequestId,
                        key,
                        LMCAxisSetPositionOutcomeRecordState.Succeeded,
                        key.TargetPosition,
                        0,
                        0,
                        LMCAdminDetailCode.None,
                        0,
                        7)),
                QueryRequestId,
                key);
            AssertEx.Equal(
                LMCAxisSetPositionOutcomeRecordState.Succeeded,
                success.RecordState);
            AssertEx.Equal(key.TargetPosition, success.AppliedPosition);
            AssertEx.Equal(7u, success.RecordGeneration);

            var rejected = LMC_AdminParser.ParseAxisSetPositionOutcome(
                TestFrame.Response(
                    0,
                    TerminalPayload(
                        QueryRequestId,
                        key,
                        LMCAxisSetPositionOutcomeRecordState.Rejected,
                        0,
                        1,
                        -31000,
                        LMCAdminDetailCode.CoordinatePreconditionFailed,
                        0,
                        8)),
                QueryRequestId,
                key);
            AssertEx.Equal(
                LMCAdminDetailCode.CoordinatePreconditionFailed,
                (LMCAdminDetailCode)rejected.OriginalDetailCode);

            var nativeRejected = LMC_AdminParser.ParseAxisSetPositionOutcome(
                TestFrame.Response(
                    0,
                    TerminalPayload(
                        QueryRequestId,
                        key,
                        LMCAxisSetPositionOutcomeRecordState.Rejected,
                        0,
                        1,
                        -6,
                        LMCAdminDetailCode.NativeCommandRejected,
                        0xA5000008u,
                        9)),
                QueryRequestId,
                key);
            AssertEx.Equal(0xA5000008u, nativeRejected.NativeCommandState);

            var echoMismatch = TerminalPayload(
                QueryRequestId,
                key,
                LMCAxisSetPositionOutcomeRecordState.Succeeded,
                key.TargetPosition,
                0,
                0,
                LMCAdminDetailCode.None,
                0,
                1);
            TestFrame.WriteUInt32(echoMismatch, 36, key.ClientIntentId0 + 1);
            AssertMalformed(echoMismatch, key);

            foreach (var queryOnlyDetail in new[]
            {
                LMCAdminDetailCode.SetPositionOutcomeNotFound,
                LMCAdminDetailCode.SetPositionOutcomeIndeterminate,
                LMCAdminDetailCode.SetPositionOutcomeStoreCorrupt,
                LMCAdminDetailCode.SetPositionOutcomeKeyMismatch
            })
            {
                AssertMalformed(
                    TerminalPayload(
                        QueryRequestId,
                        key,
                        LMCAxisSetPositionOutcomeRecordState.Rejected,
                        0,
                        1,
                        -31000,
                        queryOnlyDetail,
                        0,
                        1),
                    key);
            }

            AssertMalformed(
                TerminalPayload(
                    QueryRequestId,
                    key,
                    (LMCAxisSetPositionOutcomeRecordState)1,
                    0,
                    1,
                    -31000,
                    LMCAdminDetailCode.InvalidState,
                    0,
                    1),
                key);
            AssertMalformed(
                TerminalPayload(
                    QueryRequestId,
                    key,
                    LMCAxisSetPositionOutcomeRecordState.Succeeded,
                    key.TargetPosition,
                    0,
                    0,
                    LMCAdminDetailCode.None,
                    0,
                    0),
                key);
        }

        private static void QueryFailureTyped()
        {
            var key = RecoveryKey();
            var exception = AssertEx.Throws<
                LMCAxisSetPositionOutcomeQueryException>(
                () => LMC_AdminParser.ParseAxisSetPositionOutcome(
                    TestFrame.Response(
                        0,
                        QueryFailurePayload(
                            QueryRequestId,
                            LMCAdminDetailCode.SetPositionOutcomeNotFound)),
                    QueryRequestId,
                    key));
            AssertEx.Equal(key, exception.RecoveryKey);
            AssertEx.Equal(QueryRequestId, exception.QueryRequestId);
            AssertEx.Contains("remains unresolved", exception.Message);

            var extendedFailure = QueryFailurePayload(
                QueryRequestId,
                LMCAdminDetailCode.SetPositionOutcomeNotFound,
                20);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseAxisSetPositionOutcome(
                    TestFrame.Response(0, extendedFailure),
                    QueryRequestId,
                    key));
        }

        private static void SyncRepeatableRead()
        {
            var key = RecoveryKey(axisReference: 1);
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                AdminCapabilitiesStep(
                    1,
                    LMCAdminFeature.AxisSetPositionOutcomeRead),
                OutcomeStep(
                    2,
                    key,
                    LMCAxisSetPositionOutcomeRecordState.Succeeded,
                    key.TargetPosition),
                OutcomeStep(
                    3,
                    key,
                    LMCAxisSetPositionOutcomeRecordState.Succeeded,
                    key.TargetPosition),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var adminCapabilities = connection.Admin.GetCapabilities();
                var diagnosticCapabilities =
                    CurrentDiagnosticsCapabilities(connection);

                var first = axis.ReadSetPositionOutcome(
                    key,
                    adminCapabilities,
                    diagnosticCapabilities);
                var second = axis.ReadSetPositionOutcome(
                    key,
                    adminCapabilities,
                    diagnosticCapabilities);
                AssertEx.True(first.OriginalCommandSucceeded);
                AssertEx.True(second.OriginalCommandSucceeded);
                AssertEx.Equal(2u, first.QueryRequestId);
                AssertEx.Equal(3u, second.QueryRequestId);
                AssertEx.Equal(key, first.RecoveryKey);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(2, CountCommand(server, 0x7D14));
                AssertEx.Equal(0, CountCommand(server, 0x7D12));
            }
        }

        private static void AsyncRejectedRecord()
        {
            var key = RecoveryKey(axisReference: 1);
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                AdminCapabilitiesStep(
                    1,
                    LMCAdminFeature.AxisSetPositionOutcomeRead),
                OutcomeStep(
                    2,
                    key,
                    LMCAxisSetPositionOutcomeRecordState.Rejected,
                    0,
                    LMCAdminDetailCode.CoordinatePreconditionFailed),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var adminCapabilities = connection.Admin.GetCapabilities();
                var diagnosticCapabilities =
                    CurrentDiagnosticsCapabilities(connection);
                var result = axis.ReadSetPositionOutcomeAsync(
                        key,
                        adminCapabilities,
                        diagnosticCapabilities,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(
                    LMCAxisSetPositionOutcomeRecordState.Rejected,
                    result.RecordState);
                AssertEx.False(result.OriginalCommandSucceeded);
                AssertEx.Equal(
                    LMCAdminDetailCode.CoordinatePreconditionFailed,
                    result.OriginalDetailCode);
                AssertEx.Equal(
                    LMCConnectionState.Connected,
                    connection.State);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void PreWireGuards()
        {
            CapabilityOffIsZeroWire();
            IdentityMismatchIsZeroWire();
            StaleAdminObservationIsZeroWire();
            StaleDiagnosticsObservationIsZeroWire();
        }

        private static void CapabilityOffIsZeroWire()
        {
            var key = RecoveryKey(axisReference: 1);
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                AdminCapabilitiesStep(1, LMCAdminFeature.AxisParameterRead),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var adminCapabilities = connection.Admin.GetCapabilities();
                var diagnosticCapabilities =
                    CurrentDiagnosticsCapabilities(connection);
                AssertEx.Throws<NotSupportedException>(
                    () => axis.ReadSetPositionOutcome(
                        key,
                        adminCapabilities,
                        diagnosticCapabilities));
                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(0, CountCommand(server, 0x7D14));
            }
        }

        private static void IdentityMismatchIsZeroWire()
        {
            var key = RecoveryKey(
                axisReference: 1,
                diagnosticsBootId: DiagnosticsBootId + 1);
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                AdminCapabilitiesStep(
                    1,
                    LMCAdminFeature.AxisSetPositionOutcomeRead),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var adminCapabilities = connection.Admin.GetCapabilities();
                var diagnosticCapabilities =
                    CurrentDiagnosticsCapabilities(connection);
                AssertEx.Throws<InvalidOperationException>(
                    () => axis.ReadSetPositionOutcome(
                        key,
                        adminCapabilities,
                        diagnosticCapabilities));
                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(0, CountCommand(server, 0x7D14));
            }
        }

        private static void StaleDiagnosticsObservationIsZeroWire()
        {
            var key = RecoveryKey(axisReference: 1);
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                AdminCapabilitiesStep(
                    1,
                    LMCAdminFeature.AxisSetPositionOutcomeRead),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var adminCapabilities = connection.Admin.GetCapabilities();
                var stale = CurrentDiagnosticsCapabilities(connection);
                CurrentDiagnosticsCapabilities(connection);
                AssertEx.Throws<InvalidOperationException>(
                    () => axis.ReadSetPositionOutcome(
                        key,
                        adminCapabilities,
                        stale));
                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(0, CountCommand(server, 0x7D14));
            }
        }

        private static void StaleAdminObservationIsZeroWire()
        {
            var key = RecoveryKey(axisReference: 1);
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                AdminCapabilitiesStep(
                    1,
                    LMCAdminFeature.AxisSetPositionOutcomeRead),
                AdminCapabilitiesStep(
                    2,
                    LMCAdminFeature.AxisSetPositionOutcomeRead),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var stale = connection.Admin.GetCapabilities();
                connection.Admin.GetCapabilities();
                var diagnosticCapabilities =
                    CurrentDiagnosticsCapabilities(connection);
                AssertEx.Throws<InvalidOperationException>(
                    () => axis.ReadSetPositionOutcome(
                        key,
                        stale,
                        diagnosticCapabilities));
                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(0, CountCommand(server, 0x7D14));
            }
        }

        private static void DomainFailureKeepsSession()
        {
            var key = RecoveryKey(axisReference: 1);
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                AdminCapabilitiesStep(
                    1,
                    LMCAdminFeature.AxisSetPositionOutcomeRead),
                new FakeRpcStep(
                    0x7D14,
                    TestFrame.Response(
                        0,
                        QueryFailurePayload(
                            2,
                            LMCAdminDetailCode.SetPositionOutcomeNotFound))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var adminCapabilities = connection.Admin.GetCapabilities();
                var diagnosticCapabilities =
                    CurrentDiagnosticsCapabilities(connection);
                var exception = AssertEx.Throws<
                    LMCAxisSetPositionOutcomeQueryException>(
                    () => axis.ReadSetPositionOutcome(
                        key,
                        adminCapabilities,
                        diagnosticCapabilities));
                AssertEx.Equal(key, exception.RecoveryKey);
                AssertEx.Equal(
                    LMCConnectionState.Connected,
                    connection.State);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void MalformedFaultsSyncSession()
        {
            var key = RecoveryKey(axisReference: 1);
            var malformed = TerminalPayload(
                2,
                key,
                LMCAxisSetPositionOutcomeRecordState.Succeeded,
                key.TargetPosition,
                0,
                0,
                LMCAdminDetailCode.None,
                0,
                1);
            TestFrame.WriteUInt32(malformed, 36, key.ClientIntentId0 + 1);
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                AdminCapabilitiesStep(
                    1,
                    LMCAdminFeature.AxisSetPositionOutcomeRead),
                new FakeRpcStep(0x7D14, TestFrame.Response(0, malformed)),
                ExpectedClientDisconnectStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var adminCapabilities = connection.Admin.GetCapabilities();
                var diagnosticCapabilities =
                    CurrentDiagnosticsCapabilities(connection);
                AssertEx.Throws<InvalidDataException>(
                    () => axis.ReadSetPositionOutcome(
                        key,
                        adminCapabilities,
                        diagnosticCapabilities));
                AssertEx.Equal(
                    LMCConnectionState.Faulted,
                    connection.State);
                server.Verify();
            }
        }

        private static void MalformedFaultsAsyncSession()
        {
            var key = RecoveryKey(axisReference: 1);
            var malformed = TerminalPayload(
                2,
                key,
                LMCAxisSetPositionOutcomeRecordState.Rejected,
                0,
                1,
                -31000,
                LMCAdminDetailCode.SetPositionOutcomeIndeterminate,
                0,
                1);
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                AdminCapabilitiesStep(
                    1,
                    LMCAdminFeature.AxisSetPositionOutcomeRead),
                new FakeRpcStep(0x7D14, TestFrame.Response(0, malformed)),
                ExpectedClientDisconnectStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var adminCapabilities = connection.Admin.GetCapabilities();
                var diagnosticCapabilities =
                    CurrentDiagnosticsCapabilities(connection);
                AssertEx.Throws<InvalidDataException>(
                    () => axis.ReadSetPositionOutcomeAsync(
                            key,
                            adminCapabilities,
                            diagnosticCapabilities,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(
                    LMCConnectionState.Faulted,
                    connection.State);
                server.Verify();
            }
        }

        private static void AssertMalformed(
            byte[] payload,
            LMCAxisSetPositionRecoveryKey key)
        {
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseAxisSetPositionOutcome(
                    TestFrame.Response(0, payload),
                    QueryRequestId,
                    key));
        }

        private static LMCAxisSetPositionRecoveryKey RecoveryKey(
            ushort axisReference = 2,
            uint diagnosticsBuild = DiagnosticsBuild,
            uint diagnosticsBootId = DiagnosticsBootId,
            uint mapRevision = MapRevision)
        {
            return new LMCAxisSetPositionRecoveryKey(
                LMCAdmin.ProtocolSchemaVersion,
                OriginalRequestId,
                diagnosticsBuild,
                diagnosticsBootId,
                mapRevision,
                Intent0,
                Intent1,
                Intent2,
                Intent3,
                axisReference,
                TargetPosition,
                ExpectedActualPosition,
                LMCAxisSetPositionSemanticMode
                    .ActualAndDestinationApplicationUnits);
        }

        private static byte[] TerminalPayload(
            uint queryRequestId,
            LMCAxisSetPositionRecoveryKey key,
            LMCAxisSetPositionOutcomeRecordState state,
            int appliedPosition,
            ushort originalStatus,
            short originalErrorId,
            LMCAdminDetailCode originalDetail,
            uint nativeCommandState,
            uint recordGeneration)
        {
            var payload = CommonPayload(queryRequestId, 84);
            TestFrame.WriteUInt16(payload, 16, (ushort)state);
            TestFrame.WriteUInt16(
                payload,
                18,
                (ushort)key.SemanticMode);
            TestFrame.WriteUInt32(payload, 20, key.DiagnosticsBuild);
            TestFrame.WriteUInt32(payload, 24, key.DiagnosticsBootId);
            TestFrame.WriteUInt32(payload, 28, key.MapRevision);
            TestFrame.WriteUInt32(payload, 32, key.OriginalRequestId);
            TestFrame.WriteUInt32(payload, 36, key.ClientIntentId0);
            TestFrame.WriteUInt32(payload, 40, key.ClientIntentId1);
            TestFrame.WriteUInt32(payload, 44, key.ClientIntentId2);
            TestFrame.WriteUInt32(payload, 48, key.ClientIntentId3);
            TestFrame.WriteUInt16(payload, 52, key.AxisReference);
            TestFrame.WriteInt32(payload, 56, key.TargetPosition);
            TestFrame.WriteInt32(payload, 60, key.ExpectedActualPosition);
            TestFrame.WriteInt32(payload, 64, appliedPosition);
            TestFrame.WriteUInt16(payload, 68, originalStatus);
            TestFrame.WriteInt16(payload, 70, originalErrorId);
            TestFrame.WriteUInt32(payload, 72, (uint)originalDetail);
            TestFrame.WriteUInt32(payload, 76, nativeCommandState);
            TestFrame.WriteUInt32(payload, 80, recordGeneration);
            return payload;
        }

        private static byte[] QueryFailurePayload(
            uint queryRequestId,
            LMCAdminDetailCode detail,
            int length = 16)
        {
            var payload = CommonPayload(queryRequestId, length);
            TestFrame.WriteUInt16(payload, 4, 1);
            TestFrame.WriteInt16(payload, 6, -31000);
            TestFrame.WriteUInt32(payload, 12, (uint)detail);
            return payload;
        }

        private static byte[] CommonPayload(uint requestId, int length)
        {
            var payload = new byte[length];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, requestId);
            return payload;
        }

        private static FakeRpcStep AdminCapabilitiesStep(
            uint requestId,
            LMCAdminFeature features)
        {
            var payload = CommonPayload(requestId, 40);
            TestFrame.WriteUInt32(payload, 16, (uint)features);
            TestFrame.WriteUInt32(payload, 20, 0x3Fu);
            TestFrame.WriteUInt32(
                payload,
                24,
                (uint)LMCGroupParameterSelection.All);
            TestFrame.WriteUInt16(payload, 28, 4);
            TestFrame.WriteUInt16(payload, 30, 1);
            TestFrame.WriteUInt16(payload, 32, 0x0100);
            TestFrame.WriteUInt16(payload, 34, 3);
            TestFrame.WriteUInt16(
                payload,
                36,
                (ushort)((features
                        & (LMCAdminFeature.AxisSetPosition
                            | LMCAdminFeature.AxisSetPositionOutcomeRead)) != 0
                    ? 2
                    : 1));
            return new FakeRpcStep(
                0x7D00,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep OutcomeStep(
            uint queryRequestId,
            LMCAxisSetPositionRecoveryKey key,
            LMCAxisSetPositionOutcomeRecordState state,
            int appliedPosition,
            LMCAdminDetailCode detail = LMCAdminDetailCode.None)
        {
            return new FakeRpcStep(
                0x7D14,
                TestFrame.Response(
                    0,
                    TerminalPayload(
                        queryRequestId,
                        key,
                        state,
                        appliedPosition,
                        state
                            == LMCAxisSetPositionOutcomeRecordState.Succeeded
                            ? (ushort)0
                            : (ushort)1,
                        state
                            == LMCAxisSetPositionOutcomeRecordState.Succeeded
                            ? (short)0
                            : (short)-31000,
                        detail,
                        0,
                        1)));
        }

        private static LMCDiagnosticCapabilities
            CurrentDiagnosticsCapabilities(LMCConnection connection)
        {
            const uint requestId = 0x0A0B0C0Du;
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
            var parsed = LMC_DiagnosticsParser.ParseCapabilities(
                TestFrame.Response(0, payload),
                requestId,
                connection.SessionGeneration);
            var nextObservation = typeof(LMCDiagnostics).GetMethod(
                "NextCapabilityObservationSequence",
                BindingFlags.Instance | BindingFlags.NonPublic);
            AssertEx.NotNull(nextObservation);
            var sequence = (long)nextObservation.Invoke(
                connection.Diagnostics,
                null);
            return parsed.BindProvenance(
                connection.Diagnostics,
                connection.SessionGeneration,
                sequence);
        }

        private static void AssertReadOnly(Type type, string propertyName)
        {
            var property = type.GetProperty(propertyName);
            AssertEx.NotNull(property);
            AssertEx.True(
                property.SetMethod == null || !property.SetMethod.IsPublic,
                type.Name + "." + propertyName + " must be immutable.");
        }

        private static FakeRpcStep AxisLookupStep(ushort axisReference)
        {
            var payload = new byte[6];
            TestFrame.WriteUInt16(payload, 4, axisReference);
            return new FakeRpcStep(
                0x103C,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep AxisInfoStep(ushort axisReference)
        {
            var payload = new byte[8];
            TestFrame.WriteUInt32(payload, 0, axisReference);
            return new FakeRpcStep(
                0x202B,
                TestFrame.Response(0, payload));
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
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")));
        }

        private static FakeRpcStep CloseStep()
        {
            return new FakeRpcStep(
                0x405D,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")));
        }

        private static FakeRpcStep ExpectedClientDisconnectStep()
        {
            return new FakeRpcStep(0, new byte[0])
            {
                RequireClientDisconnectBeforeRequest = true
            };
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

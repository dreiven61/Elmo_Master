using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class AdminSetAxisPositionOutcomeRetirementContractTests
    {
        private const uint OriginalRequestId = 0x11223344u;
        private const uint RetireRequestId = 0xA1B2C3D4u;
        private const uint DiagnosticsBuild = 0x55667788u;
        private const uint OriginalDiagnosticsBootId = 0x99AABBCCu;
        private const uint CurrentDiagnosticsBootId = 0xAABBCCDDu;
        private const uint MapRevision = 0xDDEEFF00u;
        private const uint Intent0 = 0x01234567u;
        private const uint Intent1 = 0x89ABCDEFu;
        private const uint Intent2 = 0x10203040u;
        private const uint Intent3 = 0x50607080u;
        private const int TargetPosition = -12345;
        private const int ExpectedActualPosition = 6789;
        private const uint RecordGeneration = 0xA1B2C3D4u;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Contract.Admin.SetPositionOutcomeRetirement.PublicSurface",
                PublicSurface);
            tests.Add(
                "Request.Admin.SetPositionOutcomeRetirement.GoldenBytes",
                RequestGoldenBytes);
            tests.Add(
                "Response.Admin.SetPositionOutcomeRetirement.StrictTerminalSnapshots",
                StrictTerminalSnapshots);
            tests.Add(
                "Response.Admin.SetPositionOutcomeRetirement.TypedCommonFailure",
                TypedCommonFailure);
            tests.Add(
                "Rpc.Admin.SetPositionOutcomeRetirement.CapabilityOffZeroWire",
                CapabilityOffIsZeroWire);
            tests.Add(
                "Rpc.Admin.SetPositionOutcomeRetirement.BuildMapAndGenerationZeroWire",
                BuildMapAndGenerationAreZeroWire);
            tests.Add(
                "Rpc.Admin.SetPositionOutcomeRetirement.StaleObservationsZeroWire",
                StaleObservationsAreZeroWire);
            tests.Add(
                "Rpc.Admin.SetPositionOutcomeRetirement.SyncAsyncExactRetry",
                SyncAsyncExactRetry);
            tests.Add(
                "Rpc.Admin.SetPositionOutcomeRetirement.DomainFailureKeepsSession",
                DomainFailureKeepsSession);
            tests.Add(
                "Rpc.Admin.SetPositionOutcomeRetirement.MalformedFailureFaultsSession",
                MalformedFailureFaultsSession);
            tests.Add(
                "Rpc.Admin.SetPositionOutcomeRetirement.InapplicableDetailFaultsSession",
                InapplicableDetailFaultsSession);
            tests.Add(
                "Rpc.Admin.SetPositionOutcomeRetirement.MalformedGenerationFaultsSession",
                MalformedGenerationFaultsSession);
            tests.Add(
                "Rpc.Admin.SetPositionOutcomeRetirement.ResponseLossReconnectRetry",
                ResponseLossReconnectRetry);
        }

        private static void PublicSurface()
        {
            AssertEx.Equal(
                (ushort)0x7D1A,
                (ushort)LMC_CommandId.RetireAxisSetPositionOutcome);
            AssertEx.Equal(
                1u << 7,
                (uint)LMCAdminFeature.AxisSetPositionOutcomeRetirement);
            AssertEx.Equal(
                2,
                CountPublicMethods(
                    typeof(LMCAdmin),
                    "RetireAxisSetPositionOutcome"));
            AssertEx.Equal(
                2,
                CountPublicMethods(
                    typeof(LMCAdmin),
                    "RetireAxisSetPositionOutcomeAsync"));
            AssertEx.Equal(
                2,
                CountPublicMethods(
                    typeof(LMCSingleAxis),
                    "RetireSetPositionOutcome"));
            AssertEx.Equal(
                2,
                CountPublicMethods(
                    typeof(LMCSingleAxis),
                    "RetireSetPositionOutcomeAsync"));

            AssertReadOnly(
                typeof(LMCAxisSetPositionOutcomeRetirementResult),
                "RecoveryKey");
            AssertReadOnly(
                typeof(LMCAxisSetPositionOutcomeRetirementResult),
                "RecordGeneration");
            AssertReadOnly(
                typeof(LMCAxisSetPositionOutcomeRetirementException),
                "RecoveryKey");
            AssertReadOnly(
                typeof(LMCAxisSetPositionOutcomeRetirementException),
                "RecordGeneration");
        }

        private static void RequestGoldenBytes()
        {
            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "1A 7D 00 00 38 00 02 00 "
                    + "01 00 00 00 D4 C3 B2 A1 "
                    + "88 77 66 55 CC BB AA 99 "
                    + "00 FF EE DD DD CC BB AA "
                    + "44 33 22 11 67 45 23 01 "
                    + "EF CD AB 89 40 30 20 10 "
                    + "80 70 60 50 C7 CF FF FF "
                    + "85 1A 00 00 D4 C3 B2 A1"),
                LMC_AdminFrame.RetireAxisSetPositionOutcome(
                    RetireRequestId,
                    CurrentDiagnosticsBootId,
                    RecoveryKey(),
                    RecordGeneration));
            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "1A 7D 00 00 38 00 02 00 "
                    + "01 00 00 00 D4 C3 B2 A1 "
                    + "88 77 66 55 DD CC BB AA "
                    + "00 FF EE DD DD CC BB AA "
                    + "44 33 22 11 67 45 23 01 "
                    + "EF CD AB 89 40 30 20 10 "
                    + "80 70 60 50 C7 CF FF FF "
                    + "85 1A 00 00 D4 C3 B2 A1"),
                LMC_AdminFrame.RetireAxisSetPositionOutcome(
                    RetireRequestId,
                    CurrentDiagnosticsBootId,
                    RecoveryKey(
                        diagnosticsBootId: CurrentDiagnosticsBootId),
                    RecordGeneration));

            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_AdminFrame.RetireAxisSetPositionOutcome(
                    0,
                    CurrentDiagnosticsBootId,
                    RecoveryKey(),
                    RecordGeneration));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_AdminFrame.RetireAxisSetPositionOutcome(
                    RetireRequestId,
                    CurrentDiagnosticsBootId,
                    RecoveryKey(),
                    0));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_AdminFrame.RetireAxisSetPositionOutcome(
                    RetireRequestId,
                    0,
                    RecoveryKey(),
                    RecordGeneration));
            AssertEx.Throws<ArgumentNullException>(
                () => LMC_AdminFrame.RetireAxisSetPositionOutcome(
                    RetireRequestId,
                    CurrentDiagnosticsBootId,
                    null,
                    RecordGeneration));
        }

        private static void StrictTerminalSnapshots()
        {
            var key = RecoveryKey();
            var succeeded = LMC_AdminParser
                .ParseAxisSetPositionOutcomeRetirement(
                    TestFrame.Response(
                        0,
                        TerminalPayload(
                            7,
                            key,
                            LMCAxisSetPositionOutcomeRecordState.Succeeded,
                            key.TargetPosition,
                            0,
                            0,
                            LMCAdminDetailCode.None,
                            0,
                            RecordGeneration)),
                    7,
                    key,
                    RecordGeneration);
            var succeededResult = CreateRetirementResult(succeeded, key);
            AssertEx.True(succeededResult.RetirementConfirmed);
            AssertEx.True(succeededResult.OriginalCommandSucceeded);
            AssertEx.Equal(
                LMCAxisSetPositionOutcomeRecordState.Succeeded,
                succeededResult.RecordState);
            AssertEx.Equal(key.TargetPosition, succeededResult.AppliedPosition);
            AssertEx.Equal(RecordGeneration, succeededResult.RecordGeneration);

            var rejected = LMC_AdminParser
                .ParseAxisSetPositionOutcomeRetirement(
                    TestFrame.Response(
                        0,
                        TerminalPayload(
                            8,
                            key,
                            LMCAxisSetPositionOutcomeRecordState.Rejected,
                            0,
                            1,
                            -31000,
                            LMCAdminDetailCode.CoordinatePreconditionFailed,
                            0,
                            RecordGeneration)),
                    8,
                    key,
                    RecordGeneration);
            var rejectedResult = CreateRetirementResult(rejected, key);
            AssertEx.True(rejectedResult.RetirementConfirmed);
            AssertEx.False(rejectedResult.OriginalCommandSucceeded);
            AssertEx.Equal(
                LMCAxisSetPositionOutcomeRecordState.Rejected,
                rejectedResult.RecordState);
            AssertEx.Equal(
                LMCAdminDetailCode.CoordinatePreconditionFailed,
                rejectedResult.OriginalDetailCode);
            AssertEx.Equal(RecordGeneration, rejectedResult.RecordGeneration);

            var impossibleDetails = new[]
            {
                LMCAdminDetailCode.UnsupportedSchema,
                LMCAdminDetailCode.UnsupportedFlags,
                LMCAdminDetailCode.InvalidRequestId,
                LMCAdminDetailCode.InvalidReference,
                LMCAdminDetailCode.InvalidPayloadLength,
                LMCAdminDetailCode.UnsupportedParameter,
                LMCAdminDetailCode.MissingClient,
                LMCAdminDetailCode.InvalidSelection,
                LMCAdminDetailCode.InvalidMotionParameters,
                LMCAdminDetailCode.DiagnosticsBuildMismatch,
                LMCAdminDetailCode.BootIdMismatch,
                LMCAdminDetailCode.MapRevisionMismatch,
                LMCAdminDetailCode.SetPositionOutcomeSlotOccupied,
                LMCAdminDetailCode.SetPositionOutcomeStorageUnavailable
            };
            for (var index = 0; index < impossibleDetails.Length; index++)
            {
                var requestId = checked((uint)(40 + index));
                AssertRetirementMalformed(
                    TerminalPayload(
                        requestId,
                        key,
                        LMCAxisSetPositionOutcomeRecordState.Rejected,
                        0,
                        1,
                        -31000,
                        impossibleDetails[index],
                        0,
                        RecordGeneration),
                    requestId,
                    key,
                    RecordGeneration);
            }

            AssertRetirementMalformed(
                TerminalPayload(
                    9,
                    key,
                    LMCAxisSetPositionOutcomeRecordState.Succeeded,
                    key.TargetPosition,
                    0,
                    0,
                    LMCAdminDetailCode.None,
                    0,
                    RecordGeneration + 1),
                9,
                key,
                RecordGeneration);
            AssertRetirementMalformed(
                TerminalPayload(
                    10,
                    key,
                    LMCAxisSetPositionOutcomeRecordState.Succeeded,
                    0,
                    0,
                    0,
                    LMCAdminDetailCode.None,
                    0,
                    RecordGeneration),
                10,
                key,
                RecordGeneration);
            AssertRetirementMalformed(
                TerminalPayload(
                    11,
                    key,
                    LMCAxisSetPositionOutcomeRecordState.Rejected,
                    1,
                    1,
                    -31000,
                    LMCAdminDetailCode.CoordinatePreconditionFailed,
                    0,
                    RecordGeneration),
                11,
                key,
                RecordGeneration);

            var wrongIdentity = TerminalPayload(
                12,
                key,
                LMCAxisSetPositionOutcomeRecordState.Succeeded,
                key.TargetPosition,
                0,
                0,
                LMCAdminDetailCode.None,
                0,
                RecordGeneration);
            TestFrame.WriteUInt32(
                wrongIdentity,
                36,
                key.ClientIntentId0 + 1);
            AssertRetirementMalformed(
                wrongIdentity,
                12,
                key,
                RecordGeneration);
        }

        private static void TypedCommonFailure()
        {
            var key = RecoveryKey();
            var details = new[]
            {
                LMCAdminDetailCode.DiagnosticsBuildMismatch,
                LMCAdminDetailCode.BootIdMismatch,
                LMCAdminDetailCode.MapRevisionMismatch,
                LMCAdminDetailCode.SetPositionOutcomeNotFound,
                LMCAdminDetailCode.SetPositionOutcomeIndeterminate,
                LMCAdminDetailCode.SetPositionOutcomeStoreCorrupt,
                LMCAdminDetailCode.SetPositionOutcomeKeyMismatch,
                LMCAdminDetailCode.SetPositionOutcomeStorageUnavailable
            };

            for (var index = 0; index < details.Length; index++)
            {
                var requestId = checked((uint)(20 + index));
                var exception = AssertEx.Throws<
                    LMCAxisSetPositionOutcomeRetirementException>(
                    () => LMC_AdminParser
                        .ParseAxisSetPositionOutcomeRetirement(
                            TestFrame.Response(
                                0,
                                FailurePayload(requestId, details[index])),
                            requestId,
                            key,
                            RecordGeneration));
                AssertEx.Equal(details[index], exception.Response.DetailCode);
                AssertEx.Equal(key, exception.RecoveryKey);
                AssertEx.Equal(
                    RecordGeneration,
                    exception.RecordGeneration);
                AssertEx.Equal(requestId, exception.RetireRequestId);
                AssertEx.Contains("remains unresolved", exception.Message);
            }

            AssertRetirementMalformed(
                FailurePayload(
                    29,
                    LMCAdminDetailCode.SetPositionOutcomeSlotOccupied),
                29,
                key,
                RecordGeneration);

            AssertRetirementMalformed(
                FailurePayload(
                    30,
                    LMCAdminDetailCode.SetPositionOutcomeNotFound,
                    20),
                30,
                key,
                RecordGeneration);

            var wrongError = FailurePayload(
                31,
                LMCAdminDetailCode.SetPositionOutcomeNotFound);
            TestFrame.WriteInt16(wrongError, 6, -6);
            AssertRetirementMalformed(
                wrongError,
                31,
                key,
                RecordGeneration);
        }

        private static void CapabilityOffIsZeroWire()
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
                var admin = connection.Admin.GetCapabilities();
                var diagnostics =
                    CurrentDiagnosticsCapabilities(connection);

                AssertEx.Throws<NotSupportedException>(
                    () => axis.RetireSetPositionOutcome(
                        key,
                        RecordGeneration,
                        admin,
                        diagnostics));

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(0, CountCommand(server, 0x7D1A));
            }
        }

        private static void BuildMapAndGenerationAreZeroWire()
        {
            var currentKey = RecoveryKey(axisReference: 1);
            var buildMismatch = RecoveryKey(
                axisReference: 1,
                diagnosticsBuild: DiagnosticsBuild + 1);
            var mapMismatch = RecoveryKey(
                axisReference: 1,
                mapRevision: MapRevision + 1);
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                AdminCapabilitiesStep(1, RetirementFeatures()),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var admin = connection.Admin.GetCapabilities();
                var diagnostics =
                    CurrentDiagnosticsCapabilities(connection);

                AssertEx.Throws<InvalidOperationException>(
                    () => axis.RetireSetPositionOutcome(
                        buildMismatch,
                        RecordGeneration,
                        admin,
                        diagnostics));
                AssertEx.Throws<InvalidOperationException>(
                    () => axis.RetireSetPositionOutcome(
                        mapMismatch,
                        RecordGeneration,
                        admin,
                        diagnostics));
                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => axis.RetireSetPositionOutcome(
                        currentKey,
                        0,
                        admin,
                        diagnostics));

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(0, CountCommand(server, 0x7D1A));
            }
        }

        private static void StaleObservationsAreZeroWire()
        {
            var key = RecoveryKey(axisReference: 1);
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                AdminCapabilitiesStep(1, RetirementFeatures()),
                AdminCapabilitiesStep(2, RetirementFeatures()),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var staleAdmin = connection.Admin.GetCapabilities();
                var currentAdmin = connection.Admin.GetCapabilities();
                var staleDiagnostics =
                    CurrentDiagnosticsCapabilities(connection);
                var currentDiagnostics =
                    CurrentDiagnosticsCapabilities(connection);

                AssertEx.Throws<InvalidOperationException>(
                    () => axis.RetireSetPositionOutcome(
                        key,
                        RecordGeneration,
                        staleAdmin,
                        currentDiagnostics));
                AssertEx.Throws<InvalidOperationException>(
                    () => axis.RetireSetPositionOutcome(
                        key,
                        RecordGeneration,
                        currentAdmin,
                        staleDiagnostics));

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(0, CountCommand(server, 0x7D1A));
            }
        }

        private static void SyncAsyncExactRetry()
        {
            var key = RecoveryKey();
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(2),
                AxisInfoStep(2),
                AdminCapabilitiesStep(1, RetirementFeatures()),
                RetirementStep(2, key),
                RetirementStep(3, key),
                RetirementStep(4, key),
                RetirementStep(5, key),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis2");
                var admin = connection.Admin.GetCapabilities();
                var diagnostics =
                    CurrentDiagnosticsCapabilities(connection);
                var outcome = CreatePublicOutcome(
                    0x77777777u,
                    key,
                    LMCAxisSetPositionOutcomeRecordState.Succeeded,
                    key.TargetPosition,
                    LMCAdminDetailCode.None,
                    RecordGeneration);

                var first = axis.RetireSetPositionOutcome(
                    key,
                    RecordGeneration,
                    admin,
                    diagnostics);
                var second = connection.Admin
                    .RetireAxisSetPositionOutcome(
                        axis,
                        outcome,
                        admin,
                        diagnostics);
                var third = axis.RetireSetPositionOutcomeAsync(
                        key,
                        RecordGeneration,
                        admin,
                        diagnostics,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var fourth = connection.Admin
                    .RetireAxisSetPositionOutcomeAsync(
                        axis,
                        outcome,
                        admin,
                        diagnostics,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(2u, first.RetireRequestId);
                AssertEx.Equal(3u, second.RetireRequestId);
                AssertEx.Equal(4u, third.RetireRequestId);
                AssertEx.Equal(5u, fourth.RetireRequestId);
                AssertEx.True(first.RetirementConfirmed);
                AssertEx.True(second.RetirementConfirmed);
                AssertEx.True(third.RetirementConfirmed);
                AssertEx.True(fourth.RetirementConfirmed);
                AssertEx.Equal(key, fourth.RecoveryKey);
                AssertEx.Equal(
                    RecordGeneration,
                    fourth.RecordGeneration);

                connection.CloseConnection();
                server.Verify();
                AssertExactRetirementRequests(server, 4);
                AssertEx.Equal(0, CountCommand(server, 0x7D12));
                AssertEx.Equal(0, CountCommand(server, 0x7D14));
            }
        }

        private static void DomainFailureKeepsSession()
        {
            var key = RecoveryKey();
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(2),
                AxisInfoStep(2),
                AdminCapabilitiesStep(1, RetirementFeatures()),
                new FakeRpcStep(
                    0x7D1A,
                    TestFrame.Response(
                        0,
                        FailurePayload(
                            2,
                            LMCAdminDetailCode
                                .SetPositionOutcomeKeyMismatch))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis2");
                var admin = connection.Admin.GetCapabilities();
                var diagnostics =
                    CurrentDiagnosticsCapabilities(connection);

                var exception = AssertEx.Throws<
                    LMCAxisSetPositionOutcomeRetirementException>(
                    () => axis.RetireSetPositionOutcome(
                        key,
                        RecordGeneration,
                        admin,
                        diagnostics));
                AssertEx.Equal(
                    LMCAdminDetailCode.SetPositionOutcomeKeyMismatch,
                    exception.Response.DetailCode);
                AssertEx.Equal(key, exception.RecoveryKey);
                AssertEx.Equal(
                    RecordGeneration,
                    exception.RecordGeneration);
                AssertEx.Equal(
                    LMCConnectionState.Connected,
                    connection.State);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x7D1A));
            }
        }

        private static void MalformedGenerationFaultsSession()
        {
            var key = RecoveryKey();
            var malformed = TerminalPayload(
                2,
                key,
                LMCAxisSetPositionOutcomeRecordState.Succeeded,
                key.TargetPosition,
                0,
                0,
                LMCAdminDetailCode.None,
                0,
                RecordGeneration + 1);
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(2),
                AxisInfoStep(2),
                AdminCapabilitiesStep(1, RetirementFeatures()),
                new FakeRpcStep(
                    0x7D1A,
                    TestFrame.Response(0, malformed)),
                ExpectedClientDisconnectStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis2");
                var admin = connection.Admin.GetCapabilities();
                var diagnostics =
                    CurrentDiagnosticsCapabilities(connection);

                AssertEx.Throws<InvalidDataException>(
                    () => axis.RetireSetPositionOutcome(
                        key,
                        RecordGeneration,
                        admin,
                        diagnostics));
                AssertEx.Equal(
                    LMCConnectionState.Faulted,
                    connection.State);
                server.Verify();
            }
        }

        private static void MalformedFailureFaultsSession()
        {
            var key = RecoveryKey();
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(2),
                AxisInfoStep(2),
                AdminCapabilitiesStep(1, RetirementFeatures()),
                new FakeRpcStep(
                    0x7D1A,
                    TestFrame.Response(
                        0,
                        FailurePayload(
                            2,
                            LMCAdminDetailCode
                                .SetPositionOutcomeKeyMismatch,
                            20))),
                ExpectedClientDisconnectStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis2");
                var admin = connection.Admin.GetCapabilities();
                var diagnostics =
                    CurrentDiagnosticsCapabilities(connection);

                AssertEx.Throws<InvalidDataException>(
                    () => axis.RetireSetPositionOutcome(
                        key,
                        RecordGeneration,
                        admin,
                        diagnostics));
                AssertEx.Equal(
                    LMCConnectionState.Faulted,
                    connection.State);
                server.Verify();
            }
        }

        private static void InapplicableDetailFaultsSession()
        {
            var key = RecoveryKey();
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(2),
                AxisInfoStep(2),
                AdminCapabilitiesStep(1, RetirementFeatures()),
                new FakeRpcStep(
                    0x7D1A,
                    TestFrame.Response(
                        0,
                        FailurePayload(
                            2,
                            LMCAdminDetailCode.InvalidReference))),
                ExpectedClientDisconnectStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis2");
                var admin = connection.Admin.GetCapabilities();
                var diagnostics =
                    CurrentDiagnosticsCapabilities(connection);

                AssertEx.Throws<InvalidDataException>(
                    () => axis.RetireSetPositionOutcome(
                        key,
                        RecordGeneration,
                        admin,
                        diagnostics));
                AssertEx.Equal(
                    LMCConnectionState.Faulted,
                    connection.State);
                server.Verify();
            }
        }

        private static void ResponseLossReconnectRetry()
        {
            var key = RecoveryKey();
            var lostResponse = new FakeRpcStep(0x7D1A, new byte[0])
            {
                CloseClientBeforeResponseAndContinue = true,
                InspectRequest = request => AssertRetirementRequest(
                    request,
                    2,
                    key,
                    RecordGeneration)
            };
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(2),
                AxisInfoStep(2),
                AdminCapabilitiesStep(1, RetirementFeatures()),
                lostResponse,
                InitStep(),
                CallbackStep(),
                AxisLookupStep(2),
                AxisInfoStep(2),
                AdminCapabilitiesStep(3, RetirementFeatures()),
                RetirementStep(4, key),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var firstAxis = new LMCSingleAxis(
                    connection,
                    "_LMCAxis2");
                var firstAdmin = connection.Admin.GetCapabilities();
                var firstDiagnostics =
                    CurrentDiagnosticsCapabilities(connection);

                AssertEx.Throws<IOException>(
                    () => firstAxis.RetireSetPositionOutcome(
                        key,
                        RecordGeneration,
                        firstAdmin,
                        firstDiagnostics));
                AssertEx.Equal(
                    LMCConnectionState.Faulted,
                    connection.State);

                Connect(connection, server.Port);
                var retryAxis = new LMCSingleAxis(
                    connection,
                    "_LMCAxis2");
                var retryAdmin = connection.Admin.GetCapabilities();
                var retryDiagnostics =
                    CurrentDiagnosticsCapabilities(connection);
                var result = retryAxis.RetireSetPositionOutcome(
                    key,
                    RecordGeneration,
                    retryAdmin,
                    retryDiagnostics);
                AssertEx.True(result.RetirementConfirmed);
                AssertEx.Equal(4u, result.RetireRequestId);
                AssertEx.Equal(RecordGeneration, result.RecordGeneration);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(2, server.AcceptedClientCount);
                AssertExactRetirementRequests(server, 2);
            }
        }

        private static FakeRpcStep RetirementStep(
            uint retireRequestId,
            LMCAxisSetPositionRecoveryKey key,
            uint currentDiagnosticsBootId = CurrentDiagnosticsBootId)
        {
            var step = new FakeRpcStep(
                0x7D1A,
                TestFrame.Response(
                    0,
                    TerminalPayload(
                        retireRequestId,
                        key,
                        LMCAxisSetPositionOutcomeRecordState.Succeeded,
                        key.TargetPosition,
                        0,
                        0,
                        LMCAdminDetailCode.None,
                        0,
                        RecordGeneration)));
            step.InspectRequest = request => AssertRetirementRequest(
                request,
                retireRequestId,
                key,
                RecordGeneration,
                currentDiagnosticsBootId);
            return step;
        }

        private static void AssertRetirementRequest(
            byte[] request,
            uint retireRequestId,
            LMCAxisSetPositionRecoveryKey key,
            uint recordGeneration,
            uint currentDiagnosticsBootId = CurrentDiagnosticsBootId)
        {
            AssertEx.Equal(
                (ushort)0x7D1A,
                TestFrame.ReadUInt16(request, 0));
            AssertEx.Equal((ushort)56, TestFrame.ReadUInt16(request, 4));
            AssertEx.Equal(
                key.AxisReference,
                TestFrame.ReadUInt16(request, 6));
            AssertEx.Equal(
                retireRequestId,
                TestFrame.ReadUInt32(request, 12));
            AssertEx.Equal(
                key.DiagnosticsBuild,
                TestFrame.ReadUInt32(request, 16));
            AssertEx.Equal(
                key.DiagnosticsBootId,
                TestFrame.ReadUInt32(request, 20));
            AssertEx.Equal(
                key.MapRevision,
                TestFrame.ReadUInt32(request, 24));
            AssertEx.Equal(
                currentDiagnosticsBootId,
                TestFrame.ReadUInt32(request, 28));
            AssertEx.Equal(
                key.OriginalRequestId,
                TestFrame.ReadUInt32(request, 32));
            AssertEx.Equal(
                key.ClientIntentId0,
                TestFrame.ReadUInt32(request, 36));
            AssertEx.Equal(
                key.ClientIntentId1,
                TestFrame.ReadUInt32(request, 40));
            AssertEx.Equal(
                key.ClientIntentId2,
                TestFrame.ReadUInt32(request, 44));
            AssertEx.Equal(
                key.ClientIntentId3,
                TestFrame.ReadUInt32(request, 48));
            AssertEx.Equal(
                key.TargetPosition,
                TestFrame.ReadInt32(request, 52));
            AssertEx.Equal(
                key.ExpectedActualPosition,
                TestFrame.ReadInt32(request, 56));
            AssertEx.Equal(
                recordGeneration,
                TestFrame.ReadUInt32(request, 60));
        }

        private static void AssertExactRetirementRequests(
            FakeRpcServer server,
            int expectedCount)
        {
            var requests = new List<byte[]>();
            foreach (var request in server.ReceivedRequests)
            {
                if (TestFrame.ReadUInt16(request, 0) == 0x7D1A)
                {
                    requests.Add(request);
                }
            }

            AssertEx.Equal(expectedCount, requests.Count);
            var first = requests[0];
            for (var requestIndex = 1;
                requestIndex < requests.Count;
                requestIndex++)
            {
                var current = requests[requestIndex];
                AssertEx.Equal(first.Length, current.Length);
                AssertEx.False(
                    TestFrame.ReadUInt32(first, 12)
                        == TestFrame.ReadUInt32(current, 12));
                for (var offset = 0; offset < first.Length; offset++)
                {
                    if (offset >= 12 && offset < 16)
                    {
                        continue;
                    }

                    AssertEx.Equal(
                        first[offset],
                        current[offset],
                        "Retirement retry changed exact key/generation bytes at offset "
                            + offset
                            + ".");
                }
            }
        }

        private static LMCAxisSetPositionOutcomeResult CreatePublicOutcome(
            uint queryRequestId,
            LMCAxisSetPositionRecoveryKey key,
            LMCAxisSetPositionOutcomeRecordState state,
            int appliedPosition,
            LMCAdminDetailCode originalDetail,
            uint recordGeneration)
        {
            var succeeded = state
                == LMCAxisSetPositionOutcomeRecordState.Succeeded;
            var parsed = LMC_AdminParser.ParseAxisSetPositionOutcome(
                TestFrame.Response(
                    0,
                    TerminalPayload(
                        queryRequestId,
                        key,
                        state,
                        appliedPosition,
                        succeeded ? (ushort)0 : (ushort)1,
                        succeeded ? (short)0 : (short)-31000,
                        originalDetail,
                        0,
                        recordGeneration)),
                queryRequestId,
                key);
            return new LMCAxisSetPositionOutcomeResult(
                parsed.Response,
                key,
                parsed.RecordState,
                parsed.AppliedPosition,
                parsed.OriginalCommandStatus,
                parsed.OriginalErrorId,
                parsed.OriginalDetailCode,
                parsed.NativeCommandState,
                parsed.RecordGeneration);
        }

        private static LMCAxisSetPositionOutcomeRetirementResult
            CreateRetirementResult(
                LMCParsedAxisSetPositionOutcome parsed,
                LMCAxisSetPositionRecoveryKey key)
        {
            return new LMCAxisSetPositionOutcomeRetirementResult(
                parsed.Response,
                key,
                parsed.RecordState,
                parsed.AppliedPosition,
                parsed.OriginalCommandStatus,
                parsed.OriginalErrorId,
                parsed.OriginalDetailCode,
                parsed.NativeCommandState,
                parsed.RecordGeneration);
        }

        private static void AssertRetirementMalformed(
            byte[] payload,
            uint retireRequestId,
            LMCAxisSetPositionRecoveryKey key,
            uint recordGeneration)
        {
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser
                    .ParseAxisSetPositionOutcomeRetirement(
                        TestFrame.Response(0, payload),
                        retireRequestId,
                        key,
                        recordGeneration));
        }

        private static LMCAxisSetPositionRecoveryKey RecoveryKey(
            ushort axisReference = 2,
            uint diagnosticsBuild = DiagnosticsBuild,
            uint diagnosticsBootId = OriginalDiagnosticsBootId,
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
            uint requestId,
            LMCAxisSetPositionRecoveryKey key,
            LMCAxisSetPositionOutcomeRecordState state,
            int appliedPosition,
            ushort originalStatus,
            short originalErrorId,
            LMCAdminDetailCode originalDetail,
            uint nativeCommandState,
            uint recordGeneration)
        {
            var payload = CommonPayload(requestId, 84);
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
            TestFrame.WriteInt32(
                payload,
                60,
                key.ExpectedActualPosition);
            TestFrame.WriteInt32(payload, 64, appliedPosition);
            TestFrame.WriteUInt16(payload, 68, originalStatus);
            TestFrame.WriteInt16(payload, 70, originalErrorId);
            TestFrame.WriteUInt32(
                payload,
                72,
                (uint)originalDetail);
            TestFrame.WriteUInt32(payload, 76, nativeCommandState);
            TestFrame.WriteUInt32(payload, 80, recordGeneration);
            return payload;
        }

        private static byte[] FailurePayload(
            uint requestId,
            LMCAdminDetailCode detail,
            int length = 16)
        {
            var payload = CommonPayload(requestId, length);
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

        private static LMCAdminFeature RetirementFeatures()
        {
            return LMCAdminFeature.AxisSetPositionOutcomeRead
                | LMCAdminFeature.AxisSetPositionOutcomeRetirement;
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
                            | LMCAdminFeature
                                .AxisSetPositionOutcomeRead
                            | LMCAdminFeature
                                .AxisSetPositionOutcomeRetirement)) != 0
                    ? 2
                    : 1));
            return new FakeRpcStep(
                0x7D00,
                TestFrame.Response(0, payload));
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
            TestFrame.WriteUInt32(
                payload,
                64,
                CurrentDiagnosticsBootId);
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

        private static FakeRpcStep ExpectedClientDisconnectStep()
        {
            return new FakeRpcStep(0, new byte[0])
            {
                RequireClientDisconnectBeforeRequest = true
            };
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

        private static void AssertReadOnly(
            Type type,
            string propertyName)
        {
            var property = type.GetProperty(propertyName);
            AssertEx.NotNull(property);
            AssertEx.True(
                property.SetMethod == null || !property.SetMethod.IsPublic,
                type.Name + "." + propertyName + " must be immutable.");
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

        private static int CountPublicMethods(Type type, string name)
        {
            var count = 0;
            foreach (var method in type.GetMethods(
                BindingFlags.Instance | BindingFlags.Public))
            {
                if (string.Equals(
                    method.Name,
                    name,
                    StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }
    }
}

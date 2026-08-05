using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class AdminDs402HomeOutcomeRetirementContractTests
    {
        private const uint OriginalRequestId = 0x11223344u;
        private const uint DiagnosticsBuild = 0x55667788u;
        private const uint DiagnosticsBootId = 0x99AABBCCu;
        private const uint MapRevision = 0xDDEEFF00u;
        private const uint RecordGeneration = 0xA1B2C3D4u;
        private const int HomingMethod = 37;
        private const ushort PostCleanupStatusWord = 0x0027;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Contract.Admin.Ds402HomeOutcomeRetirement.PublicSurface",
                PublicSurface);
            tests.Add(
                "Request.Admin.Ds402HomeOutcomeRetirement.GoldenBytes",
                RequestGoldenBytes);
            tests.Add(
                "Response.Admin.Ds402HomeOutcomeRetirement.StrictTerminal",
                StrictTerminalResponse);
            tests.Add(
                "Response.Admin.Ds402HomeOutcome.SucceededEvidenceFailsClosed",
                SucceededEvidenceFailsClosed);
            tests.Add(
                "Response.Admin.Ds402HomeOutcome.PostCleanupStatusBitsOpaque",
                PostCleanupStatusBitsOpaque);
            tests.Add(
                "Response.Admin.Ds402HomeOutcomeRetirement.FailureDomain",
                FailureDomain);
            tests.Add(
                "Response.Admin.Ds402Home.StartSlotOccupied",
                StartSlotOccupiedDetail);
            tests.Add(
                "Response.Admin.Ds402Home.StartOwnershipDetails",
                StartOwnershipDetails);
            tests.Add(
                "Rpc.Admin.Ds402HomeOutcomeRetirement.PreWireGuards",
                PreWireGuards);
            tests.Add(
                "Rpc.Admin.Ds402HomeOutcomeRetirement.SyncAsyncIdempotent",
                SyncAsyncIdempotentFacade);
            tests.Add(
                "Rpc.Admin.Ds402HomeOutcomeRetirement.DomainFailureKeepsSession",
                DomainFailureKeepsSession);
            tests.Add(
                "Rpc.Admin.Ds402HomeOutcomeRetirement.MalformedFaultsSession",
                MalformedResponseFaultsSession);
            tests.Add(
                "Rpc.Admin.Ds402HomeOutcomeRetirement.ResponseLossRetryAfterReconnect",
                ResponseLossRetryAfterReconnect);
            tests.Add(
                "Rpc.Admin.Ds402HomeOutcomeRetirement.StaleObservationsZeroWire",
                StaleObservationsAreZeroWire);
        }

        private static void PublicSurface()
        {
            AssertEx.Equal(
                2,
                CountPublicMethods(
                    typeof(LMCAdmin),
                    "RetireAxisDs402HomeOutcome"));
            AssertEx.Equal(
                2,
                CountPublicMethods(
                    typeof(LMCAdmin),
                    "RetireAxisDs402HomeOutcomeAsync"));
            AssertEx.Equal(
                2,
                CountPublicMethods(
                    typeof(LMCSingleAxis),
                    "RetireDs402HomeOutcome"));
            AssertEx.Equal(
                2,
                CountPublicMethods(
                    typeof(LMCSingleAxis),
                    "RetireDs402HomeOutcomeAsync"));
        }

        private static void RequestGoldenBytes()
        {
            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "17 7D 00 00 30 00 02 00 "
                    + "01 00 00 00 DD CC BB AA "
                    + "88 77 66 55 CC BB AA 99 "
                    + "00 FF EE DD 44 33 22 11 "
                    + "67 45 23 01 EF CD AB 89 "
                    + "40 30 20 10 80 70 60 50 "
                + "25 00 00 00 D4 C3 B2 A1"),
                LMC_AdminFrame.RetireAxisDs402HomeOutcome(
                    0xAABBCCDDu,
                    RecoveryKey(),
                    RecordGeneration));

            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_AdminFrame.RetireAxisDs402HomeOutcome(
                    1,
                    RecoveryKey(),
                    0));
        }

        private static void StrictTerminalResponse()
        {
            var key = RecoveryKey();
            var succeeded = LMC_AdminParser
                .ParseAxisDs402HomeOutcomeRetirement(
                    TestFrame.Response(
                        0,
                        OutcomePayload(
                            7,
                            key,
                            LMCAxisDs402HomeOutcomeRecordState.Succeeded,
                            0,
                            0,
                            LMCAdminDetailCode.None,
                            PostCleanupStatusWord,
                            100,
                            200,
                            0,
                            RecordGeneration)),
                    7,
                    key,
                    RecordGeneration);
            AssertEx.Equal(
                LMCAxisDs402HomeOutcomeRecordState.Succeeded,
                succeeded.RecordState);
            AssertEx.Equal(RecordGeneration, succeeded.RecordGeneration);

            var failed = LMC_AdminParser
                .ParseAxisDs402HomeOutcomeRetirement(
                    TestFrame.Response(
                        0,
                        OutcomePayload(
                            8,
                            key,
                            LMCAxisDs402HomeOutcomeRecordState.Failed,
                            1,
                            -31000,
                            LMCAdminDetailCode.Ds402HomeExecutionFailed,
                            0x1408,
                            100,
                            201,
                            0,
                            RecordGeneration)),
                    8,
                    key,
                    RecordGeneration);
            AssertEx.Equal(
                LMCAxisDs402HomeOutcomeRecordState.Failed,
                failed.RecordState);

            var aborted = LMC_AdminParser
                .ParseAxisDs402HomeOutcomeRetirement(
                    TestFrame.Response(
                        0,
                        OutcomePayload(
                            9,
                            key,
                            LMCAxisDs402HomeOutcomeRecordState.Aborted,
                            1,
                            -31000,
                            LMCAdminDetailCode.Ds402HomeAborted,
                            0x1408,
                            100,
                            202,
                            0,
                            RecordGeneration)),
                    9,
                    key,
                    RecordGeneration);
            AssertEx.Equal(
                LMCAxisDs402HomeOutcomeRecordState.Aborted,
                aborted.RecordState);

            AssertRetireMalformed(
                OutcomePayload(
                    10,
                    key,
                    LMCAxisDs402HomeOutcomeRecordState.Running,
                    0,
                    0,
                    LMCAdminDetailCode.None,
                    0,
                    100,
                    0,
                    0,
                    RecordGeneration),
                10,
                key,
                RecordGeneration);

            var unmaskedTombstone = OutcomePayload(
                11,
                key,
                LMCAxisDs402HomeOutcomeRecordState.Succeeded,
                0,
                0,
                LMCAdminDetailCode.None,
                PostCleanupStatusWord,
                100,
                200,
                0,
                RecordGeneration);
            TestFrame.WriteUInt16(unmaskedTombstone, 16, 0x8002);
            AssertRetireMalformed(
                unmaskedTombstone,
                11,
                key,
                RecordGeneration);

            var wrongGeneration = OutcomePayload(
                12,
                key,
                LMCAxisDs402HomeOutcomeRecordState.Succeeded,
                0,
                0,
                LMCAdminDetailCode.None,
                PostCleanupStatusWord,
                100,
                200,
                0,
                RecordGeneration + 1);
            AssertRetireMalformed(
                wrongGeneration,
                12,
                key,
                RecordGeneration);

            var wrongReserved = OutcomePayload(
                13,
                key,
                LMCAxisDs402HomeOutcomeRecordState.Succeeded,
                0,
                0,
                LMCAdminDetailCode.None,
                PostCleanupStatusWord,
                100,
                200,
                0,
                RecordGeneration);
            TestFrame.WriteUInt16(wrongReserved, 70, 1);
            AssertRetireMalformed(
                wrongReserved,
                13,
                key,
                RecordGeneration);

            AssertRetireMalformed(
                CommonAdminPayload(14, 91),
                14,
                key,
                RecordGeneration);
        }

        private static void SucceededEvidenceFailsClosed()
        {
            var mutations = new Action<byte[]>[]
            {
                payload => TestFrame.WriteUInt16(payload, 60, 1),
                payload => TestFrame.WriteInt16(payload, 62, -31000),
                payload => TestFrame.WriteUInt32(
                    payload,
                    64,
                    (uint)LMCAdminDetailCode.Ds402HomeExecutionFailed),
                payload => TestFrame.WriteUInt16(payload, 68, 0),
                payload => TestFrame.WriteUInt16(payload, 68, 0x0068),
                payload => TestFrame.WriteUInt16(payload, 68, 0x2027),
                payload => TestFrame.WriteInt32(payload, 72, 1),
                payload => TestFrame.WriteUInt32(payload, 76, 0),
                payload => TestFrame.WriteUInt32(payload, 80, 99),
                payload => TestFrame.WriteUInt32(payload, 84, 1),
                payload => TestFrame.WriteUInt32(payload, 88, 0)
            };

            foreach (var mutate in mutations)
            {
                var key = RecoveryKey();
                var payload = OutcomePayload(
                    15,
                    key,
                    LMCAxisDs402HomeOutcomeRecordState.Succeeded,
                    0,
                    0,
                    LMCAdminDetailCode.None,
                    PostCleanupStatusWord,
                    100,
                    200,
                    0,
                    RecordGeneration);
                mutate(payload);
                AssertEx.Throws<InvalidDataException>(
                    () => LMC_AdminParser.ParseAxisDs402HomeOutcome(
                        TestFrame.Response(0, payload),
                        15,
                        key));
            }

            var validKey = RecoveryKey();
            var validParsed = LMC_AdminParser.ParseAxisDs402HomeOutcome(
                TestFrame.Response(
                    0,
                    OutcomePayload(
                        15,
                        validKey,
                        LMCAxisDs402HomeOutcomeRecordState.Succeeded,
                        0,
                        0,
                        LMCAdminDetailCode.None,
                        PostCleanupStatusWord,
                        100,
                        200,
                        0,
                        RecordGeneration)),
                15,
                validKey);
            var invalidPublicResult = new LMCAxisDs402HomeOutcomeResult(
                validParsed.Response,
                validKey,
                validParsed.RecordState,
                validParsed.OriginalCommandStatus,
                validParsed.OriginalErrorId,
                validParsed.OriginalDetailCode,
                validParsed.Ds402StatusWord,
                1,
                validParsed.StartCycle,
                validParsed.CompletionCycle,
                validParsed.NativeCommandState,
                validParsed.RecordGeneration);
            AssertEx.False(invalidPublicResult.HomingSucceeded);
        }

        private static void PostCleanupStatusBitsOpaque()
        {
            foreach (var statusWord in new ushort[] { 0x0027, 0x1427 })
            {
                var key = RecoveryKey();
                var parsed = LMC_AdminParser.ParseAxisDs402HomeOutcome(
                    TestFrame.Response(
                        0,
                        OutcomePayload(
                            16,
                            key,
                            LMCAxisDs402HomeOutcomeRecordState.Succeeded,
                            0,
                            0,
                            LMCAdminDetailCode.None,
                            statusWord,
                            100,
                            200,
                            0,
                            RecordGeneration)),
                    16,
                    key);
                var result = new LMCAxisDs402HomeOutcomeResult(
                    parsed.Response,
                    key,
                    parsed.RecordState,
                    parsed.OriginalCommandStatus,
                    parsed.OriginalErrorId,
                    parsed.OriginalDetailCode,
                    parsed.Ds402StatusWord,
                    parsed.ActualPosition,
                    parsed.StartCycle,
                    parsed.CompletionCycle,
                    parsed.NativeCommandState,
                    parsed.RecordGeneration);
                AssertEx.True(result.HomingSucceeded);
                AssertEx.Equal(statusWord, result.Ds402StatusWord);
            }
        }

        private static void FailureDomain()
        {
            var key = RecoveryKey();
            var details = new[]
            {
                LMCAdminDetailCode.DiagnosticsBuildMismatch,
                LMCAdminDetailCode.BootIdMismatch,
                LMCAdminDetailCode.MapRevisionMismatch,
                LMCAdminDetailCode.Ds402HomeOutcomeNotFound,
                LMCAdminDetailCode.Ds402HomeOutcomeIndeterminate,
                LMCAdminDetailCode.Ds402HomeOutcomeStoreCorrupt,
                LMCAdminDetailCode.Ds402HomeOutcomeKeyMismatch,
                LMCAdminDetailCode.Ds402HomeOutcomeStorageUnavailable
            };

            for (var index = 0; index < details.Length; index++)
            {
                var requestId = checked((uint)(20 + index));
                var exception = AssertEx.Throws<
                    LMCAxisDs402HomeOutcomeRetirementException>(
                    () => LMC_AdminParser
                        .ParseAxisDs402HomeOutcomeRetirement(
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
            }

            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseAxisDs402HomeOutcomeRetirement(
                    TestFrame.Response(
                        0,
                        FailurePayload(
                            30,
                            LMCAdminDetailCode
                                .Ds402HomeOutcomeSlotOccupied)),
                    30,
                    key,
                    RecordGeneration));

            var extendedFailure = new byte[92];
            TestFrame.WriteUInt16(extendedFailure, 0, 1);
            TestFrame.WriteUInt16(extendedFailure, 4, 1);
            TestFrame.WriteInt16(extendedFailure, 6, -31000);
            TestFrame.WriteUInt32(extendedFailure, 8, 31);
            TestFrame.WriteUInt32(
                extendedFailure,
                12,
                (uint)LMCAdminDetailCode.Ds402HomeOutcomeNotFound);
            AssertRetireMalformed(
                extendedFailure,
                31,
                key,
                RecordGeneration);
        }

        private static void StartSlotOccupiedDetail()
        {
            var payload = CommonAdminPayload(40, 24);
            TestFrame.WriteInt32(payload, 16, HomingMethod);
            SetAdminFailure(
                payload,
                LMCAdminDetailCode.Ds402HomeOutcomeSlotOccupied,
                -31000);
            var parsed = LMC_AdminParser.ParseStartAxisDs402Home(
                TestFrame.Response(0, payload),
                40,
                HomingMethod);
            AssertEx.Equal(
                LMCAdminDetailCode.Ds402HomeOutcomeSlotOccupied,
                parsed.Response.DetailCode);
            AssertEx.Equal(0u, parsed.NativeCommandState);

            TestFrame.WriteInt16(payload, 6, -6);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseStartAxisDs402Home(
                    TestFrame.Response(0, payload),
                    40,
                    HomingMethod));
        }

        private static void StartOwnershipDetails()
        {
            foreach (var detailCode in new[]
            {
                LMCAdminDetailCode.AxisOwnershipConflict,
                LMCAdminDetailCode.AxisOwnershipQuarantined
            })
            {
                var payload = CommonAdminPayload(41, 24);
                TestFrame.WriteInt32(payload, 16, HomingMethod);
                SetAdminFailure(payload, detailCode, -31000);
                var parsed = LMC_AdminParser.ParseStartAxisDs402Home(
                    TestFrame.Response(0, payload),
                    41,
                    HomingMethod);
                AssertEx.Equal(detailCode, parsed.Response.DetailCode);
                AssertEx.Equal(0u, parsed.NativeCommandState);
            }

            var wrongDomain = CommonAdminPayload(41, 24);
            TestFrame.WriteInt32(wrongDomain, 16, HomingMethod);
            SetAdminFailure(
                wrongDomain,
                LMCAdminDetailCode.AxisOwnershipConflict,
                -6);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseStartAxisDs402Home(
                    TestFrame.Response(0, wrongDomain),
                    41,
                    HomingMethod));
        }

        private static void PreWireGuards()
        {
            var key = RecoveryKey();
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(2),
                AxisInfoStep(2),
                AdminCapabilitiesStep(1),
                DiagnosticsCapabilitiesStep(1),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis2");
                var admin = connection.Admin.GetCapabilities();
                var diagnostics = connection.Diagnostics.GetCapabilities();

                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => axis.RetireDs402HomeOutcome(
                        key,
                        0,
                        admin,
                        diagnostics));

                var cancellation = new CancellationTokenSource();
                cancellation.Cancel();
                AssertEx.Throws<OperationCanceledException>(
                    () => axis.RetireDs402HomeOutcomeAsync(
                            key,
                            RecordGeneration,
                            admin,
                            diagnostics,
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());

                var running = CreatePublicOutcome(
                    77,
                    key,
                    LMCAxisDs402HomeOutcomeRecordState.Running,
                    0,
                    RecordGeneration);
                AssertEx.Throws<InvalidOperationException>(
                    () => axis.RetireDs402HomeOutcome(
                        running,
                        admin,
                        diagnostics));

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(0, CountCommand(server, 0x7D17));
            }
        }

        private static void SyncAsyncIdempotentFacade()
        {
            var key = RecoveryKey();
            var queryStep = new FakeRpcStep(
                0x7D16,
                TestFrame.Response(
                    0,
                    OutcomePayload(
                        2,
                        key,
                        LMCAxisDs402HomeOutcomeRecordState.Succeeded,
                        0,
                        0,
                        LMCAdminDetailCode.None,
                        0x1427,
                        100,
                        200,
                        0,
                        RecordGeneration)));
            var retireStep1 = RetirementStep(3, key);
            var retireStep2 = RetirementStep(4, key);
            var retireStep3 = RetirementStep(5, key);
            var retireStep4 = RetirementStep(6, key);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(2),
                AxisInfoStep(2),
                AdminCapabilitiesStep(1),
                DiagnosticsCapabilitiesStep(1),
                queryStep,
                retireStep1,
                retireStep2,
                retireStep3,
                retireStep4,
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis2");
                var admin = connection.Admin.GetCapabilities();
                var diagnostics = connection.Diagnostics.GetCapabilities();
                var outcome = axis.ReadDs402HomeOutcome(
                    key,
                    admin,
                    diagnostics);

                var first = connection.Admin.RetireAxisDs402HomeOutcome(
                    axis,
                    key,
                    RecordGeneration,
                    admin,
                    diagnostics);
                AssertEx.True(first.RetirementConfirmed);
                AssertEx.True(first.HomingSucceeded);
                AssertEx.Equal(RecordGeneration, first.RecordGeneration);

                var axisRetry = axis.RetireDs402HomeOutcome(
                    outcome,
                    admin,
                    diagnostics);
                AssertEx.True(axisRetry.RetirementConfirmed);
                AssertEx.Equal(first.RecordState, axisRetry.RecordState);

                var adminAsyncRetry = connection.Admin
                    .RetireAxisDs402HomeOutcomeAsync(
                        axis,
                        key,
                        RecordGeneration,
                        admin,
                        diagnostics,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(adminAsyncRetry.RetirementConfirmed);
                AssertEx.Equal(
                    first.RecordGeneration,
                    adminAsyncRetry.RecordGeneration);

                var axisAsyncRetry = axis.RetireDs402HomeOutcomeAsync(
                        outcome,
                        admin,
                        diagnostics,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(axisAsyncRetry.RetirementConfirmed);
                AssertEx.Equal(first.RecordState, axisAsyncRetry.RecordState);
                AssertEx.Equal(
                    first.RecordGeneration,
                    axisAsyncRetry.RecordGeneration);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x7D16));
                AssertEx.Equal(4, CountCommand(server, 0x7D17));
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
                AdminCapabilitiesStep(1),
                DiagnosticsCapabilitiesStep(1),
                new FakeRpcStep(
                    0x7D17,
                    TestFrame.Response(
                        0,
                        FailurePayload(
                            2,
                            LMCAdminDetailCode
                                .Ds402HomeOutcomeKeyMismatch))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis2");
                var admin = connection.Admin.GetCapabilities();
                var diagnostics = connection.Diagnostics.GetCapabilities();
                var exception = AssertEx.Throws<
                    LMCAxisDs402HomeOutcomeRetirementException>(
                    () => axis.RetireDs402HomeOutcome(
                        key,
                        RecordGeneration,
                        admin,
                        diagnostics));
                AssertEx.Equal(
                    LMCAdminDetailCode.Ds402HomeOutcomeKeyMismatch,
                    exception.Response.DetailCode);
                AssertEx.Equal(
                    LMCConnectionState.Connected,
                    connection.State);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void MalformedResponseFaultsSession()
        {
            var key = RecoveryKey();
            var malformed = OutcomePayload(
                2,
                key,
                LMCAxisDs402HomeOutcomeRecordState.Succeeded,
                0,
                0,
                LMCAdminDetailCode.None,
                PostCleanupStatusWord,
                100,
                200,
                0,
                RecordGeneration + 1);
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(2),
                AxisInfoStep(2),
                AdminCapabilitiesStep(1),
                DiagnosticsCapabilitiesStep(1),
                new FakeRpcStep(0x7D17, TestFrame.Response(0, malformed)),
                ExpectedClientDisconnectStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis2");
                var admin = connection.Admin.GetCapabilities();
                var diagnostics = connection.Diagnostics.GetCapabilities();
                AssertEx.Throws<InvalidDataException>(
                    () => axis.RetireDs402HomeOutcome(
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

        private static void ResponseLossRetryAfterReconnect()
        {
            var key = RecoveryKey();
            var lostResponse = new FakeRpcStep(0x7D17, new byte[0])
            {
                CloseClientBeforeResponseAndContinue = true
            };
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(2),
                AxisInfoStep(2),
                AdminCapabilitiesStep(1),
                DiagnosticsCapabilitiesStep(1),
                lostResponse,
                InitStep(),
                CallbackStep(),
                AxisLookupStep(2),
                AxisInfoStep(2),
                AdminCapabilitiesStep(3),
                DiagnosticsCapabilitiesStep(2),
                RetirementStep(4, key),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var firstAxis = new LMCSingleAxis(
                    connection,
                    "_LMCAxis2");
                var firstAdmin = connection.Admin.GetCapabilities();
                var firstDiagnostics = connection.Diagnostics.GetCapabilities();
                AssertEx.Throws<IOException>(
                    () => firstAxis.RetireDs402HomeOutcome(
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
                var retryDiagnostics = connection.Diagnostics.GetCapabilities();
                var result = retryAxis.RetireDs402HomeOutcome(
                    key,
                    RecordGeneration,
                    retryAdmin,
                    retryDiagnostics);
                AssertEx.True(result.RetirementConfirmed);
                AssertEx.Equal(RecordGeneration, result.RecordGeneration);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(2, server.AcceptedClientCount);
                AssertEx.Equal(2, CountCommand(server, 0x7D17));
            }
        }

        private static void StaleObservationsAreZeroWire()
        {
            var key = RecoveryKey();
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(2),
                AxisInfoStep(2),
                AdminCapabilitiesStep(1),
                AdminCapabilitiesStep(2),
                DiagnosticsCapabilitiesStep(1),
                DiagnosticsCapabilitiesStep(2),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis2");
                var staleAdmin = connection.Admin.GetCapabilities();
                var currentAdmin = connection.Admin.GetCapabilities();
                var staleDiagnostics = connection.Diagnostics.GetCapabilities();
                var currentDiagnostics = connection.Diagnostics.GetCapabilities();

                AssertEx.Throws<InvalidOperationException>(
                    () => axis.RetireDs402HomeOutcome(
                        key,
                        RecordGeneration,
                        staleAdmin,
                        currentDiagnostics));
                AssertEx.Throws<InvalidOperationException>(
                    () => axis.RetireDs402HomeOutcome(
                        key,
                        RecordGeneration,
                        currentAdmin,
                        staleDiagnostics));

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(0, CountCommand(server, 0x7D17));
            }
        }

        private static FakeRpcStep RetirementStep(
            uint requestId,
            LMCAxisDs402HomeRecoveryKey key)
        {
            var step = new FakeRpcStep(
                0x7D17,
                TestFrame.Response(
                    0,
                    OutcomePayload(
                        requestId,
                        key,
                        LMCAxisDs402HomeOutcomeRecordState.Succeeded,
                        0,
                        0,
                        LMCAdminDetailCode.None,
                        0x1427,
                        100,
                        200,
                        0,
                        RecordGeneration)));
            step.InspectRequest = request =>
            {
                AssertEx.Equal((ushort)48, TestFrame.ReadUInt16(request, 4));
                AssertEx.Equal((ushort)2, TestFrame.ReadUInt16(request, 6));
                AssertEx.Equal(
                    OriginalRequestId,
                    TestFrame.ReadUInt32(request, 28));
                AssertEx.Equal(
                    RecordGeneration,
                    TestFrame.ReadUInt32(request, 52));
            };
            return step;
        }

        private static LMCAxisDs402HomeOutcomeResult CreatePublicOutcome(
            uint queryRequestId,
            LMCAxisDs402HomeRecoveryKey key,
            LMCAxisDs402HomeOutcomeRecordState state,
            uint completionCycle,
            uint generation)
        {
            var parsed = LMC_AdminParser.ParseAxisDs402HomeOutcome(
                TestFrame.Response(
                    0,
                    OutcomePayload(
                        queryRequestId,
                        key,
                        state,
                        0,
                        0,
                        LMCAdminDetailCode.None,
                        0,
                        100,
                        completionCycle,
                        0,
                        generation)),
                queryRequestId,
                key);
            return new LMCAxisDs402HomeOutcomeResult(
                parsed.Response,
                key,
                parsed.RecordState,
                parsed.OriginalCommandStatus,
                parsed.OriginalErrorId,
                parsed.OriginalDetailCode,
                parsed.Ds402StatusWord,
                parsed.ActualPosition,
                parsed.StartCycle,
                parsed.CompletionCycle,
                parsed.NativeCommandState,
                parsed.RecordGeneration);
        }

        private static void AssertRetireMalformed(
            byte[] payload,
            uint requestId,
            LMCAxisDs402HomeRecoveryKey key,
            uint generation)
        {
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseAxisDs402HomeOutcomeRetirement(
                    TestFrame.Response(0, payload),
                    requestId,
                    key,
                    generation));
        }

        private static LMCAxisDs402HomeRecoveryKey RecoveryKey()
        {
            return new LMCAxisDs402HomeRecoveryKey(
                LMCAdmin.ProtocolSchemaVersion,
                OriginalRequestId,
                DiagnosticsBuild,
                DiagnosticsBootId,
                MapRevision,
                new LMCAxisDs402HomeClientIntentId(
                    0x01234567u,
                    0x89ABCDEFu,
                    0x10203040u,
                    0x50607080u),
                2,
                new LMCAxisDs402HomeParameters(
                    HomingMethod,
                    0,
                    0,
                    0,
                    0,
                    0,
                    LMCDs402HomeBufferMode.Aborting,
                    60000));
        }

        private static byte[] OutcomePayload(
            uint requestId,
            LMCAxisDs402HomeRecoveryKey key,
            LMCAxisDs402HomeOutcomeRecordState state,
            ushort originalStatus,
            short originalError,
            LMCAdminDetailCode originalDetail,
            ushort ds402StatusWord,
            uint startCycle,
            uint completionCycle,
            uint nativeCommandState,
            uint generation)
        {
            var payload = CommonAdminPayload(requestId, 92);
            TestFrame.WriteUInt16(payload, 16, (ushort)state);
            TestFrame.WriteUInt32(payload, 20, key.DiagnosticsBuild);
            TestFrame.WriteUInt32(payload, 24, key.DiagnosticsBootId);
            TestFrame.WriteUInt32(payload, 28, key.MapRevision);
            TestFrame.WriteUInt32(payload, 32, key.RequestId);
            TestFrame.WriteUInt32(payload, 36, key.ClientIntentId.Word0);
            TestFrame.WriteUInt32(payload, 40, key.ClientIntentId.Word1);
            TestFrame.WriteUInt32(payload, 44, key.ClientIntentId.Word2);
            TestFrame.WriteUInt32(payload, 48, key.ClientIntentId.Word3);
            TestFrame.WriteUInt16(payload, 52, key.AxisReference);
            TestFrame.WriteInt32(payload, 56, key.Parameters.HomingMethod);
            TestFrame.WriteUInt16(payload, 60, originalStatus);
            TestFrame.WriteInt16(payload, 62, originalError);
            TestFrame.WriteUInt32(payload, 64, (uint)originalDetail);
            TestFrame.WriteUInt16(payload, 68, ds402StatusWord);
            TestFrame.WriteInt32(payload, 72, 0);
            TestFrame.WriteUInt32(payload, 76, startCycle);
            TestFrame.WriteUInt32(payload, 80, completionCycle);
            TestFrame.WriteUInt32(payload, 84, nativeCommandState);
            TestFrame.WriteUInt32(payload, 88, generation);
            return payload;
        }

        private static byte[] FailurePayload(
            uint requestId,
            LMCAdminDetailCode detail)
        {
            var payload = CommonAdminPayload(requestId, 16);
            SetAdminFailure(payload, detail, -31000);
            return payload;
        }

        private static void SetAdminFailure(
            byte[] payload,
            LMCAdminDetailCode detail,
            short errorId)
        {
            TestFrame.WriteUInt16(payload, 4, 1);
            TestFrame.WriteInt16(payload, 6, errorId);
            TestFrame.WriteUInt32(payload, 12, (uint)detail);
        }

        private static byte[] CommonAdminPayload(uint requestId, int length)
        {
            var payload = new byte[length];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, requestId);
            return payload;
        }

        private static byte[] AdminCapabilitiesPayload(uint requestId)
        {
            var payload = CommonAdminPayload(requestId, 40);
            TestFrame.WriteUInt32(
                payload,
                16,
                (uint)LMCAdminFeature.AxisDs402Home);
            TestFrame.WriteUInt32(payload, 20, 0x3F);
            TestFrame.WriteUInt32(
                payload,
                24,
                (uint)LMCGroupParameterSelection.All);
            TestFrame.WriteUInt16(payload, 28, 4);
            TestFrame.WriteUInt16(payload, 30, 1);
            TestFrame.WriteUInt16(payload, 32, 0x0100);
            TestFrame.WriteUInt16(payload, 34, 3);
            TestFrame.WriteUInt16(payload, 36, 4);
            return payload;
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
            TestFrame.WriteUInt32(payload, 64, DiagnosticsBootId);
            return payload;
        }

        private static FakeRpcStep AdminCapabilitiesStep(uint requestId)
        {
            return new FakeRpcStep(
                0x7D00,
                TestFrame.Response(
                    0,
                    AdminCapabilitiesPayload(requestId)));
        }

        private static FakeRpcStep DiagnosticsCapabilitiesStep(uint requestId)
        {
            return new FakeRpcStep(
                0x7E00,
                TestFrame.Response(
                    0,
                    DiagnosticsCapabilitiesPayload(requestId)));
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

        private static int CountPublicMethods(Type type, string name)
        {
            var count = 0;
            foreach (var method in type.GetMethods(
                BindingFlags.Instance | BindingFlags.Public))
            {
                if (string.Equals(method.Name, name, StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }
    }
}

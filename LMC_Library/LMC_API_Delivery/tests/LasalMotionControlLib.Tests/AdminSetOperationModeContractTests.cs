using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class AdminSetOperationModeContractTests
    {
        private const uint OriginalRequestId = 0x11223344u;
        private const uint DiagnosticsBuild = 0x55667788u;
        private const uint DiagnosticsBootId = 0x99AABBCCu;
        private const uint MapRevision = 0xDDEEFF00u;
        private const uint Intent0 = 0x01234567u;
        private const uint Intent1 = 0x89ABCDEFu;
        private const uint Intent2 = 0x10203040u;
        private const uint Intent3 = 0x50607080u;
        private const uint TimeoutMilliseconds = 1000u;
        private const uint RecordGeneration = 7u;

        private const LMCAdminFeature CapabilityTriad =
            LMCAdminFeature.AxisSetOperationModeStart
            | LMCAdminFeature.AxisSetOperationModeOutcomeRead
            | LMCAdminFeature.AxisSetOperationModeOutcomeRetire;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Request.Admin.SetOperationMode.GoldenLifecycleBytes",
                GoldenLifecycleBytes);
            tests.Add(
                "Response.Admin.SetOperationMode.StartStrictSchema",
                StartStrictSchema);
            tests.Add(
                "Response.Admin.SetOperationMode.OutcomeStrictSchema",
                OutcomeStrictSchema);
            tests.Add(
                "Response.Admin.SetOperationMode.RetireExactTerminalGeneration",
                RetireExactTerminalGeneration);
            tests.Add(
                "Response.Admin.SetOperationMode.CapabilityTriadStrict",
                CapabilityTriadStrict);
            tests.Add(
                "Rpc.Admin.SetOperationMode.CapabilityOffZeroWire",
                CapabilityOffZeroWire);
            tests.Add(
                "Rpc.Admin.SetOperationMode.SyncLifecycleFacade",
                SyncLifecycleFacade);
            tests.Add(
                "Rpc.Admin.SetOperationMode.PreCanceledZeroWireReusable",
                PreCanceledZeroWireReusable);
            tests.Add(
                "Rpc.Admin.SetOperationMode.ResponseLossUncertainNoReplay",
                ResponseLossUncertainNoReplay);
            tests.Add(
                "Rpc.Admin.SetOperationMode.DefinitiveRejectNoReplay",
                DefinitiveRejectNoReplay);
            tests.Add(
                "Contract.Admin.SetOperationMode.CspOnlyImmediate",
                CspOnlyImmediate);
        }

        private static void GoldenLifecycleBytes()
        {
            var key = RecoveryKey();
            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "23 7D 00 00 38 00 02 00 "
                    + "01 00 00 00 44 33 22 11 "
                    + "01 00 00 00 88 77 66 55 "
                    + "CC BB AA 99 00 FF EE DD "
                    + "44 33 22 11 67 45 23 01 "
                    + "EF CD AB 89 40 30 20 10 "
                    + "80 70 60 50 02 00 08 00 "
                    + "E8 03 00 00 00 00 00 00"),
                LMC_AdminFrame.StartAxisSetOperationMode(key));

            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "24 7D 00 00 38 00 02 00 "
                    + "01 00 00 00 D4 C3 B2 A1 "
                    + "01 00 00 00 88 77 66 55 "
                    + "CC BB AA 99 00 FF EE DD "
                    + "44 33 22 11 67 45 23 01 "
                    + "EF CD AB 89 40 30 20 10 "
                    + "80 70 60 50 02 00 08 00 "
                    + "E8 03 00 00 00 00 00 00"),
                LMC_AdminFrame.ReadAxisSetOperationModeOutcome(
                    0xA1B2C3D4u,
                    key));

            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "25 7D 00 00 3C 00 02 00 "
                    + "01 00 00 00 04 03 02 01 "
                    + "01 00 00 00 88 77 66 55 "
                    + "CC BB AA 99 00 FF EE DD "
                    + "44 33 22 11 67 45 23 01 "
                    + "EF CD AB 89 40 30 20 10 "
                    + "80 70 60 50 02 00 08 00 "
                    + "E8 03 00 00 00 00 00 00 "
                    + "07 00 00 00"),
                LMC_AdminFrame.RetireAxisSetOperationModeOutcome(
                    0x01020304u,
                    key,
                    RecordGeneration));

            AssertEx.Equal(
                24,
                LMC_ResponsePayloadLimits.GetMaximumPayloadLength(
                    LMC_CommandId.StartAxisSetOperationMode));
            AssertEx.Equal(
                112,
                LMC_ResponsePayloadLimits.GetMaximumPayloadLength(
                    LMC_CommandId.ReadAxisSetOperationModeOutcome));
            AssertEx.Equal(
                112,
                LMC_ResponsePayloadLimits.GetMaximumPayloadLength(
                    LMC_CommandId.RetireAxisSetOperationModeOutcome));
        }

        private static void StartStrictSchema()
        {
            var parsed = LMC_AdminParser.ParseStartAxisSetOperationMode(
                TestFrame.Response(
                    0,
                    StartPayload(OriginalRequestId)),
                OriginalRequestId,
                LMCDriveOperationMode.CyclicSynchronousPosition);
            AssertEx.True(parsed.Response.IsSuccess);
            AssertEx.Equal(
                LMCDriveOperationMode.CyclicSynchronousPosition,
                parsed.RequestedMode);
            AssertEx.Equal(0u, parsed.NativeCommandState);

            var wrongEcho = StartPayload(OriginalRequestId);
            TestFrame.WriteInt32(wrongEcho, 16, 6);
            AssertStartMalformed(wrongEcho);

            var nativeState = StartPayload(OriginalRequestId);
            TestFrame.WriteUInt32(nativeState, 20, 1);
            AssertStartMalformed(nativeState);

            AssertStartMalformed(new byte[23]);
            AssertStartMalformed(new byte[25]);

            var commonFailure = FailurePayload(
                OriginalRequestId,
                LMCAdminDetailCode.InvalidPayloadLength,
                16);
            var commonParsed = LMC_AdminParser
                .ParseStartAxisSetOperationMode(
                    TestFrame.Response(0, commonFailure),
                    OriginalRequestId,
                    LMCDriveOperationMode.CyclicSynchronousPosition);
            AssertEx.False(commonParsed.Response.IsSuccess);

            var domainFailure = FailurePayload(
                OriginalRequestId,
                LMCAdminDetailCode.SetOperationModeUnsafeState,
                24);
            TestFrame.WriteInt32(domainFailure, 16, 8);
            var domainParsed = LMC_AdminParser
                .ParseStartAxisSetOperationMode(
                    TestFrame.Response(0, domainFailure),
                    OriginalRequestId,
                    LMCDriveOperationMode.CyclicSynchronousPosition);
            AssertEx.False(domainParsed.Response.IsSuccess);

            var queryOnlyFailure = FailurePayload(
                OriginalRequestId,
                LMCAdminDetailCode.SetOperationModeOutcomeNotFound,
                24);
            TestFrame.WriteInt32(queryOnlyFailure, 16, 8);
            AssertStartMalformed(queryOnlyFailure);
        }

        private static void OutcomeStrictSchema()
        {
            var key = RecoveryKey();
            var payload = TerminalPayload(
                0xA1B2C3D4u,
                key,
                LMCAxisSetOperationModeOutcomeRecordState.Succeeded,
                8,
                LMCAxisSetOperationModeEvidenceFlags.VerifyReadDispatched
                    | LMCAxisSetOperationModeEvidenceFlags.VerifyReadCompleted
                    | LMCAxisSetOperationModeEvidenceFlags.OwnerReleased
                    | LMCAxisSetOperationModeEvidenceFlags.ExecutorReusable,
                100,
                110,
                RecordGeneration);
            var parsed = LMC_AdminParser.ParseAxisSetOperationModeOutcome(
                TestFrame.Response(0, payload),
                0xA1B2C3D4u,
                key);
            var result = new LMCAxisSetOperationModeOutcomeResult(
                parsed.Response,
                key,
                parsed);
            AssertEx.True(result.ModeChangeSucceeded);
            AssertEx.True(result.SucceededWithoutWrite);
            AssertEx.False(result.WriteWasDispatched);
            AssertEx.Equal((sbyte)8, result.ObservedModeRaw);

            var writePayload = (byte[])payload.Clone();
            TestFrame.WriteUInt32(
                writePayload,
                76,
                (uint)(LMCAxisSetOperationModeEvidenceFlags.WriteRequested
                    | LMCAxisSetOperationModeEvidenceFlags.WriteDispatched
                    | LMCAxisSetOperationModeEvidenceFlags.VerifyReadDispatched
                    | LMCAxisSetOperationModeEvidenceFlags.VerifyReadCompleted
                    | LMCAxisSetOperationModeEvidenceFlags.OwnerReleased
                    | LMCAxisSetOperationModeEvidenceFlags.ExecutorReusable));
            var writeParsed = LMC_AdminParser
                .ParseAxisSetOperationModeOutcome(
                    TestFrame.Response(0, writePayload),
                    0xA1B2C3D4u,
                    key);
            var writeResult = new LMCAxisSetOperationModeOutcomeResult(
                writeParsed.Response,
                key,
                writeParsed);
            AssertEx.True(writeResult.WriteWasDispatched);
            AssertEx.False(writeResult.SucceededWithoutWrite);

            var wrongKey = (byte[])payload.Clone();
            TestFrame.WriteUInt32(wrongKey, 32, OriginalRequestId + 1);
            AssertOutcomeMalformed(wrongKey, 0xA1B2C3D4u, key);

            var reserved = (byte[])payload.Clone();
            reserved[98] = 1;
            AssertOutcomeMalformed(reserved, 0xA1B2C3D4u, key);

            var impossibleEvidence = (byte[])payload.Clone();
            TestFrame.WriteUInt32(
                impossibleEvidence,
                76,
                (uint)(LMCAxisSetOperationModeEvidenceFlags.WriteDispatched
                    | LMCAxisSetOperationModeEvidenceFlags.VerifyReadDispatched
                    | LMCAxisSetOperationModeEvidenceFlags.VerifyReadCompleted
                    | LMCAxisSetOperationModeEvidenceFlags.OwnerReleased
                    | LMCAxisSetOperationModeEvidenceFlags.ExecutorReusable));
            AssertOutcomeMalformed(
                impossibleEvidence,
                0xA1B2C3D4u,
                key);

            var failed = TerminalPayload(
                0xA1B2C3D4u,
                key,
                LMCAxisSetOperationModeOutcomeRecordState.Failed,
                6,
                LMCAxisSetOperationModeEvidenceFlags.OwnerReleased
                    | LMCAxisSetOperationModeEvidenceFlags.ExecutorReusable,
                100,
                110,
                RecordGeneration);
            TestFrame.WriteUInt16(failed, 64, 1);
            TestFrame.WriteInt16(failed, 66, -31000);
            TestFrame.WriteUInt32(
                failed,
                68,
                (uint)LMCAdminDetailCode.SetOperationModeExecutionFailed);
            var failedParsed = LMC_AdminParser
                .ParseAxisSetOperationModeOutcome(
                    TestFrame.Response(0, failed),
                    0xA1B2C3D4u,
                    key);
            AssertEx.Equal(
                LMCAxisSetOperationModeOutcomeRecordState.Failed,
                failedParsed.RecordState);

            var quarantinedFailed = (byte[])failed.Clone();
            TestFrame.WriteUInt32(quarantinedFailed, 100, 1);
            AssertOutcomeMalformed(
                quarantinedFailed,
                0xA1B2C3D4u,
                key);

            var failure = FailurePayload(
                0xA1B2C3D4u,
                LMCAdminDetailCode.SetOperationModeOutcomeIndeterminate,
                16);
            var queryError = AssertEx.Throws<
                LMCAxisSetOperationModeOutcomeQueryException>(
                    () => LMC_AdminParser
                        .ParseAxisSetOperationModeOutcome(
                            TestFrame.Response(0, failure),
                            0xA1B2C3D4u,
                            key));
            AssertEx.Equal(
                LMCAdminDetailCode.SetOperationModeOutcomeIndeterminate,
                queryError.Response.DetailCode);
        }

        private static void RetireExactTerminalGeneration()
        {
            var key = RecoveryKey();
            var payload = TerminalPayload(
                0x01020304u,
                key,
                LMCAxisSetOperationModeOutcomeRecordState.Succeeded,
                8,
                LMCAxisSetOperationModeEvidenceFlags.VerifyReadDispatched
                    | LMCAxisSetOperationModeEvidenceFlags.VerifyReadCompleted
                    | LMCAxisSetOperationModeEvidenceFlags.OwnerReleased
                    | LMCAxisSetOperationModeEvidenceFlags.ExecutorReusable,
                100,
                110,
                RecordGeneration);
            var parsed = LMC_AdminParser
                .ParseAxisSetOperationModeOutcomeRetirement(
                    TestFrame.Response(0, payload),
                    0x01020304u,
                    key,
                    RecordGeneration);
            var result =
                new LMCAxisSetOperationModeOutcomeRetirementResult(
                    parsed.Response,
                    key,
                    parsed);
            AssertEx.True(result.RetirementConfirmed);
            AssertEx.Equal(RecordGeneration, result.RecordGeneration);

            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser
                    .ParseAxisSetOperationModeOutcomeRetirement(
                        TestFrame.Response(0, payload),
                        0x01020304u,
                        key,
                        RecordGeneration + 1));

            var running = TerminalPayload(
                0x01020304u,
                key,
                LMCAxisSetOperationModeOutcomeRecordState.Running,
                6,
                LMCAxisSetOperationModeEvidenceFlags.VerifyReadDispatched,
                100,
                0,
                RecordGeneration);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser
                    .ParseAxisSetOperationModeOutcomeRetirement(
                        TestFrame.Response(0, running),
                        0x01020304u,
                        key,
                        RecordGeneration));
        }

        private static void CapabilityTriadStrict()
        {
            var capabilities = LMC_AdminParser.ParseCapabilities(
                TestFrame.Response(
                    0,
                    CapabilitiesPayload(
                        OriginalRequestId,
                        CapabilityTriad,
                        6)),
                OriginalRequestId,
                1);
            AssertEx.True(capabilities.Supports(CapabilityTriad));

            foreach (var partial in new[]
            {
                LMCAdminFeature.AxisSetOperationModeStart,
                LMCAdminFeature.AxisSetOperationModeOutcomeRead,
                LMCAdminFeature.AxisSetOperationModeOutcomeRetire,
                LMCAdminFeature.AxisSetOperationModeStart
                    | LMCAdminFeature.AxisSetOperationModeOutcomeRead,
                LMCAdminFeature.AxisSetOperationModeStart
                    | LMCAdminFeature.AxisSetOperationModeOutcomeRetire,
                LMCAdminFeature.AxisSetOperationModeOutcomeRead
                    | LMCAdminFeature.AxisSetOperationModeOutcomeRetire
            })
            {
                AssertEx.Throws<InvalidDataException>(
                    () => LMC_AdminParser.ParseCapabilities(
                        TestFrame.Response(
                            0,
                            CapabilitiesPayload(
                                OriginalRequestId,
                                partial,
                                6)),
                        OriginalRequestId,
                        1));
            }

            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseCapabilities(
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            OriginalRequestId,
                            CapabilityTriad,
                            5)),
                    OriginalRequestId,
                    1));
        }

        private static void CapabilityOffZeroWire()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                CapabilitiesStep(1, LMCAdminFeature.None),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var capabilities = connection.Admin.GetCapabilities();
                var token = LMCAxisSetOperationModeExecuteToken.Create();
                AssertEx.Throws<NotSupportedException>(
                    () => axis.PrepareSetOperationMode(
                        LMCDriveOperationMode.CyclicSynchronousPosition,
                        TimeoutMilliseconds,
                        capabilities,
                        null,
                        token));
                AssertEx.False(token.IsConsumed);
                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(
                    0,
                    CountCommand(
                        server,
                        LMC_CommandId.StartAxisSetOperationMode));
            }
        }

        private static void SyncLifecycleFacade()
        {
            var start = new FakeRpcStep(
                LMC_CommandId.StartAxisSetOperationMode,
                TestFrame.Response(0, StartPayload(2)));
            start.InspectRequest = request =>
            {
                AssertEx.Equal((ushort)56, TestFrame.ReadUInt16(request, 4));
                AssertEx.Equal((ushort)1, TestFrame.ReadUInt16(request, 6));
                AssertEx.Equal(2u, TestFrame.ReadUInt32(request, 12));
                AssertEx.Equal(1u, TestFrame.ReadUInt32(request, 16));
                AssertEx.Equal(2u, TestFrame.ReadUInt32(request, 32));
                AssertEx.Equal((byte)8, request[54]);
            };
            var query = new FakeRpcStep(
                LMC_CommandId.ReadAxisSetOperationModeOutcome,
                null);
            var retire = new FakeRpcStep(
                LMC_CommandId.RetireAxisSetOperationModeOutcome,
                null);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                CapabilitiesStep(1, CapabilityTriad),
                start,
                query,
                retire,
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var capabilities = connection.Admin.GetCapabilities();
                var diagnostics = CurrentDiagnosticsCapabilities(connection);
                var prepared = axis.PrepareSetOperationMode(
                    LMCDriveOperationMode.CyclicSynchronousPosition,
                    TimeoutMilliseconds,
                    capabilities,
                    diagnostics,
                    LMCAxisSetOperationModeExecuteToken.Create());

                // Bind the exact random key to the response factories only
                // after preparation, without replacing or replaying Start.
                query.ResponseFactory = request => TestFrame.Response(
                    0,
                    TerminalPayload(
                        TestFrame.ReadUInt32(request, 12),
                        prepared.RecoveryKey,
                        LMCAxisSetOperationModeOutcomeRecordState.Succeeded,
                        8,
                        LMCAxisSetOperationModeEvidenceFlags
                                .VerifyReadDispatched
                            | LMCAxisSetOperationModeEvidenceFlags
                                .VerifyReadCompleted
                            | LMCAxisSetOperationModeEvidenceFlags
                                .OwnerReleased
                            | LMCAxisSetOperationModeEvidenceFlags
                                .ExecutorReusable,
                        100,
                        110,
                        RecordGeneration));
                retire.ResponseFactory = request => TestFrame.Response(
                    0,
                    TerminalPayload(
                        TestFrame.ReadUInt32(request, 12),
                        prepared.RecoveryKey,
                        LMCAxisSetOperationModeOutcomeRecordState.Succeeded,
                        8,
                        LMCAxisSetOperationModeEvidenceFlags
                                .VerifyReadDispatched
                            | LMCAxisSetOperationModeEvidenceFlags
                                .VerifyReadCompleted
                            | LMCAxisSetOperationModeEvidenceFlags
                                .OwnerReleased
                            | LMCAxisSetOperationModeEvidenceFlags
                                .ExecutorReusable,
                        100,
                        110,
                        RecordGeneration));

                var acknowledgement = axis.SetOperationMode(prepared);
                AssertEx.True(acknowledgement.IsAccepted);
                AssertEx.True(prepared.IsConsumed);
                var outcome = axis.ReadSetOperationModeOutcome(
                    prepared.RecoveryKey,
                    capabilities,
                    diagnostics);
                AssertEx.True(outcome.ModeChangeSucceeded);
                var retired = axis.RetireSetOperationModeOutcome(
                    outcome,
                    capabilities,
                    diagnostics);
                AssertEx.True(retired.RetirementConfirmed);
                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(
                    1,
                    CountCommand(
                        server,
                        LMC_CommandId.StartAxisSetOperationMode));
            }
        }

        private static void CspOnlyImmediate()
        {
            AssertEx.Throws<NotSupportedException>(
                () => new LMCAxisSetOperationModeRecoveryKey(
                    1,
                    OriginalRequestId,
                    DiagnosticsBuild,
                    DiagnosticsBootId,
                    MapRevision,
                    Intent0,
                    Intent1,
                    Intent2,
                    Intent3,
                    2,
                    LMCDriveOperationMode.Homing,
                    TimeoutMilliseconds));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new LMCAxisSetOperationModeRecoveryKey(
                    1,
                    OriginalRequestId,
                    DiagnosticsBuild,
                    DiagnosticsBootId,
                    MapRevision,
                    Intent0,
                    Intent1,
                    Intent2,
                    Intent3,
                    2,
                    LMCDriveOperationMode.CyclicSynchronousPosition,
                    0));
            AssertEx.Throws<ArgumentException>(
                () => new LMCAxisSetOperationModeClientIntentId(
                    0,
                    0,
                    0,
                    0));
        }

        private static void PreCanceledZeroWireReusable()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                CapabilitiesStep(1, CapabilityTriad),
                new FakeRpcStep(
                    LMC_CommandId.StartAxisSetOperationMode,
                    TestFrame.Response(0, StartPayload(2))),
                CloseStep()))
            using (var connection = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var capabilities = connection.Admin.GetCapabilities();
                var diagnostics = CurrentDiagnosticsCapabilities(connection);
                var prepared = axis.PrepareSetOperationMode(
                    LMCDriveOperationMode.CyclicSynchronousPosition,
                    TimeoutMilliseconds,
                    capabilities,
                    diagnostics,
                    LMCAxisSetOperationModeExecuteToken.Create());
                cancellation.Cancel();

                AssertEx.Throws<OperationCanceledException>(
                    () => axis.SetOperationModeAsync(
                            prepared,
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.False(prepared.IsConsumed);

                var acknowledgement = axis.SetOperationMode(prepared);
                AssertEx.True(acknowledgement.IsAccepted);
                AssertEx.True(prepared.IsConsumed);
                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(
                    1,
                    CountCommand(
                        server,
                        LMC_CommandId.StartAxisSetOperationMode));
            }
        }

        private static void ResponseLossUncertainNoReplay()
        {
            var lostResponse = new FakeRpcStep(
                LMC_CommandId.StartAxisSetOperationMode,
                new byte[0])
            {
                CloseClientBeforeResponse = true
            };
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                CapabilitiesStep(1, CapabilityTriad),
                lostResponse))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var capabilities = connection.Admin.GetCapabilities();
                var diagnostics = CurrentDiagnosticsCapabilities(connection);
                var prepared = axis.PrepareSetOperationMode(
                    LMCDriveOperationMode.CyclicSynchronousPosition,
                    TimeoutMilliseconds,
                    capabilities,
                    diagnostics,
                    LMCAxisSetOperationModeExecuteToken.Create());

                var exception = AssertEx.Throws<
                    LMCAxisSetOperationModeOutcomeUncertainException>(
                    () => axis.SetOperationMode(prepared));
                AssertEx.Equal(prepared, exception.PreparedCommand);
                AssertEx.Equal(
                    prepared.RecoveryKey,
                    exception.RecoveryKey);
                AssertEx.True(prepared.IsConsumed);
                AssertEx.NotNull(exception.InnerException);
                AssertEx.Equal(
                    LMCConnectionState.Faulted,
                    connection.State);
                AssertEx.Throws<InvalidOperationException>(
                    () => axis.SetOperationMode(prepared));

                server.Verify();
                AssertEx.Equal(
                    1,
                    CountCommand(
                        server,
                        LMC_CommandId.StartAxisSetOperationMode));
            }
        }

        private static void DefinitiveRejectNoReplay()
        {
            var rejection = FailurePayload(
                2,
                LMCAdminDetailCode.SetOperationModeUnsafeState,
                24);
            TestFrame.WriteInt32(rejection, 16, 8);
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                CapabilitiesStep(1, CapabilityTriad),
                new FakeRpcStep(
                    LMC_CommandId.StartAxisSetOperationMode,
                    TestFrame.Response(0, rejection)),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var capabilities = connection.Admin.GetCapabilities();
                var diagnostics = CurrentDiagnosticsCapabilities(connection);
                var prepared = axis.PrepareSetOperationMode(
                    LMCDriveOperationMode.CyclicSynchronousPosition,
                    TimeoutMilliseconds,
                    capabilities,
                    diagnostics,
                    LMCAxisSetOperationModeExecuteToken.Create());

                var exception = AssertEx.Throws<
                    LMCAxisSetOperationModeRejectedException>(
                    () => axis.SetOperationMode(prepared));
                AssertEx.Equal(
                    LMCAdminDetailCode.SetOperationModeUnsafeState,
                    exception.Response.DetailCode);
                AssertEx.True(prepared.IsConsumed);
                AssertEx.Equal(
                    LMCConnectionState.Connected,
                    connection.State);
                AssertEx.Throws<InvalidOperationException>(
                    () => axis.SetOperationMode(prepared));

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(
                    1,
                    CountCommand(
                        server,
                        LMC_CommandId.StartAxisSetOperationMode));
            }
        }

        private static LMCAxisSetOperationModeRecoveryKey RecoveryKey(
            ushort axisReference = 2)
        {
            return new LMCAxisSetOperationModeRecoveryKey(
                LMCAdmin.ProtocolSchemaVersion,
                OriginalRequestId,
                DiagnosticsBuild,
                DiagnosticsBootId,
                MapRevision,
                Intent0,
                Intent1,
                Intent2,
                Intent3,
                axisReference,
                LMCDriveOperationMode.CyclicSynchronousPosition,
                TimeoutMilliseconds);
        }

        private static byte[] StartPayload(uint requestId)
        {
            var payload = CommonPayload(requestId, 24);
            TestFrame.WriteInt32(payload, 16, 8);
            return payload;
        }

        private static byte[] TerminalPayload(
            uint requestId,
            LMCAxisSetOperationModeRecoveryKey key,
            LMCAxisSetOperationModeOutcomeRecordState recordState,
            sbyte observedMode,
            LMCAxisSetOperationModeEvidenceFlags evidenceFlags,
            uint startCycle,
            uint completionCycle,
            uint recordGeneration)
        {
            var payload = CommonPayload(requestId, 112);
            TestFrame.WriteUInt16(payload, 16, (ushort)recordState);
            TestFrame.WriteUInt32(payload, 20, key.DiagnosticsBuild);
            TestFrame.WriteUInt32(payload, 24, key.DiagnosticsBootId);
            TestFrame.WriteUInt32(payload, 28, key.MapRevision);
            TestFrame.WriteUInt32(payload, 32, key.OriginalRequestId);
            TestFrame.WriteUInt32(payload, 36, key.ClientIntentId0);
            TestFrame.WriteUInt32(payload, 40, key.ClientIntentId1);
            TestFrame.WriteUInt32(payload, 44, key.ClientIntentId2);
            TestFrame.WriteUInt32(payload, 48, key.ClientIntentId3);
            TestFrame.WriteUInt16(payload, 52, key.AxisReference);
            payload[54] = unchecked((byte)key.RequestedModeRaw);
            payload[55] = unchecked((byte)observedMode);
            TestFrame.WriteUInt32(payload, 56, key.TimeoutMilliseconds);
            TestFrame.WriteUInt32(payload, 60, key.Flags);
            TestFrame.WriteUInt32(payload, 72, 0xAABBCCDDu);
            TestFrame.WriteUInt32(payload, 76, (uint)evidenceFlags);
            TestFrame.WriteUInt32(payload, 80, startCycle);
            TestFrame.WriteUInt32(payload, 84, completionCycle);
            TestFrame.WriteUInt32(payload, 92, recordGeneration);
            payload[96] = 8;
            TestFrame.WriteUInt16(payload, 104, 0x0027);
            TestFrame.WriteUInt32(payload, 108, 0x12345678u);
            return payload;
        }

        private static byte[] FailurePayload(
            uint requestId,
            LMCAdminDetailCode detail,
            int length)
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

        private static byte[] CapabilitiesPayload(
            uint requestId,
            LMCAdminFeature features,
            ushort errorCatalogVersion)
        {
            var payload = CommonPayload(requestId, 40);
            TestFrame.WriteUInt32(payload, 16, (uint)features);
            TestFrame.WriteUInt16(payload, 28, 4);
            TestFrame.WriteUInt16(payload, 36, errorCatalogVersion);
            return payload;
        }

        private static FakeRpcStep CapabilitiesStep(
            uint requestId,
            LMCAdminFeature features)
        {
            return new FakeRpcStep(
                LMC_CommandId.GetAdminCapabilities,
                TestFrame.Response(
                    0,
                    CapabilitiesPayload(
                        requestId,
                        features,
                        (ushort)((features & CapabilityTriad)
                                == CapabilityTriad
                            ? 6
                            : 1))));
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
                TestFrame.Response(
                    0,
                    TestFrame.Hex("00 00 00 00")));
        }

        private static FakeRpcStep CloseStep()
        {
            return new FakeRpcStep(
                LMC_CommandId.CloseConnection,
                TestFrame.Response(
                    0,
                    TestFrame.Hex("00 00 00 00")));
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

        private static void AssertStartMalformed(byte[] payload)
        {
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseStartAxisSetOperationMode(
                    TestFrame.Response(0, payload),
                    OriginalRequestId,
                    LMCDriveOperationMode.CyclicSynchronousPosition));
        }

        private static void AssertOutcomeMalformed(
            byte[] payload,
            uint queryRequestId,
            LMCAxisSetOperationModeRecoveryKey key)
        {
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseAxisSetOperationModeOutcome(
                    TestFrame.Response(0, payload),
                    queryRequestId,
                    key));
        }
    }
}

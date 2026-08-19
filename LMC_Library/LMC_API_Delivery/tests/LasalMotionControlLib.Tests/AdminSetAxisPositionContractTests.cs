using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class AdminSetAxisPositionContractTests
    {
        private const uint GoldenRequestId = 0x11223344u;
        private const uint DiagnosticsBuild = 0x55667788u;
        private const uint DiagnosticsBootId = 0x99AABBCCu;
        private const uint MapRevision = 0xDDEEFF00u;
        private const uint ClientIntentId0 = 0x01234567u;
        private const uint ClientIntentId1 = 0x89ABCDEFu;
        private const uint ClientIntentId2 = 0x10203040u;
        private const uint ClientIntentId3 = 0x50607080u;
        private const int TargetPosition = -12345;
        private const int ExpectedActualPosition = 6789;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Request.Admin.SetAxisPosition.GoldenBytes",
                RequestGoldenBytes);
            tests.Add(
                "Contract.Admin.SetAxisPosition.TypedImmutableSurface",
                TypedImmutableSurface);
            tests.Add(
                "Response.Admin.SetAxisPosition.StrictSchema",
                StrictResponseSchema);
            tests.Add(
                "Response.Admin.SetAxisPosition.SafetyFailuresPreserved",
                SafetyFailuresPreserved);
            tests.Add(
                "Response.Admin.SetAxisPosition.StoreAdmissionFailuresStrict",
                StoreAdmissionFailuresStrict);
            tests.Add(
                "Response.Admin.SetAxisPosition.CapabilityBitStrict",
                CapabilityBitStrict);
            tests.Add(
                "Rpc.Admin.SetAxisPosition.SyncPreparedFacade",
                SyncPreparedFacade);
            tests.Add(
                "Rpc.Admin.SetAxisPosition.AsyncPreparedFacade",
                AsyncPreparedFacade);
            tests.Add(
                "Rpc.Admin.SetAxisPosition.CapabilityGateZeroWire",
                CapabilityGateZeroWire);
            tests.Add(
                "Rpc.Admin.SetAxisPosition.FreshCapabilitySnapshotsRequired",
                FreshCapabilitySnapshotsRequired);
            tests.Add(
                "Rpc.Admin.SetAxisPosition.ConfirmationTokenOneIntent",
                ConfirmationTokenOneIntent);
            tests.Add(
                "Rpc.Admin.SetAxisPosition.PreparedOwnerAndSessionPinned",
                PreparedOwnerAndSessionPinned);
            tests.Add(
                "Rpc.Admin.SetAxisPosition.ConcurrentPreparedSingleDispatch",
                ConcurrentPreparedSingleDispatch);
            tests.Add(
                "Rpc.Admin.SetAxisPosition.PreCanceledZeroWireReusable",
                PreCanceledZeroWireReusable);
            tests.Add(
                "Rpc.Admin.SetAxisPosition.ResponseLossUncertainNoReplay",
                ResponseLossUncertainNoReplay);
            tests.Add(
                "Rpc.Admin.SetAxisPosition.NativeRejectPreservesBitfield",
                NativeRejectPreservesBitfield);
            tests.Add(
                "Rpc.Admin.SetAxisPosition.MalformedPostWriteFaultsSession",
                MalformedPostWriteFaultsSession);
            tests.Add(
                "Rpc.Admin.SetAxisPosition.PublishFailureFaultsSession",
                PublishFailureFaultsSession);
            tests.Add(
                "Connection.Admin.SetAxisPosition.UncertainInvalidationPinsSession",
                UncertainInvalidationPinsSession);
            tests.Add(
                "Rpc.Admin.SetAxisPosition.DefinitiveRejectNoReplay",
                DefinitiveRejectNoReplay);
        }

        private static void RequestGoldenBytes()
        {
            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "12 7D 00 00 30 00 02 00 "
                    + "01 00 00 00 44 33 22 11 "
                    + "88 77 66 55 CC BB AA 99 "
                    + "00 FF EE DD 67 45 23 01 "
                    + "EF CD AB 89 40 30 20 10 "
                    + "80 70 60 50 "
                    + "C7 CF FF FF 85 1A 00 00 "
                    + "53 45 54 50"),
                LMC_AdminFrame.SetAxisPosition(RecoveryKey()));

            AssertEx.Throws<ArgumentNullException>(
                () => LMC_AdminFrame.SetAxisPosition(null));
            AssertEx.Throws<ArgumentException>(
                () => new LMCAxisSetPositionClientIntentId(0, 0, 0, 0));
        }

        private static void TypedImmutableSurface()
        {
            AssertEx.Equal(
                0,
                typeof(LMCAxisSetPositionExecuteToken)
                    .GetConstructors().Length);
            AssertEx.Equal(
                0,
                typeof(LMCPreparedAxisSetPosition)
                    .GetConstructors().Length);

            AssertPublicPropertyIsReadOnly(
                typeof(LMCPreparedAxisSetPosition),
                "RequestId");
            AssertPublicPropertyIsReadOnly(
                typeof(LMCPreparedAxisSetPosition),
                "AxisReference");
            AssertPublicPropertyIsReadOnly(
                typeof(LMCPreparedAxisSetPosition),
                "TargetPosition");
            AssertPublicPropertyIsReadOnly(
                typeof(LMCPreparedAxisSetPosition),
                "ExpectedActualPosition");
            AssertPublicPropertyIsReadOnly(
                typeof(LMCPreparedAxisSetPosition),
                "SemanticMode");
            AssertPublicPropertyIsReadOnly(
                typeof(LMCPreparedAxisSetPosition),
                "RecoveryKey");
            AssertPublicPropertyIsReadOnly(
                typeof(LMCAxisSetPositionRecoveryKey),
                "ClientIntentId");
            AssertPublicPropertyIsReadOnly(
                typeof(LMCAxisSetPositionResult),
                "RequestId");
            AssertPublicPropertyIsReadOnly(
                typeof(LMCAxisSetPositionResult),
                "NativeCommandState");

            var token = LMCAxisSetPositionExecuteToken.Create();
            AssertEx.False(token.IsConsumed);
            AssertEx.Equal(
                LMCAxisSetPositionSemanticMode
                    .ActualAndDestinationApplicationUnits,
                (LMCAxisSetPositionSemanticMode)1);
            AssertEx.Equal(
                0x50544553u,
                LMC_AdminFrame.SetAxisPositionExecuteTokenValue);
            var generatedIntent =
                LMCAxisSetPositionClientIntentId.Create();
            AssertEx.True(
                generatedIntent.Word0 != 0
                    || generatedIntent.Word1 != 0
                    || generatedIntent.Word2 != 0
                    || generatedIntent.Word3 != 0);
            AssertEx.True(RecoveryKey().Equals(RecoveryKey()));
        }

        private static void StrictResponseSchema()
        {
            var parsed = LMC_AdminParser.ParseSetAxisPosition(
                TestFrame.Response(
                    0,
                    SetPositionPayload(
                        GoldenRequestId,
                        TargetPosition,
                        0)),
                GoldenRequestId,
                TargetPosition);
            AssertEx.Equal(TargetPosition, parsed.AppliedPosition);
            AssertEx.Equal(
                LMCAxisSetPositionSemanticMode
                    .ActualAndDestinationApplicationUnits,
                parsed.SemanticMode);
            AssertEx.Equal(0u, parsed.NativeCommandState);

            AssertMalformed(new byte[27]);
            AssertMalformed(new byte[29]);

            var wrongSchema = SetPositionPayload(
                GoldenRequestId,
                TargetPosition,
                0);
            TestFrame.WriteUInt16(wrongSchema, 0, 2);
            AssertMalformed(wrongSchema);

            var wrongFlags = SetPositionPayload(
                GoldenRequestId,
                TargetPosition,
                0);
            TestFrame.WriteUInt16(wrongFlags, 2, 1);
            AssertMalformed(wrongFlags);

            var wrongRequest = SetPositionPayload(
                GoldenRequestId + 1,
                TargetPosition,
                0);
            AssertMalformed(wrongRequest);

            var wrongPosition = SetPositionPayload(
                GoldenRequestId,
                TargetPosition + 1,
                0);
            AssertMalformed(wrongPosition);

            var wrongMode = SetPositionPayload(
                GoldenRequestId,
                TargetPosition,
                0);
            TestFrame.WriteUInt16(wrongMode, 20, 2);
            AssertMalformed(wrongMode);

            var wrongReserved = SetPositionPayload(
                GoldenRequestId,
                TargetPosition,
                0);
            TestFrame.WriteUInt16(wrongReserved, 22, 1);
            AssertMalformed(wrongReserved);

            var nativeStateOnSuccess = SetPositionPayload(
                GoldenRequestId,
                TargetPosition,
                1);
            AssertMalformed(nativeStateOnSuccess);

            AssertMalformed(
                SetPositionFailurePayload(
                    GoldenRequestId,
                    LMCAdminDetailCode.NonZeroVelocity,
                    -31000,
                    1));

            var appliedPositionOnFailure = SetPositionFailurePayload(
                GoldenRequestId,
                LMCAdminDetailCode.NonZeroVelocity);
            TestFrame.WriteInt32(appliedPositionOnFailure, 16, 1);
            AssertMalformed(appliedPositionOnFailure);

            AssertMalformed(
                SetPositionFailurePayload(
                    GoldenRequestId,
                    LMCAdminDetailCode.NativeCommandRejected,
                    -6,
                    0));

            AssertMalformed(
                SetPositionFailurePayload(
                    GoldenRequestId,
                    LMCAdminDetailCode.NativeCommandRejected,
                    23,
                    8));

            var appliedPositionOnNativeFailure =
                SetPositionFailurePayload(
                    GoldenRequestId,
                    LMCAdminDetailCode.NativeCommandRejected,
                    -6,
                    8);
            TestFrame.WriteInt32(
                appliedPositionOnNativeFailure,
                16,
                1);
            AssertMalformed(appliedPositionOnNativeFailure);

            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseSetAxisPosition(
                    TestFrame.Response(
                        0,
                        SetPositionPayload(
                            GoldenRequestId,
                            TargetPosition,
                            0),
                        1),
                    GoldenRequestId,
                    TargetPosition));
        }

        private static void SafetyFailuresPreserved()
        {
            foreach (var detail in new[]
            {
                LMCAdminDetailCode.NonZeroVelocity,
                LMCAdminDetailCode.ActiveAxisError,
                LMCAdminDetailCode.InvalidSetPositionSafetyConfiguration,
                LMCAdminDetailCode.CoordinatePreconditionFailed
            })
            {
                var parsed = LMC_AdminParser.ParseSetAxisPosition(
                    TestFrame.Response(
                        0,
                        SetPositionFailurePayload(
                            GoldenRequestId,
                            detail)),
                    GoldenRequestId,
                    TargetPosition);
                AssertEx.False(parsed.Response.IsSuccess);
                AssertEx.Equal(detail, parsed.Response.DetailCode);
                AssertEx.Equal((short)-31000, parsed.Response.ErrorId);
                AssertEx.Equal(0, parsed.AppliedPosition);
                AssertEx.Equal(0u, parsed.NativeCommandState);
            }

            var nativeFailure = SetPositionFailurePayload(
                GoldenRequestId,
                LMCAdminDetailCode.NativeCommandRejected,
                -6,
                0x00000008u);
            var nativeParsed = LMC_AdminParser.ParseSetAxisPosition(
                TestFrame.Response(0, nativeFailure),
                GoldenRequestId,
                TargetPosition);
            AssertEx.False(nativeParsed.Response.IsSuccess);
            AssertEx.Equal((short)-6, nativeParsed.Response.ErrorId);
            AssertEx.Equal(0x00000008u, nativeParsed.NativeCommandState);
        }

        private static void StoreAdmissionFailuresStrict()
        {
            foreach (var detail in new[]
            {
                LMCAdminDetailCode.SetPositionOutcomeIndeterminate,
                LMCAdminDetailCode.SetPositionOutcomeStoreCorrupt
            })
            {
                var parsed = LMC_AdminParser.ParseSetAxisPosition(
                    TestFrame.Response(
                        0,
                        SetPositionFailurePayload(
                            GoldenRequestId,
                            detail)),
                    GoldenRequestId,
                    TargetPosition);
                AssertEx.False(parsed.Response.IsSuccess);
                AssertEx.Equal(detail, parsed.Response.DetailCode);
                AssertEx.Equal((short)-31000, parsed.Response.ErrorId);
                AssertEx.Equal(0, parsed.AppliedPosition);
                AssertEx.Equal(0u, parsed.NativeCommandState);
            }

            foreach (var queryOnlyDetail in new[]
            {
                LMCAdminDetailCode.SetPositionOutcomeNotFound,
                LMCAdminDetailCode.SetPositionOutcomeKeyMismatch
            })
            {
                AssertMalformed(
                    SetPositionFailurePayload(
                        GoldenRequestId,
                        queryOnlyDetail));
            }
        }

        private static void CapabilityBitStrict()
        {
            var capabilities = LMC_AdminParser.ParseCapabilities(
                TestFrame.Response(
                    0,
                    CapabilitiesPayload(
                        GoldenRequestId,
                        LMCAdminFeature.AxisSetPosition
                            | LMCAdminFeature.AxisSetPositionOutcomeRead
                            | LMCAdminFeature
                                .AxisSetPositionOutcomeRetirement,
                        4)),
                GoldenRequestId,
                1);
            AssertEx.True(
                capabilities.Supports(LMCAdminFeature.AxisSetPosition));
            AssertEx.True(
                capabilities.Supports(
                    LMCAdminFeature.AxisSetPositionOutcomeRead));
            AssertEx.True(
                capabilities.Supports(
                    LMCAdminFeature.AxisSetPositionOutcomeRetirement));

            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseCapabilities(
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            GoldenRequestId,
                            LMCAdminFeature.AxisSetPosition,
                            4)),
                    GoldenRequestId,
                    1));

            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseCapabilities(
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            GoldenRequestId,
                            LMCAdminFeature.AxisSetPosition
                                | LMCAdminFeature
                                    .AxisSetPositionOutcomeRead,
                            4)),
                    GoldenRequestId,
                    1));

            var outcomeOnly = LMC_AdminParser.ParseCapabilities(
                TestFrame.Response(
                    0,
                    CapabilitiesPayload(
                        GoldenRequestId,
                        LMCAdminFeature.AxisSetPositionOutcomeRead,
                        4)),
                GoldenRequestId,
                1);
            AssertEx.True(
                outcomeOnly.Supports(
                    LMCAdminFeature.AxisSetPositionOutcomeRead));

            var outcomeLifecycle = LMC_AdminParser.ParseCapabilities(
                TestFrame.Response(
                    0,
                    CapabilitiesPayload(
                        GoldenRequestId,
                        LMCAdminFeature.AxisSetPositionOutcomeRead
                            | LMCAdminFeature
                                .AxisSetPositionOutcomeRetirement,
                        4)),
                GoldenRequestId,
                1);
            AssertEx.True(
                outcomeLifecycle.Supports(
                    LMCAdminFeature.AxisSetPositionOutcomeRetirement));

            var retirementWithOldCatalog = CapabilitiesPayload(
                GoldenRequestId,
                LMCAdminFeature.AxisSetPositionOutcomeRead
                    | LMCAdminFeature
                        .AxisSetPositionOutcomeRetirement,
                4);
            TestFrame.WriteUInt16(retirementWithOldCatalog, 36, 1);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseCapabilities(
                    TestFrame.Response(0, retirementWithOldCatalog),
                    GoldenRequestId,
                    1));

            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseCapabilities(
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            GoldenRequestId,
                            LMCAdminFeature
                                .AxisSetPositionOutcomeRetirement,
                            4)),
                    GoldenRequestId,
                    1));

            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseCapabilities(
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            GoldenRequestId,
                            LMCAdminFeature.AxisSetPositionOutcomeRead,
                            0)),
                    GoldenRequestId,
                    1));

            var unknown = CapabilitiesPayload(
                GoldenRequestId,
                LMCAdminFeature.AxisSetPosition
                    | LMCAdminFeature.AxisSetPositionOutcomeRead
                    | LMCAdminFeature.AxisSetPositionOutcomeRetirement,
                4);
            TestFrame.WriteUInt32(unknown, 16, 1u << 31);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseCapabilities(
                    TestFrame.Response(0, unknown),
                    GoldenRequestId,
                    1));
        }

        private static void SyncPreparedFacade()
        {
            var setStep = SetPositionStep(2, TargetPosition);
            setStep.InspectRequest = request =>
            {
                AssertEx.Equal((ushort)48, TestFrame.ReadUInt16(request, 4));
                AssertEx.Equal((ushort)2, TestFrame.ReadUInt16(request, 6));
                AssertEx.Equal((uint)2, TestFrame.ReadUInt32(request, 12));
                AssertEx.Equal(
                    DiagnosticsBuild,
                    TestFrame.ReadUInt32(request, 16));
                AssertEx.Equal(
                    DiagnosticsBootId,
                    TestFrame.ReadUInt32(request, 20));
                AssertEx.Equal(
                    MapRevision,
                    TestFrame.ReadUInt32(request, 24));
                AssertEx.True(
                    TestFrame.ReadUInt32(request, 28) != 0
                        || TestFrame.ReadUInt32(request, 32) != 0
                        || TestFrame.ReadUInt32(request, 36) != 0
                        || TestFrame.ReadUInt32(request, 40) != 0);
                AssertEx.Equal(TargetPosition, TestFrame.ReadInt32(request, 44));
                AssertEx.Equal(
                    ExpectedActualPosition,
                    TestFrame.ReadInt32(request, 48));
                AssertEx.Equal(
                    0x50544553u,
                    TestFrame.ReadUInt32(request, 52));
            };
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(2),
                AxisInfoStep(2),
                CapabilitiesStep(1, LMCAdminFeature.AxisSetPosition),
                CapabilitiesStep(3, LMCAdminFeature.AxisSetPosition),
                setStep,
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis2");
                var capabilities = connection.Admin.GetCapabilities();
                var diagnosticCapabilities =
                    CurrentDiagnosticsCapabilities(connection);
                var confirmation = LMCAxisSetPositionExecuteToken.Create();
                var prepared = axis.PrepareSetPosition(
                    TargetPosition,
                    ExpectedActualPosition,
                    capabilities,
                    diagnosticCapabilities,
                    confirmation);

                AssertEx.True(confirmation.IsConsumed);
                AssertEx.Equal((uint)2, prepared.RequestId);
                var laterCapabilities = connection.Admin.GetCapabilities();
                AssertEx.Equal(
                    (uint)3,
                    laterCapabilities.Response.RequestId);
                var result = axis.SetPositionEx(prepared);
                AssertEx.True(result.IsSuccess);
                AssertEx.True(prepared.IsConsumed);
                AssertEx.Equal((ushort)2, result.AxisReference);
                AssertEx.Equal(TargetPosition, result.TargetPosition);
                AssertEx.Equal(
                    ExpectedActualPosition,
                    result.ExpectedActualPosition);
                AssertEx.Equal(TargetPosition, result.AppliedPosition);
                AssertEx.Equal(0u, result.NativeCommandState);
                AssertEx.Equal(prepared.RequestId, result.RequestId);
                AssertEx.Equal((uint)2, result.Response.RequestId);

                AssertEx.Throws<InvalidOperationException>(
                    () => connection.Admin.SetAxisPosition(prepared));
                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x7D12));
            }
        }

        private static void AsyncPreparedFacade()
        {
            var setStep = SetPositionStep(2, 456);
            setStep.ResponseChunks = new[] { 1, 3, 5, 7, 11 };
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                CapabilitiesStep(1, LMCAdminFeature.AxisSetPosition),
                setStep,
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var capabilities = connection.Admin
                    .GetCapabilitiesAsync(CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var diagnosticCapabilities =
                    CurrentDiagnosticsCapabilities(connection);
                var prepared = axis.PrepareSetPosition(
                    456,
                    450,
                    capabilities,
                    diagnosticCapabilities,
                    LMCAxisSetPositionExecuteToken.Create());

                var result = axis.SetPositionExAsync(
                        prepared,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(456, result.AppliedPosition);
                AssertEx.True(prepared.IsConsumed);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void CapabilityGateZeroWire()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                CapabilitiesStep(1, LMCAdminFeature.AxisParameterRead),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var capabilities = connection.Admin.GetCapabilities();
                var confirmation = LMCAxisSetPositionExecuteToken.Create();

                AssertEx.Throws<NotSupportedException>(
                    () => connection.Admin.PrepareAxisSetPosition(
                        axis,
                        10,
                        10,
                        capabilities,
                        null,
                        confirmation));
                AssertEx.False(confirmation.IsConsumed);
                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(0, CountCommand(server, 0x7D12));
            }
        }

        private static void ConfirmationTokenOneIntent()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                CapabilitiesStep(1, LMCAdminFeature.AxisSetPosition),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var capabilities = connection.Admin.GetCapabilities();
                var diagnosticCapabilities =
                    CurrentDiagnosticsCapabilities(connection);
                var confirmation = LMCAxisSetPositionExecuteToken.Create();
                var start = new ManualResetEventSlim(false);
                var first = Task.Run(
                    () => CapturePreparation(
                        start,
                        connection,
                        axis,
                        capabilities,
                        diagnosticCapabilities,
                        confirmation,
                        100));
                var second = Task.Run(
                    () => CapturePreparation(
                        start,
                        connection,
                        axis,
                        capabilities,
                        diagnosticCapabilities,
                        confirmation,
                        200));
                start.Set();
                var firstError = first.GetAwaiter().GetResult();
                var secondError = second.GetAwaiter().GetResult();

                AssertEx.True(confirmation.IsConsumed);
                AssertEx.Equal(
                    1,
                    (firstError == null ? 1 : 0)
                        + (secondError == null ? 1 : 0));
                AssertEx.Equal(
                    1,
                    (firstError is InvalidOperationException ? 1 : 0)
                        + (secondError is InvalidOperationException ? 1 : 0));
                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(0, CountCommand(server, 0x7D12));
            }
        }

        private static void FreshCapabilitySnapshotsRequired()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                CapabilitiesStep(1, LMCAdminFeature.AxisSetPosition),
                CapabilitiesStep(2, LMCAdminFeature.AxisSetPosition),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var staleAdminCapabilities =
                    connection.Admin.GetCapabilities();
                var staleDiagnosticCapabilities =
                    CurrentDiagnosticsCapabilities(connection);
                var currentAdminCapabilities =
                    connection.Admin.GetCapabilities();
                var currentDiagnosticCapabilities =
                    CurrentDiagnosticsCapabilities(connection);
                var staleAdminToken =
                    LMCAxisSetPositionExecuteToken.Create();
                var staleDiagnosticsToken =
                    LMCAxisSetPositionExecuteToken.Create();

                AssertEx.Throws<InvalidOperationException>(
                    () => axis.PrepareSetPosition(
                        1,
                        1,
                        staleAdminCapabilities,
                        currentDiagnosticCapabilities,
                        staleAdminToken));
                AssertEx.False(staleAdminToken.IsConsumed);
                AssertEx.Throws<InvalidOperationException>(
                    () => axis.PrepareSetPosition(
                        1,
                        1,
                        currentAdminCapabilities,
                        staleDiagnosticCapabilities,
                        staleDiagnosticsToken));
                AssertEx.False(staleDiagnosticsToken.IsConsumed);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(0, CountCommand(server, 0x7D12));
            }
        }

        private static void PreparedOwnerAndSessionPinned()
        {
            PreparedOwnerPinned();
            PreparedSessionPinned();
        }

        private static void PreparedOwnerPinned()
        {
            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                CapabilitiesStep(1, LMCAdminFeature.AxisSetPosition),
                CloseStep()))
            using (var secondServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var firstConnection = new LMCConnection())
            using (var secondConnection = new LMCConnection())
            {
                Connect(firstConnection, firstServer.Port);
                var axis = new LMCSingleAxis(firstConnection, "_LMCAxis1");
                var capabilities = firstConnection.Admin.GetCapabilities();
                var diagnosticCapabilities =
                    CurrentDiagnosticsCapabilities(firstConnection);
                var prepared = axis.PrepareSetPosition(
                    1,
                    1,
                    capabilities,
                    diagnosticCapabilities,
                    LMCAxisSetPositionExecuteToken.Create());

                Connect(secondConnection, secondServer.Port);
                AssertEx.Equal(
                    firstConnection.SessionGeneration,
                    secondConnection.SessionGeneration);
                AssertEx.Throws<InvalidOperationException>(
                    () => secondConnection.Admin.SetAxisPosition(prepared));
                AssertEx.False(prepared.IsConsumed);

                firstConnection.CloseConnection();
                secondConnection.CloseConnection();
                firstServer.Verify();
                secondServer.Verify();
            }
        }

        private static void PreparedSessionPinned()
        {
            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                CapabilitiesStep(1, LMCAdminFeature.AxisSetPosition),
                CloseStep()))
            using (var secondServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, firstServer.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var capabilities = connection.Admin.GetCapabilities();
                var diagnosticCapabilities =
                    CurrentDiagnosticsCapabilities(connection);
                var prepared = axis.PrepareSetPosition(
                    2,
                    2,
                    capabilities,
                    diagnosticCapabilities,
                    LMCAxisSetPositionExecuteToken.Create());

                Connect(connection, secondServer.Port);
                AssertEx.Throws<InvalidOperationException>(
                    () => connection.Admin.SetAxisPosition(prepared));
                AssertEx.False(prepared.IsConsumed);

                connection.CloseConnection();
                firstServer.Verify();
                secondServer.Verify();
            }
        }

        private static void ConcurrentPreparedSingleDispatch()
        {
            var setStep = SetPositionStep(2, 333);
            setStep.ResponseDelayMilliseconds = 100;
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                CapabilitiesStep(1, LMCAdminFeature.AxisSetPosition),
                setStep,
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var capabilities = connection.Admin.GetCapabilities();
                var diagnosticCapabilities =
                    CurrentDiagnosticsCapabilities(connection);
                var prepared = axis.PrepareSetPosition(
                    333,
                    330,
                    capabilities,
                    diagnosticCapabilities,
                    LMCAxisSetPositionExecuteToken.Create());
                var start = new ManualResetEventSlim(false);
                var first = Task.Run(
                    () => CaptureExecution(start, connection, prepared));
                var second = Task.Run(
                    () => CaptureExecution(start, connection, prepared));
                start.Set();
                var firstError = first.GetAwaiter().GetResult();
                var secondError = second.GetAwaiter().GetResult();

                AssertEx.Equal(
                    1,
                    (firstError == null ? 1 : 0)
                        + (secondError == null ? 1 : 0));
                AssertEx.Equal(
                    1,
                    (firstError is InvalidOperationException ? 1 : 0)
                        + (secondError is InvalidOperationException ? 1 : 0));
                AssertEx.True(prepared.IsConsumed);
                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x7D12));
            }
        }

        private static void PreCanceledZeroWireReusable()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                CapabilitiesStep(1, LMCAdminFeature.AxisSetPosition),
                SetPositionStep(2, 44),
                CloseStep()))
            using (var connection = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var capabilities = connection.Admin.GetCapabilities();
                var diagnosticCapabilities =
                    CurrentDiagnosticsCapabilities(connection);
                var prepared = axis.PrepareSetPosition(
                    44,
                    40,
                    capabilities,
                    diagnosticCapabilities,
                    LMCAxisSetPositionExecuteToken.Create());
                cancellation.Cancel();

                AssertEx.Throws<OperationCanceledException>(
                    () => connection.Admin.SetAxisPositionAsync(
                            prepared,
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.False(prepared.IsConsumed);
                var result = connection.Admin.SetAxisPosition(prepared);
                AssertEx.True(result.IsSuccess);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x7D12));
            }
        }

        private static void ResponseLossUncertainNoReplay()
        {
            var lostResponse = new FakeRpcStep(0x7D12, new byte[0])
            {
                CloseClientBeforeResponse = true
            };
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                CapabilitiesStep(1, LMCAdminFeature.AxisSetPosition),
                lostResponse))
            using (var connection = new LMCConnection())
            {
                var faultTransitions = 0;
                connection.ConnectionStateChanged += (sender, args) =>
                {
                    if (args.CurrentState == LMCConnectionState.Faulted)
                    {
                        Interlocked.Increment(ref faultTransitions);
                    }
                };
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var capabilities = connection.Admin.GetCapabilities();
                var diagnosticCapabilities =
                    CurrentDiagnosticsCapabilities(connection);
                var prepared = axis.PrepareSetPosition(
                    77,
                    70,
                    capabilities,
                    diagnosticCapabilities,
                    LMCAxisSetPositionExecuteToken.Create());

                var exception = AssertEx.Throws<
                    LMCAxisSetPositionOutcomeUncertainException>(
                    () => connection.Admin.SetAxisPosition(prepared));
                AssertEx.True(prepared.IsConsumed);
                AssertEx.Equal((uint)2, exception.RequestId);
                AssertEx.Equal((ushort)1, exception.AxisReference);
                AssertEx.Equal(77, exception.TargetPosition);
                AssertEx.Equal(70, exception.ExpectedActualPosition);
                AssertEx.NotNull(exception.InnerException);
                AssertEx.Equal(
                    LMCConnectionState.Faulted,
                    connection.State);
                AssertEx.Equal(1, Volatile.Read(ref faultTransitions));
                AssertEx.Throws<InvalidOperationException>(
                    () => connection.Admin.SetAxisPosition(prepared));

                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x7D12));
            }
        }

        private static void NativeRejectPreservesBitfield()
        {
            const uint nativeCommandState = 0xA5000008u;
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                CapabilitiesStep(1, LMCAdminFeature.AxisSetPosition),
                new FakeRpcStep(
                    0x7D12,
                    TestFrame.Response(
                        0,
                        SetPositionFailurePayload(
                            2,
                            LMCAdminDetailCode.NativeCommandRejected,
                            -6,
                            nativeCommandState))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var capabilities = connection.Admin.GetCapabilities();
                var diagnosticCapabilities =
                    CurrentDiagnosticsCapabilities(connection);
                var prepared = axis.PrepareSetPosition(
                    81,
                    80,
                    capabilities,
                    diagnosticCapabilities,
                    LMCAxisSetPositionExecuteToken.Create());

                var exception = AssertEx.Throws<
                    LMCAxisSetPositionRejectedException>(
                    () => connection.Admin.SetAxisPosition(prepared));
                AssertEx.Equal(prepared, exception.PreparedCommand);
                AssertEx.Equal(prepared.RequestId, exception.RequestId);
                AssertEx.Equal((short)-6, exception.Response.ErrorId);
                AssertEx.Equal(
                    LMCAdminDetailCode.NativeCommandRejected,
                    exception.Response.DetailCode);
                AssertEx.Equal(0, exception.AppliedPosition);
                AssertEx.Equal(
                    LMCAxisSetPositionSemanticMode
                        .ActualAndDestinationApplicationUnits,
                    exception.SemanticMode);
                AssertEx.Equal(
                    nativeCommandState,
                    exception.NativeCommandState);
                AssertEx.Equal(
                    nativeCommandState,
                    exception.Result.NativeCommandState);
                AssertEx.True(prepared.IsConsumed);
                AssertEx.Equal(
                    LMCConnectionState.Connected,
                    connection.State);
                AssertEx.Throws<InvalidOperationException>(
                    () => connection.Admin.SetAxisPosition(prepared));

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x7D12));
            }
        }

        private static void MalformedPostWriteFaultsSession()
        {
            using (var setRequestReceived = new ManualResetEventSlim(false))
            using (var queuedAttemptStarted =
                new ManualResetEventSlim(false))
            {
                var contradictoryFailure = SetPositionFailurePayload(
                    2,
                    LMCAdminDetailCode.NativeCommandRejected,
                    23,
                    8);
                var malformedStep = new FakeRpcStep(
                    0x7D12,
                    TestFrame.Response(0, contradictoryFailure))
                {
                    InspectRequest = request =>
                    {
                        setRequestReceived.Set();
                        AssertEx.True(
                            queuedAttemptStarted.Wait(2000),
                            "Queued RPC attempt did not start before the malformed response.");
                    },
                    ResponseDelayMilliseconds = 100
                };
                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    AxisLookupStep(1),
                    AxisInfoStep(1),
                    CapabilitiesStep(
                        1,
                        LMCAdminFeature.AxisSetPosition),
                    malformedStep,
                    ExpectedClientDisconnectStep()))
                using (var connection = new LMCConnection())
                {
                    Connect(connection, server.Port);
                    var axis = new LMCSingleAxis(
                        connection,
                        "_LMCAxis1");
                    var capabilities = connection.Admin.GetCapabilities();
                    var diagnosticCapabilities =
                        CurrentDiagnosticsCapabilities(connection);
                    var prepared = axis.PrepareSetPosition(
                        91,
                        90,
                        capabilities,
                        diagnosticCapabilities,
                        LMCAxisSetPositionExecuteToken.Create());

                    var setTask = Task.Run(
                        () => CaptureSetPosition(
                            connection,
                            prepared));
                    AssertEx.True(
                        setRequestReceived.Wait(2000),
                        "SetAxisPosition request was not observed.");
                    var queuedTask = Task.Run(
                        () =>
                        {
                            queuedAttemptStarted.Set();
                            return CaptureCapabilities(connection);
                        });

                    var setError = setTask.GetAwaiter().GetResult();
                    var queuedError = queuedTask.GetAwaiter().GetResult();
                    var uncertain = setError as
                        LMCAxisSetPositionOutcomeUncertainException;
                    AssertEx.NotNull(uncertain);
                    AssertEx.True(
                        uncertain.InnerException
                            is InvalidDataException);
                    AssertEx.True(
                        queuedError is InvalidOperationException);
                    AssertEx.True(prepared.IsConsumed);
                    AssertEx.Equal(
                        LMCConnectionState.Faulted,
                        connection.State);
                    AssertEx.Throws<InvalidOperationException>(
                        () => connection.Admin.SetAxisPosition(prepared));

                    server.Verify();
                    AssertEx.Equal(1, CountCommand(server, 0x7D12));
                    AssertEx.Equal(1, CountCommand(server, 0x7D00));
                }
            }
        }

        private static void PublishFailureFaultsSession()
        {
            using (var setRequestReceived =
                new ManualResetEventSlim(false))
            using (var queuedAttemptStarted =
                new ManualResetEventSlim(false))
            {
                var coordinator = new LMCSendPriorityCoordinator();
                var expectedPriorityGeneration =
                    coordinator.CurrentGeneration;
                var setStep = SetPositionStep(2, 101);
                setStep.ResponseDelayMilliseconds = 100;
                setStep.InspectRequest = request =>
                {
                    coordinator.ReservePrioritySend();
                    setRequestReceived.Set();
                    AssertEx.True(
                        queuedAttemptStarted.Wait(2000),
                        "Queued RPC attempt did not start before result publication.");
                };
                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    AxisLookupStep(1),
                    AxisInfoStep(1),
                    CapabilitiesStep(
                        1,
                        LMCAdminFeature.AxisSetPosition),
                    setStep,
                    ExpectedClientDisconnectStep()))
                using (var connection = new LMCConnection(
                    new LMCConnectionOptions
                    {
                        SendPriorityCoordinator = coordinator
                    }))
                {
                    Connect(connection, server.Port);
                    var axis = new LMCSingleAxis(
                        connection,
                        "_LMCAxis1");
                    var capabilities = connection.Admin.GetCapabilities();
                    var diagnosticCapabilities =
                        CurrentDiagnosticsCapabilities(connection);
                    var prepared = axis.PrepareSetPosition(
                        101,
                        100,
                        capabilities,
                        diagnosticCapabilities,
                        LMCAxisSetPositionExecuteToken.Create());

                    var setTask = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPreemptibleScope(
                                expectedPriorityGeneration,
                                "SetAxisPosition publish failure test"))
                            {
                                return CaptureSetPosition(
                                    connection,
                                    prepared);
                            }
                        });
                    AssertEx.True(
                        setRequestReceived.Wait(2000),
                        "SetAxisPosition request was not observed.");
                    var queuedTask = Task.Run(
                        () =>
                        {
                            queuedAttemptStarted.Set();
                            return CaptureCapabilities(connection);
                        });

                    var setError = setTask.GetAwaiter().GetResult();
                    var queuedError = queuedTask.GetAwaiter().GetResult();
                    var exception = setError as
                        LMCAxisSetPositionOutcomeUncertainException;
                    AssertEx.NotNull(exception);
                    AssertEx.True(
                        exception.InnerException
                            is LMCSendPreemptedException);
                    AssertEx.True(
                        queuedError is InvalidOperationException);
                    AssertEx.True(prepared.IsConsumed);
                    AssertEx.Equal(
                        LMCConnectionState.Faulted,
                        connection.State);
                    AssertEx.Throws<InvalidOperationException>(
                        () => connection.Admin.SetAxisPosition(prepared));

                    server.Verify();
                    AssertEx.Equal(1, CountCommand(server, 0x7D12));
                    AssertEx.Equal(1, CountCommand(server, 0x7D00));
                }
            }
        }

        private static void UncertainInvalidationPinsSession()
        {
            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var secondServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, firstServer.Port);
                var oldSessionGeneration =
                    connection.SessionGeneration;

                Connect(connection, secondServer.Port);
                AssertEx.True(
                    connection.SessionGeneration
                        != oldSessionGeneration);
                AssertEx.False(
                    connection.TryInvalidateSessionAfterUncertainMutation(
                        oldSessionGeneration,
                        new InvalidDataException(
                            "stale old-session response")));
                AssertEx.Equal(
                    LMCConnectionState.Connected,
                    connection.State);

                connection.CloseConnection();
                firstServer.Verify();
                secondServer.Verify();
            }
        }

        private static void DefinitiveRejectNoReplay()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                CapabilitiesStep(1, LMCAdminFeature.AxisSetPosition),
                new FakeRpcStep(
                    0x7D12,
                    TestFrame.Response(
                        0,
                        SetPositionFailurePayload(
                            2,
                            LMCAdminDetailCode
                                .CoordinatePreconditionFailed))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var capabilities = connection.Admin.GetCapabilities();
                var diagnosticCapabilities =
                    CurrentDiagnosticsCapabilities(connection);
                var prepared = axis.PrepareSetPosition(
                    88,
                    80,
                    capabilities,
                    diagnosticCapabilities,
                    LMCAxisSetPositionExecuteToken.Create());

                var exception = AssertEx.Throws<
                    LMCAxisSetPositionRejectedException>(
                    () => connection.Admin.SetAxisPosition(prepared));
                AssertEx.Equal(
                    LMCAdminDetailCode.CoordinatePreconditionFailed,
                    exception.Response.DetailCode);
                AssertEx.True(prepared.IsConsumed);
                AssertEx.Equal(0u, exception.NativeCommandState);
                AssertEx.Equal(
                    LMCConnectionState.Connected,
                    connection.State);
                AssertEx.Throws<InvalidOperationException>(
                    () => connection.Admin.SetAxisPosition(prepared));

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x7D12));
            }
        }

        private static Exception CapturePreparation(
            ManualResetEventSlim start,
            LMCConnection connection,
            LMCSingleAxis axis,
            LMCAdminCapabilities capabilities,
            LMCDiagnosticCapabilities diagnosticCapabilities,
            LMCAxisSetPositionExecuteToken confirmation,
            int targetPosition)
        {
            start.Wait();
            try
            {
                connection.Admin.PrepareAxisSetPosition(
                    axis,
                    targetPosition,
                    0,
                    capabilities,
                    diagnosticCapabilities,
                    confirmation);
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private static Exception CaptureExecution(
            ManualResetEventSlim start,
            LMCConnection connection,
            LMCPreparedAxisSetPosition prepared)
        {
            start.Wait();
            try
            {
                connection.Admin.SetAxisPosition(prepared);
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private static Exception CaptureSetPosition(
            LMCConnection connection,
            LMCPreparedAxisSetPosition prepared)
        {
            try
            {
                connection.Admin.SetAxisPosition(prepared);
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private static Exception CaptureCapabilities(
            LMCConnection connection)
        {
            try
            {
                connection.Admin.GetCapabilities();
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private static void AssertMalformed(byte[] payload)
        {
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseSetAxisPosition(
                    TestFrame.Response(0, payload),
                    GoldenRequestId,
                    TargetPosition));
        }

        private static void AssertPublicPropertyIsReadOnly(
            Type type,
            string propertyName)
        {
            var property = type.GetProperty(propertyName);
            AssertEx.NotNull(property);
            AssertEx.True(
                property.SetMethod == null || !property.SetMethod.IsPublic,
                type.Name + "." + propertyName + " must be immutable.");
        }

        private static LMCAxisSetPositionRecoveryKey RecoveryKey(
            uint requestId = GoldenRequestId,
            ushort axisReference = 2,
            int targetPosition = TargetPosition,
            int expectedActualPosition = ExpectedActualPosition)
        {
            return new LMCAxisSetPositionRecoveryKey(
                LMCAdmin.ProtocolSchemaVersion,
                requestId,
                DiagnosticsBuild,
                DiagnosticsBootId,
                MapRevision,
                ClientIntentId0,
                ClientIntentId1,
                ClientIntentId2,
                ClientIntentId3,
                axisReference,
                targetPosition,
                expectedActualPosition,
                LMCAxisSetPositionSemanticMode
                    .ActualAndDestinationApplicationUnits);
        }

        private static LMCDiagnosticCapabilities
            CurrentDiagnosticsCapabilities(LMCConnection connection)
        {
            const uint requestId = 0xA1B2C3D4u;
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
            var observationSequence = (long)nextObservation.Invoke(
                connection.Diagnostics,
                null);
            return parsed.BindProvenance(
                connection.Diagnostics,
                connection.SessionGeneration,
                observationSequence);
        }

        private static byte[] SetPositionPayload(
            uint requestId,
            int appliedPosition,
            uint nativeCommandState)
        {
            var payload = CommonPayload(requestId, 28);
            TestFrame.WriteInt32(payload, 16, appliedPosition);
            TestFrame.WriteUInt16(
                payload,
                20,
                (ushort)LMCAxisSetPositionSemanticMode
                    .ActualAndDestinationApplicationUnits);
            TestFrame.WriteUInt32(payload, 24, nativeCommandState);
            return payload;
        }

        private static byte[] SetPositionFailurePayload(
            uint requestId,
            LMCAdminDetailCode detail,
            short errorId = -31000,
            uint nativeCommandState = 0)
        {
            var payload = SetPositionPayload(
                requestId,
                0,
                nativeCommandState);
            TestFrame.WriteUInt16(payload, 4, 1);
            TestFrame.WriteInt16(payload, 6, errorId);
            TestFrame.WriteUInt32(payload, 12, (uint)detail);
            return payload;
        }

        private static byte[] CapabilitiesPayload(
            uint requestId,
            LMCAdminFeature features,
            ushort physicalAxisCount)
        {
            var payload = CommonPayload(requestId, 40);
            TestFrame.WriteUInt32(payload, 16, (uint)features);
            TestFrame.WriteUInt32(payload, 20, 0x3F);
            TestFrame.WriteUInt32(
                payload,
                24,
                (uint)LMCGroupParameterSelection.All);
            TestFrame.WriteUInt16(payload, 28, physicalAxisCount);
            TestFrame.WriteUInt16(payload, 30, 1);
            TestFrame.WriteUInt16(payload, 32, 0x0100);
            TestFrame.WriteUInt16(payload, 34, 3);
            var durableSetPositionFeatures =
                LMCAdminFeature.AxisSetPosition
                | LMCAdminFeature.AxisSetPositionOutcomeRead
                | LMCAdminFeature.AxisSetPositionOutcomeRetirement;
            TestFrame.WriteUInt16(
                payload,
                36,
                (ushort)((features & durableSetPositionFeatures) != 0
                    ? 2
                    : 1));
            return payload;
        }

        private static byte[] CommonPayload(uint requestId, int length)
        {
            var payload = new byte[length];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, requestId);
            return payload;
        }

        private static FakeRpcStep CapabilitiesStep(
            uint requestId,
            LMCAdminFeature features)
        {
            if ((features & LMCAdminFeature.AxisSetPosition) != 0)
            {
                features |= LMCAdminFeature.AxisSetPositionOutcomeRead
                    | LMCAdminFeature.AxisSetPositionOutcomeRetirement;
            }

            return new FakeRpcStep(
                0x7D00,
                TestFrame.Response(
                    0,
                    CapabilitiesPayload(requestId, features, 4)));
        }

        private static FakeRpcStep SetPositionStep(
            uint requestId,
            int appliedPosition)
        {
            return new FakeRpcStep(
                0x7D12,
                TestFrame.Response(
                    0,
                    SetPositionPayload(requestId, appliedPosition, 0)));
        }

        private static FakeRpcStep ExpectedClientDisconnectStep()
        {
            return new FakeRpcStep(0, new byte[0])
            {
                RequireClientDisconnectBeforeRequest = true
            };
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

using System;
using System.Collections.Generic;
using System.Threading;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class DiagnosticsSdoWritePolicyEvaluationTests
    {
        private const uint MapRevision = 0x957F101Eu;
        private const uint DiagnosticsBootId = 0x89ABCDEFu;
        private const LMCDiagnosticCapability RequiredCapabilities =
            LMCDiagnosticCapability.SDORead
            | LMCDiagnosticCapability.SDOWrite
            | LMCDiagnosticCapability.SDOReadGeneralInline;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Policy.DiagnosticsD5.SdoWritePublicPolicyClosedAndImmutable",
                PublicPolicyClosedAndImmutable);
            tests.Add(
                "Rpc.DiagnosticsD5.SdoWritePolicyEvaluationIsZeroWire",
                PublicEvaluationIsZeroWire);
            tests.Add(
                "Policy.DiagnosticsD5.SdoWriteOlderSameSessionObservationIsNotCurrent",
                OlderSameSessionObservationIsNotCurrent);
            tests.Add(
                "Policy.DiagnosticsD5.SdoWriteOldSessionObservationIsNotCurrentAfterReconnect",
                OldSessionObservationIsNotCurrentAfterReconnect);
            tests.Add(
                "Policy.DiagnosticsD5.SdoWriteInjectedReadyAndBlockers",
                InjectedReadyAndBlockerMatrix);
            tests.Add(
                "Rpc.DiagnosticsD5.SdoWriteEmptyAllowlistSubmitIsZeroWire",
                EmptyAllowlistSubmitSyncAndAsyncIsZeroWire);
        }

        private static void PublicPolicyClosedAndImmutable()
        {
            using (var connection = new LMCConnection())
            {
                var evaluation = connection.Diagnostics
                    .EvaluateSdoWritePolicy(null);

                AssertEx.False(evaluation.CanAttemptSubmission);
                AssertHasBlocker(
                    evaluation,
                    LMCSdoWritePolicyBlockers.NoApprovedTarget);
                AssertHasBlocker(
                    evaluation,
                    LMCSdoWritePolicyBlockers.ConnectionUnavailable);
                AssertHasBlocker(
                    evaluation,
                    LMCSdoWritePolicyBlockers
                        .CapabilityObservationUnavailable);
                AssertEx.Equal(0, evaluation.ApprovedTargets.Count);

                var mutableView = evaluation.ApprovedTargets
                    as IList<LMCSdoWriteTarget>;
                AssertEx.NotNull(
                    mutableView,
                    "The immutable policy target snapshot must expose a read-only IList view.");
                AssertEx.Throws<NotSupportedException>(
                    () => mutableView.Add(CreateUi24Target(1)));
            }
        }

        private static void PublicEvaluationIsZeroWire()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CapabilitiesStep(1, RequiredCapabilities),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var capabilities = connection.Diagnostics.GetCapabilities();
                var requestCountBeforeEvaluation =
                    server.ReceivedRequests.Count;

                var evaluation = connection.Diagnostics
                    .EvaluateSdoWritePolicy(capabilities);

                AssertEx.Equal(
                    requestCountBeforeEvaluation,
                    server.ReceivedRequests.Count,
                    "Evaluating cached SDO Write policy sent an RPC request.");
                AssertEx.Equal(
                    LMCSdoWritePolicyBlockers.NoApprovedTarget,
                    evaluation.Blockers);
                AssertEx.False(evaluation.CanAttemptSubmission);
                AssertEx.Equal(0, evaluation.ApprovedTargets.Count);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void OlderSameSessionObservationIsNotCurrent()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CapabilitiesStep(1, RequiredCapabilities),
                CapabilitiesStep(2, RequiredCapabilities),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var olderCapabilities =
                    connection.Diagnostics.GetCapabilities();
                var currentCapabilities =
                    connection.Diagnostics.GetCapabilities();
                var requestCountBeforeEvaluation =
                    server.ReceivedRequests.Count;

                AssertExactBlocker(
                    LMCSdoWritePolicyBlockers.NoApprovedTarget
                        | LMCSdoWritePolicyBlockers
                            .CapabilityObservationNotCurrent,
                    connection.Diagnostics.EvaluateSdoWritePolicy(
                        olderCapabilities));
                AssertExactBlocker(
                    LMCSdoWritePolicyBlockers.NoApprovedTarget,
                    connection.Diagnostics.EvaluateSdoWritePolicy(
                        currentCapabilities));
                AssertEx.Equal(
                    requestCountBeforeEvaluation,
                    server.ReceivedRequests.Count,
                    "Evaluating same-session capability observations sent an RPC request.");

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void OldSessionObservationIsNotCurrentAfterReconnect()
        {
            using (var connection = new LMCConnection())
            {
                LMCDiagnosticCapabilities oldSessionCapabilities;
                using (var firstServer = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    CapabilitiesStep(1, RequiredCapabilities),
                    CloseStep()))
                {
                    Connect(connection, firstServer.Port);
                    oldSessionCapabilities =
                        connection.Diagnostics.GetCapabilities();
                    connection.CloseConnection();
                    firstServer.Verify();
                }

                using (var secondServer = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    CapabilitiesStep(2, RequiredCapabilities),
                    CloseStep()))
                {
                    Connect(connection, secondServer.Port);
                    var currentSessionCapabilities =
                        connection.Diagnostics.GetCapabilities();
                    var requestCountBeforeEvaluation =
                        secondServer.ReceivedRequests.Count;

                    AssertExactBlocker(
                        LMCSdoWritePolicyBlockers.NoApprovedTarget
                            | LMCSdoWritePolicyBlockers
                                .CapabilityObservationNotCurrent,
                        connection.Diagnostics.EvaluateSdoWritePolicy(
                            oldSessionCapabilities));
                    AssertExactBlocker(
                        LMCSdoWritePolicyBlockers.NoApprovedTarget,
                        connection.Diagnostics.EvaluateSdoWritePolicy(
                            currentSessionCapabilities));
                    AssertEx.Equal(
                        requestCountBeforeEvaluation,
                        secondServer.ReceivedRequests.Count,
                        "Evaluating old and current session capability observations sent an RPC request.");

                    connection.CloseConnection();
                    secondServer.Verify();
                }
            }
        }

        private static void InjectedReadyAndBlockerMatrix()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CapabilitiesStep(1, RequiredCapabilities),
                CloseStep()))
            using (var ownerConnection = new LMCConnection())
            using (var foreignConnection = new LMCConnection())
            {
                Connect(ownerConnection, server.Port);
                var target = CreateUi24Target(2);
                var sourceTargets = new List<LMCSdoWriteTarget>
                {
                    target
                };
                var readyCapabilities =
                    ownerConnection.Diagnostics.GetCapabilities();
                var ready = EvaluateInjected(
                    ownerConnection.Diagnostics,
                    ownerConnection.SessionGeneration,
                    true,
                    readyCapabilities,
                    sourceTargets);

                AssertEx.True(ready.CanAttemptSubmission);
                AssertEx.Equal(
                    LMCSdoWritePolicyBlockers.None,
                    ready.Blockers);
                AssertEx.Equal(1, ready.ApprovedTargets.Count);
                AssertEx.True(
                    ReferenceEquals(target, ready.ApprovedTargets[0]),
                    "The immutable policy snapshot changed the approved target identity.");

                sourceTargets.Clear();
                AssertEx.Equal(
                    1,
                    ready.ApprovedTargets.Count,
                    "The policy result retained a mutable caller target collection.");
                var mutableReadyTargets = ready.ApprovedTargets
                    as IList<LMCSdoWriteTarget>;
                AssertEx.NotNull(mutableReadyTargets);
                AssertEx.Throws<NotSupportedException>(
                    () => mutableReadyTargets.RemoveAt(0));

                AssertExactBlocker(
                    LMCSdoWritePolicyBlockers.ConnectionUnavailable,
                    EvaluateInjected(
                        ownerConnection.Diagnostics,
                        ownerConnection.SessionGeneration,
                        false,
                        readyCapabilities,
                        OneTarget(target)));
                AssertExactBlocker(
                    LMCSdoWritePolicyBlockers
                        .CapabilityObservationUnavailable,
                    EvaluateInjected(
                        ownerConnection.Diagnostics,
                        ownerConnection.SessionGeneration,
                        true,
                        null,
                        OneTarget(target)));
                AssertExactBlocker(
                    LMCSdoWritePolicyBlockers.CapabilityResponseInvalid,
                    EvaluateInjected(
                        ownerConnection.Diagnostics,
                        ownerConnection.SessionGeneration,
                        true,
                        CreateCapabilities(
                            ownerConnection,
                            RequiredCapabilities,
                            includeResponse: false),
                        OneTarget(target)));
                AssertExactBlocker(
                    LMCSdoWritePolicyBlockers
                        .CapabilityObservationNotCurrent,
                    EvaluateInjected(
                        foreignConnection.Diagnostics,
                        foreignConnection.SessionGeneration,
                        true,
                        readyCapabilities,
                        OneTarget(target)));
                AssertExactBlocker(
                    LMCSdoWritePolicyBlockers
                        .CapabilityObservationNotCurrent,
                    EvaluateInjected(
                        ownerConnection.Diagnostics,
                        ownerConnection.SessionGeneration + 1,
                        true,
                        readyCapabilities,
                        OneTarget(target)));

                AssertMissingCapability(
                    ownerConnection,
                    target,
                    LMCDiagnosticCapability.SDORead,
                    LMCSdoWritePolicyBlockers.SdoReadCapabilityMissing);
                AssertMissingCapability(
                    ownerConnection,
                    target,
                    LMCDiagnosticCapability.SDOWrite,
                    LMCSdoWritePolicyBlockers.SdoWriteCapabilityMissing);
                AssertMissingCapability(
                    ownerConnection,
                    target,
                    LMCDiagnosticCapability.SDOReadGeneralInline,
                    LMCSdoWritePolicyBlockers
                        .GeneralInlineReadCapabilityMissing);

                AssertExactBlocker(
                    LMCSdoWritePolicyBlockers.CapabilityIdentityInvalid,
                    EvaluateInjected(
                        ownerConnection.Diagnostics,
                        ownerConnection.SessionGeneration,
                        true,
                        CreateCapabilities(
                            ownerConnection,
                            RequiredCapabilities,
                            mapRevision: 0),
                        OneTarget(target)));
                AssertExactBlocker(
                    LMCSdoWritePolicyBlockers.CapabilityIdentityInvalid,
                    EvaluateInjected(
                        ownerConnection.Diagnostics,
                        ownerConnection.SessionGeneration,
                        true,
                        CreateCapabilities(
                            ownerConnection,
                            RequiredCapabilities,
                            diagnosticsBootId: 0),
                        OneTarget(target)));

                AssertPayloadBlocker(
                    ownerConnection,
                    target,
                    maxSdoDataBytes: 3,
                    maxRequestPayloadBytes: 1320,
                    maxResponsePayloadBytes: 2040);
                AssertPayloadBlocker(
                    ownerConnection,
                    target,
                    maxSdoDataBytes: 4,
                    maxRequestPayloadBytes: 35,
                    maxResponsePayloadBytes: 2040);
                AssertPayloadBlocker(
                    ownerConnection,
                    target,
                    maxSdoDataBytes: 4,
                    maxRequestPayloadBytes: 1320,
                    maxResponsePayloadBytes: 63);

                ownerConnection.CloseConnection();
                server.Verify();
            }
        }

        private static void EmptyAllowlistSubmitSyncAndAsyncIsZeroWire()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var request = CreateUi24Target(1).CreateRequest(0, 1000);
                var requestCountBeforeSubmissions =
                    server.ReceivedRequests.Count;

                var syncError = AssertEx.Throws<NotSupportedException>(
                    () => connection.Diagnostics.SubmitSdo(request));
                AssertRequestValidationNotAttempted(syncError);
                AssertEx.Equal(
                    requestCountBeforeSubmissions,
                    server.ReceivedRequests.Count,
                    "Synchronous empty-allowlist SDO Write sent an RPC request.");

                var asyncError = AssertEx.Throws<NotSupportedException>(
                    () => connection.Diagnostics.SubmitSdoAsync(
                            request,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertRequestValidationNotAttempted(asyncError);
                AssertEx.Equal(
                    requestCountBeforeSubmissions,
                    server.ReceivedRequests.Count,
                    "Asynchronous empty-allowlist SDO Write sent an RPC request.");

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void AssertRequestValidationNotAttempted(
            Exception error)
        {
            LMCSdoSubmissionFailureContext context;
            AssertEx.True(
                LMCSdoSubmissionFailureContext.TryGet(error, out context),
                "The SDO Write policy rejection did not preserve failure context.");
            AssertEx.Equal(
                LMCSdoSubmissionPhase.RequestValidation,
                context.Phase);
            AssertEx.Equal(
                LMCSdoSubmissionOutcome.NotAttempted,
                context.SubmissionOutcome);
            AssertEx.Equal(0u, context.DiagnosticsBootId);
            AssertEx.Equal(0u, context.MapRevision);
            AssertEx.True(context.Ticket == null);
        }

        private static void AssertMissingCapability(
            LMCConnection ownerConnection,
            LMCSdoWriteTarget target,
            LMCDiagnosticCapability missingCapability,
            LMCSdoWritePolicyBlockers expectedBlocker)
        {
            var capabilities = CreateCapabilities(
                ownerConnection,
                RequiredCapabilities & ~missingCapability);
            AssertExactBlocker(
                expectedBlocker,
                EvaluateInjected(
                    ownerConnection.Diagnostics,
                    ownerConnection.SessionGeneration,
                    true,
                    capabilities,
                    OneTarget(target)));
        }

        private static void AssertPayloadBlocker(
            LMCConnection ownerConnection,
            LMCSdoWriteTarget target,
            ushort maxSdoDataBytes,
            ushort maxRequestPayloadBytes,
            ushort maxResponsePayloadBytes)
        {
            AssertExactBlocker(
                LMCSdoWritePolicyBlockers.PayloadCapacityInsufficient,
                EvaluateInjected(
                    ownerConnection.Diagnostics,
                    ownerConnection.SessionGeneration,
                    true,
                    CreateCapabilities(
                        ownerConnection,
                        RequiredCapabilities,
                        maxSdoDataBytes: maxSdoDataBytes,
                        maxRequestPayloadBytes: maxRequestPayloadBytes,
                        maxResponsePayloadBytes: maxResponsePayloadBytes),
                    OneTarget(target)));
        }

        private static void AssertExactBlocker(
            LMCSdoWritePolicyBlockers expected,
            LMCSdoWritePolicyEvaluation actual)
        {
            AssertEx.Equal(expected, actual.Blockers);
            AssertEx.False(actual.CanAttemptSubmission);
        }

        private static void AssertHasBlocker(
            LMCSdoWritePolicyEvaluation evaluation,
            LMCSdoWritePolicyBlockers blocker)
        {
            AssertEx.True(
                (evaluation.Blockers & blocker) == blocker,
                "Expected SDO Write policy blocker " + blocker
                    + ", actual " + evaluation.Blockers + ".");
        }

        private static LMCSdoWritePolicyEvaluation EvaluateInjected(
            LMCDiagnostics owner,
            long sessionGeneration,
            bool isConnected,
            LMCDiagnosticCapabilities capabilities,
            IReadOnlyList<LMCSdoWriteTarget> approvedTargets)
        {
            return LMCDiagnosticsWritePolicy.EvaluateSdoWritePolicy(
                owner,
                sessionGeneration,
                isConnected,
                capabilities,
                approvedTargets);
        }

        private static IReadOnlyList<LMCSdoWriteTarget> OneTarget(
            LMCSdoWriteTarget target)
        {
            return new[] { target };
        }

        private static LMCSdoWriteTarget CreateUi24Target(
            ushort slaveReference)
        {
            return new LMCSdoWriteTarget(
                "Reserved diagnostic UI[24]",
                slaveReference,
                0x2F00,
                24,
                LMCSignalValueType.Int32,
                4,
                -1073741823,
                1073741823);
        }

        private static LMCDiagnosticCapabilities CreateCapabilities(
            LMCConnection ownerConnection,
            LMCDiagnosticCapability capabilities,
            bool includeResponse = true,
            uint mapRevision = MapRevision,
            uint diagnosticsBootId = DiagnosticsBootId,
            ushort maxSdoDataBytes = 4,
            ushort maxRequestPayloadBytes = 1320,
            ushort maxResponsePayloadBytes = 2040)
        {
            var sessionGeneration = ownerConnection.SessionGeneration;
            var response = includeResponse
                ? CreateSuccessfulResponse()
                : null;
            return new LMCDiagnosticCapabilities(
                    response,
                    sessionGeneration,
                    1,
                    (uint)capabilities,
                    mapRevision,
                    0,
                    0,
                    0,
                    0,
                    0,
                    1000,
                    maxRequestPayloadBytes,
                    maxResponsePayloadBytes,
                    1280,
                    80,
                    16,
                    0,
                    maxSdoDataBytes,
                    diagnosticsBootId)
                .BindProvenance(
                    ownerConnection.Diagnostics,
                    sessionGeneration,
                    1);
        }

        private static LMCDiagnosticsResponse CreateSuccessfulResponse()
        {
            return new LMCDiagnosticsResponse(
                LMCConnection.Parse(
                    TestFrame.Response(0, new byte[0])),
                1,
                LMCDiagnosticsResponseFlags.None,
                0,
                0,
                1,
                0);
        }

        private static void Connect(
            LMCConnection connection,
            int port)
        {
            connection.RpcInitConnection(
                "127.0.0.1",
                port,
                "127.0.0.1",
                0,
                LMCConnection.DefaultEventMask);
        }

        private static FakeRpcStep CapabilitiesStep(
            uint requestId,
            LMCDiagnosticCapability capabilities)
        {
            return new FakeRpcStep(
                0x7E00,
                TestFrame.Response(
                    0,
                    CapabilitiesPayload(requestId, capabilities)));
        }

        private static byte[] CapabilitiesPayload(
            uint requestId,
            LMCDiagnosticCapability capabilities)
        {
            var payload = CommonPayload(68, requestId);
            TestFrame.WriteUInt32(payload, 16, 1);
            TestFrame.WriteUInt32(payload, 20, (uint)capabilities);
            TestFrame.WriteUInt32(payload, 24, MapRevision);
            TestFrame.WriteUInt32(payload, 40, 1000);
            TestFrame.WriteUInt16(payload, 44, 1320);
            TestFrame.WriteUInt16(payload, 46, 2040);
            TestFrame.WriteUInt16(payload, 48, 1280);
            TestFrame.WriteUInt16(payload, 50, 80);
            TestFrame.WriteUInt16(payload, 52, 16);
            TestFrame.WriteUInt16(payload, 60, 4);
            TestFrame.WriteUInt32(payload, 64, DiagnosticsBootId);
            return payload;
        }

        private static byte[] CommonPayload(
            int length,
            uint requestId)
        {
            var payload = new byte[length];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, requestId);
            return payload;
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
    }
}

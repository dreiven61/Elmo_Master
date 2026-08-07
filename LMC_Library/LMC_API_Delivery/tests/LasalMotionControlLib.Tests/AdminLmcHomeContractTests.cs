using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class AdminLmcHomeContractTests
    {
        private const uint OriginalRequestId = 0x11223344u;
        private const uint DiagnosticsBuild = 2u;
        private const uint OriginalBootId = 0x55667788u;
        private const uint CurrentBootId = 0x99AABBCCu;
        private const uint MapRevision = 0x10203040u;
        private const int ExpectedActualPosition = -123456;
        private const int TimeoutMilliseconds = 2500;
        private const uint RecordGeneration = 17u;
        private const uint AxisStandstill = 0x02000000u;
        private const uint RequiredEvidenceFlags = 0x0000003Bu;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Request.Admin.LMC_Home.StartGolden",
                StartRequestGolden);
            tests.Add(
                "Request.Admin.LMC_Home.QueryAndRetireGolden",
                QueryAndRetireGolden);
            tests.Add(
                "Contract.Admin.LMC_Home.NoMotionOrSwitchSurface",
                NoMotionOrSwitchSurface);
            tests.Add(
                "Contract.Admin.LMC_Home.StrictValidation",
                StrictValidation);
            tests.Add(
                "Response.Admin.LMC_Home.StartAckStrict",
                StartAckStrict);
            tests.Add(
                "Response.Admin.LMC_Home.FailureCatalogScoped",
                FailureCatalogScoped);
            tests.Add(
                "Response.Admin.LMC_Home.OutcomeSuccessStrict",
                OutcomeSuccessStrict);
            tests.Add(
                "Response.Admin.LMC_Home.RawDriveDeltaDiagnosticOnly",
                RawDriveDeltaDiagnosticOnly);
            tests.Add(
                "Response.Admin.LMC_Home.OutcomeSuccessEvidenceFailsClosed",
                OutcomeSuccessEvidenceFailsClosed);
            tests.Add(
                "Response.Admin.LMC_Home.OutcomeRuntimePhaseOpaque",
                OutcomeRuntimePhaseOpaque);
            tests.Add(
                "Response.Admin.LMC_Home.RunningCannotRetire",
                RunningCannotRetire);
            tests.Add(
                "Response.Admin.LMC_Home.QuarantinePreserved",
                QuarantinePreserved);
            tests.Add(
                "Response.Admin.LMC_Home.KeyAndGenerationPinned",
                KeyAndGenerationPinned);
            tests.Add(
                "Rpc.Admin.LMC_Home.StartQueryRetireSync",
                StartQueryRetireSync);
            tests.Add(
                "Rpc.Admin.LMC_Home.ResponseLossUncertainNoReplay",
                ResponseLossUncertainNoReplay);
        }

        private static void StartRequestGolden()
        {
            var key = RecoveryKey();
            var request = LMC_AdminFrame.StartLmcHome(key);

            AssertEx.Equal((ushort)0x7D13, TestFrame.ReadUInt16(request, 0));
            AssertEx.Equal((ushort)56, TestFrame.ReadUInt16(request, 4));
            AssertEx.Equal((ushort)2, TestFrame.ReadUInt16(request, 6));
            AssertEx.Equal((ushort)1, TestFrame.ReadUInt16(request, 8));
            AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(request, 10));
            AssertEx.Equal(OriginalRequestId, TestFrame.ReadUInt32(request, 12));
            AssertEx.Equal(DiagnosticsBuild, TestFrame.ReadUInt32(request, 16));
            AssertEx.Equal(OriginalBootId, TestFrame.ReadUInt32(request, 20));
            AssertEx.Equal(MapRevision, TestFrame.ReadUInt32(request, 24));
            AssertIntent(request, 28);
            AssertEx.Equal((ushort)1, TestFrame.ReadUInt16(request, 44));
            AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(request, 46));
            AssertEx.Equal(ExpectedActualPosition, TestFrame.ReadInt32(request, 48));
            AssertEx.Equal(0, TestFrame.ReadInt32(request, 52));
            AssertEx.Equal((uint)TimeoutMilliseconds, TestFrame.ReadUInt32(request, 56));
            AssertEx.Equal(0x454D4F48u, TestFrame.ReadUInt32(request, 60));
        }

        private static void QueryAndRetireGolden()
        {
            var key = RecoveryKey();
            var query = LMC_AdminFrame.ReadLmcHomeOutcome(
                3,
                CurrentBootId,
                key);
            AssertEx.Equal((ushort)0x7D18, TestFrame.ReadUInt16(query, 0));
            AssertEx.Equal((ushort)56, TestFrame.ReadUInt16(query, 4));
            AssertEx.Equal((ushort)2, TestFrame.ReadUInt16(query, 6));
            AssertEx.Equal(3u, TestFrame.ReadUInt32(query, 12));
            AssertEx.Equal(DiagnosticsBuild, TestFrame.ReadUInt32(query, 16));
            AssertEx.Equal(OriginalBootId, TestFrame.ReadUInt32(query, 20));
            AssertEx.Equal(MapRevision, TestFrame.ReadUInt32(query, 24));
            AssertEx.Equal(CurrentBootId, TestFrame.ReadUInt32(query, 28));
            AssertEx.Equal(OriginalRequestId, TestFrame.ReadUInt32(query, 32));
            AssertIntent(query, 36);
            AssertEx.Equal((ushort)1, TestFrame.ReadUInt16(query, 52));
            AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(query, 54));
            AssertEx.Equal(ExpectedActualPosition, TestFrame.ReadInt32(query, 56));
            AssertEx.Equal(0, TestFrame.ReadInt32(query, 60));

            var retire = LMC_AdminFrame.RetireLmcHomeOutcome(
                4,
                CurrentBootId,
                key,
                RecordGeneration);
            AssertEx.Equal((ushort)0x7D19, TestFrame.ReadUInt16(retire, 0));
            AssertEx.Equal((ushort)60, TestFrame.ReadUInt16(retire, 4));
            AssertEx.Equal((ushort)2, TestFrame.ReadUInt16(retire, 6));
            AssertEx.Equal(4u, TestFrame.ReadUInt32(retire, 12));
            AssertEx.Equal(RecordGeneration, TestFrame.ReadUInt32(retire, 64));
        }

        private static void NoMotionOrSwitchSurface()
        {
            var parameters = new LMCHomeParameters(
                ExpectedActualPosition,
                TimeoutMilliseconds);
            AssertEx.Equal(
                LMCHomeSemanticMode.CurrentPositionZero,
                parameters.SemanticMode);
            AssertEx.Equal(ExpectedActualPosition, parameters.ExpectedActualPosition);
            AssertEx.Equal(0, parameters.TargetPosition);

            var forbidden = new[]
            {
                "Recipe",
                "SearchVelocity",
                "BackoffVelocity",
                "Acceleration",
                "PositionWindow",
                "Jerk",
                "MaxTravel",
                "ReferenceSwitch",
                "LimitSwitch"
            };
            foreach (var name in forbidden)
            {
                AssertEx.True(
                    typeof(LMCHomeParameters).GetProperty(name) == null);
                AssertEx.True(
                    typeof(LMCPreparedHome).GetProperty(name) == null);
            }

            AssertEx.True(HasPublicMethod("LMC_Home"));
            AssertEx.True(HasPublicMethod("ReadLMC_HomeOutcome"));
            AssertEx.True(HasPublicMethod("RetireLMC_HomeOutcome"));
            AssertEx.False(HasPublicMethod("MMC_Home"));
            AssertEx.False(HasPublicMethod("MmcHome"));
            AssertEx.False(HasPublicMethod("ReadMMC_HomeOutcome"));
            AssertEx.False(HasPublicMethod("RetireMMC_HomeOutcome"));
        }

        private static void StrictValidation()
        {
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new LMCHomeParameters(ExpectedActualPosition, 99));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new LMCHomeParameters(ExpectedActualPosition, 5001));
            AssertEx.Throws<ArgumentException>(
                () => new LMCHomeClientIntentId(0, 0, 0, 0));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new LMCHomeRecoveryKey(
                    1,
                    OriginalRequestId,
                    DiagnosticsBuild,
                    OriginalBootId,
                    MapRevision,
                    Intent(),
                    2,
                    ExpectedActualPosition,
                    TimeoutMilliseconds,
                    (LMCHomeSemanticMode)2));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_AdminFrame.ReadLmcHomeOutcome(3, 0, RecoveryKey()));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_AdminFrame.RetireLmcHomeOutcome(
                    4,
                    CurrentBootId,
                    RecoveryKey(),
                    0));
        }

        private static void StartAckStrict()
        {
            var payload = CommonAdminPayload(3, 24);
            TestFrame.WriteUInt16(payload, 16, 1);
            var parsed = LMC_AdminParser.ParseStartLmcHome(
                TestFrame.Response(0, payload),
                3,
                LMCHomeSemanticMode.CurrentPositionZero);
            AssertEx.True(parsed.Response.IsSuccess);
            AssertEx.Equal(
                LMCHomeSemanticMode.CurrentPositionZero,
                parsed.SemanticMode);
            AssertEx.Equal(0u, parsed.NativeCommandState);

            var wrongMode = (byte[])payload.Clone();
            TestFrame.WriteUInt16(wrongMode, 16, 2);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseStartLmcHome(
                    TestFrame.Response(0, wrongMode),
                    3,
                    LMCHomeSemanticMode.CurrentPositionZero));

            var wrongNative = (byte[])payload.Clone();
            TestFrame.WriteUInt32(wrongNative, 20, 1);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseStartLmcHome(
                    TestFrame.Response(0, wrongNative),
                    3,
                    LMCHomeSemanticMode.CurrentPositionZero));

            var oversized = new byte[25];
            Buffer.BlockCopy(payload, 0, oversized, 0, payload.Length);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseStartLmcHome(
                    TestFrame.Response(0, oversized),
                    3,
                    LMCHomeSemanticMode.CurrentPositionZero));
        }

        private static void OutcomeSuccessStrict()
        {
            var key = RecoveryKey();
            var parsed = LMC_AdminParser.ParseLmcHomeOutcome(
                TestFrame.Response(
                    0,
                    OutcomePayload(
                        3,
                        key,
                        LMCHomeOutcomeRecordState.Succeeded,
                        0,
                        0,
                        0,
                        0,
                        0,
                        200,
                        RecordGeneration)),
                3,
                key);
            var result = CreatePublicOutcome(parsed, key);
            AssertEx.True(result.IsTerminal);
            AssertEx.True(result.HomeSucceeded);
            AssertEx.Equal(ExpectedActualPosition, result.RawDrivePositionBefore);
            AssertEx.Equal(0, result.ActualApplicationPositionAfter);
            AssertEx.Equal(0, result.ActualInternalPositionAfter);
            AssertEx.Equal(RequiredEvidenceFlags, result.EvidenceFlags);
            AssertEx.Equal(RecordGeneration, result.RecordGeneration);
        }

        private static void OutcomeSuccessEvidenceFailsClosed()
        {
            var mutations = new Action<byte[]>[]
            {
                payload => TestFrame.WriteUInt16(payload, 68, 1),
                payload => TestFrame.WriteInt16(payload, 70, -31000),
                payload => TestFrame.WriteUInt32(payload, 72, 33),
                payload => TestFrame.WriteUInt32(payload, 76, 0),
                payload => TestFrame.WriteInt32(payload, 80, 1),
                payload => TestFrame.WriteInt32(payload, 92, 1),
                payload => TestFrame.WriteInt32(payload, 96, 1),
                payload => TestFrame.WriteInt32(payload, 100, 1),
                payload => TestFrame.WriteInt32(payload, 104, 1),
                payload => TestFrame.WriteInt32(payload, 108, 1),
                payload => TestFrame.WriteInt32(payload, 112, 1),
                payload => TestFrame.WriteUInt32(payload, 116, 1),
                payload => TestFrame.WriteUInt32(payload, 120, 0x1F),
                payload => TestFrame.WriteUInt32(payload, 120, 0x3F),
                payload => TestFrame.WriteUInt32(payload, 120, 0x7F),
                payload => TestFrame.WriteUInt32(payload, 124, 0),
                payload => TestFrame.WriteUInt32(payload, 128, 0),
                payload => TestFrame.WriteUInt32(payload, 132, 1),
                payload => TestFrame.WriteUInt32(payload, 140, 0)
            };

            foreach (var mutate in mutations)
            {
                var key = RecoveryKey();
                var payload = OutcomePayload(
                    3,
                    key,
                    LMCHomeOutcomeRecordState.Succeeded,
                    0,
                    0,
                    0,
                    0,
                    0,
                    200,
                    RecordGeneration);
                mutate(payload);
                AssertEx.Throws<InvalidDataException>(
                    () => LMC_AdminParser.ParseLmcHomeOutcome(
                        TestFrame.Response(0, payload),
                        3,
                        key));
            }

            var validKey = RecoveryKey();
            var validParsed = LMC_AdminParser.ParseLmcHomeOutcome(
                TestFrame.Response(
                    0,
                    OutcomePayload(
                        3,
                        validKey,
                        LMCHomeOutcomeRecordState.Succeeded,
                        0,
                        0,
                        0,
                        0,
                        0,
                        200,
                        RecordGeneration)),
                3,
                validKey);
            AssertEx.False(
                CreatePublicOutcome(
                    validParsed,
                    validKey,
                    RequiredEvidenceFlags - 1).HomeSucceeded);
        }

        private static void RawDriveDeltaDiagnosticOnly()
        {
            foreach (var rawDrivePair in new[]
            {
                new[] { ExpectedActualPosition, ExpectedActualPosition + 4 },
                new[] { ExpectedActualPosition, ExpectedActualPosition - 4 },
                new[] { int.MaxValue, int.MinValue + 2 },
                new[] { int.MinValue, int.MaxValue - 2 }
            })
            {
                var key = RecoveryKey();
                var payload = OutcomePayload(
                    3,
                    key,
                    LMCHomeOutcomeRecordState.Succeeded,
                    0,
                    0,
                    0,
                    0,
                    0,
                    200,
                    RecordGeneration);
                TestFrame.WriteInt32(payload, 84, rawDrivePair[0]);
                TestFrame.WriteInt32(payload, 88, rawDrivePair[1]);
                var parsed = LMC_AdminParser.ParseLmcHomeOutcome(
                    TestFrame.Response(0, payload),
                    3,
                    key);
                var result = CreatePublicOutcome(parsed, key);
                AssertEx.True(result.HomeSucceeded);
                AssertEx.Equal(rawDrivePair[0], result.RawDrivePositionBefore);
                AssertEx.Equal(rawDrivePair[1], result.RawDrivePositionAfter);
            }
        }

        private static void OutcomeRuntimePhaseOpaque()
        {
            foreach (var runtimePhase in new[] { 0u, uint.MaxValue })
            {
                var key = RecoveryKey();
                var payload = OutcomePayload(
                    3,
                    key,
                    LMCHomeOutcomeRecordState.Succeeded,
                    0,
                    0,
                    0,
                    0,
                    0,
                    200,
                    RecordGeneration);
                TestFrame.WriteUInt32(payload, 136, runtimePhase);
                var parsed = LMC_AdminParser.ParseLmcHomeOutcome(
                    TestFrame.Response(0, payload),
                    3,
                    key);
                var result = CreatePublicOutcome(parsed, key);
                AssertEx.True(result.HomeSucceeded);
                AssertEx.Equal(runtimePhase, result.RuntimePhase);
            }
        }

        private static void FailureCatalogScoped()
        {
            foreach (var detail in new[]
            {
                LMCAdminDetailCode.InvalidState,
                LMCAdminDetailCode.ActiveAxisError,
                LMCAdminDetailCode.CoordinatePreconditionFailed,
                LMCAdminDetailCode.DiagnosticsBuildMismatch,
                LMCAdminDetailCode.BootIdMismatch,
                LMCAdminDetailCode.MapRevisionMismatch,
                LMCAdminDetailCode.LmcHomeOutcomeSlotOccupied,
                LMCAdminDetailCode.AxisOwnershipConflict,
                LMCAdminDetailCode.AxisOwnershipQuarantined
            })
            {
                var startFailure = CommonAdminPayload(3, 16);
                SetAdminFailure(startFailure, detail);
                var parsed = LMC_AdminParser.ParseStartLmcHome(
                    TestFrame.Response(0, startFailure),
                    3,
                    LMCHomeSemanticMode.CurrentPositionZero);
                AssertEx.True(!parsed.Response.IsSuccess);
                AssertEx.Equal(detail, parsed.Response.DetailCode);
            }

            var wrongStartDomain = CommonAdminPayload(3, 16);
            SetAdminFailure(
                wrongStartDomain,
                LMCAdminDetailCode.LmcHomeOutcomeNotFound);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseStartLmcHome(
                    TestFrame.Response(0, wrongStartDomain),
                    3,
                    LMCHomeSemanticMode.CurrentPositionZero));

            foreach (var detail in new[]
            {
                LMCAdminDetailCode.DiagnosticsBuildMismatch,
                LMCAdminDetailCode.BootIdMismatch,
                LMCAdminDetailCode.MapRevisionMismatch,
                LMCAdminDetailCode.LmcHomeOutcomeNotFound,
                LMCAdminDetailCode.LmcHomeOutcomeIndeterminate,
                LMCAdminDetailCode.LmcHomeOutcomeStoreCorrupt,
                LMCAdminDetailCode.LmcHomeOutcomeKeyMismatch,
                LMCAdminDetailCode.LmcHomeOutcomeStorageUnavailable
            })
            {
                var queryFailure = CommonAdminPayload(4, 16);
                SetAdminFailure(queryFailure, detail);
                var exception = AssertEx.Throws<LMCHomeOutcomeQueryException>(
                    () => LMC_AdminParser.ParseLmcHomeOutcome(
                        TestFrame.Response(0, queryFailure),
                        4,
                        RecoveryKey()));
                AssertEx.Equal(detail, exception.Response.DetailCode);
            }

            var wrongOutcomeDomain = CommonAdminPayload(4, 16);
            SetAdminFailure(
                wrongOutcomeDomain,
                LMCAdminDetailCode.Ds402HomeOutcomeNotFound);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseLmcHomeOutcome(
                    TestFrame.Response(0, wrongOutcomeDomain),
                    4,
                    RecoveryKey()));
        }

        private static void RunningCannotRetire()
        {
            var key = RecoveryKey();
            var payload = OutcomePayload(
                3,
                key,
                LMCHomeOutcomeRecordState.Running,
                0,
                0,
                0,
                ExpectedActualPosition,
                ExpectedActualPosition,
                0,
                RecordGeneration);
            var parsed = LMC_AdminParser.ParseLmcHomeOutcome(
                TestFrame.Response(0, payload),
                3,
                key);
            AssertEx.Equal(
                LMCHomeOutcomeRecordState.Running,
                parsed.RecordState);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseLmcHomeOutcomeRetirement(
                    TestFrame.Response(0, payload),
                    3,
                    key,
                    RecordGeneration));
        }

        private static void QuarantinePreserved()
        {
            var key = RecoveryKey();
            var parsed = LMC_AdminParser.ParseLmcHomeOutcome(
                TestFrame.Response(
                    0,
                    OutcomePayload(
                        3,
                        key,
                        LMCHomeOutcomeRecordState.Quarantined,
                        1,
                        -31000,
                        33,
                        5,
                        ExpectedActualPosition,
                        200,
                        RecordGeneration)),
                3,
                key);
            var result = CreatePublicOutcome(parsed, key);
            AssertEx.True(result.IsTerminal);
            AssertEx.True(result.IsQuarantined);
            AssertEx.True(!result.HomeSucceeded);
            AssertEx.Equal(5, result.AxisError);
        }

        private static void KeyAndGenerationPinned()
        {
            var key = RecoveryKey();
            var payload = OutcomePayload(
                4,
                key,
                LMCHomeOutcomeRecordState.Succeeded,
                0,
                0,
                0,
                0,
                0,
                200,
                RecordGeneration);
            var wrongKey = new LMCHomeRecoveryKey(
                1,
                OriginalRequestId,
                DiagnosticsBuild,
                OriginalBootId,
                MapRevision,
                Intent(),
                2,
                ExpectedActualPosition + 1,
                TimeoutMilliseconds,
                LMCHomeSemanticMode.CurrentPositionZero);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseLmcHomeOutcome(
                    TestFrame.Response(0, payload),
                    4,
                    wrongKey));
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseLmcHomeOutcomeRetirement(
                    TestFrame.Response(0, payload),
                    4,
                    key,
                    RecordGeneration + 1));

            var retired = LMC_AdminParser.ParseLmcHomeOutcomeRetirement(
                TestFrame.Response(0, payload),
                4,
                key,
                RecordGeneration);
            AssertEx.Equal(RecordGeneration, retired.RecordGeneration);
        }

        private static void StartQueryRetireSync()
        {
            LMCHomeRecoveryKey preparedKey = null;
            var startStep = new FakeRpcStep(0x7D13, new byte[0])
            {
                ResponseFactory = request =>
                {
                    return TestFrame.Response(
                        0,
                        StartAcknowledgementPayload(
                            TestFrame.ReadUInt32(request, 12)));
                }
            };
            var queryStep = new FakeRpcStep(0x7D18, new byte[0])
            {
                ResponseFactory = request =>
                {
                    return TestFrame.Response(
                        0,
                        OutcomePayload(
                            TestFrame.ReadUInt32(request, 12),
                            preparedKey,
                            LMCHomeOutcomeRecordState.Succeeded,
                            0,
                            0,
                            0,
                            0,
                            0,
                            200,
                            RecordGeneration));
                }
            };
            var retireStep = new FakeRpcStep(0x7D19, new byte[0])
            {
                ResponseFactory = request =>
                {
                    return TestFrame.Response(
                        0,
                        OutcomePayload(
                            TestFrame.ReadUInt32(request, 12),
                            preparedKey,
                            LMCHomeOutcomeRecordState.Succeeded,
                            0,
                            0,
                            0,
                            0,
                            0,
                            200,
                            RecordGeneration));
                }
            };

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(2),
                AxisInfoStep(2),
                CapabilitiesStep(),
                DiagnosticsCapabilitiesStep(),
                startStep,
                queryStep,
                retireStep,
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis2");
                var capabilities = connection.Admin.GetCapabilities();
                var diagnosticCapabilities = connection.Diagnostics
                    .GetCapabilities();
                var prepared = axis.PrepareLMC_Home(
                    ExpectedActualPosition,
                    TimeoutMilliseconds,
                    capabilities,
                    diagnosticCapabilities,
                    LMCHomeExecuteToken.Create());
                preparedKey = prepared.RecoveryKey;

                var acknowledgement = axis.LMC_Home(prepared);
                AssertEx.Equal(preparedKey, acknowledgement.RecoveryKey);
                AssertEx.True(prepared.IsConsumed);

                var outcome = axis.ReadLMC_HomeOutcome(
                    preparedKey,
                    capabilities,
                    diagnosticCapabilities);
                AssertEx.True(outcome.HomeSucceeded);
                AssertEx.Equal(RecordGeneration, outcome.RecordGeneration);

                var retirement = axis.RetireLMC_HomeOutcome(
                    outcome,
                    capabilities,
                    diagnosticCapabilities);
                AssertEx.True(retirement.RetirementConfirmed);
                AssertEx.True(retirement.Outcome.HomeSucceeded);
                AssertEx.Equal(RecordGeneration, retirement.RecordGeneration);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x7D13));
                AssertEx.Equal(1, CountCommand(server, 0x7D18));
                AssertEx.Equal(1, CountCommand(server, 0x7D19));
            }
        }

        private static void ResponseLossUncertainNoReplay()
        {
            var lostResponse = new FakeRpcStep(0x7D13, new byte[0])
            {
                CloseClientBeforeResponseAndContinue = true
            };
            LMCHomeRecoveryKey recoveryKey = null;
            var queryStep = new FakeRpcStep(0x7D18, new byte[0])
            {
                ResponseFactory = request => TestFrame.Response(
                    0,
                    OutcomePayload(
                        TestFrame.ReadUInt32(request, 12),
                        recoveryKey,
                        LMCHomeOutcomeRecordState.Succeeded,
                        0,
                        0,
                        0,
                        0,
                        0,
                        200,
                        RecordGeneration))
            };
            var retireStep = new FakeRpcStep(0x7D19, new byte[0])
            {
                ResponseFactory = request => TestFrame.Response(
                    0,
                    OutcomePayload(
                        TestFrame.ReadUInt32(request, 12),
                        recoveryKey,
                        LMCHomeOutcomeRecordState.Succeeded,
                        0,
                        0,
                        0,
                        0,
                        0,
                        200,
                        RecordGeneration))
            };
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                CapabilitiesStep(),
                DiagnosticsCapabilitiesStep(),
                lostResponse,
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                CapabilitiesStep(),
                DiagnosticsCapabilitiesStep(),
                queryStep,
                retireStep,
                CloseStep()))
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
                var diagnosticCapabilities = connection.Diagnostics
                    .GetCapabilities();
                var prepared = axis.PrepareLMC_Home(
                    ExpectedActualPosition,
                    TimeoutMilliseconds,
                    capabilities,
                    diagnosticCapabilities,
                    LMCHomeExecuteToken.Create());
                recoveryKey = prepared.RecoveryKey;

                var exception = AssertEx.Throws<
                    LMCHomeStartOutcomeUncertainException>(
                    () => axis.LMC_Home(prepared));
                AssertEx.True(prepared.IsConsumed);
                AssertEx.Equal(prepared, exception.PreparedCommand);
                AssertEx.Equal(prepared.RecoveryKey, exception.RecoveryKey);
                AssertEx.NotNull(exception.InnerException);
                AssertEx.Equal(
                    LMCConnectionState.Faulted,
                    connection.State);
                AssertEx.Equal(1, Volatile.Read(ref faultTransitions));
                AssertEx.Throws<InvalidOperationException>(
                    () => axis.LMC_Home(prepared));

                Connect(connection, server.Port);
                var recoveryAxis = new LMCSingleAxis(
                    connection,
                    "_LMCAxis1");
                var recoveryCapabilities = connection.Admin.GetCapabilities();
                var recoveryDiagnosticCapabilities = connection.Diagnostics
                    .GetCapabilities();
                var outcome = recoveryAxis.ReadLMC_HomeOutcome(
                    recoveryKey,
                    recoveryCapabilities,
                    recoveryDiagnosticCapabilities);
                AssertEx.True(outcome.HomeSucceeded);
                var retirement = recoveryAxis.RetireLMC_HomeOutcome(
                    outcome,
                    recoveryCapabilities,
                    recoveryDiagnosticCapabilities);
                AssertEx.True(retirement.RetirementConfirmed);

                connection.CloseConnection();

                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x7D13));
                AssertEx.Equal(1, CountCommand(server, 0x7D18));
                AssertEx.Equal(1, CountCommand(server, 0x7D19));
                AssertEx.Equal(2, server.AcceptedClientCount);
            }
        }

        private static LMCHomeRecoveryKey RecoveryKey()
        {
            return new LMCHomeRecoveryKey(
                1,
                OriginalRequestId,
                DiagnosticsBuild,
                OriginalBootId,
                MapRevision,
                Intent(),
                2,
                ExpectedActualPosition,
                TimeoutMilliseconds,
                LMCHomeSemanticMode.CurrentPositionZero);
        }

        private static LMCHomeClientIntentId Intent()
        {
            return new LMCHomeClientIntentId(
                0x01234567u,
                0x89ABCDEFu,
                0x10203040u,
                0x50607080u);
        }

        private static byte[] OutcomePayload(
            uint requestId,
            LMCHomeRecoveryKey key,
            LMCHomeOutcomeRecordState state,
            ushort originalStatus,
            short originalError,
            uint originalDetail,
            int axisError,
            int actualApplicationPositionAfter,
            uint completionMilliseconds,
            uint generation)
        {
            var payload = CommonAdminPayload(requestId, 144);
            TestFrame.WriteUInt16(payload, 16, (ushort)state);
            TestFrame.WriteUInt16(payload, 18, (ushort)key.SemanticMode);
            TestFrame.WriteUInt32(payload, 20, key.DiagnosticsBuild);
            TestFrame.WriteUInt32(payload, 24, key.OriginalDiagnosticsBootId);
            TestFrame.WriteUInt32(payload, 28, key.MapRevision);
            TestFrame.WriteUInt32(payload, 32, key.OriginalRequestId);
            TestFrame.WriteUInt32(payload, 36, key.ClientIntentId0);
            TestFrame.WriteUInt32(payload, 40, key.ClientIntentId1);
            TestFrame.WriteUInt32(payload, 44, key.ClientIntentId2);
            TestFrame.WriteUInt32(payload, 48, key.ClientIntentId3);
            TestFrame.WriteUInt16(payload, 52, key.AxisReference);
            TestFrame.WriteInt32(payload, 56, key.ExpectedActualPosition);
            TestFrame.WriteInt32(payload, 60, 0);
            TestFrame.WriteUInt32(payload, 64, (uint)key.TimeoutMilliseconds);
            TestFrame.WriteUInt16(payload, 68, originalStatus);
            TestFrame.WriteInt16(payload, 70, originalError);
            TestFrame.WriteUInt32(payload, 72, originalDetail);
            TestFrame.WriteUInt32(payload, 76, AxisStandstill);
            TestFrame.WriteInt32(payload, 80, axisError);
            TestFrame.WriteInt32(payload, 84, key.ExpectedActualPosition);
            TestFrame.WriteInt32(payload, 88, key.ExpectedActualPosition);
            TestFrame.WriteInt32(payload, 92, actualApplicationPositionAfter);
            TestFrame.WriteInt32(payload, 96, actualApplicationPositionAfter);
            TestFrame.WriteInt32(payload, 100, actualApplicationPositionAfter);
            TestFrame.WriteInt32(payload, 104, actualApplicationPositionAfter);
            TestFrame.WriteInt32(payload, 108, actualApplicationPositionAfter);
            TestFrame.WriteInt32(payload, 112, actualApplicationPositionAfter);
            TestFrame.WriteUInt32(payload, 116, 0);
            TestFrame.WriteUInt32(payload, 120, RequiredEvidenceFlags);
            TestFrame.WriteUInt32(payload, 124, 100);
            TestFrame.WriteUInt32(payload, 128, completionMilliseconds);
            TestFrame.WriteUInt32(payload, 132, 0);
            TestFrame.WriteUInt32(payload, 136, 7);
            TestFrame.WriteUInt32(payload, 140, generation);
            return payload;
        }

        private static byte[] StartAcknowledgementPayload(uint requestId)
        {
            var payload = CommonAdminPayload(requestId, 24);
            TestFrame.WriteUInt16(
                payload,
                16,
                (ushort)LMCHomeSemanticMode.CurrentPositionZero);
            TestFrame.WriteUInt32(payload, 20, 0);
            return payload;
        }

        private static byte[] CapabilitiesPayload(uint requestId)
        {
            var payload = CommonAdminPayload(requestId, 40);
            TestFrame.WriteUInt32(
                payload,
                16,
                (uint)LMCAdminFeature.AxisHome);
            TestFrame.WriteUInt32(payload, 20, 0x0000003Fu);
            TestFrame.WriteUInt32(
                payload,
                24,
                (uint)LMCGroupParameterSelection.None);
            TestFrame.WriteUInt16(payload, 28, 4);
            TestFrame.WriteUInt16(payload, 30, 1);
            TestFrame.WriteUInt16(payload, 32, 0);
            TestFrame.WriteUInt16(payload, 34, 0);
            TestFrame.WriteUInt16(payload, 36, 5);
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
            TestFrame.WriteUInt16(payload, 48, 1280);
            TestFrame.WriteUInt32(payload, 64, OriginalBootId);
            return payload;
        }

        private static FakeRpcStep CapabilitiesStep()
        {
            return new FakeRpcStep(0x7D00, new byte[0])
            {
                ResponseFactory = request => TestFrame.Response(
                    0,
                    CapabilitiesPayload(TestFrame.ReadUInt32(request, 12)))
            };
        }

        private static FakeRpcStep DiagnosticsCapabilitiesStep()
        {
            return new FakeRpcStep(0x7E00, new byte[0])
            {
                ResponseFactory = request => TestFrame.Response(
                    0,
                    DiagnosticsCapabilitiesPayload(
                        TestFrame.ReadUInt32(request, 12)))
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

        private static LMCHomeOutcomeResult CreatePublicOutcome(
            LMCParsedHomeOutcome parsed,
            LMCHomeRecoveryKey key)
        {
            return CreatePublicOutcome(parsed, key, parsed.EvidenceFlags);
        }

        private static LMCHomeOutcomeResult CreatePublicOutcome(
            LMCParsedHomeOutcome parsed,
            LMCHomeRecoveryKey key,
            uint evidenceFlags)
        {
            return new LMCHomeOutcomeResult(
                parsed.Response,
                key,
                parsed.RecordState,
                parsed.OriginalCommandStatus,
                parsed.OriginalErrorId,
                parsed.OriginalDetailCode,
                parsed.AxisStatus,
                parsed.AxisError,
                parsed.RawDrivePositionBefore,
                parsed.RawDrivePositionAfter,
                parsed.ActualApplicationPositionAfter,
                parsed.SetApplicationPositionAfter,
                parsed.ActualInternalPositionAfter,
                parsed.SetInternalPositionAfter,
                parsed.DestinationInternalPositionAfter,
                parsed.MasterInternalPositionAfter,
                parsed.NativeCommandState,
                evidenceFlags,
                parsed.StartMilliseconds,
                parsed.CompletionMilliseconds,
                parsed.StopState,
                parsed.RuntimePhase,
                parsed.RecordGeneration);
        }

        private static byte[] CommonAdminPayload(uint requestId, int length)
        {
            var payload = new byte[length];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, requestId);
            return payload;
        }

        private static void SetAdminFailure(
            byte[] payload,
            LMCAdminDetailCode detail)
        {
            TestFrame.WriteUInt16(payload, 4, 1);
            TestFrame.WriteInt16(payload, 6, -31000);
            TestFrame.WriteUInt32(payload, 12, (uint)detail);
        }

        private static void AssertIntent(byte[] request, int offset)
        {
            AssertEx.Equal(0x01234567u, TestFrame.ReadUInt32(request, offset));
            AssertEx.Equal(0x89ABCDEFu, TestFrame.ReadUInt32(request, offset + 4));
            AssertEx.Equal(0x10203040u, TestFrame.ReadUInt32(request, offset + 8));
            AssertEx.Equal(0x50607080u, TestFrame.ReadUInt32(request, offset + 12));
        }

        private static bool HasPublicMethod(string name)
        {
            foreach (var method in typeof(LMCSingleAxis).GetMethods(
                BindingFlags.Instance | BindingFlags.Public))
            {
                if (string.Equals(method.Name, name, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

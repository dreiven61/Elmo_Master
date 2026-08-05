using System;
using System.Collections.Generic;
using System.IO;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class EncoderMaintenanceProtocolContractTests
    {
        private const uint OriginalRequestId = 0x11223344u;
        private const uint QueryRequestId = 0x55667788u;
        private const uint DiagnosticsBuild = 0xA1A2A3A4u;
        private const uint DiagnosticsBootId = 0xB1B2B3B4u;
        private const uint MapRevision = 0xC1C2C3C4u;
        private const uint RecordGeneration = 0x0BADF00Du;
        private const uint OwnerGeneration = 0x01020304u;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Request.EncoderMaintenance.Tw20GoldenBytes",
                Tw20StartGoldenBytes);
            tests.Add(
                "Request.EncoderMaintenance.Tw19GoldenBytes",
                Tw19StartGoldenBytes);
            tests.Add(
                "Request.EncoderMaintenance.RecoveryQueryRetireGoldenBytes",
                RecoveryQueryRetireGoldenBytes);
            tests.Add(
                "Response.EncoderMaintenance.StartStrictParser",
                StartStrictParser);
            tests.Add(
                "Response.EncoderMaintenance.OutcomeStates",
                OutcomeStates);
            tests.Add(
                "Response.EncoderMaintenance.MalformedRejected",
                MalformedOutcomeRejected);
            tests.Add(
                "Response.EncoderMaintenance.ExactRetirementSnapshot",
                ExactRetirementSnapshot);
            tests.Add(
                "Contract.EncoderMaintenance.CommandValueIsFixedOne",
                CommandValueIsFixedOne);
            tests.Add(
                "Contract.EncoderMaintenance.DedicatedSdoObject",
                DedicatedSdoObject);
            tests.Add(
                "Contract.EncoderMaintenance.TimeoutIsOverallMilliseconds",
                TimeoutIsOverallMilliseconds);
            tests.Add(
                "Rpc.EncoderMaintenance.Tw20StartQueryRetireSync",
                Tw20StartQueryRetireSync);
            tests.Add(
                "Rpc.EncoderMaintenance.Tw19StartQueryRetireSync",
                Tw19StartQueryRetireSync);
        }

        private static void Tw20StartGoldenBytes()
        {
            var key = RecoveryKey(
                LMCEncoderMaintenanceKind.Tw20ErrorWarningReset);
            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "53 7E 00 00 48 00 00 00 "
                    + "01 00 00 00 44 33 22 11 "
                    + "A4 A3 A2 A1 B4 B3 B2 B1 C4 C3 C2 C1 "
                    + "04 03 02 01 08 07 06 05 "
                    + "0C 0B 0A 09 10 0F 0E 0D "
                    + "01 00 34 12 03 00 04 00 "
                    + "01 00 00 00 34 12 00 00 "
                    + "14 13 12 11 18 17 16 15 "
                    + "1C 1B 1A 19 20 1F 1E 1D "
                    + "54 57 32 30"),
                LMC_DiagnosticsFrame.StartEncoderMaintenance(
                    key,
                    0x30325754u));

            AssertEx.Throws<ArgumentException>(
                () => LMC_DiagnosticsFrame.StartEncoderMaintenance(
                    key,
                    0x39315754u));
        }

        private static void Tw19StartGoldenBytes()
        {
            var key = RecoveryKey(
                LMCEncoderMaintenanceKind.Tw19MultiturnPositionReset);
            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "53 7E 00 00 48 00 00 00 "
                    + "01 00 00 00 44 33 22 11 "
                    + "A4 A3 A2 A1 B4 B3 B2 B1 C4 C3 C2 C1 "
                    + "04 03 02 01 08 07 06 05 "
                    + "0C 0B 0A 09 10 0F 0E 0D "
                    + "02 00 34 12 03 00 04 00 "
                    + "01 00 00 00 34 12 00 00 "
                    + "14 13 12 11 18 17 16 15 "
                    + "1C 1B 1A 19 20 1F 1E 1D "
                    + "54 57 31 39"),
                LMC_DiagnosticsFrame.StartEncoderMaintenance(
                    key,
                    0x39315754u));
        }

        private static void RecoveryQueryRetireGoldenBytes()
        {
            var original = RecoveryKey(
                LMCEncoderMaintenanceKind.Tw19MultiturnPositionReset);
            var restored = new LMCEncoderMaintenanceRecoveryKey(
                original.SchemaVersion,
                original.OriginalRequestId,
                original.DiagnosticsBuild,
                original.DiagnosticsBootId,
                original.MapRevision,
                new LMCEncoderMaintenanceClientIntentId(
                    original.ClientIntentId.Word0,
                    original.ClientIntentId.Word1,
                    original.ClientIntentId.Word2,
                    original.ClientIntentId.Word3),
                original.Kind,
                original.CompatibilityProfileId,
                original.DriveReference,
                original.FeedbackSocket,
                original.TimeoutMilliseconds,
                new LMCEncoderMaintenanceCompatibilityEvidenceId(
                    original.CompatibilityEvidenceId.Word0,
                    original.CompatibilityEvidenceId.Word1,
                    original.CompatibilityEvidenceId.Word2,
                    original.CompatibilityEvidenceId.Word3));

            AssertEx.True(original.Equals(restored));
            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "54 7E 00 00 48 00 00 00 "
                    + "01 00 00 00 88 77 66 55 "
                    + "A4 A3 A2 A1 B4 B3 B2 B1 C4 C3 C2 C1 "
                    + "44 33 22 11 "
                    + "04 03 02 01 08 07 06 05 "
                    + "0C 0B 0A 09 10 0F 0E 0D "
                    + "02 00 34 12 03 00 04 00 "
                    + "01 00 00 00 34 12 00 00 "
                    + "14 13 12 11 18 17 16 15 "
                    + "1C 1B 1A 19 20 1F 1E 1D"),
                LMC_DiagnosticsFrame.ReadEncoderMaintenanceOutcome(
                    QueryRequestId,
                    restored));

            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "55 7E 00 00 4C 00 00 00 "
                    + "01 00 00 00 88 77 66 55 "
                    + "A4 A3 A2 A1 B4 B3 B2 B1 C4 C3 C2 C1 "
                    + "44 33 22 11 "
                    + "04 03 02 01 08 07 06 05 "
                    + "0C 0B 0A 09 10 0F 0E 0D "
                    + "02 00 34 12 03 00 04 00 "
                    + "01 00 00 00 34 12 00 00 "
                    + "14 13 12 11 18 17 16 15 "
                    + "1C 1B 1A 19 20 1F 1E 1D "
                    + "0D F0 AD 0B"),
                LMC_DiagnosticsFrame.RetireEncoderMaintenanceOutcome(
                    QueryRequestId,
                    restored,
                    RecordGeneration));
        }

        private static void StartStrictParser()
        {
            var key = RecoveryKey(
                LMCEncoderMaintenanceKind.Tw20ErrorWarningReset);
            var payload = StartResponsePayload(key);
            var parsed = LMC_DiagnosticsParser.ParseStartEncoderMaintenance(
                TestFrame.Response(0, payload),
                key);

            AssertEx.Equal(key.Kind, parsed.Kind);
            AssertEx.Equal(key.FeedbackSocket, parsed.FeedbackSocket);
            AssertEx.Equal(1u, parsed.CommandValue);
            AssertEx.Equal(RecordGeneration, parsed.RecordGeneration);
            AssertEx.Equal(OwnerGeneration, parsed.OwnerGeneration);
            AssertEx.Equal(100u, parsed.StartCycle);

            var wrongCommandValue = StartResponsePayload(key);
            TestFrame.WriteUInt32(wrongCommandValue, 24, 3);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseStartEncoderMaintenance(
                    TestFrame.Response(0, wrongCommandValue),
                    key));

            var zeroGeneration = StartResponsePayload(key);
            TestFrame.WriteUInt32(zeroGeneration, 28, 0);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseStartEncoderMaintenance(
                    TestFrame.Response(0, zeroGeneration),
                    key));
        }

        private static void OutcomeStates()
        {
            var key = RecoveryKey(
                LMCEncoderMaintenanceKind.Tw19MultiturnPositionReset);
            var running = LMC_DiagnosticsParser
                .ParseEncoderMaintenanceOutcome(
                    TestFrame.Response(
                        0,
                        OutcomePayload(
                            key,
                            QueryRequestId,
                            LMCEncoderMaintenanceOutcomeRecordState.Running)),
                    QueryRequestId,
                    key);
            AssertEx.False(running.IsTerminal);
            AssertEx.False(running.IsSuccessful);
            AssertEx.Equal(1u, key.CommandValue);

            var succeeded = LMC_DiagnosticsParser
                .ParseEncoderMaintenanceOutcome(
                    TestFrame.Response(
                        0,
                        OutcomePayload(
                            key,
                            QueryRequestId,
                            LMCEncoderMaintenanceOutcomeRecordState.Succeeded)),
                    QueryRequestId,
                    key);
            AssertEx.True(succeeded.IsTerminal);
            AssertEx.True(succeeded.IsSuccessful);
            AssertEx.False(succeeded.IsPhysicalEffectVerified);
            AssertEx.Equal(0x000003FFu, succeeded.VerificationFlagsValue);
            AssertEx.Equal(12345, succeeded.ActualPosition);

            var failed = LMC_DiagnosticsParser
                .ParseEncoderMaintenanceOutcome(
                    TestFrame.Response(
                        0,
                        OutcomePayload(
                            key,
                            QueryRequestId,
                            LMCEncoderMaintenanceOutcomeRecordState.Failed)),
                    QueryRequestId,
                    key);
            AssertEx.True(failed.IsTerminal);
            AssertEx.False(failed.IsSuccessful);
            AssertEx.Equal(
                (uint)LMCEncoderMaintenanceDetailCode.ExecutionFailed,
                failed.OriginalDetailCode);

            var aborted = LMC_DiagnosticsParser
                .ParseEncoderMaintenanceOutcome(
                    TestFrame.Response(
                        0,
                        OutcomePayload(
                            key,
                            QueryRequestId,
                            LMCEncoderMaintenanceOutcomeRecordState.Aborted)),
                    QueryRequestId,
                    key);
            AssertEx.Equal(
                (uint)LMCEncoderMaintenanceDetailCode.Aborted,
                aborted.OriginalDetailCode);
        }

        private static void MalformedOutcomeRejected()
        {
            var key = RecoveryKey(
                LMCEncoderMaintenanceKind.Tw20ErrorWarningReset);

            AssertMalformed(
                key,
                payload => TestFrame.WriteUInt16(payload, 58, 1));
            AssertMalformed(
                key,
                payload => TestFrame.WriteUInt32(payload, 60, 3));
            AssertMalformed(
                key,
                payload => TestFrame.WriteUInt32(payload, 32, 7));
            AssertMalformed(
                key,
                payload => TestFrame.WriteUInt32(payload, 148, 0));
            AssertMalformed(
                key,
                payload => TestFrame.WriteUInt32(payload, 112, 0x80000000u));
            AssertMalformed(
                key,
                payload => TestFrame.WriteUInt16(payload, 16, 9));

            var runningWithCompletion = OutcomePayload(
                key,
                QueryRequestId,
                LMCEncoderMaintenanceOutcomeRecordState.Running);
            TestFrame.WriteUInt32(runningWithCompletion, 104, 108);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseEncoderMaintenanceOutcome(
                    TestFrame.Response(0, runningWithCompletion),
                    QueryRequestId,
                    key));

            var truncated = OutcomePayload(
                key,
                QueryRequestId,
                LMCEncoderMaintenanceOutcomeRecordState.Succeeded);
            Array.Resize(ref truncated, truncated.Length - 1);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseEncoderMaintenanceOutcome(
                    TestFrame.Response(0, truncated),
                    QueryRequestId,
                    key));
        }

        private static void ExactRetirementSnapshot()
        {
            var key = RecoveryKey(
                LMCEncoderMaintenanceKind.Tw20ErrorWarningReset);
            var terminal = LMC_DiagnosticsParser
                .ParseEncoderMaintenanceOutcome(
                    TestFrame.Response(
                        0,
                        OutcomePayload(
                            key,
                            QueryRequestId,
                            LMCEncoderMaintenanceOutcomeRecordState.Succeeded)),
                    QueryRequestId,
                    key);

            var retirementPayload = OutcomePayload(
                key,
                QueryRequestId + 1,
                LMCEncoderMaintenanceOutcomeRecordState.Succeeded);
            var retired = LMC_DiagnosticsParser
                .ParseEncoderMaintenanceOutcomeRetirement(
                    TestFrame.Response(0, retirementPayload),
                    QueryRequestId + 1,
                    terminal);
            AssertEx.Equal(
                terminal.RecordGeneration,
                retired.RecordGeneration);
            AssertEx.True(
                terminal.RecoveryKey.Equals(retired.RecoveryKey));

            TestFrame.WriteInt32(retirementPayload, 144, 12346);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser
                    .ParseEncoderMaintenanceOutcomeRetirement(
                        TestFrame.Response(0, retirementPayload),
                        QueryRequestId + 1,
                        terminal));

            var running = LMC_DiagnosticsParser
                .ParseEncoderMaintenanceOutcome(
                    TestFrame.Response(
                        0,
                        OutcomePayload(
                            key,
                            QueryRequestId,
                            LMCEncoderMaintenanceOutcomeRecordState.Running)),
                    QueryRequestId,
                    key);
            AssertEx.Throws<ArgumentException>(
                () => LMC_DiagnosticsParser
                    .ParseEncoderMaintenanceOutcomeRetirement(
                        TestFrame.Response(0, retirementPayload),
                        QueryRequestId + 1,
                        running));
        }

        private static void CommandValueIsFixedOne()
        {
            var evidence = Evidence();
            var tw20 = new LMCTw20EncoderErrorWarningResetRequest(
                1,
                100);
            var tw19 = new LMCTw19MultiturnPositionResetRequest(
                1,
                1,
                LMCEncoderFeedbackSocket.Socket2,
                100,
                evidence);

            AssertEx.Equal(1u, tw20.CommandValue);
            AssertEx.Equal(1u, tw19.CommandValue);
            AssertEx.Equal(
                LMCEncoderMaintenanceSdoContract
                    .WireSchemaCompatibilityProfileId,
                tw20.CompatibilityProfileId);
            AssertEx.Equal(
                LMCEncoderMaintenanceSdoContract.WireSchemaFeedbackSocket,
                tw20.FeedbackSocket);
            AssertEx.True(tw20.CompatibilityEvidenceId != null);
            AssertEx.Equal(
                1u,
                LMCEncoderMaintenanceSdoContract.ResetCommandValue);
            AssertEx.Equal(
                1u << 18,
                LMCEncoderMaintenanceCapabilities.Tw20ErrorWarningReset);
            AssertEx.Equal(
                1u << 19,
                LMCEncoderMaintenanceCapabilities
                    .Tw19MultiturnPositionReset);
        }

        private static void TimeoutIsOverallMilliseconds()
        {
            var evidence = Evidence();
            var request = new LMCTw20EncoderErrorWarningResetRequest(
                1,
                1,
                LMCEncoderFeedbackSocket.Socket1,
                60000,
                evidence);
            AssertEx.Equal(60000u, request.TimeoutMilliseconds);
            AssertEx.True(
                typeof(LMCEncoderMaintenanceRequest).GetProperty(
                    "TimeoutMilliseconds") != null);
            AssertEx.True(
                typeof(LMCEncoderMaintenanceRequest).GetProperty(
                    "TimeoutCycles") == null);
            AssertEx.True(
                typeof(LMCEncoderMaintenanceRecoveryKey).GetProperty(
                    "TimeoutMilliseconds") != null);
            AssertEx.True(
                typeof(LMCEncoderMaintenanceRecoveryKey).GetProperty(
                    "TimeoutCycles") == null);

            var zeroTimeout = AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new LMCTw20EncoderErrorWarningResetRequest(
                    1,
                    1,
                    LMCEncoderFeedbackSocket.Socket1,
                    0,
                    evidence));
            AssertEx.Equal(
                "timeoutMilliseconds",
                zeroTimeout.ParamName);
            var excessiveTimeout = AssertEx
                .Throws<ArgumentOutOfRangeException>(
                () => new LMCTw19MultiturnPositionResetRequest(
                    1,
                    1,
                    LMCEncoderFeedbackSocket.Socket1,
                    60001,
                    evidence));
            AssertEx.Equal(
                "timeoutMilliseconds",
                excessiveTimeout.ParamName);
        }

        private static void DedicatedSdoObject()
        {
            var evidence = Evidence();
            var tw20 = new LMCTw20EncoderErrorWarningResetRequest(
                1,
                1,
                LMCEncoderFeedbackSocket.Socket1,
                100,
                evidence);
            var tw19 = new LMCTw19MultiturnPositionResetRequest(
                1,
                1,
                LMCEncoderFeedbackSocket.Socket1,
                100,
                evidence);

            AssertEx.Equal((ushort)0x20FC, tw20.ObjectIndex);
            AssertEx.Equal((byte)0x02, tw20.SubIndex);
            AssertEx.Equal((byte)0x01, tw19.SubIndex);
            AssertEx.Equal(LMCSignalValueType.UInt16, tw20.ValueType);
            AssertEx.Equal((ushort)2, tw20.WriteLength);
            AssertEx.Equal(
                (ushort)0x20FC,
                LMCEncoderMaintenanceSdoContract.ObjectIndex);
            AssertEx.Equal(
                (byte)0x01,
                LMCEncoderMaintenanceSdoContract.SubIndex(
                    LMCEncoderMaintenanceKind
                        .Tw19MultiturnPositionReset));
            AssertEx.Equal(
                (byte)0x02,
                LMCEncoderMaintenanceSdoContract.SubIndex(
                    LMCEncoderMaintenanceKind.Tw20ErrorWarningReset));

            var key = RecoveryKey(
                LMCEncoderMaintenanceKind.Tw20ErrorWarningReset);
            AssertEx.Equal((ushort)0x20FC, key.ObjectIndex);
            AssertEx.Equal((byte)0x02, key.SubIndex);
            AssertEx.Equal(LMCSignalValueType.UInt16, key.ValueType);
            AssertEx.Equal((ushort)2, key.WriteLength);
        }

        private static void Tw20StartQueryRetireSync()
        {
            StartQueryRetireSync(
                LMCEncoderMaintenanceKind.Tw20ErrorWarningReset);
        }

        private static void Tw19StartQueryRetireSync()
        {
            StartQueryRetireSync(
                LMCEncoderMaintenanceKind.Tw19MultiturnPositionReset);
        }

        private static void StartQueryRetireSync(
            LMCEncoderMaintenanceKind kind)
        {
            LMCEncoderMaintenanceRecoveryKey preparedKey = null;
            var startStep = new FakeRpcStep(0x7E53, new byte[0])
            {
                ResponseFactory = request => TestFrame.Response(
                    0,
                    StartResponsePayload(
                        preparedKey,
                        TestFrame.ReadUInt32(request, 12)))
            };
            var queryStep = new FakeRpcStep(0x7E54, new byte[0])
            {
                ResponseFactory = request => TestFrame.Response(
                    0,
                    OutcomePayload(
                        preparedKey,
                        TestFrame.ReadUInt32(request, 12),
                        LMCEncoderMaintenanceOutcomeRecordState.Succeeded))
            };
            var retireStep = new FakeRpcStep(0x7E55, new byte[0])
            {
                ResponseFactory = request => TestFrame.Response(
                    0,
                    OutcomePayload(
                        preparedKey,
                        TestFrame.ReadUInt32(request, 12),
                        LMCEncoderMaintenanceOutcomeRecordState.Succeeded))
            };

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CapabilitiesStep(),
                CapabilitiesStep(),
                startStep,
                CapabilitiesStep(),
                queryStep,
                CapabilitiesStep(),
                retireStep,
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var capabilities = connection.Diagnostics.GetCapabilities();
                LMCPreparedEncoderMaintenance prepared;
                if (kind
                    == LMCEncoderMaintenanceKind.Tw20ErrorWarningReset)
                {
                    prepared = connection.Diagnostics
                        .PrepareTw20EncoderErrorWarningReset(
                            new LMCTw20EncoderErrorWarningResetRequest(
                                3,
                                1000),
                            capabilities,
                            LMCTw20EncoderErrorWarningResetExecuteToken
                                .Create());
                }
                else
                {
                    prepared = connection.Diagnostics
                        .PrepareTw19MultiturnPositionReset(
                            new LMCTw19MultiturnPositionResetRequest(
                                3,
                                1000),
                            capabilities,
                            LMCTw19MultiturnPositionResetExecuteToken
                                .Create());
                }
                preparedKey = prepared.RecoveryKey;

                var acknowledgement = connection.Diagnostics
                    .StartEncoderMaintenance(prepared);
                AssertEx.Equal(preparedKey, acknowledgement.RecoveryKey);
                AssertEx.True(prepared.IsConsumed);

                var outcome = connection.Diagnostics
                    .ReadEncoderMaintenanceOutcome(preparedKey);
                AssertEx.True(outcome.IsSuccessful);
                AssertEx.False(outcome.IsPhysicalEffectVerified);
                AssertEx.Equal(0x000003FFu, outcome.VerificationFlagsValue);

                var retirement = connection.Diagnostics
                    .RetireEncoderMaintenanceOutcome(outcome);
                AssertEx.Equal(preparedKey, retirement.RecoveryKey);
                AssertEx.Equal(RecordGeneration, retirement.RecordGeneration);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x7E53));
                AssertEx.Equal(1, CountCommand(server, 0x7E54));
                AssertEx.Equal(1, CountCommand(server, 0x7E55));
            }
        }

        private static void AssertMalformed(
            LMCEncoderMaintenanceRecoveryKey key,
            Action<byte[]> mutation)
        {
            var payload = OutcomePayload(
                key,
                QueryRequestId,
                LMCEncoderMaintenanceOutcomeRecordState.Succeeded);
            mutation(payload);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseEncoderMaintenanceOutcome(
                    TestFrame.Response(0, payload),
                    QueryRequestId,
                    key));
        }

        private static LMCEncoderMaintenanceRecoveryKey RecoveryKey(
            LMCEncoderMaintenanceKind kind)
        {
            return new LMCEncoderMaintenanceRecoveryKey(
                LMCDiagnostics.ProtocolSchemaVersion,
                OriginalRequestId,
                DiagnosticsBuild,
                DiagnosticsBootId,
                MapRevision,
                new LMCEncoderMaintenanceClientIntentId(
                    0x01020304u,
                    0x05060708u,
                    0x090A0B0Cu,
                    0x0D0E0F10u),
                kind,
                0x1234,
                3,
                LMCEncoderFeedbackSocket.Socket4,
                0x1234,
                Evidence());
        }

        private static LMCEncoderMaintenanceCompatibilityEvidenceId Evidence()
        {
            return new LMCEncoderMaintenanceCompatibilityEvidenceId(
                0x11121314u,
                0x15161718u,
                0x191A1B1Cu,
                0x1D1E1F20u);
        }

        private static byte[] StartResponsePayload(
            LMCEncoderMaintenanceRecoveryKey key,
            uint requestId = OriginalRequestId)
        {
            var payload = CommonSuccessPayload(
                requestId,
                LMC_DiagnosticsParser
                    .StartEncoderMaintenanceResponsePayloadLength);
            TestFrame.WriteUInt16(payload, 16, (ushort)key.Kind);
            TestFrame.WriteUInt16(
                payload,
                18,
                key.CompatibilityProfileId);
            TestFrame.WriteUInt16(payload, 20, key.DriveReference);
            TestFrame.WriteUInt16(
                payload,
                22,
                (ushort)key.FeedbackSocket);
            TestFrame.WriteUInt32(payload, 24, key.CommandValue);
            TestFrame.WriteUInt32(payload, 28, RecordGeneration);
            TestFrame.WriteUInt32(payload, 32, OwnerGeneration);
            TestFrame.WriteUInt32(payload, 36, 100);
            return payload;
        }

        private static byte[] OutcomePayload(
            LMCEncoderMaintenanceRecoveryKey key,
            uint requestId,
            LMCEncoderMaintenanceOutcomeRecordState state)
        {
            var payload = CommonSuccessPayload(
                requestId,
                LMC_DiagnosticsParser
                    .EncoderMaintenanceOutcomeResponsePayloadLength);
            TestFrame.WriteUInt16(payload, 16, (ushort)state);
            TestFrame.WriteUInt16(payload, 18, (ushort)key.Kind);
            TestFrame.WriteUInt32(payload, 20, key.DiagnosticsBuild);
            TestFrame.WriteUInt32(payload, 24, key.DiagnosticsBootId);
            TestFrame.WriteUInt32(payload, 28, key.MapRevision);
            TestFrame.WriteUInt32(payload, 32, key.OriginalRequestId);
            TestFrame.WriteUInt32(payload, 36, key.ClientIntentId.Word0);
            TestFrame.WriteUInt32(payload, 40, key.ClientIntentId.Word1);
            TestFrame.WriteUInt32(payload, 44, key.ClientIntentId.Word2);
            TestFrame.WriteUInt32(payload, 48, key.ClientIntentId.Word3);
            TestFrame.WriteUInt16(
                payload,
                52,
                key.CompatibilityProfileId);
            TestFrame.WriteUInt16(payload, 54, key.DriveReference);
            TestFrame.WriteUInt16(
                payload,
                56,
                (ushort)key.FeedbackSocket);
            TestFrame.WriteUInt32(payload, 60, key.CommandValue);
            TestFrame.WriteUInt32(
                payload,
                64,
                key.TimeoutMilliseconds);
            TestFrame.WriteUInt32(
                payload,
                68,
                key.CompatibilityEvidenceId.Word0);
            TestFrame.WriteUInt32(
                payload,
                72,
                key.CompatibilityEvidenceId.Word1);
            TestFrame.WriteUInt32(
                payload,
                76,
                key.CompatibilityEvidenceId.Word2);
            TestFrame.WriteUInt32(
                payload,
                80,
                key.CompatibilityEvidenceId.Word3);

            TestFrame.WriteUInt32(payload, 96, 100);
            TestFrame.WriteUInt32(payload, 108, 1);
            TestFrame.WriteUInt32(payload, 112, 0x00000007u);
            TestFrame.WriteUInt32(payload, 116, 0x11111111u);
            TestFrame.WriteUInt32(payload, 120, 0x22222222u);
            TestFrame.WriteUInt32(payload, 124, 0x33333333u);
            TestFrame.WriteUInt32(payload, 128, 0x44444444u);
            TestFrame.WriteUInt16(payload, 132, 0x1234);
            TestFrame.WriteInt32(payload, 136, 0);
            TestFrame.WriteUInt32(payload, 140, 0);
            TestFrame.WriteInt32(payload, 144, 12345);
            TestFrame.WriteUInt32(payload, 148, RecordGeneration);
            TestFrame.WriteUInt32(payload, 152, OwnerGeneration);

            switch (state)
            {
                case LMCEncoderMaintenanceOutcomeRecordState.Running:
                    break;
                case LMCEncoderMaintenanceOutcomeRecordState.Succeeded:
                    TestFrame.WriteUInt32(payload, 100, 104);
                    TestFrame.WriteUInt32(payload, 104, 108);
                    TestFrame.WriteUInt32(payload, 108, 0);
                    TestFrame.WriteUInt32(payload, 112, 0x000003FFu);
                    break;
                case LMCEncoderMaintenanceOutcomeRecordState.Failed:
                    TestFrame.WriteUInt16(payload, 84, 1);
                    TestFrame.WriteInt16(payload, 86, -32000);
                    TestFrame.WriteUInt32(
                        payload,
                        88,
                        (uint)LMCEncoderMaintenanceDetailCode.ExecutionFailed);
                    TestFrame.WriteUInt32(payload, 92, 0x06020000u);
                    TestFrame.WriteUInt32(payload, 104, 108);
                    break;
                case LMCEncoderMaintenanceOutcomeRecordState.Aborted:
                    TestFrame.WriteUInt16(payload, 84, 1);
                    TestFrame.WriteInt16(payload, 86, -32000);
                    TestFrame.WriteUInt32(
                        payload,
                        88,
                        (uint)LMCEncoderMaintenanceDetailCode.Aborted);
                    TestFrame.WriteUInt32(payload, 104, 108);
                    break;
            }

            return payload;
        }

        private static byte[] EncoderCapabilitiesPayload(uint requestId)
        {
            var capabilityBits =
                (uint)(LMCDiagnosticCapability.EncoderTw20ErrorWarningReset
                    | LMCDiagnosticCapability
                        .EncoderTw19MultiturnPositionReset);
            var payload = new byte[68];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, requestId);
            TestFrame.WriteUInt32(payload, 16, DiagnosticsBuild);
            TestFrame.WriteUInt32(payload, 20, capabilityBits);
            TestFrame.WriteUInt32(payload, 24, MapRevision);
            TestFrame.WriteUInt16(payload, 28, 24);
            TestFrame.WriteUInt16(payload, 30, 32);
            TestFrame.WriteUInt16(payload, 32, 32);
            TestFrame.WriteUInt16(payload, 34, 2);
            TestFrame.WriteUInt32(payload, 36, 31250);
            TestFrame.WriteUInt32(payload, 40, 1000);
            TestFrame.WriteUInt16(payload, 44, 1320);
            TestFrame.WriteUInt16(payload, 46, 2040);
            TestFrame.WriteUInt16(payload, 48, 1280);
            TestFrame.WriteUInt16(payload, 50, 80);
            TestFrame.WriteUInt16(payload, 52, 16);
            TestFrame.WriteUInt32(payload, 56, 4000000);
            TestFrame.WriteUInt16(payload, 60, 12);
            TestFrame.WriteUInt32(payload, 64, DiagnosticsBootId);
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
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")));
        }

        private static FakeRpcStep CapabilitiesStep()
        {
            return new FakeRpcStep(0x7E00, new byte[0])
            {
                ResponseFactory = request => TestFrame.Response(
                    0,
                    EncoderCapabilitiesPayload(
                        TestFrame.ReadUInt32(request, 12)))
            };
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

        private static byte[] CommonSuccessPayload(
            uint requestId,
            int payloadLength)
        {
            var payload = new byte[payloadLength];
            TestFrame.WriteUInt16(
                payload,
                0,
                LMCDiagnostics.ProtocolSchemaVersion);
            TestFrame.WriteUInt32(payload, 8, requestId);
            return payload;
        }
    }
}

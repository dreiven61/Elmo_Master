using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class DiagnosticsParserDeterministicFuzzTests
    {
        internal const uint RequestId = 0x6D4A2B19u;
        internal const uint TopologyRevision = 0xA1B2C3D4u;
        internal const uint NodeId = 0xEC000101u;
        internal const uint IOReference = 0x0000FF01u;
        internal const uint SlotNodeId = 0xEC010001u;
        internal const uint TicketId = 0x10203040u;
        internal const uint DiagnosticsBootId = 0x89ABCDEFu;
        internal const uint SubmissionRevision = 0x55667788u;
        internal const uint RecorderConfigId = 0x0BADB002u;
        internal const uint RecorderConfigRevision = 0x01020304u;
        internal const uint RecorderOwnerSessionEpoch = 0x10293847u;
        internal static readonly Guid RecorderRecoveryToken =
            new Guid("7e4c7e4d-1020-3040-5060-708090a0b0c0");
        private const int RandomMutationCount = 192;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Property.DiagnosticsParser.TopologyIoDeterministicFuzz",
                TopologyIoDeterministicFuzz);
            tests.Add(
                "Property.DiagnosticsParser.D5VariableInlineDeterministicFuzz",
                D5VariableInlineDeterministicFuzz);
            tests.Add(
                "Property.DiagnosticsParser.RecorderRecoverableDeterministicFuzz",
                RecorderRecoverableDeterministicFuzz);
        }

        private static void TopologyIoDeterministicFuzz()
        {
            var topologyInfoPayload = CreateTopologyInfoPayload();
            ExercisePayload(
                "TopologyInfo",
                topologyInfoPayload,
                0x13572468,
                raw => LMC_DiagnosticsParser.ParseEtherCATTopologyInfo(
                    raw,
                    RequestId),
                ValidateTopologyInfo);
            AssertInvalidData(
                "TopologyInfo.Reserved",
                MutatePayload(topologyInfoPayload, 40, 1),
                raw => LMC_DiagnosticsParser.ParseEtherCATTopologyInfo(
                    raw,
                    RequestId));

            var topologyChunkPayload = CreateTopologyChunkPayload();
            ExercisePayload(
                "TopologyChunk",
                topologyChunkPayload,
                0x24681357,
                raw => LMC_DiagnosticsParser.ParseEtherCATTopologyChunk(
                    raw,
                    RequestId,
                    TopologyRevision,
                    0,
                    1),
                chunk => ValidateTopologyChunk(chunk, 0, 1));
            AssertInvalidData(
                "TopologyChunk.EntryReservedByte",
                MutatePayload(topologyChunkPayload, 28 + 13, 1),
                raw => LMC_DiagnosticsParser.ParseEtherCATTopologyChunk(
                    raw,
                    RequestId,
                    TopologyRevision,
                    0,
                    1));
            var badEntryReserved = (byte[])topologyChunkPayload.Clone();
            TestFrame.WriteUInt16(badEntryReserved, 28 + 22, 1);
            AssertInvalidData(
                "TopologyChunk.EntryReservedWord",
                badEntryReserved,
                raw => LMC_DiagnosticsParser.ParseEtherCATTopologyChunk(
                    raw,
                    RequestId,
                    TopologyRevision,
                    0,
                    1));
            AssertInvalidData(
                "TopologyChunk.NamePadding",
                MutatePayload(topologyChunkPayload, 28 + 44 + 12, 1),
                raw => LMC_DiagnosticsParser.ParseEtherCATTopologyChunk(
                    raw,
                    RequestId,
                    TopologyRevision,
                    0,
                    1));
            AssertInvalidData(
                "TopologyChunk.NameNonAscii",
                MutatePayload(topologyChunkPayload, 28 + 44, 0x80),
                raw => LMC_DiagnosticsParser.ParseEtherCATTopologyChunk(
                    raw,
                    RequestId,
                    TopologyRevision,
                    0,
                    1));

            var twoEntryChunkPayload = CreateTwoEntryTopologyChunkPayload();
            ExercisePayload(
                "TopologyChunk.TwoEntryNonLastWithSlot",
                twoEntryChunkPayload,
                0x10293847,
                raw => LMC_DiagnosticsParser.ParseEtherCATTopologyChunk(
                    raw,
                    RequestId,
                    TopologyRevision,
                    0,
                    2),
                chunk => ValidateTopologyChunk(chunk, 0, 2),
                48,
                false);

            var nodeHealthPayload = CreateNodeHealthPayload();
            ExercisePayload(
                "NodeHealth",
                nodeHealthPayload,
                0x31415926,
                raw => LMC_DiagnosticsParser.ParseEtherCATNodeHealth(
                    raw,
                    RequestId,
                    TopologyRevision,
                    NodeId),
                ValidateNodeHealth);
            var badCapturePhase = (byte[])nodeHealthPayload.Clone();
            TestFrame.WriteUInt16(badCapturePhase, 24, ushort.MaxValue);
            AssertInvalidData(
                "NodeHealth.CapturePhase",
                badCapturePhase,
                raw => LMC_DiagnosticsParser.ParseEtherCATNodeHealth(
                    raw,
                    RequestId,
                    TopologyRevision,
                    NodeId));
            AssertInvalidData(
                "NodeHealth.OnlineEncoding",
                MutatePayload(nodeHealthPayload, 44, 2),
                raw => LMC_DiagnosticsParser.ParseEtherCATNodeHealth(
                    raw,
                    RequestId,
                    TopologyRevision,
                    NodeId));

            var offlineNodeHealthPayload = CreateOfflineNodeHealthPayload();
            ExercisePayload(
                "NodeHealth.OfflineDefaulted",
                offlineNodeHealthPayload,
                0x11235813,
                raw => LMC_DiagnosticsParser.ParseEtherCATNodeHealth(
                    raw,
                    RequestId,
                    TopologyRevision,
                    NodeId),
                ValidateNodeHealth,
                48,
                false);

            var request = new LMCDigitalIOReadRequest(
                TopologyRevision,
                IOReference,
                LMCDigitalIODirection.Output,
                64);
            var digitalIoPayload = CreateDigitalIoPayload();
            ExercisePayload(
                "DigitalIO",
                digitalIoPayload,
                0x27182818,
                raw => LMC_DiagnosticsParser.ParseDigitalIO(
                    raw,
                    RequestId,
                    request),
                value => ValidateDigitalIo(value, request));
            AssertInvalidData(
                "DigitalIO.Width",
                MutatePayload(digitalIoPayload, 29, 63),
                raw => LMC_DiagnosticsParser.ParseDigitalIO(
                    raw,
                    RequestId,
                    request));

            var inputRequest = new LMCDigitalIOReadRequest(
                TopologyRevision,
                IOReference,
                LMCDigitalIODirection.Input,
                16);
            ExercisePayload(
                "DigitalIO.ValidInput16",
                CreateDigitalIoPayload(
                    LMCDigitalIODirection.Input,
                    16,
                    LMCDigitalIOStatusFlags.Valid,
                    0xA55Au,
                    0xFFFFu,
                    0),
                0x16180339,
                raw => LMC_DiagnosticsParser.ParseDigitalIO(
                    raw,
                    RequestId,
                    inputRequest),
                value => ValidateDigitalIo(value, inputRequest),
                48,
                false);

            var defaultedOutputRequest = new LMCDigitalIOReadRequest(
                TopologyRevision,
                IOReference,
                LMCDigitalIODirection.Output,
                32);
            ExercisePayload(
                "DigitalIO.DefaultedOutput32",
                CreateDigitalIoPayload(
                    LMCDigitalIODirection.Output,
                    32,
                    LMCDigitalIOStatusFlags.NodeOffline
                        | LMCDigitalIOStatusFlags.DataDefaulted,
                    0,
                    0,
                    0x01020305u),
                0x57721566,
                raw => LMC_DiagnosticsParser.ParseDigitalIO(
                    raw,
                    RequestId,
                    defaultedOutputRequest),
                value => ValidateDigitalIo(value, defaultedOutputRequest),
                48,
                false);

            var invalidValue = CreateDigitalIoPayload(
                LMCDigitalIODirection.Output,
                32,
                LMCDigitalIOStatusFlags.NodeOffline
                    | LMCDigitalIOStatusFlags.DataDefaulted,
                1,
                0,
                0x01020305u);
            AssertInvalidData(
                "DigitalIO.DefaultedNonzeroValue",
                invalidValue,
                raw => LMC_DiagnosticsParser.ParseDigitalIO(
                    raw,
                    RequestId,
                    defaultedOutputRequest));

            var missingDefaulted = CreateDigitalIoPayload(
                LMCDigitalIODirection.Output,
                32,
                LMCDigitalIOStatusFlags.NodeOffline,
                0,
                0,
                0x01020305u);
            AssertInvalidData(
                "DigitalIO.MissingDataDefaulted",
                missingDefaulted,
                raw => LMC_DiagnosticsParser.ParseDigitalIO(
                    raw,
                    RequestId,
                    defaultedOutputRequest));

            var defaultedWithoutCause = CreateDigitalIoPayload(
                LMCDigitalIODirection.Output,
                32,
                LMCDigitalIOStatusFlags.DataDefaulted,
                0,
                0,
                0x01020305u);
            AssertInvalidData(
                "DigitalIO.DataDefaultedWithoutCause",
                defaultedWithoutCause,
                raw => LMC_DiagnosticsParser.ParseDigitalIO(
                    raw,
                    RequestId,
                    defaultedOutputRequest));
        }

        private static void D5VariableInlineDeterministicFuzz()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = new LMCOperationTicket(
                    TicketId,
                    LMCOperationKind.SDORead,
                    100,
                    DiagnosticsBootId,
                    SubmissionRevision,
                    0,
                    connection.Diagnostics,
                    true,
                    4,
                    LMCSignalValueType.UInt32);
                var payload = CreateD5VariableInlineStatusPayload(ticket);

                ExercisePayload(
                    "D5VariableInlineStatus",
                    payload,
                    0x5A17C0DE,
                    raw => LMC_DiagnosticsParser.ParseOperationStatus(
                        raw,
                        RequestId,
                        ticket),
                    status => ValidateD5Status(status, ticket));

                AssertInvalidData(
                    "D5VariableInlineStatus.Reserved",
                    MutatePayload(payload, 46, 1),
                    raw => LMC_DiagnosticsParser.ParseOperationStatus(
                        raw,
                        RequestId,
                        ticket));
                AssertInvalidData(
                    "D5VariableInlineStatus.ResultDataLength",
                    MutatePayload(payload, 45, 12),
                    raw => LMC_DiagnosticsParser.ParseOperationStatus(
                        raw,
                        RequestId,
                        ticket));
                AssertInvalidData(
                    "D5VariableInlineStatus.UnusedResultTail",
                    MutatePayload(payload, 52, 1),
                    raw => LMC_DiagnosticsParser.ParseOperationStatus(
                        raw,
                        RequestId,
                        ticket));

                ExerciseCompletedD5Seed(
                    connection,
                    "D5VariableInlineStatus.Bool1",
                    TicketId + 1,
                    1,
                    LMCSignalValueType.Bool,
                    new byte[] { 1 },
                    0x11010001);
                ExerciseCompletedD5Seed(
                    connection,
                    "D5VariableInlineStatus.BitField16",
                    TicketId + 2,
                    2,
                    LMCSignalValueType.BitField16,
                    new byte[] { 0x37, 0x12 },
                    0x11020002);
                ExerciseCompletedD5Seed(
                    connection,
                    "D5VariableInlineStatus.UInt32Length8",
                    TicketId + 3,
                    8,
                    LMCSignalValueType.UInt32,
                    new byte[]
                    {
                        0x01, 0x23, 0x45, 0x67,
                        0x89, 0xAB, 0xCD, 0xEF
                    },
                    0x11080008);
                ExerciseCompletedD5Seed(
                    connection,
                    "D5VariableInlineStatus.BitField32Length12",
                    TicketId + 4,
                    12,
                    LMCSignalValueType.BitField32,
                    new byte[]
                    {
                        0x00, 0x11, 0x22, 0x33,
                        0x44, 0x55, 0x66, 0x77,
                        0x88, 0x99, 0xAA, 0xBB
                    },
                    0x11120012);

                AssertValidD5State(
                    "D5VariableInlineStatus.Running",
                    ticket,
                    CreateD5StatusPayload(
                        ticket,
                        LMCOperationState.Running,
                        LMCOperationOutcome.NoneOrPending,
                        0,
                        0,
                        0,
                        LMCSignalValueType.Invalid,
                        new byte[0]));
                AssertValidD5State(
                    "D5VariableInlineStatus.Failed",
                    ticket,
                    CreateD5StatusPayload(
                        ticket,
                        LMCOperationState.Failed,
                        LMCOperationOutcome.Failed,
                        200,
                        LMC_DiagnosticsParser.DiagnosticsErrorId,
                        0x05040005u,
                        LMCSignalValueType.Invalid,
                        new byte[0]));

                var writeTicket = new LMCOperationTicket(
                    TicketId + 5,
                    LMCOperationKind.SDOWrite,
                    100,
                    DiagnosticsBootId,
                    SubmissionRevision,
                    0,
                    connection.Diagnostics,
                    false,
                    0,
                    LMCSignalValueType.Invalid);
                AssertValidD5State(
                    "D5VariableInlineStatus.WriteCompleted",
                    writeTicket,
                    CreateD5StatusPayload(
                        writeTicket,
                        LMCOperationState.Completed,
                        LMCOperationOutcome.Success,
                        200,
                        0,
                        0,
                        LMCSignalValueType.Invalid,
                        new byte[0]));
            }
        }

        private static void RecorderRecoverableDeterministicFuzz()
        {
            var configuration = CreateRecoverableDoubleConfiguration();
            var capabilities = CreateRecoverableCapabilities();
            var configurePayload = CreateRecoverableConfigurePayload();
            ExercisePayload(
                "RecorderRecoverableConfigure",
                configurePayload,
                0x7E4C4C4C,
                raw => LMC_DiagnosticsParser
                    .ParseConfigureRecoverableDoubleRecorder(
                        raw,
                        RequestId,
                        configuration,
                        RecorderRecoveryToken,
                        capabilities,
                        7,
                        null),
                ValidateRecoverableConfigure);
            AssertInvalidData(
                "RecorderRecoverableConfigure.Reserved",
                MutatePayload(configurePayload, 46, 1),
                raw => LMC_DiagnosticsParser
                    .ParseConfigureRecoverableDoubleRecorder(
                        raw,
                        RequestId,
                        configuration,
                        RecorderRecoveryToken,
                        capabilities,
                        7,
                        null));

            var inventoryPayload =
                CreateRecoverableRecorderBankInventoryPayload();
            ExercisePayload(
                "RecorderRecoverableInventory",
                inventoryPayload,
                0x7E4D4D4D,
                raw => LMC_DiagnosticsParser
                    .ParseRecoverableRecorderBankInventory(
                        raw,
                        RequestId,
                        DiagnosticsBootId,
                        RecorderConfigId,
                        TopologyRevision,
                        RecorderRecoveryToken),
                ValidateRecoverableInventory);
            AssertInvalidData(
                "RecorderRecoverableInventory.EmptyState",
                MutatePayload(
                    inventoryPayload,
                    40,
                    (byte)LMCRecorderState.Ready),
                raw => LMC_DiagnosticsParser
                    .ParseRecoverableRecorderBankInventory(
                        raw,
                        RequestId,
                        DiagnosticsBootId,
                        RecorderConfigId,
                        TopologyRevision,
                        RecorderRecoveryToken));
            AssertInvalidData(
                "RecorderRecoverableInventory.EmptyPresentContradiction",
                MutatePayload(inventoryPayload, 44, 1),
                raw => LMC_DiagnosticsParser
                    .ParseRecoverableRecorderBankInventory(
                        raw,
                        RequestId,
                        DiagnosticsBootId,
                        RecorderConfigId,
                        TopologyRevision,
                        RecorderRecoveryToken));

            var absencePayload = CommonPayload(
                LMC_DiagnosticsParser.CommonResponsePayloadLength);
            TestFrame.WriteUInt16(absencePayload, 4, 1);
            TestFrame.WriteInt16(
                absencePayload,
                6,
                LMC_DiagnosticsParser.DiagnosticsErrorId);
            TestFrame.WriteUInt32(
                absencePayload,
                12,
                (uint)LMCDiagnosticsDetailCode
                    .RecorderConfigurationAbsent);
            var absence = AssertExactException<
                LMCRecoverableRecorderConfigurationAbsentException>(
                    "RecorderRecoverableInventory.TypedAbsence",
                    () => LMC_DiagnosticsParser
                        .ParseRecoverableRecorderBankInventory(
                            TestFrame.Response(0, absencePayload),
                            RequestId,
                            DiagnosticsBootId,
                            RecorderConfigId,
                            TopologyRevision,
                            RecorderRecoveryToken));
            AssertEx.Equal(DiagnosticsBootId, absence.DiagnosticsBootId);
            AssertEx.Equal(RecorderConfigId, absence.ConfigId);
            AssertEx.Equal(TopologyRevision, absence.MapRevision);
            AssertEx.Equal(RecorderRecoveryToken, absence.RecoveryToken);
        }

        private static void ExercisePayload<T>(
            string parserName,
            byte[] seedPayload,
            int randomSeed,
            Func<byte[], T> parser,
            Action<T> validate,
            int randomMutationCount = RandomMutationCount,
            bool exerciseCommonEnvelope = true)
            where T : class
        {
            AssertParserOutcome(
                parserName + ".Golden",
                TestFrame.Response(0, seedPayload),
                parser,
                validate,
                true);

            foreach (var payloadLength in MutationLengths(seedPayload.Length))
            {
                AssertParserOutcome(
                    parserName + ".PayloadLength." + payloadLength,
                    TestFrame.Response(
                        0,
                        ResizeCopy(seedPayload, payloadLength)),
                    parser,
                    validate,
                    payloadLength == seedPayload.Length);
            }

            var goldenRaw = TestFrame.Response(0, seedPayload);
            foreach (var rawLength in MutationLengths(goldenRaw.Length))
            {
                AssertParserOutcome(
                    parserName + ".RawLength." + rawLength,
                    ResizeCopy(goldenRaw, rawLength),
                    parser,
                    validate,
                    rawLength == goldenRaw.Length);
            }

            foreach (var declaredLength in new ushort[]
            {
                0,
                1,
                15,
                16,
                checked((ushort)(seedPayload.Length - 1)),
                checked((ushort)seedPayload.Length),
                checked((ushort)(seedPayload.Length + 1)),
                ushort.MaxValue
            })
            {
                var raw = (byte[])goldenRaw.Clone();
                TestFrame.WriteUInt16(raw, 2, declaredLength);
                AssertParserOutcome(
                    parserName + ".DeclaredPayloadLength." + declaredLength,
                    raw,
                    parser,
                    validate,
                    declaredLength == seedPayload.Length);
            }

            foreach (var headerReserved in new uint[]
            {
                1,
                0x01020304u,
                uint.MaxValue
            })
            {
                AssertParserOutcome(
                    parserName + ".HeaderReserved.0x"
                        + headerReserved.ToString("X8"),
                    TestFrame.Response(0, seedPayload, headerReserved),
                    parser,
                    validate,
                    false);
            }

            if (exerciseCommonEnvelope)
            {
                ExerciseCommonEnvelope(
                    parserName,
                    seedPayload,
                    parser);
            }

            var mutationOffsets = BuildMutationOffsets(seedPayload.Length);
            var fixedValues = new byte[]
            {
                0,
                1,
                2,
                0x7F,
                0x80,
                0xFE,
                0xFF
            };
            for (var offsetIndex = 0;
                offsetIndex < mutationOffsets.Length;
                offsetIndex++)
            {
                var offset = mutationOffsets[offsetIndex];
                for (var valueIndex = 0;
                    valueIndex < fixedValues.Length;
                    valueIndex++)
                {
                    var candidate = fixedValues[
                        (offsetIndex + valueIndex) % fixedValues.Length];
                    if (candidate == seedPayload[offset])
                    {
                        continue;
                    }

                    AssertParserOutcome(
                        parserName + ".Offset." + offset
                            + ".Value." + candidate,
                        TestFrame.Response(
                            0,
                            MutatePayload(seedPayload, offset, candidate)),
                        parser,
                        validate,
                        false);
                }
            }

            var random = new Random(randomSeed);
            for (var mutationIndex = 0;
                mutationIndex < randomMutationCount;
                mutationIndex++)
            {
                var payload = (byte[])seedPayload.Clone();
                var mutationWidth = 1 + random.Next(4);
                for (var index = 0; index < mutationWidth; index++)
                {
                    var offset = mutationOffsets[
                        random.Next(mutationOffsets.Length)];
                    var delta = 1 + random.Next(255);
                    payload[offset] ^= checked((byte)delta);
                }

                AssertParserOutcome(
                    parserName + ".Seeded." + mutationIndex,
                    TestFrame.Response(0, payload),
                    parser,
                    validate,
                    false);
            }
        }

        private static void AssertParserOutcome<T>(
            string scenario,
            byte[] raw,
            Func<byte[], T> parser,
            Action<T> validate,
            bool mustSucceed)
            where T : class
        {
            T result;
            try
            {
                result = parser(raw);
            }
            catch (Exception error)
            {
                if (!mustSucceed
                    && error.GetType() == typeof(InvalidDataException))
                {
                    return;
                }

                throw new InvalidOperationException(
                    scenario + " escaped the parser contract with "
                        + error.GetType().FullName + ".",
                    error);
            }

            AssertEx.NotNull(result, scenario + " returned null.");
            try
            {
                validate(result);
            }
            catch (Exception error)
            {
                throw new InvalidOperationException(
                    scenario
                        + " produced a value that violates the independent oracle.",
                    error);
            }
        }

        private static void AssertInvalidData<T>(
            string scenario,
            byte[] payload,
            Func<byte[], T> parser)
        {
            AssertExactException<InvalidDataException>(
                scenario,
                () => parser(TestFrame.Response(0, payload)));
        }

        private static void ExerciseCommonEnvelope<T>(
            string parserName,
            byte[] seedPayload,
            Func<byte[], T> parser)
        {
            var invalidCommandStatus = (byte[])seedPayload.Clone();
            TestFrame.WriteUInt16(invalidCommandStatus, 4, 2);
            AssertInvalidData(
                parserName + ".Common.CommandStatus",
                invalidCommandStatus,
                parser);

            var successWithError = (byte[])seedPayload.Clone();
            TestFrame.WriteInt16(successWithError, 6, 1);
            AssertInvalidData(
                parserName + ".Common.SuccessWithErrorId",
                successWithError,
                parser);

            var successWithDetail = (byte[])seedPayload.Clone();
            TestFrame.WriteUInt32(
                successWithDetail,
                12,
                (uint)LMCDiagnosticsDetailCode.UnsupportedFeature);
            AssertInvalidData(
                parserName + ".Common.SuccessWithDetail",
                successWithDetail,
                parser);

            var domainError = CommonPayload(
                LMC_DiagnosticsParser.CommonResponsePayloadLength);
            TestFrame.WriteUInt16(domainError, 4, 1);
            TestFrame.WriteInt16(
                domainError,
                6,
                LMC_DiagnosticsParser.DiagnosticsErrorId);
            TestFrame.WriteUInt32(
                domainError,
                12,
                (uint)LMCDiagnosticsDetailCode.UnsupportedFeature);
            var commandError = AssertExactException<
                LMCDiagnosticsCommandException>(
                    parserName + ".Common.DomainError",
                    () => parser(TestFrame.Response(0, domainError)));
            AssertEx.Equal(
                LMCDiagnosticsDetailCode.UnsupportedFeature,
                commandError.Response.Detail);
            AssertEx.Equal(RequestId, commandError.Response.RequestId);

            var domainErrorWithoutErrorId = (byte[])domainError.Clone();
            TestFrame.WriteInt16(domainErrorWithoutErrorId, 6, 0);
            AssertInvalidData(
                parserName + ".Common.DomainErrorWithoutErrorId",
                domainErrorWithoutErrorId,
                parser);

            var domainErrorWithoutDetail = (byte[])domainError.Clone();
            TestFrame.WriteUInt32(domainErrorWithoutDetail, 12, 0);
            AssertInvalidData(
                parserName + ".Common.DomainErrorWithoutDetail",
                domainErrorWithoutDetail,
                parser);

            var domainErrorWithUnknownDetail = (byte[])domainError.Clone();
            TestFrame.WriteUInt32(
                domainErrorWithUnknownDetail,
                12,
                (uint)LMCDiagnosticsDetailCode.RecorderConfigurationAbsent + 1u);
            AssertInvalidData(
                parserName + ".Common.DomainErrorUnknownDetail",
                domainErrorWithUnknownDetail,
                parser);

            var domainErrorWithCommandTail = (byte[])seedPayload.Clone();
            TestFrame.WriteUInt16(domainErrorWithCommandTail, 4, 1);
            TestFrame.WriteInt16(
                domainErrorWithCommandTail,
                6,
                LMC_DiagnosticsParser.DiagnosticsErrorId);
            TestFrame.WriteUInt32(
                domainErrorWithCommandTail,
                12,
                (uint)LMCDiagnosticsDetailCode.UnsupportedFeature);
            AssertInvalidData(
                parserName + ".Common.DomainErrorWithCommandTail",
                domainErrorWithCommandTail,
                parser);

            foreach (var headerStatus in new ushort[] { 1, ushort.MaxValue })
            {
                AssertExactException<LMCDiagnosticsDispatchRejectedException>(
                    parserName + ".Outer.HeaderStatus." + headerStatus,
                    () => parser(TestFrame.Response(
                        headerStatus,
                        seedPayload)));
            }
        }

        private static TException AssertExactException<TException>(
            string scenario,
            Action action)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (Exception error)
            {
                if (error.GetType() == typeof(TException))
                {
                    return (TException)error;
                }

                throw new InvalidOperationException(
                    scenario + " must fail with exact "
                        + typeof(TException).FullName + ", actual "
                        + error.GetType().FullName + ".",
                    error);
            }

            throw new InvalidOperationException(
                scenario + " unexpectedly produced a value; expected exact "
                    + typeof(TException).FullName + ".");
        }

        internal static void ValidateTopologyInfo(
            LMCEtherCATTopologyInfo info)
        {
            AssertPubliclyImmutable(typeof(LMCEtherCATTopologyInfo));
            ValidateCommonSuccessResponse(
                info.Response,
                LMCDiagnosticsResponseFlags.None);
            AssertEx.True(info.TopologyRevision != 0);
            AssertEx.True(info.TotalNodeCount != 0);
            AssertEx.Equal((ushort)96, info.EntryStride);
            AssertEx.True(info.MaxEntriesPerChunk >= 1);
            AssertEx.True(info.MaxEntriesPerChunk <= 16);
            AssertEx.Equal(
                info.TotalNodeCount,
                checked((ushort)(info.ConfiguredSlaveCount
                    + info.SlotModuleCount)));
            AssertEx.True(
                info.PhysicalAxisCount <= info.ConfiguredSlaveCount);
            AssertEx.Equal(0x0000000Fu, info.TopologyFlagsValue);
            AssertEx.Equal(1u, info.CrcKindValue);
        }

        internal static void ValidateTopologyChunk(
            LMCEtherCATTopologyChunk chunk,
            ushort expectedStartIndex,
            ushort requestedMaxEntries)
        {
            AssertPubliclyImmutable(typeof(LMCEtherCATTopologyChunk));
            AssertPubliclyImmutable(typeof(LMCEtherCATTopologyEntry));
            AssertEx.True(chunk.TotalNodeCount != 0);
            AssertEx.True(chunk.StartIndex < chunk.TotalNodeCount);
            var expectedReturnedCount = checked((ushort)Math.Min(
                requestedMaxEntries,
                chunk.TotalNodeCount - chunk.StartIndex));
            var expectedFlags = chunk.StartIndex + expectedReturnedCount
                    == chunk.TotalNodeCount
                ? LMCDiagnosticsResponseFlags.LastChunk
                : LMCDiagnosticsResponseFlags.None;
            ValidateCommonSuccessResponse(chunk.Response, expectedFlags);
            AssertEx.Equal(TopologyRevision, chunk.TopologyRevision);
            AssertEx.Equal(expectedStartIndex, chunk.StartIndex);
            AssertEx.Equal(expectedReturnedCount, chunk.ReturnedCount);
            AssertEx.Equal((ushort)96, chunk.EntryStride);
            AssertEx.Equal((int)expectedReturnedCount, chunk.Entries.Count);
            for (var index = 0; index < chunk.Entries.Count; index++)
            {
                ValidateTopologyEntry(
                    chunk.Entries[index],
                    checked((ushort)(expectedStartIndex + index)));
            }

            var collection = chunk.Entries
                as ICollection<LMCEtherCATTopologyEntry>;
            AssertEx.True(collection == null || collection.IsReadOnly);
        }

        private static void ValidateTopologyEntry(
            LMCEtherCATTopologyEntry entry,
            ushort expectedTopologyIndex)
        {
            AssertEx.NotNull(entry);
            AssertEx.True(entry.NodeId != 0);
            AssertEx.Equal(expectedTopologyIndex, entry.TopologyIndex);
            AssertEx.True(
                entry.NodeKind == LMCEtherCATTopologyNodeKind.EtherCATSlave
                    || entry.NodeKind
                        == LMCEtherCATTopologyNodeKind.SlotModule);
            AssertEx.True(((ushort)entry.NodeFlags & ~0x00FF) == 0);

            var hasMasterIndex = entry.MasterSlaveIndex != ushort.MaxValue;
            var supportsSdo = entry.SdoSlaveReference != 0;
            var isPhysicalAxis = entry.PhysicalAxisReference != 0;
            AssertEx.Equal(
                hasMasterIndex,
                HasTopologyFlag(
                    entry.NodeFlags,
                    LMCEtherCATTopologyNodeFlags.HasMasterSlaveIndex));
            AssertEx.Equal(
                supportsSdo,
                HasTopologyFlag(
                    entry.NodeFlags,
                    LMCEtherCATTopologyNodeFlags.SupportsSdo));
            AssertEx.Equal(
                isPhysicalAxis,
                HasTopologyFlag(
                    entry.NodeFlags,
                    LMCEtherCATTopologyNodeFlags.PhysicalAxis));
            AssertEx.Equal(
                entry.InputBytes != 0,
                HasTopologyFlag(
                    entry.NodeFlags,
                    LMCEtherCATTopologyNodeFlags.HasInputs));
            AssertEx.Equal(
                entry.OutputBytes != 0,
                HasTopologyFlag(
                    entry.NodeFlags,
                    LMCEtherCATTopologyNodeFlags.HasOutputs));
            AssertEx.Equal(
                entry.IOReference != 0,
                HasTopologyFlag(
                    entry.NodeFlags,
                    LMCEtherCATTopologyNodeFlags.HasDigitalIO));

            if (entry.IOReference != 0)
            {
                AssertEx.True(
                    entry.InputBytes != 0 || entry.OutputBytes != 0);
                AssertEx.True(entry.InputBytes <= sizeof(ulong));
                AssertEx.True(entry.OutputBytes <= sizeof(ulong));
            }

            if (HasTopologyFlag(
                    entry.NodeFlags,
                    LMCEtherCATTopologyNodeFlags.Ds402Drive))
            {
                AssertEx.True(isPhysicalAxis);
            }

            if (isPhysicalAxis)
            {
                AssertEx.Equal(
                    LMCEtherCATTopologyNodeKind.EtherCATSlave,
                    entry.NodeKind);
            }

            AssertEx.True(entry.VendorId != 0);
            AssertEx.True(entry.ProductCode != 0);
            if (entry.NodeKind
                == LMCEtherCATTopologyNodeKind.EtherCATSlave)
            {
                AssertEx.Equal(0u, entry.ParentNodeId);
                AssertEx.Equal(ushort.MaxValue, entry.SlotIndex);
                AssertEx.True(hasMasterIndex);
            }
            else
            {
                AssertEx.True(entry.ParentNodeId != 0);
                AssertEx.True(entry.SlotIndex != ushort.MaxValue);
                AssertEx.False(hasMasterIndex);
            }

            AssertEx.True(!string.IsNullOrEmpty(entry.Name));
            AssertEx.True(entry.Name.Length < 48);
            for (var index = 0; index < entry.Name.Length; index++)
            {
                AssertEx.True(entry.Name[index] <= 0x7F);
            }
        }

        internal static void ValidateNodeHealth(
            LMCEtherCATNodeHealth health)
        {
            AssertPubliclyImmutable(typeof(LMCEtherCATNodeHealth));
            ValidateCommonSuccessResponse(
                health.Response,
                LMCDiagnosticsResponseFlags.None);
            AssertEx.Equal(TopologyRevision, health.TopologyRevision);
            AssertEx.Equal(NodeId, health.NodeId);
            AssertEx.Equal(LMCCapturePhase.InputMapped, health.CapturePhase);
            AssertEx.True(health.SnapshotSequence != 0);
            AssertEx.True((health.SnapshotSequence & 1) == 0);
            AssertEx.True(((ushort)health.HealthFlags & ~0x003F) == 0);
            var configured = (health.HealthFlags
                & LMCEtherCATNodeHealthFlags.Configured) != 0;
            var detected = (health.HealthFlags
                & LMCEtherCATNodeHealthFlags.Detected) != 0;
            var identityMatched = (health.HealthFlags
                & LMCEtherCATNodeHealthFlags.IdentityMatched) != 0;
            var dataValid = (health.HealthFlags
                & LMCEtherCATNodeHealthFlags.DataValid) != 0;
            var dataDefaulted = (health.HealthFlags
                & LMCEtherCATNodeHealthFlags.DataDefaulted) != 0;
            var hasDs402Data = (health.HealthFlags
                & LMCEtherCATNodeHealthFlags.Ds402DataPresent) != 0;
            AssertEx.True(configured);
            AssertEx.Equal(health.Online, detected);
            AssertEx.Equal(health.Online, health.EtherCATState != 0);
            AssertEx.True(IsEtherCATState(health.EtherCATState));
            AssertEx.False(identityMatched && !detected);
            AssertEx.True(dataValid != dataDefaulted);
            AssertEx.False(dataValid && (!detected || !identityMatched));
            AssertEx.False(hasDs402Data && !dataValid);
            if (!hasDs402Data)
            {
                AssertEx.Equal(0u, health.DS402StatusWord);
                AssertEx.Equal(0u, health.AxisError);
            }
        }

        internal static void ValidateDigitalIo(
            LMCDigitalIOValue value,
            LMCDigitalIOReadRequest request)
        {
            AssertPubliclyImmutable(typeof(LMCDigitalIOValue));
            ValidateCommonSuccessResponse(
                value.Response,
                LMCDiagnosticsResponseFlags.None);
            AssertEx.Equal(request.TopologyRevision, value.TopologyRevision);
            AssertEx.Equal(request.IOReference, value.IOReference);
            AssertEx.True(value.NodeId != 0);
            AssertEx.Equal(request.ExpectedDirection, value.Direction);
            AssertEx.Equal(request.ExpectedBitWidth, value.BitWidth);
            AssertEx.True(value.BitWidth >= 1 && value.BitWidth <= 64);
            AssertEx.True(((ushort)value.StatusFlags & ~0x01FF) == 0);
            var widthMask = GetBitWidthMask(value.BitWidth);
            AssertEx.Equal(0UL, value.Value & ~widthMask);
            AssertEx.Equal(0UL, value.ValidMask & ~widthMask);
            if (value.IsValid)
            {
                AssertEx.Equal(
                    LMCDigitalIOStatusFlags.Valid,
                    value.StatusFlags);
                AssertEx.Equal(widthMask, value.ValidMask);
            }
            else
            {
                AssertEx.Equal(0UL, value.Value);
                AssertEx.Equal(0UL, value.ValidMask);
                AssertEx.True((value.StatusFlags
                    & LMCDigitalIOStatusFlags.DataDefaulted) != 0);
                AssertEx.True((value.StatusFlags
                    & (LMCDigitalIOStatusFlags.StaleFrame
                        | LMCDigitalIOStatusFlags.MasterNotOperational
                        | LMCDigitalIOStatusFlags.NodeOffline
                        | LMCDigitalIOStatusFlags.NodeNotOperational
                        | LMCDigitalIOStatusFlags.AlError
                        | LMCDigitalIOStatusFlags.SourceUnavailable
                        | LMCDigitalIOStatusFlags.IdentityMismatch)) != 0);
            }

            if (value.Direction == LMCDigitalIODirection.Input)
            {
                AssertEx.Equal(0u, value.OutputRevision);
            }
            else
            {
                AssertEx.True(value.OutputRevision != 0);
            }
        }

        internal static void ValidateD5Status(
            LMCOperationStatus status,
            LMCOperationTicket ticket)
        {
            AssertPubliclyImmutable(typeof(LMCOperationStatus));
            ValidateCommonSuccessResponse(
                status.Response,
                LMCDiagnosticsResponseFlags.None);
            AssertEx.Equal(ticket.TicketId, status.TicketId);
            AssertEx.Equal(ticket.OperationKind, status.OperationKind);
            AssertEx.Equal(ticket.QueuedCycle, status.SubmitCycle);
            AssertEx.Equal(ticket.DiagnosticsBootId, status.DiagnosticsBootId);

            var validStateOutcome = (status.State == LMCOperationState.Queued
                    || status.State == LMCOperationState.Running)
                ? status.Outcome == LMCOperationOutcome.NoneOrPending
                : status.State == LMCOperationState.Completed
                    ? status.Outcome == LMCOperationOutcome.Success
                    : status.State == LMCOperationState.Failed
                        ? status.Outcome == LMCOperationOutcome.Failed
                        : status.State == LMCOperationState.Cancelled
                            ? status.Outcome
                                == LMCOperationOutcome.Cancelled
                            : status.State == LMCOperationState.Expired
                                && status.Outcome
                                    == LMCOperationOutcome.TimedOut;
            AssertEx.True(validStateOutcome);

            var pending = status.State == LMCOperationState.Queued
                || status.State == LMCOperationState.Running;
            if (pending)
            {
                AssertEx.Equal(0u, status.CompletionCycle);
                AssertEx.Equal((short)0, status.OperationErrorId);
                AssertEx.Equal(0u, status.OperationDetail);
            }

            if (status.State == LMCOperationState.Completed)
            {
                AssertEx.Equal((short)0, status.OperationErrorId);
                AssertEx.Equal(0u, status.OperationDetail);
            }

            if (status.State == LMCOperationState.Cancelled
                || status.State == LMCOperationState.Expired)
            {
                AssertEx.Equal((short)0, status.OperationErrorId);
            }

            if (status.IsSuccessful && ticket.ExpectsResultData)
            {
                AssertEx.Equal(ticket.ExpectedResultLength, status.ResultLength);
                AssertEx.Equal(
                    ticket.ExpectedResultValueType,
                    status.ResultValueType);
                AssertEx.Equal(
                    (int)ticket.ExpectedResultLength,
                    status.ResultData.Length);
                ValidateCanonicalD5Result(
                    status.ResultValueType,
                    status.ResultData);
                var first = status.ResultData;
                var original = first[0];
                first[0] ^= 0xFF;
                AssertEx.Equal(original, status.ResultData[0]);
            }
            else
            {
                AssertEx.Equal(0u, status.ResultLength);
                AssertEx.Equal(0, status.ResultData.Length);
                AssertEx.Equal(
                    LMCSignalValueType.Invalid,
                    status.ResultValueType);
            }
        }

        internal static void ValidateRecoverableConfigure(
            LMCRecorderConfigurationHandle handle)
        {
            AssertPubliclyImmutable(typeof(LMCRecorderConfigurationHandle));
            ValidateCommonSuccessResponse(
                handle.ConfigurationResponse,
                LMCDiagnosticsResponseFlags.None);
            AssertEx.True(handle.IsRecoverable);
            AssertEx.Equal(RecorderRecoveryToken, handle.RecoveryToken);
            AssertEx.Equal(DiagnosticsBootId, handle.DiagnosticsBootId);
            AssertEx.Equal(RecorderConfigId, handle.ConfigId);
            AssertEx.True(handle.ConfigRevision != 0);
            AssertEx.Equal(TopologyRevision, handle.MapRevision);
            AssertEx.True(handle.AcceptedCapacity >= 1);
            AssertEx.True(handle.AcceptedCapacity <= 10);
            AssertEx.Equal(
                checked(handle.AcceptedCapacity * 16u),
                handle.ReservedDataBytes);
            AssertEx.Equal(
                LMCRecorderState.Configured,
                handle.InitialState);
            AssertEx.Equal((ushort)2, handle.ChannelCount);
            AssertEx.Equal((ushort)8, handle.SampleStrideBytes);
            AssertEx.Equal((ushort)2, handle.RecorderBufferCount);
            AssertEx.True(
                handle.CapturePhase == LMCCapturePhase.InputMapped
                    || handle.CapturePhase == LMCCapturePhase.PreOutput);
            AssertEx.True(handle.OwnerSessionEpoch != 0);
        }

        internal static void ValidateRecoverableInventory(
            LMCRecorderBankInventory inventory)
        {
            AssertPubliclyImmutable(typeof(LMCRecorderBankInventory));
            AssertPubliclyImmutable(typeof(LMCRecorderBankInventoryEntry));
            ValidateCommonSuccessResponse(
                inventory.Response,
                LMCDiagnosticsResponseFlags.None);
            AssertEx.True(inventory.IsRecoverable);
            AssertEx.Equal(RecorderRecoveryToken, inventory.RecoveryToken);
            AssertEx.Equal(DiagnosticsBootId, inventory.DiagnosticsBootId);
            AssertEx.Equal(RecorderConfigId, inventory.ConfigId);
            AssertEx.True(inventory.ConfigRevision != 0);
            AssertEx.Equal(TopologyRevision, inventory.MapRevision);
            AssertEx.True(
                inventory.ConfigurationOwnerSessionEpoch != 0);
            AssertEx.True(inventory.IsConfigurationOwnerSessionClosed);
            AssertEx.Equal(
                LMCRecorderState.Configured,
                inventory.ConfigurationState);
            AssertEx.Equal(
                LMCRecorderBufferMode.Double,
                inventory.BufferMode);
            AssertEx.Equal((byte)2, inventory.RecorderBufferCount);
            AssertEx.Equal(0, inventory.OccupiedBanks.Count);
        }

        private static void ValidateCommonSuccessResponse(
            LMCDiagnosticsResponse response,
            LMCDiagnosticsResponseFlags expectedFlags)
        {
            AssertEx.NotNull(response);
            AssertEx.NotNull(response.TransportResponse);
            AssertEx.True(response.TransportResponse.IsFrameValid);
            AssertEx.Equal((ushort)0, response.TransportResponse.HeaderStatus);
            AssertEx.Equal(0u, response.TransportResponse.HeaderReserved);
            AssertEx.Equal((ushort)1, response.SchemaVersion);
            AssertEx.Equal(expectedFlags, response.ResponseFlags);
            AssertEx.Equal((ushort)0, response.CommandStatus);
            AssertEx.Equal((short)0, response.ErrorId);
            AssertEx.Equal(RequestId, response.RequestId);
            AssertEx.Equal(0u, response.DetailCode);
            AssertEx.True(response.IsSuccess);
        }

        private static void ValidateCanonicalD5Result(
            LMCSignalValueType valueType,
            byte[] data)
        {
            AssertEx.NotNull(data);
            AssertEx.True(
                data.Length == 1
                    || data.Length == 2
                    || data.Length == 4
                    || data.Length == 8
                    || data.Length == 12);
            int tailOffset;
            byte expectedTail;
            if (valueType == LMCSignalValueType.Bool)
            {
                AssertEx.True(data[0] <= 1);
                tailOffset = 1;
                expectedTail = 0;
            }
            else if (valueType == LMCSignalValueType.Int16)
            {
                AssertEx.True(data.Length >= 2);
                tailOffset = 2;
                expectedTail = (data[1] & 0x80) == 0
                    ? (byte)0
                    : (byte)0xFF;
            }
            else if (valueType == LMCSignalValueType.UInt16
                || valueType == LMCSignalValueType.BitField16)
            {
                AssertEx.True(data.Length >= 2);
                tailOffset = 2;
                expectedTail = 0;
            }
            else
            {
                return;
            }

            for (var index = tailOffset; index < data.Length; index++)
            {
                AssertEx.Equal(expectedTail, data[index]);
            }
        }

        private static bool HasTopologyFlag(
            LMCEtherCATTopologyNodeFlags value,
            LMCEtherCATTopologyNodeFlags flag)
        {
            return (value & flag) != 0;
        }

        private static bool IsEtherCATState(byte value)
        {
            return value == 0
                || value == 1
                || value == 2
                || value == 3
                || value == 4
                || value == 8;
        }

        private static ulong GetBitWidthMask(byte bitWidth)
        {
            return bitWidth == 64
                ? ulong.MaxValue
                : (1UL << bitWidth) - 1UL;
        }

        private static void AssertPubliclyImmutable(Type type)
        {
            foreach (var property in type.GetProperties(
                BindingFlags.Instance | BindingFlags.Public))
            {
                AssertEx.True(
                    property.GetSetMethod(false) == null,
                    type.Name + "." + property.Name
                        + " exposes a public setter.");
            }
        }

        internal static byte[] CreateTopologyInfoPayload()
        {
            var payload = CommonPayload(44);
            TestFrame.WriteUInt32(payload, 16, TopologyRevision);
            TestFrame.WriteUInt16(payload, 20, 1);
            TestFrame.WriteUInt16(payload, 22, 96);
            TestFrame.WriteUInt16(payload, 24, 16);
            TestFrame.WriteUInt16(payload, 26, 1);
            TestFrame.WriteUInt16(payload, 28, 0);
            TestFrame.WriteUInt16(payload, 30, 1);
            TestFrame.WriteUInt32(payload, 32, 0x0000000Fu);
            TestFrame.WriteUInt32(payload, 36, 1);
            return payload;
        }

        internal static byte[] CreateTopologyChunkPayload()
        {
            var payload = CommonPayload(28 + 96);
            TestFrame.WriteUInt16(
                payload,
                2,
                (ushort)LMCDiagnosticsResponseFlags.LastChunk);
            TestFrame.WriteUInt32(payload, 16, TopologyRevision);
            TestFrame.WriteUInt16(payload, 20, 0);
            TestFrame.WriteUInt16(payload, 22, 1);
            TestFrame.WriteUInt16(payload, 24, 1);
            TestFrame.WriteUInt16(payload, 26, 96);

            const int offset = 28;
            TestFrame.WriteUInt32(payload, offset, NodeId);
            TestFrame.WriteUInt32(payload, offset + 4, 0);
            TestFrame.WriteUInt16(payload, offset + 8, 0);
            TestFrame.WriteUInt16(payload, offset + 10, 0);
            payload[offset + 12] =
                (byte)LMCEtherCATTopologyNodeKind.EtherCATSlave;
            TestFrame.WriteUInt16(
                payload,
                offset + 14,
                (ushort)(
                    LMCEtherCATTopologyNodeFlags.HasMasterSlaveIndex
                    | LMCEtherCATTopologyNodeFlags.SupportsSdo
                    | LMCEtherCATTopologyNodeFlags.PhysicalAxis
                    | LMCEtherCATTopologyNodeFlags.Ds402Drive));
            TestFrame.WriteUInt16(payload, offset + 16, 1);
            TestFrame.WriteUInt16(payload, offset + 18, 1);
            TestFrame.WriteUInt16(payload, offset + 20, ushort.MaxValue);
            TestFrame.WriteUInt32(payload, offset + 24, 0x0000009Au);
            TestFrame.WriteUInt32(payload, offset + 28, 0x00030924u);
            TestFrame.WriteUInt32(payload, offset + 32, 1);
            TestFrame.WriteUInt32(payload, offset + 36, 1);
            var name = System.Text.Encoding.ASCII.GetBytes("fuzz-drive");
            Buffer.BlockCopy(name, 0, payload, offset + 44, name.Length);
            return payload;
        }

        private static byte[] CreateTwoEntryTopologyChunkPayload()
        {
            var payload = CommonPayload(28 + 2 * 96);
            TestFrame.WriteUInt32(payload, 16, TopologyRevision);
            TestFrame.WriteUInt16(payload, 20, 0);
            TestFrame.WriteUInt16(payload, 22, 2);
            TestFrame.WriteUInt16(payload, 24, 3);
            TestFrame.WriteUInt16(payload, 26, 96);

            var first = CreateTopologyChunkPayload();
            Buffer.BlockCopy(first, 28, payload, 28, 96);

            const int offset = 28 + 96;
            TestFrame.WriteUInt32(payload, offset, SlotNodeId);
            TestFrame.WriteUInt32(payload, offset + 4, NodeId);
            TestFrame.WriteUInt16(payload, offset + 8, 1);
            TestFrame.WriteUInt16(
                payload,
                offset + 10,
                ushort.MaxValue);
            payload[offset + 12] =
                (byte)LMCEtherCATTopologyNodeKind.SlotModule;
            TestFrame.WriteUInt16(
                payload,
                offset + 14,
                (ushort)(LMCEtherCATTopologyNodeFlags.HasInputs
                    | LMCEtherCATTopologyNodeFlags.HasOutputs
                    | LMCEtherCATTopologyNodeFlags.HasDigitalIO));
            TestFrame.WriteUInt16(payload, offset + 20, 0);
            TestFrame.WriteUInt32(payload, offset + 24, 0x0000009Au);
            TestFrame.WriteUInt32(payload, offset + 28, 0x000A0404u);
            TestFrame.WriteUInt32(payload, offset + 32, 1);
            TestFrame.WriteUInt32(payload, offset + 36, 1);
            TestFrame.WriteUInt16(payload, offset + 40, 2);
            TestFrame.WriteUInt16(payload, offset + 42, 2);
            var name = System.Text.Encoding.ASCII.GetBytes("fuzz-slot");
            Buffer.BlockCopy(name, 0, payload, offset + 44, name.Length);
            TestFrame.WriteUInt32(payload, offset + 92, IOReference);
            return payload;
        }

        internal static byte[] CreateNodeHealthPayload()
        {
            var payload = CommonPayload(72);
            TestFrame.WriteUInt32(payload, 16, TopologyRevision);
            TestFrame.WriteUInt32(payload, 20, NodeId);
            TestFrame.WriteUInt16(
                payload,
                24,
                (ushort)LMCCapturePhase.InputMapped);
            TestFrame.WriteUInt16(
                payload,
                26,
                (ushort)(LMCEtherCATNodeHealthFlags.Configured
                    | LMCEtherCATNodeHealthFlags.Detected
                    | LMCEtherCATNodeHealthFlags.IdentityMatched
                    | LMCEtherCATNodeHealthFlags.DataValid
                    | LMCEtherCATNodeHealthFlags.Ds402DataPresent));
            TestFrame.WriteUInt32(payload, 28, 100);
            TestFrame.WriteUInt64(payload, 32, 0x1122334455667788UL);
            TestFrame.WriteUInt32(payload, 40, 2);
            payload[44] = 1;
            payload[45] = 8;
            TestFrame.WriteUInt32(payload, 48, 0x0000009Au);
            TestFrame.WriteUInt32(payload, 52, 0x00030924u);
            TestFrame.WriteUInt32(payload, 56, 0x1234u);
            TestFrame.WriteUInt32(payload, 60, 0x5678u);
            TestFrame.WriteUInt32(payload, 64, 99);
            TestFrame.WriteUInt32(payload, 68, 90);
            return payload;
        }

        private static byte[] CreateOfflineNodeHealthPayload()
        {
            var payload = CreateNodeHealthPayload();
            TestFrame.WriteUInt16(
                payload,
                26,
                (ushort)(LMCEtherCATNodeHealthFlags.Configured
                    | LMCEtherCATNodeHealthFlags.DataDefaulted));
            payload[44] = 0;
            payload[45] = 0;
            TestFrame.WriteUInt32(payload, 56, 0);
            TestFrame.WriteUInt32(payload, 60, 0);
            return payload;
        }

        private static byte[] CreateDigitalIoPayload()
        {
            return CreateDigitalIoPayload(
                LMCDigitalIODirection.Output,
                64,
                LMCDigitalIOStatusFlags.Valid,
                0x1122334455667788UL,
                ulong.MaxValue,
                0x01020304u);
        }

        internal static byte[] CreateDigitalIoPayload(
            LMCDigitalIODirection direction,
            byte bitWidth,
            LMCDigitalIOStatusFlags statusFlags,
            ulong value,
            ulong validMask,
            uint outputRevision)
        {
            var payload = CommonPayload(56);
            TestFrame.WriteUInt32(payload, 16, TopologyRevision);
            TestFrame.WriteUInt32(payload, 20, IOReference);
            TestFrame.WriteUInt32(payload, 24, NodeId);
            payload[28] = (byte)direction;
            payload[29] = bitWidth;
            TestFrame.WriteUInt16(
                payload,
                30,
                (ushort)statusFlags);
            TestFrame.WriteUInt64(payload, 32, value);
            TestFrame.WriteUInt64(payload, 40, validMask);
            TestFrame.WriteUInt32(payload, 48, 100);
            TestFrame.WriteUInt32(payload, 52, outputRevision);
            return payload;
        }

        internal static byte[] CreateD5VariableInlineStatusPayload(
            LMCOperationTicket ticket)
        {
            return CreateD5StatusPayload(
                ticket,
                LMCOperationState.Completed,
                LMCOperationOutcome.Success,
                200,
                0,
                0,
                ticket.ExpectedResultValueType,
                new byte[] { 0x78, 0x56, 0x34, 0x12 });
        }

        internal static LMCRecorderConfiguration
            CreateRecoverableDoubleConfiguration()
        {
            return new LMCRecorderConfiguration(
                new[] { 0xEC000201u, 0xEC000202u },
                1,
                10,
                LMCRecorderBufferMode.Double,
                LMCRecorderTriggerType.Edge,
                LMCSignalValueType.Int32,
                4,
                5,
                0xEC000201u,
                LMCRecorderTriggerOperator.RisingEdge,
                100,
                0,
                RecorderConfigId);
        }

        internal static LMCDiagnosticCapabilities
            CreateRecoverableCapabilities()
        {
            return new LMCDiagnosticCapabilities(
                null,
                7,
                3,
                (uint)(LMCDiagnosticCapability.SignalCatalog
                    | LMCDiagnosticCapability.RecorderSingleBank
                    | LMCDiagnosticCapability.RecorderTrigger
                    | LMCDiagnosticCapability.RecorderDoubleBank),
                TopologyRevision,
                24,
                32,
                32,
                2,
                100,
                1000,
                1320,
                2040,
                16,
                80,
                16,
                800,
                0,
                DiagnosticsBootId);
        }

        internal static byte[] CreateRecoverableConfigurePayload()
        {
            var payload = CommonPayload(72);
            TestFrame.WriteUInt32(payload, 16, RecorderConfigId);
            TestFrame.WriteUInt32(
                payload,
                20,
                RecorderConfigRevision);
            TestFrame.WriteUInt32(payload, 24, TopologyRevision);
            TestFrame.WriteUInt32(payload, 28, 10);
            TestFrame.WriteUInt32(payload, 32, 160);
            TestFrame.WriteUInt16(
                payload,
                36,
                (ushort)LMCRecorderState.Configured);
            TestFrame.WriteUInt16(payload, 38, 2);
            TestFrame.WriteUInt16(payload, 40, 8);
            TestFrame.WriteUInt16(payload, 42, 2);
            TestFrame.WriteUInt16(
                payload,
                44,
                (ushort)LMCCapturePhase.InputMapped);
            TestFrame.WriteUInt32(
                payload,
                48,
                RecorderOwnerSessionEpoch);
            TestFrame.WriteUInt32(payload, 52, DiagnosticsBootId);
            Buffer.BlockCopy(
                RecorderRecoveryToken.ToByteArray(),
                0,
                payload,
                56,
                16);
            return payload;
        }

        internal static byte[]
            CreateRecoverableRecorderBankInventoryPayload()
        {
            var payload = CommonPayload(104);
            TestFrame.WriteUInt32(payload, 16, DiagnosticsBootId);
            TestFrame.WriteUInt32(payload, 20, RecorderConfigId);
            TestFrame.WriteUInt32(
                payload,
                24,
                RecorderConfigRevision);
            TestFrame.WriteUInt32(payload, 28, TopologyRevision);
            TestFrame.WriteUInt32(
                payload,
                32,
                RecorderOwnerSessionEpoch);
            TestFrame.WriteUInt32(
                payload,
                36,
                RecorderOwnerSessionEpoch);
            TestFrame.WriteUInt16(
                payload,
                40,
                (ushort)LMCRecorderState.Configured);
            payload[42] = (byte)LMCRecorderBufferMode.Double;
            payload[43] = 2;
            payload[44] = 0;
            Buffer.BlockCopy(
                RecorderRecoveryToken.ToByteArray(),
                0,
                payload,
                88,
                16);
            return payload;
        }

        private static byte[] CreateD5StatusPayload(
            LMCOperationTicket ticket,
            LMCOperationState state,
            LMCOperationOutcome outcome,
            uint completionCycle,
            short operationErrorId,
            uint operationDetail,
            LMCSignalValueType resultValueType,
            byte[] resultData)
        {
            var safeResultData = resultData ?? new byte[0];
            var payload = CommonPayload(64);
            TestFrame.WriteUInt32(payload, 16, ticket.TicketId);
            TestFrame.WriteUInt16(
                payload,
                20,
                (ushort)ticket.OperationKind);
            TestFrame.WriteUInt16(
                payload,
                22,
                (ushort)state);
            TestFrame.WriteUInt32(payload, 24, ticket.QueuedCycle);
            TestFrame.WriteUInt32(payload, 28, completionCycle);
            TestFrame.WriteUInt16(
                payload,
                32,
                (ushort)outcome);
            TestFrame.WriteInt16(payload, 34, operationErrorId);
            TestFrame.WriteUInt32(payload, 36, operationDetail);
            TestFrame.WriteUInt32(
                payload,
                40,
                checked((uint)safeResultData.Length));
            payload[44] = (byte)resultValueType;
            payload[45] = checked((byte)safeResultData.Length);
            Buffer.BlockCopy(
                safeResultData,
                0,
                payload,
                48,
                safeResultData.Length);
            TestFrame.WriteUInt32(
                payload,
                60,
                ticket.DiagnosticsBootId);
            return payload;
        }

        private static void ExerciseCompletedD5Seed(
            LMCConnection connection,
            string scenario,
            uint ticketId,
            ushort resultLength,
            LMCSignalValueType resultValueType,
            byte[] resultData,
            int randomSeed)
        {
            var ticket = new LMCOperationTicket(
                ticketId,
                LMCOperationKind.SDORead,
                100,
                DiagnosticsBootId,
                SubmissionRevision,
                0,
                connection.Diagnostics,
                true,
                resultLength,
                resultValueType);
            var payload = CreateD5StatusPayload(
                ticket,
                LMCOperationState.Completed,
                LMCOperationOutcome.Success,
                200,
                0,
                0,
                resultValueType,
                resultData);
            ExercisePayload(
                scenario,
                payload,
                randomSeed,
                raw => LMC_DiagnosticsParser.ParseOperationStatus(
                    raw,
                    RequestId,
                    ticket),
                status => ValidateD5Status(status, ticket),
                32,
                false);
        }

        private static void AssertValidD5State(
            string scenario,
            LMCOperationTicket ticket,
            byte[] payload)
        {
            AssertParserOutcome(
                scenario,
                TestFrame.Response(0, payload),
                raw => LMC_DiagnosticsParser.ParseOperationStatus(
                    raw,
                    RequestId,
                    ticket),
                status => ValidateD5Status(status, ticket),
                true);
        }

        private static byte[] CommonPayload(int length)
        {
            var payload = new byte[length];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, RequestId);
            return payload;
        }

        private static int[] BuildMutationOffsets(int payloadLength)
        {
            var offsets = new List<int>();
            offsets.Add(0);
            offsets.Add(1);
            offsets.Add(2);
            offsets.Add(3);
            for (var offset = 8; offset < 12; offset++)
            {
                offsets.Add(offset);
            }

            for (var offset = 16; offset < payloadLength; offset++)
            {
                offsets.Add(offset);
            }

            return offsets.ToArray();
        }

        private static int[] MutationLengths(int goldenLength)
        {
            var values = new SortedSet<int>
            {
                0,
                1,
                7,
                8,
                15,
                16,
                Math.Max(0, goldenLength - 1),
                goldenLength,
                goldenLength + 1,
                goldenLength + 17,
                255
            };
            var result = new int[values.Count];
            values.CopyTo(result);
            return result;
        }

        private static byte[] ResizeCopy(byte[] source, int length)
        {
            var result = new byte[length];
            Buffer.BlockCopy(
                source,
                0,
                result,
                0,
                Math.Min(source.Length, length));
            return result;
        }

        private static byte[] MutatePayload(
            byte[] source,
            int offset,
            byte value)
        {
            var result = (byte[])source.Clone();
            result[offset] = value;
            return result;
        }
    }
}

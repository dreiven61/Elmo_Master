using System;
using System.IO;

namespace LasalMotionControlLib
{
    internal static partial class LMC_DiagnosticsFrame
    {
        internal const ushort StartEncoderMaintenanceCommandId = 0x7E53;
        internal const ushort ReadEncoderMaintenanceOutcomeCommandId = 0x7E54;
        internal const ushort RetireEncoderMaintenanceOutcomeCommandId = 0x7E55;

        internal const int StartEncoderMaintenanceRequestPayloadLength = 72;
        internal const int ReadEncoderMaintenanceOutcomeRequestPayloadLength = 72;
        internal const int RetireEncoderMaintenanceOutcomeRequestPayloadLength = 76;

        internal static byte[] StartEncoderMaintenance(
            LMCEncoderMaintenanceRecoveryKey recoveryKey,
            uint executeToken)
        {
            ValidateRecoveryKey(recoveryKey);
            if (executeToken
                != LMCEncoderMaintenanceContract.ExecuteToken(
                    recoveryKey.Kind))
            {
                throw new ArgumentException(
                    "Execute token does not match the encoder maintenance kind.",
                    "executeToken");
            }

            var buffer = CreateCommonRequest(
                StartEncoderMaintenanceCommandId,
                recoveryKey.OriginalRequestId,
                StartEncoderMaintenanceRequestPayloadLength);
            var payloadOffset = LMC_Frame.HeaderSize;

            WriteStartIdentity(buffer, payloadOffset, recoveryKey);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 68, executeToken);
            return buffer;
        }

        internal static byte[] ReadEncoderMaintenanceOutcome(
            uint requestId,
            LMCEncoderMaintenanceRecoveryKey recoveryKey)
        {
            ValidateRecoveryKey(recoveryKey);
            var buffer = CreateCommonRequest(
                ReadEncoderMaintenanceOutcomeCommandId,
                requestId,
                ReadEncoderMaintenanceOutcomeRequestPayloadLength);
            WriteOutcomeKey(buffer, LMC_Frame.HeaderSize, recoveryKey);
            return buffer;
        }

        internal static byte[] RetireEncoderMaintenanceOutcome(
            uint requestId,
            LMCEncoderMaintenanceRecoveryKey recoveryKey,
            uint recordGeneration)
        {
            ValidateRecoveryKey(recoveryKey);
            if (recordGeneration == 0)
            {
                throw new ArgumentOutOfRangeException("recordGeneration");
            }

            var buffer = CreateCommonRequest(
                RetireEncoderMaintenanceOutcomeCommandId,
                requestId,
                RetireEncoderMaintenanceOutcomeRequestPayloadLength);
            var payloadOffset = LMC_Frame.HeaderSize;
            WriteOutcomeKey(buffer, payloadOffset, recoveryKey);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 72,
                recordGeneration);
            return buffer;
        }

        private static void WriteStartIdentity(
            byte[] buffer,
            int payloadOffset,
            LMCEncoderMaintenanceRecoveryKey recoveryKey)
        {
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 8,
                recoveryKey.DiagnosticsBuild);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 12,
                recoveryKey.DiagnosticsBootId);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 16,
                recoveryKey.MapRevision);
            WriteClientIntent(buffer, payloadOffset + 20, recoveryKey);
            LMC_Frame.WriteUInt16(
                buffer,
                payloadOffset + 36,
                (ushort)recoveryKey.Kind);
            LMC_Frame.WriteUInt16(
                buffer,
                payloadOffset + 38,
                recoveryKey.CompatibilityProfileId);
            LMC_Frame.WriteUInt16(
                buffer,
                payloadOffset + 40,
                recoveryKey.DriveReference);
            LMC_Frame.WriteUInt16(
                buffer,
                payloadOffset + 42,
                (ushort)recoveryKey.FeedbackSocket);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 44,
                recoveryKey.CommandValue);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 48,
                recoveryKey.TimeoutMilliseconds);
            WriteCompatibilityEvidence(
                buffer,
                payloadOffset + 52,
                recoveryKey);
        }

        private static void WriteOutcomeKey(
            byte[] buffer,
            int payloadOffset,
            LMCEncoderMaintenanceRecoveryKey recoveryKey)
        {
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 8,
                recoveryKey.DiagnosticsBuild);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 12,
                recoveryKey.DiagnosticsBootId);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 16,
                recoveryKey.MapRevision);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 20,
                recoveryKey.OriginalRequestId);
            WriteClientIntent(buffer, payloadOffset + 24, recoveryKey);
            LMC_Frame.WriteUInt16(
                buffer,
                payloadOffset + 40,
                (ushort)recoveryKey.Kind);
            LMC_Frame.WriteUInt16(
                buffer,
                payloadOffset + 42,
                recoveryKey.CompatibilityProfileId);
            LMC_Frame.WriteUInt16(
                buffer,
                payloadOffset + 44,
                recoveryKey.DriveReference);
            LMC_Frame.WriteUInt16(
                buffer,
                payloadOffset + 46,
                (ushort)recoveryKey.FeedbackSocket);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 48,
                recoveryKey.CommandValue);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 52,
                recoveryKey.TimeoutMilliseconds);
            WriteCompatibilityEvidence(
                buffer,
                payloadOffset + 56,
                recoveryKey);
        }

        private static void WriteClientIntent(
            byte[] buffer,
            int offset,
            LMCEncoderMaintenanceRecoveryKey recoveryKey)
        {
            LMC_Frame.WriteUInt32(
                buffer,
                offset,
                recoveryKey.ClientIntentId.Word0);
            LMC_Frame.WriteUInt32(
                buffer,
                offset + 4,
                recoveryKey.ClientIntentId.Word1);
            LMC_Frame.WriteUInt32(
                buffer,
                offset + 8,
                recoveryKey.ClientIntentId.Word2);
            LMC_Frame.WriteUInt32(
                buffer,
                offset + 12,
                recoveryKey.ClientIntentId.Word3);
        }

        private static void WriteCompatibilityEvidence(
            byte[] buffer,
            int offset,
            LMCEncoderMaintenanceRecoveryKey recoveryKey)
        {
            LMC_Frame.WriteUInt32(
                buffer,
                offset,
                recoveryKey.CompatibilityEvidenceId.Word0);
            LMC_Frame.WriteUInt32(
                buffer,
                offset + 4,
                recoveryKey.CompatibilityEvidenceId.Word1);
            LMC_Frame.WriteUInt32(
                buffer,
                offset + 8,
                recoveryKey.CompatibilityEvidenceId.Word2);
            LMC_Frame.WriteUInt32(
                buffer,
                offset + 12,
                recoveryKey.CompatibilityEvidenceId.Word3);
        }

        private static void ValidateRecoveryKey(
            LMCEncoderMaintenanceRecoveryKey recoveryKey)
        {
            if (recoveryKey == null)
            {
                throw new ArgumentNullException("recoveryKey");
            }

            if (recoveryKey.SchemaVersion != SchemaVersion
                || recoveryKey.OriginalRequestId == 0
                || recoveryKey.DiagnosticsBuild == 0
                || recoveryKey.DiagnosticsBootId == 0
                || recoveryKey.MapRevision == 0
                || recoveryKey.CommandValue
                    != LMCEncoderMaintenanceSdoContract.ResetCommandValue)
            {
                throw new ArgumentException(
                    "Encoder maintenance recovery key is invalid.",
                    "recoveryKey");
            }
        }
    }

    internal static partial class LMC_DiagnosticsParser
    {
        internal const int StartEncoderMaintenanceResponsePayloadLength = 40;
        internal const int EncoderMaintenanceOutcomeResponsePayloadLength = 156;

        internal static LMCEncoderMaintenanceStartAcknowledgement
            ParseStartEncoderMaintenance(
                byte[] raw,
                LMCEncoderMaintenanceRecoveryKey expectedRecoveryKey)
        {
            if (expectedRecoveryKey == null)
            {
                throw new ArgumentNullException("expectedRecoveryKey");
            }

            var response = ParseSuccessfulCommand(
                raw,
                expectedRecoveryKey.OriginalRequestId,
                "StartEncoderMaintenance");
            RequireExactPayloadLength(
                response,
                StartEncoderMaintenanceResponsePayloadLength,
                "StartEncoderMaintenance");
            RequireNoResponseFlags(response, "StartEncoderMaintenance");

            var payload = response.TransportResponse.Payload;
            if (LMC_Frame.ReadUInt16(payload, 16)
                    != (ushort)expectedRecoveryKey.Kind
                || LMC_Frame.ReadUInt16(payload, 18)
                    != expectedRecoveryKey.CompatibilityProfileId
                || LMC_Frame.ReadUInt16(payload, 20)
                    != expectedRecoveryKey.DriveReference
                || LMC_Frame.ReadUInt16(payload, 22)
                    != (ushort)expectedRecoveryKey.FeedbackSocket
                || LMC_Frame.ReadUInt32(payload, 24)
                    != expectedRecoveryKey.CommandValue)
            {
                throw new InvalidDataException(
                    "StartEncoderMaintenance response does not echo the exact maintenance command identity.");
            }

            var recordGeneration = LMC_Frame.ReadUInt32(payload, 28);
            var ownerGeneration = LMC_Frame.ReadUInt32(payload, 32);
            var startCycle = LMC_Frame.ReadUInt32(payload, 36);
            if (recordGeneration == 0
                || ownerGeneration == 0
                || startCycle == 0)
            {
                throw new InvalidDataException(
                    "StartEncoderMaintenance returned a reserved zero generation or cycle.");
            }

            return new LMCEncoderMaintenanceStartAcknowledgement(
                response,
                expectedRecoveryKey,
                recordGeneration,
                ownerGeneration,
                startCycle);
        }

        internal static LMCEncoderMaintenanceOutcomeResult
            ParseEncoderMaintenanceOutcome(
                byte[] raw,
                uint expectedRequestId,
                LMCEncoderMaintenanceRecoveryKey expectedRecoveryKey)
        {
            return ParseEncoderMaintenanceOutcomeCore(
                raw,
                expectedRequestId,
                expectedRecoveryKey,
                "ReadEncoderMaintenanceOutcome");
        }

        internal static LMCEncoderMaintenanceOutcomeRetirementResult
            ParseEncoderMaintenanceOutcomeRetirement(
                byte[] raw,
                uint expectedRequestId,
                LMCEncoderMaintenanceOutcomeResult expectedTerminalOutcome)
        {
            if (expectedTerminalOutcome == null)
            {
                throw new ArgumentNullException("expectedTerminalOutcome");
            }

            if (!expectedTerminalOutcome.IsTerminal
                || expectedTerminalOutcome.RecordGeneration == 0)
            {
                throw new ArgumentException(
                    "Only an exact terminal outcome can authorize retirement.",
                    "expectedTerminalOutcome");
            }

            var parsed = ParseEncoderMaintenanceOutcomeCore(
                raw,
                expectedRequestId,
                expectedTerminalOutcome.RecoveryKey,
                "RetireEncoderMaintenanceOutcome");
            if (!parsed.IsTerminal
                || !parsed.HasExactTerminalSnapshot(expectedTerminalOutcome))
            {
                throw new InvalidDataException(
                    "RetireEncoderMaintenanceOutcome did not return the exact terminal snapshot selected for retirement.");
            }

            return new LMCEncoderMaintenanceOutcomeRetirementResult(parsed);
        }

        private static LMCEncoderMaintenanceOutcomeResult
            ParseEncoderMaintenanceOutcomeCore(
                byte[] raw,
                uint expectedRequestId,
                LMCEncoderMaintenanceRecoveryKey expectedRecoveryKey,
                string commandName)
        {
            if (expectedRecoveryKey == null)
            {
                throw new ArgumentNullException("expectedRecoveryKey");
            }

            var response = ParseSuccessfulCommand(
                raw,
                expectedRequestId,
                commandName);
            RequireExactPayloadLength(
                response,
                EncoderMaintenanceOutcomeResponsePayloadLength,
                commandName);
            RequireNoResponseFlags(response, commandName);

            var payload = response.TransportResponse.Payload;
            RequireExactOutcomeKey(payload, expectedRecoveryKey, commandName);

            var recordState = (LMCEncoderMaintenanceOutcomeRecordState)
                LMC_Frame.ReadUInt16(payload, 16);
            var originalCommandStatus = LMC_Frame.ReadUInt16(payload, 84);
            var originalErrorId = unchecked(
                (short)LMC_Frame.ReadUInt16(payload, 86));
            var originalDetailCode = LMC_Frame.ReadUInt32(payload, 88);
            var sdoAbortCode = LMC_Frame.ReadUInt32(payload, 92);
            var startCycle = LMC_Frame.ReadUInt32(payload, 96);
            var writeCompletionCycle = LMC_Frame.ReadUInt32(payload, 100);
            var completionCycle = LMC_Frame.ReadUInt32(payload, 104);
            var executorState = LMC_Frame.ReadUInt32(payload, 108);
            var verificationFlags = LMC_Frame.ReadUInt32(payload, 112);
            var recordGeneration = LMC_Frame.ReadUInt32(payload, 148);
            var ownerGeneration = LMC_Frame.ReadUInt32(payload, 152);

            ValidateOutcomeState(
                recordState,
                originalCommandStatus,
                originalErrorId,
                originalDetailCode,
                sdoAbortCode,
                startCycle,
                writeCompletionCycle,
                completionCycle,
                executorState,
                verificationFlags,
                recordGeneration,
                ownerGeneration,
                commandName);

            return new LMCEncoderMaintenanceOutcomeResult(
                response,
                expectedRecoveryKey,
                recordState,
                originalCommandStatus,
                originalErrorId,
                originalDetailCode,
                sdoAbortCode,
                startCycle,
                writeCompletionCycle,
                completionCycle,
                executorState,
                verificationFlags,
                LMC_Frame.ReadUInt32(payload, 116),
                LMC_Frame.ReadUInt32(payload, 120),
                LMC_Frame.ReadUInt32(payload, 124),
                LMC_Frame.ReadUInt32(payload, 128),
                LMC_Frame.ReadUInt16(payload, 132),
                LMC_Frame.ReadInt32(payload, 136),
                LMC_Frame.ReadUInt32(payload, 140),
                LMC_Frame.ReadInt32(payload, 144),
                recordGeneration,
                ownerGeneration);
        }

        private static void RequireExactOutcomeKey(
            byte[] payload,
            LMCEncoderMaintenanceRecoveryKey expected,
            string commandName)
        {
            if (LMC_Frame.ReadUInt16(payload, 18) != (ushort)expected.Kind
                || LMC_Frame.ReadUInt32(payload, 20)
                    != expected.DiagnosticsBuild
                || LMC_Frame.ReadUInt32(payload, 24)
                    != expected.DiagnosticsBootId
                || LMC_Frame.ReadUInt32(payload, 28)
                    != expected.MapRevision
                || LMC_Frame.ReadUInt32(payload, 32)
                    != expected.OriginalRequestId
                || LMC_Frame.ReadUInt32(payload, 36)
                    != expected.ClientIntentId.Word0
                || LMC_Frame.ReadUInt32(payload, 40)
                    != expected.ClientIntentId.Word1
                || LMC_Frame.ReadUInt32(payload, 44)
                    != expected.ClientIntentId.Word2
                || LMC_Frame.ReadUInt32(payload, 48)
                    != expected.ClientIntentId.Word3
                || LMC_Frame.ReadUInt16(payload, 52)
                    != expected.CompatibilityProfileId
                || LMC_Frame.ReadUInt16(payload, 54)
                    != expected.DriveReference
                || LMC_Frame.ReadUInt16(payload, 56)
                    != (ushort)expected.FeedbackSocket
                || LMC_Frame.ReadUInt16(payload, 58) != 0
                || LMC_Frame.ReadUInt32(payload, 60)
                    != expected.CommandValue
                || LMC_Frame.ReadUInt32(payload, 64)
                    != expected.TimeoutMilliseconds
                || LMC_Frame.ReadUInt32(payload, 68)
                    != expected.CompatibilityEvidenceId.Word0
                || LMC_Frame.ReadUInt32(payload, 72)
                    != expected.CompatibilityEvidenceId.Word1
                || LMC_Frame.ReadUInt32(payload, 76)
                    != expected.CompatibilityEvidenceId.Word2
                || LMC_Frame.ReadUInt32(payload, 80)
                    != expected.CompatibilityEvidenceId.Word3
                || LMC_Frame.ReadUInt16(payload, 134) != 0)
            {
                throw new InvalidDataException(
                    commandName
                    + " response does not echo the exact recovery key or contains non-zero reserved fields.");
            }
        }

        private static void ValidateOutcomeState(
            LMCEncoderMaintenanceOutcomeRecordState recordState,
            ushort originalCommandStatus,
            short originalErrorId,
            uint originalDetailCode,
            uint sdoAbortCode,
            uint startCycle,
            uint writeCompletionCycle,
            uint completionCycle,
            uint executorState,
            uint verificationFlags,
            uint recordGeneration,
            uint ownerGeneration,
            string commandName)
        {
            if (startCycle == 0
                || recordGeneration == 0
                || ownerGeneration == 0
                || (verificationFlags
                    & ~LMCEncoderMaintenanceContract
                        .KnownVerificationFlagsMask) != 0
                || (writeCompletionCycle != 0
                    && writeCompletionCycle < startCycle)
                || (completionCycle != 0
                    && completionCycle < startCycle)
                || (writeCompletionCycle != 0
                    && completionCycle != 0
                    && writeCompletionCycle > completionCycle))
            {
                throw new InvalidDataException(
                    commandName
                    + " returned invalid generations, cycles, or verification flags.");
            }

            switch (recordState)
            {
                case LMCEncoderMaintenanceOutcomeRecordState.Running:
                    if (originalCommandStatus != 0
                        || originalErrorId != 0
                        || originalDetailCode != 0
                        || sdoAbortCode != 0
                        || completionCycle != 0)
                    {
                        throw new InvalidDataException(
                            commandName
                            + " Running record contains terminal status fields.");
                    }
                    break;

                case LMCEncoderMaintenanceOutcomeRecordState.Succeeded:
                    if (originalCommandStatus != 0
                        || originalErrorId != 0
                        || originalDetailCode != 0
                        || sdoAbortCode != 0
                        || writeCompletionCycle == 0
                        || completionCycle == 0
                        || executorState != 0
                        || verificationFlags
                            != LMCEncoderMaintenanceContract
                                .RequiredSuccessVerificationFlags)
                    {
                        throw new InvalidDataException(
                            commandName
                            + " Succeeded record is not fully verified or contains failure state.");
                    }
                    break;

                case LMCEncoderMaintenanceOutcomeRecordState.Failed:
                    if (originalCommandStatus != 1
                        || originalErrorId != DiagnosticsErrorId
                        || (originalDetailCode
                                != (uint)LMCEncoderMaintenanceDetailCode
                                    .ExecutionFailed
                            && originalDetailCode
                                != (uint)LMCEncoderMaintenanceDetailCode
                                    .SemanticVerificationFailed)
                        || completionCycle == 0)
                    {
                        throw new InvalidDataException(
                            commandName
                            + " Failed record contains an invalid terminal error tuple.");
                    }
                    break;

                case LMCEncoderMaintenanceOutcomeRecordState.Aborted:
                    if (originalCommandStatus != 1
                        || originalErrorId != DiagnosticsErrorId
                        || originalDetailCode
                            != (uint)LMCEncoderMaintenanceDetailCode.Aborted
                        || sdoAbortCode != 0
                        || completionCycle == 0)
                    {
                        throw new InvalidDataException(
                            commandName
                            + " Aborted record contains an invalid terminal error tuple.");
                    }
                    break;

                default:
                    throw new InvalidDataException(
                        commandName
                        + " returned an outcome record state not defined by schema version 1.");
            }
        }
    }
}

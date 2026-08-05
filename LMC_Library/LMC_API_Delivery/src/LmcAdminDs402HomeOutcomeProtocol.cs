using System;
using System.IO;

namespace LasalMotionControlLib
{
    internal static partial class LMC_AdminFrame
    {
        internal const int AxisDs402HomeOutcomeRequestPayloadLength = 44;

        internal static byte[] ReadAxisDs402HomeOutcome(
            uint queryRequestId,
            LMCAxisDs402HomeRecoveryKey recoveryKey)
        {
            if (recoveryKey == null)
            {
                throw new ArgumentNullException("recoveryKey");
            }

            ValidateAxisReference(recoveryKey.AxisReference);
            var buffer = CreateCommonRequest(
                LMC_CommandId.ReadAxisDs402HomeOutcome,
                recoveryKey.AxisReference,
                AxisDs402HomeOutcomeRequestPayloadLength,
                queryRequestId);
            var payloadOffset = LMC_Frame.HeaderSize;
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
                recoveryKey.RequestId);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 24,
                recoveryKey.ClientIntentId.Word0);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 28,
                recoveryKey.ClientIntentId.Word1);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 32,
                recoveryKey.ClientIntentId.Word2);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 36,
                recoveryKey.ClientIntentId.Word3);
            LMC_Frame.WriteInt32(
                buffer,
                payloadOffset + 40,
                recoveryKey.Parameters.HomingMethod);
            return buffer;
        }
    }

    internal sealed class LMCParsedAxisDs402HomeOutcome
    {
        internal LMCParsedAxisDs402HomeOutcome(
            LMCAdminResponse response,
            LMCAxisDs402HomeOutcomeRecordState recordState,
            ushort originalCommandStatus,
            short originalErrorId,
            uint originalDetailCode,
            ushort ds402StatusWord,
            int actualPosition,
            uint startCycle,
            uint completionCycle,
            uint nativeCommandState,
            uint recordGeneration)
        {
            Response = response;
            RecordState = recordState;
            OriginalCommandStatus = originalCommandStatus;
            OriginalErrorId = originalErrorId;
            OriginalDetailCode = originalDetailCode;
            Ds402StatusWord = ds402StatusWord;
            ActualPosition = actualPosition;
            StartCycle = startCycle;
            CompletionCycle = completionCycle;
            NativeCommandState = nativeCommandState;
            RecordGeneration = recordGeneration;
        }

        internal LMCAdminResponse Response { get; private set; }
        internal LMCAxisDs402HomeOutcomeRecordState RecordState
        {
            get;
            private set;
        }
        internal ushort OriginalCommandStatus { get; private set; }
        internal short OriginalErrorId { get; private set; }
        internal uint OriginalDetailCode { get; private set; }
        internal ushort Ds402StatusWord { get; private set; }
        internal int ActualPosition { get; private set; }
        internal uint StartCycle { get; private set; }
        internal uint CompletionCycle { get; private set; }
        internal uint NativeCommandState { get; private set; }
        internal uint RecordGeneration { get; private set; }
    }

    internal static partial class LMC_AdminParser
    {
        internal const int AxisDs402HomeOutcomeResponsePayloadLength = 92;

        internal static LMCParsedAxisDs402HomeOutcome
            ParseAxisDs402HomeOutcome(
                byte[] raw,
                uint expectedQueryRequestId,
                LMCAxisDs402HomeRecoveryKey expectedRecoveryKey)
        {
            if (expectedRecoveryKey == null)
            {
                throw new ArgumentNullException("expectedRecoveryKey");
            }

            var transport = ParseTransport(
                raw,
                "ReadAxisDs402HomeOutcome",
                false);
            var response = ParseCommonResponse(
                transport,
                expectedQueryRequestId,
                false,
                false,
                false,
                true);
            if (!response.IsSuccess)
            {
                EnsurePayloadLength(
                    transport,
                    CommonResponsePayloadLength,
                    "ReadAxisDs402HomeOutcome failure");
                throw new LMCAxisDs402HomeOutcomeQueryException(
                    response,
                    expectedRecoveryKey);
            }

            return ParseAxisDs402HomeOutcomeSuccess(
                transport,
                response,
                expectedRecoveryKey,
                "ReadAxisDs402HomeOutcome",
                false,
                0);
        }

        internal static LMCParsedAxisDs402HomeOutcome
            ParseAxisDs402HomeOutcomeSuccess(
                LMC_Response transport,
                LMCAdminResponse response,
                LMCAxisDs402HomeRecoveryKey expectedRecoveryKey,
                string operation,
                bool requireTerminal,
                uint expectedRecordGeneration)
        {
            EnsurePayloadLength(
                transport,
                AxisDs402HomeOutcomeResponsePayloadLength,
                operation + " success");
            var payload = transport.Payload;
            var recordState =
                (LMCAxisDs402HomeOutcomeRecordState)LMC_Frame.ReadUInt16(
                    payload,
                    16);
            var originalCommandStatus = LMC_Frame.ReadUInt16(payload, 60);
            var originalErrorId = unchecked(
                (short)LMC_Frame.ReadUInt16(payload, 62));
            var originalDetailCode = LMC_Frame.ReadUInt32(payload, 64);
            var ds402StatusWord = LMC_Frame.ReadUInt16(payload, 68);
            var actualPosition = LMC_Frame.ReadInt32(payload, 72);
            var startCycle = LMC_Frame.ReadUInt32(payload, 76);
            var completionCycle = LMC_Frame.ReadUInt32(payload, 80);
            var nativeCommandState = LMC_Frame.ReadUInt32(payload, 84);
            var recordGeneration = LMC_Frame.ReadUInt32(payload, 88);

            if (LMC_Frame.ReadUInt16(payload, 18) != 0
                || LMC_Frame.ReadUInt32(payload, 20)
                    != expectedRecoveryKey.DiagnosticsBuild
                || LMC_Frame.ReadUInt32(payload, 24)
                    != expectedRecoveryKey.DiagnosticsBootId
                || LMC_Frame.ReadUInt32(payload, 28)
                    != expectedRecoveryKey.MapRevision
                || LMC_Frame.ReadUInt32(payload, 32)
                    != expectedRecoveryKey.RequestId
                || LMC_Frame.ReadUInt32(payload, 36)
                    != expectedRecoveryKey.ClientIntentId.Word0
                || LMC_Frame.ReadUInt32(payload, 40)
                    != expectedRecoveryKey.ClientIntentId.Word1
                || LMC_Frame.ReadUInt32(payload, 44)
                    != expectedRecoveryKey.ClientIntentId.Word2
                || LMC_Frame.ReadUInt32(payload, 48)
                    != expectedRecoveryKey.ClientIntentId.Word3
                || LMC_Frame.ReadUInt16(payload, 52)
                    != expectedRecoveryKey.AxisReference
                || LMC_Frame.ReadUInt16(payload, 54) != 0
                || LMC_Frame.ReadInt32(payload, 56)
                    != expectedRecoveryKey.Parameters.HomingMethod
                || LMC_Frame.ReadUInt16(payload, 70) != 0
                || startCycle == 0
                || recordGeneration == 0
                || (expectedRecordGeneration != 0
                    && recordGeneration != expectedRecordGeneration))
            {
                throw new InvalidDataException(
                    operation
                    + " record does not exactly match the requested recovery key and generation.");
            }

            if (!IsValidDs402HomeOutcome(
                    response.IsSuccess,
                    recordState,
                    originalCommandStatus,
                    originalErrorId,
                    originalDetailCode,
                    ds402StatusWord,
                    actualPosition,
                    startCycle,
                    completionCycle,
                    nativeCommandState,
                    recordGeneration)
                || (requireTerminal
                    && recordState
                        == LMCAxisDs402HomeOutcomeRecordState.Running))
            {
                throw new InvalidDataException(
                    operation
                    + " contains an invalid or non-terminal runtime result combination.");
            }

            return new LMCParsedAxisDs402HomeOutcome(
                response,
                recordState,
                originalCommandStatus,
                originalErrorId,
                originalDetailCode,
                ds402StatusWord,
                actualPosition,
                startCycle,
                completionCycle,
                nativeCommandState,
                recordGeneration);
        }

        private static bool IsValidDs402HomeOutcome(
            bool responseSucceeded,
            LMCAxisDs402HomeOutcomeRecordState recordState,
            ushort originalCommandStatus,
            short originalErrorId,
            uint originalDetailCode,
            ushort ds402StatusWord,
            int actualPosition,
            uint startCycle,
            uint completionCycle,
            uint nativeCommandState,
            uint recordGeneration)
        {
            if (recordState
                    == LMCAxisDs402HomeOutcomeRecordState.Running)
            {
                return completionCycle == 0
                    && originalCommandStatus == 0
                    && originalErrorId == 0
                    && originalDetailCode == 0
                    && nativeCommandState == 0;
            }

            if (recordState
                    == LMCAxisDs402HomeOutcomeRecordState.Succeeded)
            {
                // Succeeded is committed only after the PLC observed fresh
                // homing-attained and target-reached samples. P68 is the last
                // StatusWord after CSP cleanup, where bits 10/12 no longer
                // carry the homing-mode meaning, so validate only invariants
                // that remain valid after the mode-8 restore.
                return LMCAxisDs402HomeOutcomeSemantics.IsSucceeded(
                    responseSucceeded,
                    recordState,
                    originalCommandStatus,
                    originalErrorId,
                    originalDetailCode,
                    ds402StatusWord,
                    actualPosition,
                    startCycle,
                    completionCycle,
                    nativeCommandState,
                    recordGeneration);
            }

            if (recordState
                    == LMCAxisDs402HomeOutcomeRecordState.Aborted)
            {
                return completionCycle >= startCycle
                    && originalCommandStatus == 1
                    && originalErrorId == AdminErrorId
                    && originalDetailCode
                        == (uint)LMCAdminDetailCode.Ds402HomeAborted
                    && nativeCommandState == 0;
            }

            if (recordState
                    != LMCAxisDs402HomeOutcomeRecordState.Failed
                || completionCycle < startCycle
                || originalCommandStatus != 1)
            {
                return false;
            }

            if (originalDetailCode
                    == (uint)LMCAdminDetailCode.NativeCommandRejected)
            {
                return originalErrorId == -6 && nativeCommandState != 0;
            }

            if (originalDetailCode
                    == (uint)LMCAdminDetailCode.InvalidState)
            {
                return originalErrorId == AdminErrorId
                    && nativeCommandState == 0;
            }

            return originalDetailCode
                    == (uint)LMCAdminDetailCode.Ds402HomeExecutionFailed
                && originalErrorId == AdminErrorId;
        }
    }
}

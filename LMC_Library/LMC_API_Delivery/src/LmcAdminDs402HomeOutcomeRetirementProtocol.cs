using System;

namespace LasalMotionControlLib
{
    internal static partial class LMC_AdminFrame
    {
        internal const int AxisDs402HomeOutcomeRetirementRequestPayloadLength =
            48;

        internal static byte[] RetireAxisDs402HomeOutcome(
            uint retireRequestId,
            LMCAxisDs402HomeRecoveryKey recoveryKey,
            uint recordGeneration)
        {
            if (recoveryKey == null)
            {
                throw new ArgumentNullException("recoveryKey");
            }

            if (recordGeneration == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "recordGeneration",
                    "RecordGeneration must be nonzero.");
            }

            ValidateAxisReference(recoveryKey.AxisReference);
            var buffer = CreateCommonRequest(
                LMC_CommandId.RetireAxisDs402HomeOutcome,
                recoveryKey.AxisReference,
                AxisDs402HomeOutcomeRetirementRequestPayloadLength,
                retireRequestId);
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
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 44,
                recordGeneration);
            return buffer;
        }
    }

    internal static partial class LMC_AdminParser
    {
        internal const int AxisDs402HomeOutcomeRetirementResponsePayloadLength =
            AxisDs402HomeOutcomeResponsePayloadLength;

        internal static LMCParsedAxisDs402HomeOutcome
            ParseAxisDs402HomeOutcomeRetirement(
                byte[] raw,
                uint expectedRetireRequestId,
                LMCAxisDs402HomeRecoveryKey expectedRecoveryKey,
                uint expectedRecordGeneration)
        {
            if (expectedRecoveryKey == null)
            {
                throw new ArgumentNullException("expectedRecoveryKey");
            }

            if (expectedRecordGeneration == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "expectedRecordGeneration",
                    "RecordGeneration must be nonzero.");
            }

            var transport = ParseTransport(
                raw,
                "RetireAxisDs402HomeOutcome",
                false);
            var response = ParseCommonResponse(
                transport,
                expectedRetireRequestId,
                false,
                false,
                false,
                true);
            if (!response.IsSuccess)
            {
                EnsurePayloadLength(
                    transport,
                    CommonResponsePayloadLength,
                    "RetireAxisDs402HomeOutcome failure");
                throw new LMCAxisDs402HomeOutcomeRetirementException(
                    response,
                    expectedRecoveryKey,
                    expectedRecordGeneration);
            }

            return ParseAxisDs402HomeOutcomeSuccess(
                transport,
                response,
                expectedRecoveryKey,
                "RetireAxisDs402HomeOutcome",
                true,
                expectedRecordGeneration);
        }
    }
}

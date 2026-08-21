using System;
using System.IO;

namespace LasalMotionControlLib
{
    internal static partial class LMC_AdminFrame
    {
        internal const int
            AxisSetOperationModeOutcomeRetirementRequestPayloadLength = 60;

        internal static byte[] RetireAxisSetOperationModeOutcome(
            uint retireRequestId,
            LMCAxisSetOperationModeRecoveryKey recoveryKey,
            uint recordGeneration)
        {
            if (recoveryKey == null)
            {
                throw new ArgumentNullException("recoveryKey");
            }

            if (recordGeneration == 0)
            {
                throw new ArgumentOutOfRangeException("recordGeneration");
            }

            ValidateAxisReference(recoveryKey.AxisReference);
            var buffer = CreateCommonRequest(
                LMC_CommandId.RetireAxisSetOperationModeOutcome,
                recoveryKey.AxisReference,
                AxisSetOperationModeOutcomeRetirementRequestPayloadLength,
                retireRequestId);
            WriteSetOperationModeKey(
                buffer,
                LMC_Frame.HeaderSize,
                recoveryKey,
                recoveryKey.OriginalRequestId);
            LMC_Frame.WriteUInt32(
                buffer,
                LMC_Frame.HeaderSize + 56,
                recordGeneration);
            return buffer;
        }
    }

    internal static partial class LMC_AdminParser
    {
        internal static LMCParsedAxisSetOperationModeOutcome
            ParseAxisSetOperationModeOutcomeRetirement(
                byte[] raw,
                uint expectedRetireRequestId,
                LMCAxisSetOperationModeRecoveryKey expectedRecoveryKey,
                uint expectedRecordGeneration)
        {
            if (expectedRecoveryKey == null)
            {
                throw new ArgumentNullException("expectedRecoveryKey");
            }

            if (expectedRecordGeneration == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "expectedRecordGeneration");
            }

            var transport = ParseTransport(
                raw,
                "RetireAxisSetOperationModeOutcome",
                false);
            var response = ParseSetOperationModeCommonResponse(
                transport,
                expectedRetireRequestId);
            if (!response.IsSuccess)
            {
                EnsurePayloadLength(
                    transport,
                    CommonResponsePayloadLength,
                    "RetireAxisSetOperationModeOutcome failure");
                if (!IsSetOperationModeOutcomeFailure(
                        response.DetailCode))
                {
                    throw new InvalidDataException(
                        "RetireAxisSetOperationModeOutcome returned an inapplicable detail code.");
                }

                throw new
                    LMCAxisSetOperationModeOutcomeRetirementException(
                        response,
                        expectedRecoveryKey,
                        expectedRecordGeneration);
            }

            return ParseAxisSetOperationModeOutcomeSuccess(
                transport,
                response,
                expectedRecoveryKey,
                "RetireAxisSetOperationModeOutcome",
                true,
                expectedRecordGeneration);
        }
    }
}

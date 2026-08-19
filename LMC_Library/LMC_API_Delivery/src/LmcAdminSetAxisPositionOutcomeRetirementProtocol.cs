using System;

namespace LasalMotionControlLib
{
    internal static partial class LMC_AdminFrame
    {
        internal const int AxisSetPositionOutcomeRetirementRequestPayloadLength =
            56;

        internal static byte[] RetireAxisSetPositionOutcome(
            uint retireRequestId,
            uint currentDiagnosticsBootId,
            LMCAxisSetPositionRecoveryKey recoveryKey,
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

            if (currentDiagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "currentDiagnosticsBootId");
            }

            ValidateAxisReference(recoveryKey.AxisReference);
            var buffer = CreateCommonRequest(
                LMC_CommandId.RetireAxisSetPositionOutcome,
                recoveryKey.AxisReference,
                AxisSetPositionOutcomeRetirementRequestPayloadLength,
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
                currentDiagnosticsBootId);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 24,
                recoveryKey.OriginalRequestId);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 28,
                recoveryKey.ClientIntentId0);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 32,
                recoveryKey.ClientIntentId1);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 36,
                recoveryKey.ClientIntentId2);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 40,
                recoveryKey.ClientIntentId3);
            LMC_Frame.WriteInt32(
                buffer,
                payloadOffset + 44,
                recoveryKey.TargetPosition);
            LMC_Frame.WriteInt32(
                buffer,
                payloadOffset + 48,
                recoveryKey.ExpectedActualPosition);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 52,
                recordGeneration);
            return buffer;
        }
    }

    internal static partial class LMC_AdminParser
    {
        internal const int AxisSetPositionOutcomeRetirementResponsePayloadLength =
            AxisSetPositionOutcomeResponsePayloadLength;

        internal static LMCParsedAxisSetPositionOutcome
            ParseAxisSetPositionOutcomeRetirement(
                byte[] raw,
                uint expectedRetireRequestId,
                LMCAxisSetPositionRecoveryKey expectedRecoveryKey,
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
                "RetireAxisSetPositionOutcome",
                false);
            var response = ParseCommonResponse(
                transport,
                expectedRetireRequestId,
                false,
                false,
                true);
            if (!response.IsSuccess)
            {
                if (response.DetailCode
                        < LMCAdminDetailCode.DiagnosticsBuildMismatch
                    || response.DetailCode
                        > LMCAdminDetailCode
                            .SetPositionOutcomeStorageUnavailable
                    || response.DetailCode
                        == LMCAdminDetailCode
                            .SetPositionOutcomeSlotOccupied)
                {
                    throw new System.IO.InvalidDataException(
                        "RetireAxisSetPositionOutcome returned an operation-inapplicable detail code.");
                }

                EnsurePayloadLength(
                    transport,
                    CommonResponsePayloadLength,
                    "RetireAxisSetPositionOutcome failure");
                throw new LMCAxisSetPositionOutcomeRetirementException(
                    response,
                    expectedRecoveryKey,
                    expectedRecordGeneration);
            }

            var parsed = ParseAxisSetPositionOutcomeSuccess(
                transport,
                response,
                expectedRecoveryKey,
                "RetireAxisSetPositionOutcome");
            if (parsed.RecordGeneration != expectedRecordGeneration)
            {
                throw new System.IO.InvalidDataException(
                    "RetireAxisSetPositionOutcome returned a different record generation.");
            }

            return parsed;
        }
    }
}

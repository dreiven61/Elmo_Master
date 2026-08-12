using System;
using System.IO;

namespace LasalMotionControlLib
{
    internal static partial class LMC_AdminFrame
    {
        internal const int AxisSetPositionOutcomeRequestPayloadLength = 48;

        internal static byte[] ReadAxisSetPositionOutcome(
            uint queryRequestId,
            LMCAxisSetPositionRecoveryKey recoveryKey)
        {
            if (recoveryKey == null)
            {
                throw new ArgumentNullException("recoveryKey");
            }

            ValidateAxisReference(recoveryKey.AxisReference);
            var buffer = CreateCommonRequest(
                LMC_CommandId.ReadAxisSetPositionOutcome,
                recoveryKey.AxisReference,
                AxisSetPositionOutcomeRequestPayloadLength,
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
                recoveryKey.OriginalRequestId);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 24,
                recoveryKey.ClientIntentId0);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 28,
                recoveryKey.ClientIntentId1);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 32,
                recoveryKey.ClientIntentId2);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 36,
                recoveryKey.ClientIntentId3);
            LMC_Frame.WriteInt32(
                buffer,
                payloadOffset + 40,
                recoveryKey.TargetPosition);
            LMC_Frame.WriteInt32(
                buffer,
                payloadOffset + 44,
                recoveryKey.ExpectedActualPosition);
            return buffer;
        }
    }

    internal sealed class LMCParsedAxisSetPositionOutcome
    {
        internal LMCParsedAxisSetPositionOutcome(
            LMCAdminResponse response,
            LMCAxisSetPositionOutcomeRecordState recordState,
            int appliedPosition,
            ushort originalCommandStatus,
            short originalErrorId,
            uint originalDetailCode,
            uint nativeCommandState,
            uint recordGeneration)
        {
            Response = response;
            RecordState = recordState;
            AppliedPosition = appliedPosition;
            OriginalCommandStatus = originalCommandStatus;
            OriginalErrorId = originalErrorId;
            OriginalDetailCode = originalDetailCode;
            NativeCommandState = nativeCommandState;
            RecordGeneration = recordGeneration;
        }

        internal LMCAdminResponse Response { get; private set; }
        internal LMCAxisSetPositionOutcomeRecordState RecordState
        {
            get;
            private set;
        }
        internal int AppliedPosition { get; private set; }
        internal ushort OriginalCommandStatus { get; private set; }
        internal short OriginalErrorId { get; private set; }
        internal uint OriginalDetailCode { get; private set; }
        internal uint NativeCommandState { get; private set; }
        internal uint RecordGeneration { get; private set; }
    }

    internal static partial class LMC_AdminParser
    {
        internal const int AxisSetPositionOutcomeResponsePayloadLength = 84;

        internal static LMCParsedAxisSetPositionOutcome
            ParseAxisSetPositionOutcome(
                byte[] raw,
                uint expectedQueryRequestId,
                LMCAxisSetPositionRecoveryKey expectedRecoveryKey)
        {
            if (expectedRecoveryKey == null)
            {
                throw new ArgumentNullException("expectedRecoveryKey");
            }

            var transport = ParseTransport(
                raw,
                "ReadAxisSetPositionOutcome",
                false);
            var response = ParseCommonResponse(
                transport,
                expectedQueryRequestId,
                false,
                false,
                true);
            if (!response.IsSuccess)
            {
                EnsurePayloadLength(
                    transport,
                    CommonResponsePayloadLength,
                    "ReadAxisSetPositionOutcome failure");
                throw new LMCAxisSetPositionOutcomeQueryException(
                    response,
                    expectedRecoveryKey);
            }

            return ParseAxisSetPositionOutcomeSuccess(
                transport,
                response,
                expectedRecoveryKey,
                "ReadAxisSetPositionOutcome");
        }

        internal static LMCParsedAxisSetPositionOutcome
            ParseAxisSetPositionOutcomeSuccess(
                LMC_Response transport,
                LMCAdminResponse response,
                LMCAxisSetPositionRecoveryKey expectedRecoveryKey,
                string operation)
        {
            if (expectedRecoveryKey == null)
            {
                throw new ArgumentNullException("expectedRecoveryKey");
            }

            EnsurePayloadLength(
                transport,
                AxisSetPositionOutcomeResponsePayloadLength,
                operation + " success");
            var payload = transport.Payload;
            var recordState =
                (LMCAxisSetPositionOutcomeRecordState)LMC_Frame.ReadUInt16(
                    payload,
                    16);
            var semanticMode =
                (LMCAxisSetPositionSemanticMode)LMC_Frame.ReadUInt16(
                    payload,
                    18);
            var appliedPosition = LMC_Frame.ReadInt32(payload, 64);
            var originalCommandStatus = LMC_Frame.ReadUInt16(payload, 68);
            var originalErrorId = unchecked(
                (short)LMC_Frame.ReadUInt16(payload, 70));
            var originalDetailCode = LMC_Frame.ReadUInt32(payload, 72);
            var nativeCommandState = LMC_Frame.ReadUInt32(payload, 76);
            var recordGeneration = LMC_Frame.ReadUInt32(payload, 80);

            if (semanticMode != expectedRecoveryKey.SemanticMode
                || LMC_Frame.ReadUInt32(payload, 20)
                    != expectedRecoveryKey.DiagnosticsBuild
                || LMC_Frame.ReadUInt32(payload, 24)
                    != expectedRecoveryKey.DiagnosticsBootId
                || LMC_Frame.ReadUInt32(payload, 28)
                    != expectedRecoveryKey.MapRevision
                || LMC_Frame.ReadUInt32(payload, 32)
                    != expectedRecoveryKey.OriginalRequestId
                || LMC_Frame.ReadUInt32(payload, 36)
                    != expectedRecoveryKey.ClientIntentId0
                || LMC_Frame.ReadUInt32(payload, 40)
                    != expectedRecoveryKey.ClientIntentId1
                || LMC_Frame.ReadUInt32(payload, 44)
                    != expectedRecoveryKey.ClientIntentId2
                || LMC_Frame.ReadUInt32(payload, 48)
                    != expectedRecoveryKey.ClientIntentId3
                || LMC_Frame.ReadUInt16(payload, 52)
                    != expectedRecoveryKey.AxisReference
                || LMC_Frame.ReadUInt16(payload, 54) != 0
                || LMC_Frame.ReadInt32(payload, 56)
                    != expectedRecoveryKey.TargetPosition
                || LMC_Frame.ReadInt32(payload, 60)
                    != expectedRecoveryKey.ExpectedActualPosition
                || recordGeneration == 0)
            {
                throw new InvalidDataException(
                    operation
                    + " terminal record does not exactly match the requested recovery key.");
            }

            var validTerminalResult = recordState
                == LMCAxisSetPositionOutcomeRecordState.Succeeded
                    ? originalCommandStatus == 0
                        && originalErrorId == 0
                        && originalDetailCode == 0
                        && appliedPosition
                            == expectedRecoveryKey.TargetPosition
                        && nativeCommandState == 0
                    : recordState
                            == LMCAxisSetPositionOutcomeRecordState.Rejected
                        && originalCommandStatus == 1
                        && appliedPosition == 0
                        && IsValidOriginalSetPositionRejection(
                            originalErrorId,
                            originalDetailCode,
                            nativeCommandState);
            if (!validTerminalResult)
            {
                throw new InvalidDataException(
                    operation
                    + " contains an invalid terminal SetAxisPosition result combination.");
            }

            return new LMCParsedAxisSetPositionOutcome(
                response,
                recordState,
                appliedPosition,
                originalCommandStatus,
                originalErrorId,
                originalDetailCode,
                nativeCommandState,
                recordGeneration);
        }

        private static bool IsValidOriginalSetPositionRejection(
            short errorId,
            uint detailCode,
            uint nativeCommandState)
        {
            if (detailCode
                    == (uint)LMCAdminDetailCode.NativeCommandRejected)
            {
                return errorId == -6 && nativeCommandState != 0;
            }

            var isPreNativeDetail =
                (detailCode
                        >= (uint)LMCAdminDetailCode.UnsupportedSchema
                    && detailCode
                        <= (uint)LMCAdminDetailCode.InvalidState)
                || (detailCode
                        >= (uint)LMCAdminDetailCode.NonZeroVelocity
                    && detailCode
                        <= (uint)LMCAdminDetailCode.MapRevisionMismatch)
                || (detailCode
                        >= (uint)LMCAdminDetailCode
                            .SetPositionOutcomeSlotOccupied
                    && detailCode
                        <= (uint)LMCAdminDetailCode
                            .SetPositionOutcomeStorageUnavailable);
            return isPreNativeDetail
                && errorId == AdminErrorId
                && nativeCommandState == 0;
        }
    }
}

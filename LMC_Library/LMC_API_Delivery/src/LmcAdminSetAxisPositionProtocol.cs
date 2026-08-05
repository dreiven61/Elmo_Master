using System;
using System.IO;

namespace LasalMotionControlLib
{
    internal static partial class LMC_AdminFrame
    {
        internal const int SetAxisPositionRequestPayloadLength = 48;
        internal const uint SetAxisPositionExecuteTokenValue = 0x50544553u;

        internal static byte[] SetAxisPosition(
            LMCAxisSetPositionRecoveryKey recoveryKey)
        {
            if (recoveryKey == null)
            {
                throw new ArgumentNullException("recoveryKey");
            }

            ValidateAxisReference(recoveryKey.AxisReference);

            var buffer = CreateCommonRequest(
                LMC_CommandId.SetAxisPosition,
                recoveryKey.AxisReference,
                SetAxisPositionRequestPayloadLength,
                recoveryKey.OriginalRequestId);
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
                recoveryKey.ClientIntentId0);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 24,
                recoveryKey.ClientIntentId1);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 28,
                recoveryKey.ClientIntentId2);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 32,
                recoveryKey.ClientIntentId3);
            LMC_Frame.WriteInt32(
                buffer,
                payloadOffset + 36,
                recoveryKey.TargetPosition);
            LMC_Frame.WriteInt32(
                buffer,
                payloadOffset + 40,
                recoveryKey.ExpectedActualPosition);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 44,
                SetAxisPositionExecuteTokenValue);
            return buffer;
        }
    }

    internal sealed class LMCParsedAxisSetPositionResponse
    {
        internal LMCParsedAxisSetPositionResponse(
            LMCAdminResponse response,
            int appliedPosition,
            LMCAxisSetPositionSemanticMode semanticMode,
            uint nativeCommandState)
        {
            Response = response;
            AppliedPosition = appliedPosition;
            SemanticMode = semanticMode;
            NativeCommandState = nativeCommandState;
        }

        internal LMCAdminResponse Response { get; private set; }
        internal int AppliedPosition { get; private set; }
        internal LMCAxisSetPositionSemanticMode SemanticMode { get; private set; }
        internal uint NativeCommandState { get; private set; }
    }

    internal static partial class LMC_AdminParser
    {
        internal const int SetAxisPositionResponsePayloadLength = 28;

        internal static LMCParsedAxisSetPositionResponse ParseSetAxisPosition(
            byte[] raw,
            uint expectedRequestId,
            int expectedTargetPosition)
        {
            var transport = ParseTransport(
                raw,
                "SetAxisPosition",
                false);
            var response = ParseCommonResponse(
                transport,
                expectedRequestId,
                true,
                true);
            EnsurePayloadLength(
                transport,
                SetAxisPositionResponsePayloadLength,
                "SetAxisPosition");

            var payload = transport.Payload;
            var appliedPosition = LMC_Frame.ReadInt32(payload, 16);
            var semanticMode =
                (LMCAxisSetPositionSemanticMode)LMC_Frame.ReadUInt16(
                    payload,
                    20);
            var reserved = LMC_Frame.ReadUInt16(payload, 22);
            var nativeCommandState = LMC_Frame.ReadUInt32(payload, 24);
            var nativeCommandRejected = response.DetailCode
                == LMCAdminDetailCode.NativeCommandRejected;
            var invalidResultFields = response.IsSuccess
                ? appliedPosition != expectedTargetPosition
                    || nativeCommandState != 0
                : appliedPosition != 0
                    || (nativeCommandRejected
                        && response.ErrorId != -6)
                    || (nativeCommandRejected
                        ? nativeCommandState == 0
                        : nativeCommandState != 0);

            if (semanticMode
                    != LMCAxisSetPositionSemanticMode
                        .ActualAndDestinationApplicationUnits
                || reserved != 0
                || invalidResultFields)
            {
                throw new InvalidDataException(
                    "SetAxisPosition response position, semantic mode, reserved field, or native command state does not match schema version 1.");
            }

            return new LMCParsedAxisSetPositionResponse(
                response,
                appliedPosition,
                semanticMode,
                nativeCommandState);
        }
    }
}

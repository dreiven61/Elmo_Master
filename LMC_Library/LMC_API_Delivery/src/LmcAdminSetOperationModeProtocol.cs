using System;
using System.IO;

namespace LasalMotionControlLib
{
    internal static partial class LMC_AdminFrame
    {
        internal const int StartAxisSetOperationModeRequestPayloadLength = 56;

        internal static byte[] StartAxisSetOperationMode(
            LMCAxisSetOperationModeRecoveryKey recoveryKey)
        {
            if (recoveryKey == null)
            {
                throw new ArgumentNullException("recoveryKey");
            }

            ValidateAxisReference(recoveryKey.AxisReference);
            ValidateSetOperationModeRequest(
                recoveryKey.RequestedMode,
                recoveryKey.TimeoutMilliseconds,
                recoveryKey.Flags);

            var buffer = CreateCommonRequest(
                LMC_CommandId.StartAxisSetOperationMode,
                recoveryKey.AxisReference,
                StartAxisSetOperationModeRequestPayloadLength,
                recoveryKey.OriginalRequestId);
            var payloadOffset = LMC_Frame.HeaderSize;
            WriteSetOperationModeKey(
                buffer,
                payloadOffset,
                recoveryKey,
                recoveryKey.OriginalRequestId);
            return buffer;
        }

        internal static void ValidateSetOperationModeRequest(
            LMCDriveOperationMode requestedMode,
            uint timeoutMilliseconds,
            uint flags)
        {
            var softwareModeAllowed = requestedMode
                    == LMCDriveOperationMode.CyclicSynchronousPosition
                || requestedMode == LMCDriveOperationMode.ProfilePosition
                || requestedMode == LMCDriveOperationMode.ProfileVelocity
                || requestedMode == LMCDriveOperationMode.InterpolatedPosition;
            if (!softwareModeAllowed)
            {
                throw new NotSupportedException(
                    "SetOperationMode software implementation supports PP(1), PV(3), IP(7), and CSP(8). "
                    + "Homing(6) remains owned by HomeDS402/HomeDS402Ex.");
            }

            if (timeoutMilliseconds == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "timeoutMilliseconds");
            }

            if (flags != 0)
            {
                throw new NotSupportedException(
                    "SetOperationMode schema version 1 is Immediate-only and requires Flags=0.");
            }
        }

        internal static void WriteSetOperationModeKey(
            byte[] buffer,
            int payloadOffset,
            LMCAxisSetOperationModeRecoveryKey recoveryKey,
            uint originalRequestId)
        {
            LMC_Frame.WriteUInt16(
                buffer,
                payloadOffset + 8,
                recoveryKey.SchemaVersion);
            LMC_Frame.WriteUInt16(buffer, payloadOffset + 10, 0);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 12,
                recoveryKey.DiagnosticsBuild);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 16,
                recoveryKey.DiagnosticsBootId);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 20,
                recoveryKey.MapRevision);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 24,
                originalRequestId);
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
            LMC_Frame.WriteUInt16(
                buffer,
                payloadOffset + 44,
                recoveryKey.AxisReference);
            buffer[payloadOffset + 46] = unchecked(
                (byte)recoveryKey.RequestedModeRaw);
            buffer[payloadOffset + 47] = 0;
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 48,
                recoveryKey.TimeoutMilliseconds);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 52,
                recoveryKey.Flags);
        }
    }

    internal sealed class LMCParsedAxisSetOperationModeStartResponse
    {
        internal LMCParsedAxisSetOperationModeStartResponse(
            LMCAdminResponse response,
            LMCDriveOperationMode requestedMode,
            uint nativeCommandState)
        {
            Response = response;
            RequestedMode = requestedMode;
            NativeCommandState = nativeCommandState;
        }

        internal LMCAdminResponse Response { get; private set; }
        internal LMCDriveOperationMode RequestedMode { get; private set; }
        internal uint NativeCommandState { get; private set; }
    }

    internal static partial class LMC_AdminParser
    {
        internal const int StartAxisSetOperationModeResponsePayloadLength = 24;

        internal static LMCParsedAxisSetOperationModeStartResponse
            ParseStartAxisSetOperationMode(
                byte[] raw,
                uint expectedRequestId,
                LMCDriveOperationMode expectedRequestedMode)
        {
            var transport = ParseTransport(
                raw,
                "StartAxisSetOperationMode",
                false);
            var response = ParseSetOperationModeCommonResponse(
                transport,
                expectedRequestId);

            var isMalformedCommonFailure = !response.IsSuccess
                && response.DetailCodeValue
                    <= (uint)LMCAdminDetailCode.InvalidSelection;
            if (isMalformedCommonFailure)
            {
                EnsurePayloadLength(
                    transport,
                    CommonResponsePayloadLength,
                    "StartAxisSetOperationMode common failure");
                return new LMCParsedAxisSetOperationModeStartResponse(
                    response,
                    expectedRequestedMode,
                    0);
            }

            EnsurePayloadLength(
                transport,
                StartAxisSetOperationModeResponsePayloadLength,
                "StartAxisSetOperationMode");
            var requestedModeValue = LMC_Frame.ReadInt32(
                transport.Payload,
                16);
            var nativeCommandState = LMC_Frame.ReadUInt32(
                transport.Payload,
                20);
            var isAdmissionIdentityEvidence = !response.IsSuccess
                && response.DetailCode
                    == LMCAdminDetailCode.SetOperationModeAdmissionIdentityUnavailable
                && IsValidAdmissionIdentityEvidence(nativeCommandState);
            if (requestedModeValue != (sbyte)expectedRequestedMode
                || (nativeCommandState != 0 && !isAdmissionIdentityEvidence)
                || (!response.IsSuccess
                    && !IsStartAxisSetOperationModeFailure(
                        response.DetailCode)))
            {
                throw new InvalidDataException(
                    "StartAxisSetOperationMode response echo, native state, or detail code is invalid.");
            }

            return new LMCParsedAxisSetOperationModeStartResponse(
                response,
                expectedRequestedMode,
                nativeCommandState);
        }

        private static bool IsStartAxisSetOperationModeFailure(
            LMCAdminDetailCode detailCode)
        {
            return (detailCode >= LMCAdminDetailCode.DiagnosticsBuildMismatch
                    && detailCode <= LMCAdminDetailCode.MapRevisionMismatch)
                || detailCode == LMCAdminDetailCode.AxisOwnershipConflict
                || detailCode == LMCAdminDetailCode.AxisOwnershipQuarantined
                || detailCode
                    == LMCAdminDetailCode.SetOperationModeUnsupportedMode
                || detailCode
                    == LMCAdminDetailCode.SetOperationModeUnsafeState
                || detailCode
                    == LMCAdminDetailCode.SetOperationModeOutcomeStoreCorrupt
                || detailCode
                    == LMCAdminDetailCode
                        .SetOperationModeOutcomeStorageUnavailable
                || detailCode
                    == LMCAdminDetailCode
                        .SetOperationModeAdmissionIdentityUnavailable
                || detailCode
                    == LMCAdminDetailCode
                        .SetOperationModeFeatureDisabled
                || detailCode
                    == LMCAdminDetailCode.SetOperationModeOutcomeSlotOccupied;
        }

        private static bool IsValidAdmissionIdentityEvidence(
            uint nativeCommandState)
        {
            const uint allowedMask = 0x000F1F1Fu;
            const uint diagnosticsBitmapMask = 0x000F0000u;
            return (nativeCommandState & ~allowedMask) == 0
                && (nativeCommandState & diagnosticsBitmapMask)
                    != diagnosticsBitmapMask;
        }
    }
}

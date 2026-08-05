using System;
using System.IO;

namespace LasalMotionControlLib
{
    internal static partial class LMC_AdminFrame
    {
        internal const int StartAxisDs402HomeRequestPayloadLength = 72;
        internal const uint StartAxisDs402HomeExecuteTokenValue = 0x32303448u;

        internal static byte[] StartAxisDs402Home(
            LMCAxisDs402HomeRecoveryKey recoveryKey)
        {
            if (recoveryKey == null)
            {
                throw new ArgumentNullException("recoveryKey");
            }

            ValidateAxisReference(recoveryKey.AxisReference);
            var parameters = recoveryKey.Parameters;
            ValidateAxisDs402HomeParameters(
                parameters.HomingMethod,
                parameters.Position,
                parameters.Velocity,
                parameters.Acceleration,
                parameters.DistanceLimit,
                parameters.TorqueLimit,
                parameters.BufferMode,
                parameters.TimeoutMilliseconds);

            var buffer = CreateCommonRequest(
                LMC_CommandId.StartAxisDs402Home,
                recoveryKey.AxisReference,
                StartAxisDs402HomeRequestPayloadLength,
                recoveryKey.RequestId);
            var offset = LMC_Frame.HeaderSize + CommonRequestPayloadLength;
            LMC_Frame.WriteUInt32(buffer, offset, recoveryKey.DiagnosticsBuild);
            LMC_Frame.WriteUInt32(buffer, offset + 4, recoveryKey.DiagnosticsBootId);
            LMC_Frame.WriteUInt32(buffer, offset + 8, recoveryKey.MapRevision);
            LMC_Frame.WriteUInt32(buffer, offset + 12, recoveryKey.ClientIntentId.Word0);
            LMC_Frame.WriteUInt32(buffer, offset + 16, recoveryKey.ClientIntentId.Word1);
            LMC_Frame.WriteUInt32(buffer, offset + 20, recoveryKey.ClientIntentId.Word2);
            LMC_Frame.WriteUInt32(buffer, offset + 24, recoveryKey.ClientIntentId.Word3);
            LMC_Frame.WriteInt32(buffer, offset + 28, parameters.HomingMethod);
            LMC_Frame.WriteInt32(buffer, offset + 32, parameters.Position);
            LMC_Frame.WriteInt32(buffer, offset + 36, parameters.Velocity);
            LMC_Frame.WriteInt32(buffer, offset + 40, parameters.Acceleration);
            LMC_Frame.WriteInt32(buffer, offset + 44, parameters.DistanceLimit);
            LMC_Frame.WriteInt32(buffer, offset + 48, parameters.TorqueLimit);
            LMC_Frame.WriteUInt16(buffer, offset + 52, (ushort)parameters.BufferMode);
            LMC_Frame.WriteUInt16(buffer, offset + 54, 0);
            LMC_Frame.WriteUInt32(buffer, offset + 56, parameters.TimeoutMilliseconds);
            LMC_Frame.WriteUInt32(
                buffer,
                offset + 60,
                StartAxisDs402HomeExecuteTokenValue);
            return buffer;
        }

        internal static void ValidateAxisDs402HomeParameters(
            int homingMethod,
            int position,
            int velocity,
            int acceleration,
            int distanceLimit,
            int torqueLimit,
            LMCDs402HomeBufferMode bufferMode,
            uint timeoutMilliseconds)
        {
            if (homingMethod == 35)
            {
                throw new NotSupportedException(
                    "DS402 homing method 35 is obsolete. Use method 37 for the non-moving current-position-zero operation.");
            }

            if (homingMethod
                != LMCAxisDs402HomeParameters
                    .CurrentPositionZeroHomingMethod)
            {
                throw new NotSupportedException(
                    "This LMC_HomeDS402 surface supports only method 37, the non-moving current-position operation.");
            }

            if (position
                != LMCAxisDs402HomeParameters
                    .CurrentPositionZeroHomeOffset)
            {
                throw new NotSupportedException(
                    "LMC_HomeDS402 method 37 requires Home offset Position=0 so the completed actual position is zero.");
            }

            if (velocity != 0)
            {
                throw new NotSupportedException(
                    "LMC_HomeDS402 method 37 is non-moving and requires Velocity=0.");
            }

            if (acceleration != 0)
            {
                throw new NotSupportedException(
                    "LMC_HomeDS402 method 37 is non-moving and requires Acceleration=0.");
            }

            if (distanceLimit != 0)
            {
                throw new NotSupportedException(
                    "LMC_HomeDS402 method 37 is non-moving and requires DistanceLimit=0.");
            }

            if (torqueLimit != 0)
            {
                throw new NotSupportedException(
                    "LMC_HomeDS402 method 37 is non-moving and requires TorqueLimit=0.");
            }

            if (bufferMode != LMCDs402HomeBufferMode.Aborting)
            {
                throw new NotSupportedException(
                    "DS402 Home schema version 1 has no PLC command queue and accepts Aborting mode only.");
            }

            if (timeoutMilliseconds == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "timeoutMilliseconds");
            }
        }
    }

    internal sealed class LMCParsedAxisDs402HomeStartResponse
    {
        internal LMCParsedAxisDs402HomeStartResponse(
            LMCAdminResponse response,
            int homingMethod,
            uint nativeCommandState)
        {
            Response = response;
            HomingMethod = homingMethod;
            NativeCommandState = nativeCommandState;
        }

        internal LMCAdminResponse Response { get; private set; }
        internal int HomingMethod { get; private set; }
        internal uint NativeCommandState { get; private set; }
    }

    internal static partial class LMC_AdminParser
    {
        internal const int StartAxisDs402HomeResponsePayloadLength = 24;

        internal static LMCParsedAxisDs402HomeStartResponse
            ParseStartAxisDs402Home(
                byte[] raw,
                uint expectedRequestId,
                int expectedHomingMethod)
        {
            var transport = ParseTransport(raw, "StartAxisDs402Home", false);
            var response = ParseCommonResponse(
                transport,
                expectedRequestId,
                true,
                true,
                false,
                false,
                true);
            EnsurePayloadLength(
                transport,
                StartAxisDs402HomeResponsePayloadLength,
                "StartAxisDs402Home");

            if (!response.IsSuccess
                && response.DetailCode != LMCAdminDetailCode.InvalidMotionParameters
                && response.DetailCode != LMCAdminDetailCode.InvalidState
                && response.DetailCode != LMCAdminDetailCode.NativeCommandRejected
                && response.DetailCode != LMCAdminDetailCode.DiagnosticsBuildMismatch
                && response.DetailCode != LMCAdminDetailCode.BootIdMismatch
                && response.DetailCode != LMCAdminDetailCode.MapRevisionMismatch
                && response.DetailCode
                    != LMCAdminDetailCode.Ds402HomeOutcomeSlotOccupied
                && response.DetailCode
                    != LMCAdminDetailCode.AxisOwnershipConflict
                && response.DetailCode
                    != LMCAdminDetailCode.AxisOwnershipQuarantined)
            {
                throw new InvalidDataException(
                    "StartAxisDs402Home returned an operation-inapplicable detail code.");
            }

            var homingMethod = LMC_Frame.ReadInt32(transport.Payload, 16);
            var nativeCommandState = LMC_Frame.ReadUInt32(transport.Payload, 20);
            var nativeRejected = response.DetailCode
                == LMCAdminDetailCode.NativeCommandRejected;
            var invalidNativeState = response.IsSuccess
                ? nativeCommandState != 0
                : nativeRejected
                    ? response.ErrorId != -6 || nativeCommandState == 0
                    : nativeCommandState != 0;
            if (homingMethod != expectedHomingMethod || invalidNativeState)
            {
                throw new InvalidDataException(
                    "StartAxisDs402Home response method or native state does not match schema version 1.");
            }

            return new LMCParsedAxisDs402HomeStartResponse(
                response,
                homingMethod,
                nativeCommandState);
        }
    }
}

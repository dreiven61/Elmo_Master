using System;
using System.IO;

namespace LasalMotionControlLib
{
    internal static partial class LMC_AdminFrame
    {
        internal const int StartAxisDs402HomeExRequestPayloadLength = 116;
        internal const int AxisDs402HomeExOutcomeRequestPayloadLength = 116;
        internal const int AxisDs402HomeExRetireRequestPayloadLength = 120;
        internal const uint StartAxisDs402HomeExExecuteTokenValue = 0x58453448u;

        internal static byte[] StartAxisDs402HomeEx(
            LMCAxisDs402HomeExRecoveryKey recoveryKey)
        {
            if (recoveryKey == null)
            {
                throw new ArgumentNullException("recoveryKey");
            }

            ValidateAxisReference(recoveryKey.AxisReference);
            var buffer = CreateCommonRequest(
                LMC_CommandId.StartAxisDs402HomeEx,
                recoveryKey.AxisReference,
                StartAxisDs402HomeExRequestPayloadLength,
                recoveryKey.OriginalRequestId);
            var payloadOffset = LMC_Frame.HeaderSize;
            var plan = recoveryKey.ExecutionPlan;

            LMC_Frame.WriteUInt32(buffer, payloadOffset + 8, recoveryKey.DiagnosticsBuild);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 12, recoveryKey.DiagnosticsBootId);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 16, recoveryKey.MapRevision);
            WriteIntent(buffer, payloadOffset + 20, recoveryKey.ClientIntentId);
            LMC_Frame.WriteInt32(buffer, payloadOffset + 36, plan.HomingMethod);
            WritePlanValues(buffer, payloadOffset + 40, plan);
            LMC_Frame.WriteUInt16(buffer, payloadOffset + 68, (ushort)plan.BufferMode);
            LMC_Frame.WriteUInt16(buffer, payloadOffset + 70, 0);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 72, plan.OverallTimeoutMilliseconds);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 76, plan.DetectionTimeoutMilliseconds);
            Buffer.BlockCopy(
                plan.Spare,
                0,
                buffer,
                payloadOffset + 80,
                LMCAxisDs402HomeExExecutionPlan.SpareLength);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 112,
                StartAxisDs402HomeExExecuteTokenValue);
            return buffer;
        }

        internal static byte[] ReadAxisDs402HomeExOutcome(
            uint queryRequestId,
            LMCAxisDs402HomeExRecoveryKey recoveryKey)
        {
            return BuildAxisDs402HomeExOutcomeRequest(
                LMC_CommandId.ReadAxisDs402HomeExOutcome,
                queryRequestId,
                recoveryKey,
                0,
                false);
        }

        internal static byte[] RetireAxisDs402HomeExOutcome(
            uint retireRequestId,
            LMCAxisDs402HomeExRecoveryKey recoveryKey,
            uint expectedRecordGeneration)
        {
            if (expectedRecordGeneration == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "expectedRecordGeneration");
            }

            return BuildAxisDs402HomeExOutcomeRequest(
                LMC_CommandId.RetireAxisDs402HomeExOutcome,
                retireRequestId,
                recoveryKey,
                expectedRecordGeneration,
                true);
        }

        private static byte[] BuildAxisDs402HomeExOutcomeRequest(
            ushort command,
            uint requestId,
            LMCAxisDs402HomeExRecoveryKey recoveryKey,
            uint expectedRecordGeneration,
            bool includeGeneration)
        {
            if (recoveryKey == null)
            {
                throw new ArgumentNullException("recoveryKey");
            }

            ValidateAxisReference(recoveryKey.AxisReference);
            var payloadLength = includeGeneration
                ? AxisDs402HomeExRetireRequestPayloadLength
                : AxisDs402HomeExOutcomeRequestPayloadLength;
            var buffer = CreateCommonRequest(
                command,
                recoveryKey.AxisReference,
                payloadLength,
                requestId);
            var payloadOffset = LMC_Frame.HeaderSize;
            var plan = recoveryKey.ExecutionPlan;

            LMC_Frame.WriteUInt32(buffer, payloadOffset + 8, recoveryKey.DiagnosticsBuild);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 12, recoveryKey.DiagnosticsBootId);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 16, recoveryKey.MapRevision);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 20, recoveryKey.OriginalRequestId);
            WriteIntent(buffer, payloadOffset + 24, recoveryKey.ClientIntentId);
            LMC_Frame.WriteInt32(buffer, payloadOffset + 40, plan.HomingMethod);
            WritePlanValues(buffer, payloadOffset + 44, plan);
            LMC_Frame.WriteUInt16(buffer, payloadOffset + 72, (ushort)plan.BufferMode);
            LMC_Frame.WriteUInt16(buffer, payloadOffset + 74, 0);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 76, plan.OverallTimeoutMilliseconds);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 80, plan.DetectionTimeoutMilliseconds);
            Buffer.BlockCopy(
                plan.Spare,
                0,
                buffer,
                payloadOffset + 84,
                LMCAxisDs402HomeExExecutionPlan.SpareLength);
            if (includeGeneration)
            {
                LMC_Frame.WriteUInt32(buffer, payloadOffset + 116, expectedRecordGeneration);
            }
            return buffer;
        }

        private static void WritePlanValues(
            byte[] buffer,
            int offset,
            LMCAxisDs402HomeExExecutionPlan plan)
        {
            LMC_Frame.WriteInt32(buffer, offset, plan.Position);
            LMC_Frame.WriteInt32(buffer, offset + 4, plan.DetectionVelocityLimit);
            LMC_Frame.WriteInt32(buffer, offset + 8, plan.Acceleration);
            LMC_Frame.WriteInt32(buffer, offset + 12, plan.VelocityHigh);
            LMC_Frame.WriteInt32(buffer, offset + 16, plan.VelocityLow);
            LMC_Frame.WriteInt32(buffer, offset + 20, plan.DistanceLimit);
            LMC_Frame.WriteInt32(buffer, offset + 24, plan.TorqueLimit);
        }

        private static void WriteIntent(
            byte[] buffer,
            int offset,
            LMCAxisDs402HomeExClientIntentId intent)
        {
            LMC_Frame.WriteUInt32(buffer, offset, intent.Word0);
            LMC_Frame.WriteUInt32(buffer, offset + 4, intent.Word1);
            LMC_Frame.WriteUInt32(buffer, offset + 8, intent.Word2);
            LMC_Frame.WriteUInt32(buffer, offset + 12, intent.Word3);
        }
    }

    internal sealed class LMCParsedAxisDs402HomeExStartResponse
    {
        internal LMCParsedAxisDs402HomeExStartResponse(
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
        internal const int StartAxisDs402HomeExResponsePayloadLength = 24;

        internal static LMCParsedAxisDs402HomeExStartResponse
            ParseStartAxisDs402HomeEx(
                byte[] raw,
                uint expectedRequestId,
                int expectedHomingMethod)
        {
            var transport = ParseTransport(
                raw,
                "StartAxisDs402HomeEx",
                false);
            var response = ParseDs402HomeExCommonResponse(
                transport,
                expectedRequestId);

            var malformedCommonFailure = !response.IsSuccess
                && response.DetailCodeValue
                    <= (uint)LMCAdminDetailCode.InvalidSelection;
            if (malformedCommonFailure)
            {
                EnsurePayloadLength(
                    transport,
                    CommonResponsePayloadLength,
                    "StartAxisDs402HomeEx common failure");
                return new LMCParsedAxisDs402HomeExStartResponse(
                    response,
                    expectedHomingMethod,
                    0);
            }

            EnsurePayloadLength(
                transport,
                StartAxisDs402HomeExResponsePayloadLength,
                "StartAxisDs402HomeEx");
            var homingMethod = LMC_Frame.ReadInt32(
                transport.Payload,
                16);
            var nativeCommandState = LMC_Frame.ReadUInt32(
                transport.Payload,
                20);
            if (homingMethod != expectedHomingMethod
                || nativeCommandState != 0
                || (!response.IsSuccess
                    && !IsStartAxisDs402HomeExFailure(
                        response.DetailCodeValue)))
            {
                throw new InvalidDataException(
                    "StartAxisDs402HomeEx response echo, native state, or detail code is invalid.");
            }

            return new LMCParsedAxisDs402HomeExStartResponse(
                response,
                homingMethod,
                nativeCommandState);
        }

        internal static LMCAdminResponse ParseDs402HomeExCommonResponse(
            LMC_Response transport,
            uint expectedRequestId)
        {
            if (transport == null
                || transport.Payload.Length < CommonResponsePayloadLength)
            {
                throw new InvalidDataException(
                    "HomeDS402Ex response does not contain the 16-byte common envelope.");
            }

            var payload = transport.Payload;
            var schemaVersion = LMC_Frame.ReadUInt16(payload, 0);
            var responseFlags = LMC_Frame.ReadUInt16(payload, 2);
            var commandStatus = LMC_Frame.ReadUInt16(payload, 4);
            var errorId = unchecked((short)LMC_Frame.ReadUInt16(payload, 6));
            var requestId = LMC_Frame.ReadUInt32(payload, 8);
            var detailCode = LMC_Frame.ReadUInt32(payload, 12);

            if (schemaVersion != LMC_AdminFrame.SchemaVersion
                || responseFlags != 0
                || requestId != expectedRequestId
                || commandStatus > 1
                || (commandStatus == 0
                    && (errorId != 0 || detailCode != 0))
                || (commandStatus == 1
                    && (errorId != AdminErrorId
                        || !IsDs402HomeExFailureDetail(detailCode))))
            {
                throw new InvalidDataException(
                    "HomeDS402Ex common response status, identity, or detail is invalid.");
            }

            return new LMCAdminResponse(
                transport,
                schemaVersion,
                responseFlags,
                commandStatus,
                errorId,
                requestId,
                detailCode);
        }

        private static bool IsDs402HomeExFailureDetail(uint detailCode)
        {
            return (detailCode >= 1u && detailCode <= 8u)
                || (detailCode >= 16u && detailCode <= 18u)
                || detailCode == 41u
                || detailCode == 42u
                || (detailCode >= 53u && detailCode <= 62u);
        }

        private static bool IsStartAxisDs402HomeExFailure(
            uint detailCode)
        {
            return (detailCode >= 16u && detailCode <= 18u)
                || detailCode == 41u
                || detailCode == 42u
                || detailCode == 55u
                || detailCode == 57u
                || detailCode == 60u
                || detailCode == 61u;
        }
    }
}

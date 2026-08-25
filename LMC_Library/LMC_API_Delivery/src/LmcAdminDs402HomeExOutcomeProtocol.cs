using System;
using System.IO;

namespace LasalMotionControlLib
{
    internal static partial class LMC_AdminParser
    {
        internal const int AxisDs402HomeExOutcomeResponsePayloadLength = 176;

        internal static LMCParsedAxisDs402HomeExOutcome
            ParseAxisDs402HomeExOutcome(
                byte[] raw,
                uint expectedQueryRequestId,
                LMCAxisDs402HomeExRecoveryKey expectedRecoveryKey)
        {
            if (expectedRecoveryKey == null)
            {
                throw new ArgumentNullException("expectedRecoveryKey");
            }

            var transport = ParseTransport(
                raw,
                "ReadAxisDs402HomeExOutcome",
                false);
            var response = ParseDs402HomeExCommonResponse(
                transport,
                expectedQueryRequestId);
            if (!response.IsSuccess)
            {
                EnsurePayloadLength(
                    transport,
                    CommonResponsePayloadLength,
                    "ReadAxisDs402HomeExOutcome failure");
                throw new LMCAxisDs402HomeExOutcomeQueryException(
                    response,
                    expectedRecoveryKey);
            }

            return ParseAxisDs402HomeExOutcomeSuccess(
                transport,
                response,
                expectedRecoveryKey,
                "ReadAxisDs402HomeExOutcome",
                false,
                0);
        }

        internal static LMCParsedAxisDs402HomeExOutcome
            ParseAxisDs402HomeExOutcomeRetirement(
                byte[] raw,
                uint expectedRetireRequestId,
                LMCAxisDs402HomeExRecoveryKey expectedRecoveryKey,
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
                "RetireAxisDs402HomeExOutcome",
                false);
            var response = ParseDs402HomeExCommonResponse(
                transport,
                expectedRetireRequestId);
            if (!response.IsSuccess)
            {
                EnsurePayloadLength(
                    transport,
                    CommonResponsePayloadLength,
                    "RetireAxisDs402HomeExOutcome failure");
                throw new LMCAxisDs402HomeExOutcomeRetirementException(
                    response,
                    expectedRecoveryKey,
                    expectedRecordGeneration);
            }

            return ParseAxisDs402HomeExOutcomeSuccess(
                transport,
                response,
                expectedRecoveryKey,
                "RetireAxisDs402HomeExOutcome",
                true,
                expectedRecordGeneration);
        }

        private static LMCParsedAxisDs402HomeExOutcome
            ParseAxisDs402HomeExOutcomeSuccess(
                LMC_Response transport,
                LMCAdminResponse response,
                LMCAxisDs402HomeExRecoveryKey expectedRecoveryKey,
                string operation,
                bool requireTerminal,
                uint expectedRecordGeneration)
        {
            EnsurePayloadLength(
                transport,
                AxisDs402HomeExOutcomeResponsePayloadLength,
                operation + " success");
            var payload = transport.Payload;
            var plan = expectedRecoveryKey.ExecutionPlan;
            var recordStateRaw = LMC_Frame.ReadUInt16(payload, 16);
            if (recordStateRaw < 1 || recordStateRaw > 4)
            {
                throw new InvalidDataException(
                    operation + " contains an invalid HomeDS402Ex record state.");
            }

            var recordState =
                (LMCAxisDs402HomeExOutcomeRecordState)recordStateRaw;
            var originalCommandStatus = LMC_Frame.ReadUInt16(payload, 132);
            var originalErrorId = unchecked(
                (short)LMC_Frame.ReadUInt16(payload, 134));
            var originalDetailCode = LMC_Frame.ReadUInt32(payload, 136);
            var ds402StatusWord = LMC_Frame.ReadUInt16(payload, 140);
            var actualPosition = LMC_Frame.ReadInt32(payload, 144);
            var expectedFinalPosition = LMC_Frame.ReadInt32(payload, 148);
            var startCycle = LMC_Frame.ReadUInt32(payload, 152);
            var completionCycle = LMC_Frame.ReadUInt32(payload, 156);
            var nativeCommandState = LMC_Frame.ReadUInt32(payload, 160);
            var recordGeneration = LMC_Frame.ReadUInt32(payload, 164);
            var cleanupProofFlags =
                (LMCAxisDs402HomeExCleanupProofFlags)LMC_Frame.ReadUInt32(
                    payload,
                    168);
            var sdoExecutorToken = LMC_Frame.ReadUInt32(payload, 172);

            if (LMC_Frame.ReadUInt16(payload, 18) != 0
                || LMC_Frame.ReadUInt32(payload, 20)
                    != expectedRecoveryKey.DiagnosticsBuild
                || LMC_Frame.ReadUInt32(payload, 24)
                    != expectedRecoveryKey.DiagnosticsBootId
                || LMC_Frame.ReadUInt32(payload, 28)
                    != expectedRecoveryKey.MapRevision
                || LMC_Frame.ReadUInt32(payload, 32)
                    != expectedRecoveryKey.OriginalRequestId
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
                || LMC_Frame.ReadInt32(payload, 56) != plan.HomingMethod
                || LMC_Frame.ReadInt32(payload, 60) != plan.Position
                || LMC_Frame.ReadInt32(payload, 64)
                    != plan.DetectionVelocityLimit
                || LMC_Frame.ReadInt32(payload, 68) != plan.Acceleration
                || LMC_Frame.ReadInt32(payload, 72) != plan.VelocityHigh
                || LMC_Frame.ReadInt32(payload, 76) != plan.VelocityLow
                || LMC_Frame.ReadInt32(payload, 80) != plan.DistanceLimit
                || LMC_Frame.ReadInt32(payload, 84) != plan.TorqueLimit
                || LMC_Frame.ReadUInt16(payload, 88)
                    != (ushort)plan.BufferMode
                || LMC_Frame.ReadUInt16(payload, 90) != 0
                || LMC_Frame.ReadUInt32(payload, 92)
                    != plan.OverallTimeoutMilliseconds
                || LMC_Frame.ReadUInt32(payload, 96)
                    != plan.DetectionTimeoutMilliseconds
                || !IsZeroRange(payload, 100, 32)
                || LMC_Frame.ReadUInt16(payload, 142) != 0
                || recordGeneration == 0
                || (expectedRecordGeneration != 0
                    && recordGeneration != expectedRecordGeneration))
            {
                throw new InvalidDataException(
                    operation
                    + " outcome does not exactly match the HomeDS402Ex recovery key and generation.");
            }

            if (!IsValidDs402HomeExOutcome(
                    recordState,
                    originalCommandStatus,
                    originalErrorId,
                    originalDetailCode,
                    actualPosition,
                    expectedFinalPosition,
                    startCycle,
                    completionCycle,
                    nativeCommandState,
                    cleanupProofFlags,
                    sdoExecutorToken,
                    plan)
                || (requireTerminal
                    && recordState
                        == LMCAxisDs402HomeExOutcomeRecordState.Running))
            {
                throw new InvalidDataException(
                    operation
                    + " contains an invalid HomeDS402Ex runtime/cleanup result combination.");
            }

            return new LMCParsedAxisDs402HomeExOutcome(
                response,
                recordState,
                originalCommandStatus,
                originalErrorId,
                originalDetailCode,
                ds402StatusWord,
                actualPosition,
                expectedFinalPosition,
                startCycle,
                completionCycle,
                nativeCommandState,
                recordGeneration,
                cleanupProofFlags,
                sdoExecutorToken);
        }

        private static bool IsValidDs402HomeExOutcome(
            LMCAxisDs402HomeExOutcomeRecordState recordState,
            ushort originalCommandStatus,
            short originalErrorId,
            uint originalDetailCode,
            int actualPosition,
            int expectedFinalPosition,
            uint startCycle,
            uint completionCycle,
            uint nativeCommandState,
            LMCAxisDs402HomeExCleanupProofFlags cleanupProofFlags,
            uint sdoExecutorToken,
            LMCAxisDs402HomeExExecutionPlan plan)
        {
            if (startCycle == 0)
            {
                return false;
            }

            var knownCleanup =
                LMCAxisDs402HomeExCleanupProofFlags.RequiredForSafeTerminal;
            if ((cleanupProofFlags & ~knownCleanup) != 0)
            {
                return false;
            }

            if (recordState == LMCAxisDs402HomeExOutcomeRecordState.Running)
            {
                return completionCycle == 0
                    && originalCommandStatus == 0
                    && originalErrorId == 0
                    && originalDetailCode == 0
                    && nativeCommandState == 0
                    && cleanupProofFlags
                        == LMCAxisDs402HomeExCleanupProofFlags.None;
            }

            if (completionCycle < startCycle
                || nativeCommandState != 0
                || cleanupProofFlags != knownCleanup
                || sdoExecutorToken == 0)
            {
                return false;
            }

            if (recordState == LMCAxisDs402HomeExOutcomeRecordState.Succeeded)
            {
                int planExpectedFinalPosition;
                try
                {
                    planExpectedFinalPosition = checked(-plan.Position);
                }
                catch (OverflowException)
                {
                    return false;
                }

                return originalCommandStatus == 0
                    && originalErrorId == 0
                    && originalDetailCode == 0
                    && expectedFinalPosition == planExpectedFinalPosition
                    && actualPosition == expectedFinalPosition;
            }

            if (recordState == LMCAxisDs402HomeExOutcomeRecordState.Aborted)
            {
                return originalCommandStatus == 1
                    && originalErrorId == AdminErrorId
                    && originalDetailCode == 59u;
            }

            return recordState == LMCAxisDs402HomeExOutcomeRecordState.Failed
                && originalCommandStatus == 1
                && originalErrorId == AdminErrorId
                && (originalDetailCode == 58u
                    || originalDetailCode == 61u);
        }

        private static bool IsZeroRange(
            byte[] buffer,
            int offset,
            int count)
        {
            for (var index = 0; index < count; index++)
            {
                if (buffer[offset + index] != 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}

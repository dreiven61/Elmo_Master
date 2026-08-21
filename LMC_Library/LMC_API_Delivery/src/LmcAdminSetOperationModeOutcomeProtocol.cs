using System;
using System.IO;

namespace LasalMotionControlLib
{
    internal static partial class LMC_AdminFrame
    {
        internal const int AxisSetOperationModeOutcomeRequestPayloadLength = 56;

        internal static byte[] ReadAxisSetOperationModeOutcome(
            uint queryRequestId,
            LMCAxisSetOperationModeRecoveryKey recoveryKey)
        {
            if (recoveryKey == null)
            {
                throw new ArgumentNullException("recoveryKey");
            }

            ValidateAxisReference(recoveryKey.AxisReference);
            var buffer = CreateCommonRequest(
                LMC_CommandId.ReadAxisSetOperationModeOutcome,
                recoveryKey.AxisReference,
                AxisSetOperationModeOutcomeRequestPayloadLength,
                queryRequestId);
            WriteSetOperationModeKey(
                buffer,
                LMC_Frame.HeaderSize,
                recoveryKey,
                recoveryKey.OriginalRequestId);
            return buffer;
        }
    }

    internal sealed class LMCParsedAxisSetOperationModeOutcome
    {
        internal LMCParsedAxisSetOperationModeOutcome(
            LMCAdminResponse response,
            LMCAxisSetOperationModeOutcomeRecordState recordState,
            sbyte observedModeRaw,
            ushort originalCommandStatus,
            short originalErrorId,
            uint originalDetailCode,
            uint sdoExecutorToken,
            LMCAxisSetOperationModeEvidenceFlags evidenceFlags,
            uint startCycle,
            uint completionCycle,
            uint nativeCommandState,
            uint recordGeneration,
            sbyte previousModeRaw,
            uint quarantineReason,
            ushort ds402StatusWord,
            uint contextCheck)
        {
            Response = response;
            RecordState = recordState;
            ObservedModeRaw = observedModeRaw;
            OriginalCommandStatus = originalCommandStatus;
            OriginalErrorId = originalErrorId;
            OriginalDetailCode = originalDetailCode;
            SdoExecutorToken = sdoExecutorToken;
            EvidenceFlags = evidenceFlags;
            StartCycle = startCycle;
            CompletionCycle = completionCycle;
            NativeCommandState = nativeCommandState;
            RecordGeneration = recordGeneration;
            PreviousModeRaw = previousModeRaw;
            QuarantineReason = quarantineReason;
            Ds402StatusWord = ds402StatusWord;
            ContextCheck = contextCheck;
        }

        internal LMCAdminResponse Response { get; private set; }
        internal LMCAxisSetOperationModeOutcomeRecordState RecordState
        {
            get;
            private set;
        }
        internal sbyte ObservedModeRaw { get; private set; }
        internal ushort OriginalCommandStatus { get; private set; }
        internal short OriginalErrorId { get; private set; }
        internal uint OriginalDetailCode { get; private set; }
        internal uint SdoExecutorToken { get; private set; }
        internal LMCAxisSetOperationModeEvidenceFlags EvidenceFlags
        {
            get;
            private set;
        }
        internal uint StartCycle { get; private set; }
        internal uint CompletionCycle { get; private set; }
        internal uint NativeCommandState { get; private set; }
        internal uint RecordGeneration { get; private set; }
        internal sbyte PreviousModeRaw { get; private set; }
        internal uint QuarantineReason { get; private set; }
        internal ushort Ds402StatusWord { get; private set; }
        internal uint ContextCheck { get; private set; }
    }

    internal static partial class LMC_AdminParser
    {
        internal const int AxisSetOperationModeOutcomeResponsePayloadLength =
            112;

        private const LMCAxisSetOperationModeEvidenceFlags
            KnownSetOperationModeEvidenceFlags =
                LMCAxisSetOperationModeEvidenceFlags.WriteRequested
                | LMCAxisSetOperationModeEvidenceFlags.WriteDispatched
                | LMCAxisSetOperationModeEvidenceFlags.VerifyReadDispatched
                | LMCAxisSetOperationModeEvidenceFlags.VerifyReadCompleted
                | LMCAxisSetOperationModeEvidenceFlags.OwnerReleased
                | LMCAxisSetOperationModeEvidenceFlags.ExecutorReusable;

        internal static LMCParsedAxisSetOperationModeOutcome
            ParseAxisSetOperationModeOutcome(
                byte[] raw,
                uint expectedQueryRequestId,
                LMCAxisSetOperationModeRecoveryKey expectedRecoveryKey)
        {
            if (expectedRecoveryKey == null)
            {
                throw new ArgumentNullException("expectedRecoveryKey");
            }

            var transport = ParseTransport(
                raw,
                "ReadAxisSetOperationModeOutcome",
                false);
            var response = ParseSetOperationModeCommonResponse(
                transport,
                expectedQueryRequestId);
            if (!response.IsSuccess)
            {
                EnsurePayloadLength(
                    transport,
                    CommonResponsePayloadLength,
                    "ReadAxisSetOperationModeOutcome failure");
                if (!IsSetOperationModeOutcomeFailure(
                        response.DetailCode))
                {
                    throw new InvalidDataException(
                        "ReadAxisSetOperationModeOutcome returned an inapplicable detail code.");
                }

                throw new LMCAxisSetOperationModeOutcomeQueryException(
                    response,
                    expectedRecoveryKey);
            }

            return ParseAxisSetOperationModeOutcomeSuccess(
                transport,
                response,
                expectedRecoveryKey,
                "ReadAxisSetOperationModeOutcome",
                false,
                0);
        }

        internal static LMCParsedAxisSetOperationModeOutcome
            ParseAxisSetOperationModeOutcomeSuccess(
                LMC_Response transport,
                LMCAdminResponse response,
                LMCAxisSetOperationModeRecoveryKey expectedRecoveryKey,
                string operation,
                bool requireTerminal,
                uint expectedRecordGeneration)
        {
            EnsurePayloadLength(
                transport,
                AxisSetOperationModeOutcomeResponsePayloadLength,
                operation + " success");
            var payload = transport.Payload;
            var recordState =
                (LMCAxisSetOperationModeOutcomeRecordState)
                    LMC_Frame.ReadUInt16(payload, 16);
            var requestedModeRaw = unchecked((sbyte)payload[54]);
            var observedModeRaw = unchecked((sbyte)payload[55]);
            var originalCommandStatus = LMC_Frame.ReadUInt16(payload, 64);
            var originalErrorId = unchecked(
                (short)LMC_Frame.ReadUInt16(payload, 66));
            var originalDetailCode = LMC_Frame.ReadUInt32(payload, 68);
            var sdoExecutorToken = LMC_Frame.ReadUInt32(payload, 72);
            var evidenceFlags =
                (LMCAxisSetOperationModeEvidenceFlags)
                    LMC_Frame.ReadUInt32(payload, 76);
            var startCycle = LMC_Frame.ReadUInt32(payload, 80);
            var completionCycle = LMC_Frame.ReadUInt32(payload, 84);
            var nativeCommandState = LMC_Frame.ReadUInt32(payload, 88);
            var recordGeneration = LMC_Frame.ReadUInt32(payload, 92);
            var previousModeRaw = unchecked((sbyte)payload[96]);
            var quarantineReason = LMC_Frame.ReadUInt32(payload, 100);
            var ds402StatusWord = LMC_Frame.ReadUInt16(payload, 104);
            var contextCheck = LMC_Frame.ReadUInt32(payload, 108);

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
                    != expectedRecoveryKey.ClientIntentId0
                || LMC_Frame.ReadUInt32(payload, 40)
                    != expectedRecoveryKey.ClientIntentId1
                || LMC_Frame.ReadUInt32(payload, 44)
                    != expectedRecoveryKey.ClientIntentId2
                || LMC_Frame.ReadUInt32(payload, 48)
                    != expectedRecoveryKey.ClientIntentId3
                || LMC_Frame.ReadUInt16(payload, 52)
                    != expectedRecoveryKey.AxisReference
                || requestedModeRaw != expectedRecoveryKey.RequestedModeRaw
                || LMC_Frame.ReadUInt32(payload, 56)
                    != expectedRecoveryKey.TimeoutMilliseconds
                || LMC_Frame.ReadUInt32(payload, 60)
                    != expectedRecoveryKey.Flags
                || payload[97] != 0
                || payload[98] != 0
                || payload[99] != 0
                || LMC_Frame.ReadUInt16(payload, 106) != 0
                || recordGeneration == 0
                || startCycle == 0
                || sdoExecutorToken == 0
                || quarantineReason != 0
                || (evidenceFlags
                    & ~KnownSetOperationModeEvidenceFlags) != 0
                || (expectedRecordGeneration != 0
                    && recordGeneration != expectedRecordGeneration))
            {
                throw new InvalidDataException(
                    operation
                    + " record does not exactly match the requested key, generation, or schema.");
            }

            if (!IsValidSetOperationModeOutcome(
                    recordState,
                    observedModeRaw,
                    expectedRecoveryKey.RequestedModeRaw,
                    originalCommandStatus,
                    originalErrorId,
                    originalDetailCode,
                    evidenceFlags,
                    startCycle,
                    completionCycle,
                    nativeCommandState,
                    quarantineReason)
                || (requireTerminal
                    && recordState
                        == LMCAxisSetOperationModeOutcomeRecordState.Running))
            {
                throw new InvalidDataException(
                    operation
                    + " contains an invalid or non-terminal runtime result combination.");
            }

            return new LMCParsedAxisSetOperationModeOutcome(
                response,
                recordState,
                observedModeRaw,
                originalCommandStatus,
                originalErrorId,
                originalDetailCode,
                sdoExecutorToken,
                evidenceFlags,
                startCycle,
                completionCycle,
                nativeCommandState,
                recordGeneration,
                previousModeRaw,
                quarantineReason,
                ds402StatusWord,
                contextCheck);
        }

        internal static bool IsSetOperationModeOutcomeFailure(
            LMCAdminDetailCode detailCode)
        {
            return detailCode >= LMCAdminDetailCode.UnsupportedSchema
                    && detailCode <= LMCAdminDetailCode.InvalidSelection
                || detailCode >= LMCAdminDetailCode.DiagnosticsBuildMismatch
                    && detailCode <= LMCAdminDetailCode.MapRevisionMismatch
                || detailCode
                    >= LMCAdminDetailCode.SetOperationModeOutcomeNotFound
                    && detailCode
                        <= LMCAdminDetailCode
                            .SetOperationModeOutcomeStorageUnavailable;
        }

        private static bool IsValidSetOperationModeOutcome(
            LMCAxisSetOperationModeOutcomeRecordState recordState,
            sbyte observedModeRaw,
            sbyte requestedModeRaw,
            ushort originalCommandStatus,
            short originalErrorId,
            uint originalDetailCode,
            LMCAxisSetOperationModeEvidenceFlags evidenceFlags,
            uint startCycle,
            uint completionCycle,
            uint nativeCommandState,
            uint quarantineReason)
        {
            var writeRequested = (evidenceFlags
                & LMCAxisSetOperationModeEvidenceFlags.WriteRequested) != 0;
            var writeDispatched = (evidenceFlags
                & LMCAxisSetOperationModeEvidenceFlags.WriteDispatched) != 0;
            var verifyDispatched = (evidenceFlags
                & LMCAxisSetOperationModeEvidenceFlags
                    .VerifyReadDispatched) != 0;
            var verifyCompleted = (evidenceFlags
                & LMCAxisSetOperationModeEvidenceFlags
                    .VerifyReadCompleted) != 0;
            var ownerReleased = (evidenceFlags
                & LMCAxisSetOperationModeEvidenceFlags.OwnerReleased) != 0;
            var executorReusable = (evidenceFlags
                & LMCAxisSetOperationModeEvidenceFlags.ExecutorReusable) != 0;

            if ((writeDispatched && !writeRequested)
                || (verifyCompleted && !verifyDispatched))
            {
                return false;
            }

            if (recordState
                == LMCAxisSetOperationModeOutcomeRecordState.Running)
            {
                return completionCycle == 0
                    && originalCommandStatus == 0
                    && originalErrorId == 0
                    && originalDetailCode == 0
                    && !ownerReleased
                    && !executorReusable
                    && nativeCommandState == 0
                    && quarantineReason == 0;
            }

            if (completionCycle < startCycle
                || !ownerReleased
                || !executorReusable)
            {
                return false;
            }

            if (recordState
                == LMCAxisSetOperationModeOutcomeRecordState.Succeeded)
            {
                return observedModeRaw == requestedModeRaw
                    && originalCommandStatus == 0
                    && originalErrorId == 0
                    && originalDetailCode == 0
                    && verifyDispatched
                    && verifyCompleted
                    && (!writeRequested || writeDispatched)
                    && nativeCommandState == 0
                    && quarantineReason == 0;
            }

            if (recordState
                    != LMCAxisSetOperationModeOutcomeRecordState.Failed
                && recordState
                    != LMCAxisSetOperationModeOutcomeRecordState.Aborted)
            {
                return false;
            }

            return originalCommandStatus == 1
                && originalErrorId == AdminErrorId
                && (originalDetailCode
                        == (uint)LMCAdminDetailCode
                            .SetOperationModeUnsafeState
                    || originalDetailCode
                        == (uint)LMCAdminDetailCode
                            .SetOperationModeExecutionFailed)
                && nativeCommandState == 0;
        }
    }
}

using System;
using System.IO;

namespace LasalMotionControlLib
{
    internal static partial class LMC_AdminFrame
    {
        internal const ushort StartLmcHomeCommandId = 0x7D13;
        internal const ushort ReadLmcHomeOutcomeCommandId = 0x7D18;
        internal const ushort RetireLmcHomeOutcomeCommandId = 0x7D19;
        internal const int StartLmcHomeRequestPayloadLength = 56;
        internal const int LmcHomeOutcomeRequestPayloadLength = 56;
        internal const int LmcHomeRetirementRequestPayloadLength = 60;
        internal const int LmcHomeMinimumTimeoutMilliseconds = 100;
        internal const int LmcHomeMaximumTimeoutMilliseconds = 5000;
        internal const uint LmcHomeExecuteTokenValue = 0x454D4F48u;

        internal static void ValidateLmcHome(
            LMCHomeSemanticMode semanticMode,
            int timeoutMilliseconds)
        {
            if (semanticMode
                != LMCHomeSemanticMode.CurrentPositionZero)
            {
                throw new ArgumentOutOfRangeException(
                    "semanticMode",
                    "LMC_Home supports only CurrentPositionZero.");
            }

            if (timeoutMilliseconds
                    < LmcHomeMinimumTimeoutMilliseconds
                || timeoutMilliseconds
                    > LmcHomeMaximumTimeoutMilliseconds)
            {
                throw new ArgumentOutOfRangeException(
                    "timeoutMilliseconds",
                    "LMC_Home timeout must be from 100 through 5000 milliseconds.");
            }
        }

        internal static byte[] StartLmcHome(
            LMCHomeRecoveryKey recoveryKey)
        {
            if (recoveryKey == null)
            {
                throw new ArgumentNullException("recoveryKey");
            }

            ValidateAxisReference(recoveryKey.AxisReference);
            ValidateLmcHome(
                recoveryKey.SemanticMode,
                recoveryKey.TimeoutMilliseconds);

            var buffer = CreateCommonRequest(
                StartLmcHomeCommandId,
                recoveryKey.AxisReference,
                StartLmcHomeRequestPayloadLength,
                recoveryKey.OriginalRequestId);
            var payloadOffset = LMC_Frame.HeaderSize;
            WriteLmcHomeIdentity(
                buffer,
                payloadOffset,
                recoveryKey,
                false,
                0);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 52,
                LmcHomeExecuteTokenValue);
            return buffer;
        }

        internal static byte[] ReadLmcHomeOutcome(
            uint queryRequestId,
            uint currentDiagnosticsBootId,
            LMCHomeRecoveryKey recoveryKey)
        {
            if (recoveryKey == null)
            {
                throw new ArgumentNullException("recoveryKey");
            }

            if (currentDiagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "currentDiagnosticsBootId");
            }

            ValidateAxisReference(recoveryKey.AxisReference);
            var buffer = CreateCommonRequest(
                ReadLmcHomeOutcomeCommandId,
                recoveryKey.AxisReference,
                LmcHomeOutcomeRequestPayloadLength,
                queryRequestId);
            var payloadOffset = LMC_Frame.HeaderSize;
            WriteLmcHomeIdentity(
                buffer,
                payloadOffset,
                recoveryKey,
                true,
                currentDiagnosticsBootId);
            return buffer;
        }

        internal static byte[] RetireLmcHomeOutcome(
            uint retireRequestId,
            uint currentDiagnosticsBootId,
            LMCHomeRecoveryKey recoveryKey,
            uint recordGeneration)
        {
            if (recordGeneration == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "recordGeneration",
                    "RecordGeneration must be nonzero.");
            }

            var query = ReadLmcHomeOutcome(
                retireRequestId,
                currentDiagnosticsBootId,
                recoveryKey);
            var buffer = LMC_Frame.CreateRequest(
                RetireLmcHomeOutcomeCommandId,
                recoveryKey.AxisReference,
                LmcHomeRetirementRequestPayloadLength);
            Buffer.BlockCopy(
                query,
                LMC_Frame.HeaderSize,
                buffer,
                LMC_Frame.HeaderSize,
                LmcHomeOutcomeRequestPayloadLength);
            LMC_Frame.WriteUInt32(
                buffer,
                LMC_Frame.HeaderSize + 56,
                recordGeneration);
            return buffer;
        }

        private static void WriteLmcHomeIdentity(
            byte[] buffer,
            int payloadOffset,
            LMCHomeRecoveryKey recoveryKey,
            bool isOutcomeRequest,
            uint currentDiagnosticsBootId)
        {
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 8,
                recoveryKey.DiagnosticsBuild);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 12,
                recoveryKey.OriginalDiagnosticsBootId);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 16,
                recoveryKey.MapRevision);

            var identityOffset = 20;
            if (isOutcomeRequest)
            {
                LMC_Frame.WriteUInt32(
                    buffer,
                    payloadOffset + 20,
                    currentDiagnosticsBootId);
                identityOffset = 24;
                LMC_Frame.WriteUInt32(
                    buffer,
                    payloadOffset + identityOffset,
                    recoveryKey.OriginalRequestId);
                identityOffset += 4;
            }

            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + identityOffset,
                recoveryKey.ClientIntentId0);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + identityOffset + 4,
                recoveryKey.ClientIntentId1);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + identityOffset + 8,
                recoveryKey.ClientIntentId2);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + identityOffset + 12,
                recoveryKey.ClientIntentId3);

            var semanticOffset = isOutcomeRequest ? 44 : 36;
            LMC_Frame.WriteUInt16(
                buffer,
                payloadOffset + semanticOffset,
                (ushort)recoveryKey.SemanticMode);
            LMC_Frame.WriteUInt16(
                buffer,
                payloadOffset + semanticOffset + 2,
                0);
            LMC_Frame.WriteInt32(
                buffer,
                payloadOffset + semanticOffset + 4,
                recoveryKey.ExpectedActualPosition);
            LMC_Frame.WriteInt32(
                buffer,
                payloadOffset + semanticOffset + 8,
                0);
            if (!isOutcomeRequest)
            {
                LMC_Frame.WriteUInt32(
                    buffer,
                    payloadOffset + 48,
                    checked((uint)recoveryKey.TimeoutMilliseconds));
            }
        }
    }

    internal sealed class LMCParsedHomeStartResponse
    {
        internal LMCParsedHomeStartResponse(
            LMCAdminResponse response,
            LMCHomeSemanticMode semanticMode,
            uint nativeCommandState)
        {
            Response = response;
            SemanticMode = semanticMode;
            NativeCommandState = nativeCommandState;
        }

        internal LMCAdminResponse Response { get; private set; }
        internal LMCHomeSemanticMode SemanticMode { get; private set; }
        internal uint NativeCommandState { get; private set; }
    }

    internal sealed class LMCParsedHomeOutcome
    {
        internal LMCParsedHomeOutcome(
            LMCAdminResponse response,
            LMCHomeOutcomeRecordState recordState,
            ushort originalCommandStatus,
            short originalErrorId,
            uint originalDetailCode,
            uint axisStatus,
            int axisError,
            int rawDrivePositionBefore,
            int rawDrivePositionAfter,
            int actualApplicationPositionAfter,
            int setApplicationPositionAfter,
            int actualInternalPositionAfter,
            int setInternalPositionAfter,
            int destinationInternalPositionAfter,
            int masterInternalPositionAfter,
            uint nativeCommandState,
            uint evidenceFlags,
            uint startMilliseconds,
            uint completionMilliseconds,
            uint stopState,
            uint runtimePhase,
            uint recordGeneration)
        {
            Response = response;
            RecordState = recordState;
            OriginalCommandStatus = originalCommandStatus;
            OriginalErrorId = originalErrorId;
            OriginalDetailCode = originalDetailCode;
            AxisStatus = axisStatus;
            AxisError = axisError;
            RawDrivePositionBefore = rawDrivePositionBefore;
            RawDrivePositionAfter = rawDrivePositionAfter;
            ActualApplicationPositionAfter = actualApplicationPositionAfter;
            SetApplicationPositionAfter = setApplicationPositionAfter;
            ActualInternalPositionAfter = actualInternalPositionAfter;
            SetInternalPositionAfter = setInternalPositionAfter;
            DestinationInternalPositionAfter =
                destinationInternalPositionAfter;
            MasterInternalPositionAfter = masterInternalPositionAfter;
            NativeCommandState = nativeCommandState;
            EvidenceFlags = evidenceFlags;
            StartMilliseconds = startMilliseconds;
            CompletionMilliseconds = completionMilliseconds;
            StopState = stopState;
            RuntimePhase = runtimePhase;
            RecordGeneration = recordGeneration;
        }

        internal LMCAdminResponse Response { get; private set; }
        internal LMCHomeOutcomeRecordState RecordState
        {
            get;
            private set;
        }
        internal ushort OriginalCommandStatus { get; private set; }
        internal short OriginalErrorId { get; private set; }
        internal uint OriginalDetailCode { get; private set; }
        internal uint AxisStatus { get; private set; }
        internal int AxisError { get; private set; }
        internal int RawDrivePositionBefore { get; private set; }
        internal int RawDrivePositionAfter { get; private set; }
        internal int ActualApplicationPositionAfter { get; private set; }
        internal int SetApplicationPositionAfter { get; private set; }
        internal int ActualInternalPositionAfter { get; private set; }
        internal int SetInternalPositionAfter { get; private set; }
        internal int DestinationInternalPositionAfter { get; private set; }
        internal int MasterInternalPositionAfter { get; private set; }
        internal uint NativeCommandState { get; private set; }
        internal uint EvidenceFlags { get; private set; }
        internal uint StartMilliseconds { get; private set; }
        internal uint CompletionMilliseconds { get; private set; }
        internal uint StopState { get; private set; }
        internal uint RuntimePhase { get; private set; }
        internal uint RecordGeneration { get; private set; }
    }

    internal static partial class LMC_AdminParser
    {
        internal const int StartLmcHomeResponsePayloadLength = 24;
        internal const int LmcHomeOutcomeResponsePayloadLength = 144;

        internal static LMCParsedHomeStartResponse
            ParseStartLmcHome(
                byte[] raw,
                uint expectedRequestId,
                LMCHomeSemanticMode expectedSemanticMode)
        {
            var transport = ParseTransport(raw, "LMC_Home", false);
            var response = ParseCommonResponse(
                transport,
                expectedRequestId,
                true,
                false,
                false,
                false,
                false,
                false,
                true);
            if (!response.IsSuccess)
            {
                EnsurePayloadLength(
                    transport,
                    CommonResponsePayloadLength,
                    "LMC_Home rejection");
                if (response.DetailCode
                    == LMCAdminDetailCode.NativeCommandRejected)
                {
                    throw new InvalidDataException(
                        "LMC_Home start rejection cannot claim a native rejection without the terminal retained record.");
                }

                return new LMCParsedHomeStartResponse(
                    response,
                    expectedSemanticMode,
                    0);
            }

            EnsurePayloadLength(
                transport,
                StartLmcHomeResponsePayloadLength,
                "LMC_Home acceptance");
            var payload = transport.Payload;
            var semanticMode =
                (LMCHomeSemanticMode)LMC_Frame.ReadUInt16(
                    payload,
                    16);
            var reserved = LMC_Frame.ReadUInt16(payload, 18);
            var nativeCommandState = LMC_Frame.ReadUInt32(payload, 20);
            if (semanticMode != expectedSemanticMode
                || semanticMode
                    != LMCHomeSemanticMode.CurrentPositionZero
                || reserved != 0
                || nativeCommandState != 0)
            {
                throw new InvalidDataException(
                    "LMC_Home ACK must echo CurrentPositionZero with zero reserved and NativeCommandState fields.");
            }

            return new LMCParsedHomeStartResponse(
                response,
                semanticMode,
                nativeCommandState);
        }

        internal static LMCParsedHomeOutcome
            ParseLmcHomeOutcome(
                byte[] raw,
                uint expectedQueryRequestId,
                LMCHomeRecoveryKey expectedRecoveryKey)
        {
            return ParseLmcHomeOutcomeCore(
                raw,
                expectedQueryRequestId,
                expectedRecoveryKey,
                false,
                0,
                "ReadLMC_HomeOutcome");
        }

        internal static LMCParsedHomeOutcome
            ParseLmcHomeOutcomeRetirement(
                byte[] raw,
                uint expectedRetireRequestId,
                LMCHomeRecoveryKey expectedRecoveryKey,
                uint expectedRecordGeneration)
        {
            if (expectedRecordGeneration == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "expectedRecordGeneration");
            }

            return ParseLmcHomeOutcomeCore(
                raw,
                expectedRetireRequestId,
                expectedRecoveryKey,
                true,
                expectedRecordGeneration,
                "RetireLMC_HomeOutcome");
        }

        private static LMCParsedHomeOutcome
            ParseLmcHomeOutcomeCore(
                byte[] raw,
                uint expectedRequestId,
                LMCHomeRecoveryKey expectedRecoveryKey,
                bool requireTerminal,
                uint expectedRecordGeneration,
                string operation)
        {
            if (expectedRecoveryKey == null)
            {
                throw new ArgumentNullException("expectedRecoveryKey");
            }

            var transport = ParseTransport(raw, operation, false);
            var response = ParseCommonResponse(
                transport,
                expectedRequestId,
                false,
                false,
                false,
                false,
                false,
                true,
                false);
            if (!response.IsSuccess)
            {
                EnsurePayloadLength(
                    transport,
                    CommonResponsePayloadLength,
                    operation + " failure");
                if (requireTerminal)
                {
                    throw new LMCHomeOutcomeRetirementException(
                        response,
                        expectedRecoveryKey,
                        expectedRecordGeneration);
                }

                throw new LMCHomeOutcomeQueryException(
                    response,
                    expectedRecoveryKey);
            }

            EnsurePayloadLength(
                transport,
                LmcHomeOutcomeResponsePayloadLength,
                operation + " success");
            var payload = transport.Payload;
            var recordState =
                (LMCHomeOutcomeRecordState)LMC_Frame.ReadUInt16(
                    payload,
                    16);
            var originalCommandStatus = LMC_Frame.ReadUInt16(payload, 68);
            var originalErrorId = unchecked(
                (short)LMC_Frame.ReadUInt16(payload, 70));
            var originalDetailCode = LMC_Frame.ReadUInt32(payload, 72);
            var axisStatus = LMC_Frame.ReadUInt32(payload, 76);
            var axisError = LMC_Frame.ReadInt32(payload, 80);
            var rawDrivePositionBefore = LMC_Frame.ReadInt32(payload, 84);
            var rawDrivePositionAfter = LMC_Frame.ReadInt32(payload, 88);
            var actualApplicationPositionAfter =
                LMC_Frame.ReadInt32(payload, 92);
            var setApplicationPositionAfter =
                LMC_Frame.ReadInt32(payload, 96);
            var actualInternalPositionAfter =
                LMC_Frame.ReadInt32(payload, 100);
            var setInternalPositionAfter = LMC_Frame.ReadInt32(payload, 104);
            var destinationInternalPositionAfter =
                LMC_Frame.ReadInt32(payload, 108);
            var masterInternalPositionAfter =
                LMC_Frame.ReadInt32(payload, 112);
            var nativeCommandState = LMC_Frame.ReadUInt32(payload, 116);
            var evidenceFlags = LMC_Frame.ReadUInt32(payload, 120);
            var startMilliseconds = LMC_Frame.ReadUInt32(payload, 124);
            var completionMilliseconds = LMC_Frame.ReadUInt32(payload, 128);
            var stopState = LMC_Frame.ReadUInt32(payload, 132);
            var runtimePhase = LMC_Frame.ReadUInt32(payload, 136);
            var recordGeneration = LMC_Frame.ReadUInt32(payload, 140);

            if ((LMCHomeSemanticMode)LMC_Frame.ReadUInt16(
                        payload,
                        18)
                    != expectedRecoveryKey.SemanticMode
                || LMC_Frame.ReadUInt32(payload, 20)
                    != expectedRecoveryKey.DiagnosticsBuild
                || LMC_Frame.ReadUInt32(payload, 24)
                    != expectedRecoveryKey.OriginalDiagnosticsBootId
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
                    != expectedRecoveryKey.ExpectedActualPosition
                || LMC_Frame.ReadInt32(payload, 60) != 0
                || LMC_Frame.ReadUInt32(payload, 64)
                    != checked((uint)expectedRecoveryKey.TimeoutMilliseconds)
                || recordGeneration == 0
                || (expectedRecordGeneration != 0
                    && recordGeneration != expectedRecordGeneration))
            {
                throw new InvalidDataException(
                    operation
                    + " record does not exactly match the recovery key and generation.");
            }

            if (!IsValidLmcHomeRuntimeResult(
                    response.IsSuccess,
                    recordState,
                    originalCommandStatus,
                    originalErrorId,
                    originalDetailCode,
                    axisStatus,
                    axisError,
                    rawDrivePositionBefore,
                    rawDrivePositionAfter,
                    actualApplicationPositionAfter,
                    setApplicationPositionAfter,
                    actualInternalPositionAfter,
                    setInternalPositionAfter,
                    destinationInternalPositionAfter,
                    masterInternalPositionAfter,
                    nativeCommandState,
                    evidenceFlags,
                    startMilliseconds,
                    completionMilliseconds,
                    stopState,
                    recordGeneration)
                || (requireTerminal
                    && recordState
                        == LMCHomeOutcomeRecordState.Running))
            {
                throw new InvalidDataException(
                    operation
                    + " contains an invalid or non-terminal runtime result combination.");
            }

            return new LMCParsedHomeOutcome(
                response,
                recordState,
                originalCommandStatus,
                originalErrorId,
                originalDetailCode,
                axisStatus,
                axisError,
                rawDrivePositionBefore,
                rawDrivePositionAfter,
                actualApplicationPositionAfter,
                setApplicationPositionAfter,
                actualInternalPositionAfter,
                setInternalPositionAfter,
                destinationInternalPositionAfter,
                masterInternalPositionAfter,
                nativeCommandState,
                evidenceFlags,
                startMilliseconds,
                completionMilliseconds,
                stopState,
                runtimePhase,
                recordGeneration);
        }

        private static bool IsValidLmcHomeRuntimeResult(
            bool responseSucceeded,
            LMCHomeOutcomeRecordState recordState,
            ushort originalCommandStatus,
            short originalErrorId,
            uint originalDetailCode,
            uint axisStatus,
            int axisError,
            int rawDrivePositionBefore,
            int rawDrivePositionAfter,
            int actualApplicationPositionAfter,
            int setApplicationPositionAfter,
            int actualInternalPositionAfter,
            int setInternalPositionAfter,
            int destinationInternalPositionAfter,
            int masterInternalPositionAfter,
            uint nativeCommandState,
            uint evidenceFlags,
            uint startMilliseconds,
            uint completionMilliseconds,
            uint stopState,
            uint recordGeneration)
        {
            if (recordState == LMCHomeOutcomeRecordState.Running)
            {
                return startMilliseconds != 0
                    && completionMilliseconds == 0
                    && originalCommandStatus == 0
                    && originalErrorId == 0
                    && originalDetailCode == 0;
            }

            if (recordState == LMCHomeOutcomeRecordState.Succeeded)
            {
                return LMCHomeOutcomeSemantics.IsSucceeded(
                    responseSucceeded,
                    recordState,
                    originalCommandStatus,
                    originalErrorId,
                    originalDetailCode,
                    axisStatus,
                    axisError,
                    rawDrivePositionBefore,
                    rawDrivePositionAfter,
                    actualApplicationPositionAfter,
                    setApplicationPositionAfter,
                    actualInternalPositionAfter,
                    setInternalPositionAfter,
                    destinationInternalPositionAfter,
                    masterInternalPositionAfter,
                    nativeCommandState,
                    evidenceFlags,
                    startMilliseconds,
                    completionMilliseconds,
                    stopState,
                    recordGeneration);
            }

            var terminalFailure = recordState
                    == LMCHomeOutcomeRecordState.Failed
                || recordState
                    == LMCHomeOutcomeRecordState.Aborted
                || recordState
                    == LMCHomeOutcomeRecordState.Quarantined;
            return terminalFailure
                && startMilliseconds != 0
                && completionMilliseconds != 0
                && originalCommandStatus == 1
                && (originalErrorId != 0 || originalDetailCode != 0);
        }
    }
}

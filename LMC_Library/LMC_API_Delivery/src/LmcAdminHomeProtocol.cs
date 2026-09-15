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
        internal const int StartLmcHomeV2RequestPayloadLength = 76;
        internal const int LmcHomeOutcomeRequestPayloadLength = 56;
        internal const int LmcHomeV2OutcomeRequestPayloadLength = 80;
        internal const int LmcHomeRetirementRequestPayloadLength = 60;
        internal const int LmcHomeV2RetirementRequestPayloadLength = 84;
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

        internal static void ValidateLmcHome(LMCHomeParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
            if (parameters.TimeoutMilliseconds
                    < LmcHomeMinimumTimeoutMilliseconds
                || parameters.TimeoutMilliseconds > 300000)
            {
                throw new ArgumentOutOfRangeException(
                    "parameters",
                    "Generic LMC_Home timeout must be from 100 through 300000 milliseconds.");
            }
            if (parameters.BufferMode != LMCHomeBufferMode.Buffered)
            {
                throw new ArgumentOutOfRangeException("parameters");
            }

            var direct = parameters.HomingMode == LMCHomeMode.Direct;
            if (direct)
            {
                if (parameters.Velocity != 0
                    || parameters.Acceleration != 0
                    || parameters.DistanceLimit != 0
                    || parameters.TorqueLimit != 0
                    || parameters.Direction != LMCHomeDirection.NotApplicable
                    || parameters.SwitchMode
                        != LMCHomeSwitchMode.NotApplicable)
                {
                    throw new ArgumentException(
                        "Direct LMC_Home requires zero motion fields and NotApplicable direction/switch mode.",
                        "parameters");
                }
                return;
            }

            if (parameters.HomingMode < LMCHomeMode.AbsoluteSwitch
                || parameters.HomingMode > LMCHomeMode.ReferencePulse)
            {
                throw new NotSupportedException(
                    "Block LMC_Home remains disabled because the LASAL MoveReference contract has no torque/block-detection input.");
            }
            if (parameters.Velocity <= 0
                || parameters.Acceleration <= 0
                || parameters.DistanceLimit < 0
                || parameters.TorqueLimit <= 0)
            {
                throw new ArgumentException(
                    "Moving LMC_Home requires positive ReferenceVelocity1/ReferenceVelocity2/ReferenceAcceleration and a non-negative ReferencePositionWindow.",
                    "parameters");
            }
            if (parameters.Direction != LMCHomeDirection.Positive
                && parameters.Direction != LMCHomeDirection.Negative)
            {
                throw new ArgumentException(
                    "The LASAL MoveReference adapter supports Positive or Negative direction.",
                    "parameters");
            }
            if (parameters.HomingMode == LMCHomeMode.AbsoluteSwitch)
            {
                if (parameters.SwitchMode != LMCHomeSwitchMode.On)
                {
                    throw new ArgumentException(
                        "AbsoluteSwitch currently supports the normalized LASAL RefSwitch input with SwitchMode.On.",
                        "parameters");
                }
            }
            else if (parameters.SwitchMode != LMCHomeSwitchMode.NotApplicable)
            {
                throw new ArgumentException(
                    "LimitSwitch and ReferencePulse require NotApplicable switch mode.",
                    "parameters");
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
            if (recoveryKey.HomeContractVersion == 2)
            {
                ValidateLmcHome(recoveryKey.Parameters);
                return StartLmcHomeV2(recoveryKey);
            }
            ValidateLmcHome(recoveryKey.SemanticMode, recoveryKey.TimeoutMilliseconds);

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

        private static byte[] StartLmcHomeV2(LMCHomeRecoveryKey recoveryKey)
        {
            var buffer = CreateCommonRequest(
                StartLmcHomeCommandId,
                recoveryKey.AxisReference,
                StartLmcHomeV2RequestPayloadLength,
                recoveryKey.OriginalRequestId);
            var p = LMC_Frame.HeaderSize;
            WriteLmcHomeCommonIdentity(buffer, p, recoveryKey);
            WriteLmcHomeV2Parameters(buffer, p + 36, recoveryKey.Parameters);
            LMC_Frame.WriteUInt32(buffer, p + 72, LmcHomeExecuteTokenValue);
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
            var payloadLength = recoveryKey.HomeContractVersion == 2
                ? LmcHomeV2OutcomeRequestPayloadLength
                : LmcHomeOutcomeRequestPayloadLength;
            var buffer = CreateCommonRequest(
                ReadLmcHomeOutcomeCommandId,
                recoveryKey.AxisReference,
                payloadLength,
                queryRequestId);
            var payloadOffset = LMC_Frame.HeaderSize;
            WriteLmcHomeIdentity(
                buffer,
                payloadOffset,
                recoveryKey,
                true,
                currentDiagnosticsBootId);
            if (recoveryKey.HomeContractVersion == 2)
            {
                WriteLmcHomeV2Parameters(
                    buffer,
                    LMC_Frame.HeaderSize + 44,
                    recoveryKey.Parameters);
            }
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
            var queryLength = recoveryKey.HomeContractVersion == 2
                ? LmcHomeV2OutcomeRequestPayloadLength
                : LmcHomeOutcomeRequestPayloadLength;
            var retirementLength = recoveryKey.HomeContractVersion == 2
                ? LmcHomeV2RetirementRequestPayloadLength
                : LmcHomeRetirementRequestPayloadLength;
            var buffer = LMC_Frame.CreateRequest(
                RetireLmcHomeOutcomeCommandId,
                recoveryKey.AxisReference,
                checked((ushort)retirementLength));
            Buffer.BlockCopy(
                query,
                LMC_Frame.HeaderSize,
                buffer,
                LMC_Frame.HeaderSize,
                queryLength);
            LMC_Frame.WriteUInt32(
                buffer,
                LMC_Frame.HeaderSize + queryLength,
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

        private static void WriteLmcHomeCommonIdentity(
            byte[] buffer,
            int payloadOffset,
            LMCHomeRecoveryKey recoveryKey)
        {
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 8, recoveryKey.DiagnosticsBuild);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 12, recoveryKey.OriginalDiagnosticsBootId);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 16, recoveryKey.MapRevision);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 20, recoveryKey.ClientIntentId0);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 24, recoveryKey.ClientIntentId1);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 28, recoveryKey.ClientIntentId2);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 32, recoveryKey.ClientIntentId3);
        }

        private static void WriteLmcHomeV2Parameters(
            byte[] buffer,
            int offset,
            LMCHomeParameters parameters)
        {
            LMC_Frame.WriteUInt16(buffer, offset, 2);
            LMC_Frame.WriteUInt16(buffer, offset + 2, (ushort)parameters.HomingMode);
            LMC_Frame.WriteInt32(buffer, offset + 4, parameters.Position);
            LMC_Frame.WriteInt32(buffer, offset + 8, parameters.Velocity);
            LMC_Frame.WriteInt32(buffer, offset + 12, parameters.Acceleration);
            LMC_Frame.WriteInt32(buffer, offset + 16, parameters.DistanceLimit);
            LMC_Frame.WriteInt32(buffer, offset + 20, parameters.TorqueLimit);
            LMC_Frame.WriteUInt16(buffer, offset + 24, (ushort)parameters.BufferMode);
            LMC_Frame.WriteUInt16(buffer, offset + 26, (ushort)parameters.Direction);
            LMC_Frame.WriteUInt16(buffer, offset + 28, (ushort)parameters.SwitchMode);
            LMC_Frame.WriteUInt16(buffer, offset + 30, 0);
            LMC_Frame.WriteUInt32(
                buffer,
                offset + 32,
                checked((uint)parameters.TimeoutMilliseconds));
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
        internal const int LmcHomeV2OutcomeResponsePayloadLength = 164;

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

        internal static LMCParsedHomeStartResponse
            ParseStartLmcHome(
                byte[] raw,
                uint expectedRequestId,
                LMCHomeRecoveryKey recoveryKey)
        {
            if (recoveryKey == null)
            {
                throw new ArgumentNullException("recoveryKey");
            }
            if (recoveryKey.HomeContractVersion == 1)
            {
                return ParseStartLmcHome(
                    raw,
                    expectedRequestId,
                    recoveryKey.SemanticMode);
            }

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
                return new LMCParsedHomeStartResponse(
                    response,
                    LMCHomeSemanticMode.GenericHome,
                    0);
            }

            EnsurePayloadLength(
                transport,
                StartLmcHomeResponsePayloadLength,
                "LMC_Home v2 acceptance");
            var payload = transport.Payload;
            var contractVersion = LMC_Frame.ReadUInt16(payload, 16);
            var homingMode = (LMCHomeMode)LMC_Frame.ReadUInt16(payload, 18);
            var nativeCommandState = LMC_Frame.ReadUInt32(payload, 20);
            if (contractVersion != 2
                || homingMode != recoveryKey.Parameters.HomingMode
                || nativeCommandState != 0)
            {
                throw new InvalidDataException(
                    "LMC_Home v2 ACK did not echo the exact contract version and homing mode.");
            }
            return new LMCParsedHomeStartResponse(
                response,
                LMCHomeSemanticMode.GenericHome,
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
            if (expectedRecoveryKey.HomeContractVersion == 2)
            {
                return ParseLmcHomeV2OutcomeCore(
                    raw,
                    expectedRequestId,
                    expectedRecoveryKey,
                    requireTerminal,
                    expectedRecordGeneration,
                    operation);
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

        private static LMCParsedHomeOutcome ParseLmcHomeV2OutcomeCore(
            byte[] raw,
            uint expectedRequestId,
            LMCHomeRecoveryKey key,
            bool requireTerminal,
            uint expectedRecordGeneration,
            string operation)
        {
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
                        key,
                        expectedRecordGeneration);
                }
                throw new LMCHomeOutcomeQueryException(response, key);
            }

            EnsurePayloadLength(
                transport,
                LmcHomeV2OutcomeResponsePayloadLength,
                operation + " v2 success");
            var p = transport.Payload;
            var state = (LMCHomeOutcomeRecordState)LMC_Frame.ReadUInt16(p, 16);
            var parameters = key.Parameters;
            if (LMC_Frame.ReadUInt16(p, 18) != 2
                || LMC_Frame.ReadUInt32(p, 20) != key.DiagnosticsBuild
                || LMC_Frame.ReadUInt32(p, 24) != key.OriginalDiagnosticsBootId
                || LMC_Frame.ReadUInt32(p, 28) != key.MapRevision
                || LMC_Frame.ReadUInt32(p, 32) != key.OriginalRequestId
                || LMC_Frame.ReadUInt32(p, 36) != key.ClientIntentId0
                || LMC_Frame.ReadUInt32(p, 40) != key.ClientIntentId1
                || LMC_Frame.ReadUInt32(p, 44) != key.ClientIntentId2
                || LMC_Frame.ReadUInt32(p, 48) != key.ClientIntentId3
                || LMC_Frame.ReadUInt16(p, 52) != key.AxisReference
                || (LMCHomeMode)LMC_Frame.ReadUInt16(p, 54)
                    != parameters.HomingMode
                || LMC_Frame.ReadInt32(p, 56) != parameters.Position
                || LMC_Frame.ReadInt32(p, 60) != parameters.Velocity
                || LMC_Frame.ReadInt32(p, 64) != parameters.Acceleration
                || LMC_Frame.ReadInt32(p, 68) != parameters.DistanceLimit
                || LMC_Frame.ReadInt32(p, 72) != parameters.TorqueLimit
                || (LMCHomeBufferMode)LMC_Frame.ReadUInt16(p, 76)
                    != parameters.BufferMode
                || (LMCHomeDirection)LMC_Frame.ReadUInt16(p, 78)
                    != parameters.Direction
                || (LMCHomeSwitchMode)LMC_Frame.ReadUInt16(p, 80)
                    != parameters.SwitchMode
                || LMC_Frame.ReadUInt16(p, 82) != 0
                || LMC_Frame.ReadUInt32(p, 84)
                    != checked((uint)parameters.TimeoutMilliseconds))
            {
                throw new InvalidDataException(
                    operation + " v2 record does not match the recovery key.");
            }

            var originalStatus = LMC_Frame.ReadUInt16(p, 88);
            var originalError = unchecked((short)LMC_Frame.ReadUInt16(p, 90));
            var originalDetail = LMC_Frame.ReadUInt32(p, 92);
            var axisStatus = LMC_Frame.ReadUInt32(p, 96);
            var axisError = LMC_Frame.ReadInt32(p, 100);
            var rawBefore = LMC_Frame.ReadInt32(p, 104);
            var rawAfter = LMC_Frame.ReadInt32(p, 108);
            var actualApp = LMC_Frame.ReadInt32(p, 112);
            var setApp = LMC_Frame.ReadInt32(p, 116);
            var actualInternal = LMC_Frame.ReadInt32(p, 120);
            var setInternal = LMC_Frame.ReadInt32(p, 124);
            var destinationInternal = LMC_Frame.ReadInt32(p, 128);
            var masterInternal = LMC_Frame.ReadInt32(p, 132);
            var nativeState = LMC_Frame.ReadUInt32(p, 136);
            var evidence = LMC_Frame.ReadUInt32(p, 140);
            var startMs = LMC_Frame.ReadUInt32(p, 144);
            var completionMs = LMC_Frame.ReadUInt32(p, 148);
            var stopState = LMC_Frame.ReadUInt32(p, 152);
            var runtimePhase = LMC_Frame.ReadUInt32(p, 156);
            var generation = LMC_Frame.ReadUInt32(p, 160);
            if (generation == 0
                || (expectedRecordGeneration != 0
                    && generation != expectedRecordGeneration)
                || (requireTerminal
                    && state == LMCHomeOutcomeRecordState.Running)
                || !IsValidLmcHomeV2RuntimeResult(
                    key,
                    response.IsSuccess,
                    state,
                    originalStatus,
                    originalError,
                    originalDetail,
                    axisStatus,
                    axisError,
                    rawBefore,
                    rawAfter,
                    actualApp,
                    setApp,
                    actualInternal,
                    setInternal,
                    destinationInternal,
                    masterInternal,
                    nativeState,
                    evidence,
                    startMs,
                    completionMs,
                    stopState,
                    generation))
            {
                throw new InvalidDataException(
                    operation + " contains an invalid v2 runtime result.");
            }
            return new LMCParsedHomeOutcome(
                response,
                state,
                originalStatus,
                originalError,
                originalDetail,
                axisStatus,
                axisError,
                rawBefore,
                rawAfter,
                actualApp,
                setApp,
                actualInternal,
                setInternal,
                destinationInternal,
                masterInternal,
                nativeState,
                evidence,
                startMs,
                completionMs,
                stopState,
                runtimePhase,
                generation);
        }

        private static bool IsValidLmcHomeV2RuntimeResult(
            LMCHomeRecoveryKey key,
            bool responseSucceeded,
            LMCHomeOutcomeRecordState state,
            ushort originalStatus,
            short originalError,
            uint originalDetail,
            uint axisStatus,
            int axisError,
            int rawBefore,
            int rawAfter,
            int actualApp,
            int setApp,
            int actualInternal,
            int setInternal,
            int destinationInternal,
            int masterInternal,
            uint nativeState,
            uint evidence,
            uint startMs,
            uint completionMs,
            uint stopState,
            uint generation)
        {
            if (state == LMCHomeOutcomeRecordState.Running)
            {
                return startMs != 0
                    && completionMs == 0
                    && originalStatus == 0
                    && originalError == 0
                    && originalDetail == 0;
            }
            if (state == LMCHomeOutcomeRecordState.Succeeded)
            {
                return LMCHomeOutcomeSemantics.IsSucceeded(
                    key,
                    responseSucceeded,
                    state,
                    originalStatus,
                    originalError,
                    originalDetail,
                    axisStatus,
                    axisError,
                    rawBefore,
                    rawAfter,
                    actualApp,
                    setApp,
                    actualInternal,
                    setInternal,
                    destinationInternal,
                    masterInternal,
                    nativeState,
                    evidence,
                    startMs,
                    completionMs,
                    stopState,
                    generation);
            }
            return (state == LMCHomeOutcomeRecordState.Failed
                    || state == LMCHomeOutcomeRecordState.Aborted
                    || state == LMCHomeOutcomeRecordState.Quarantined)
                && startMs != 0
                && completionMs != 0
                && originalStatus == 1
                && (originalError != 0 || originalDetail != 0);
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

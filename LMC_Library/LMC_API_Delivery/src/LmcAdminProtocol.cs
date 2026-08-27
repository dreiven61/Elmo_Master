using System;
using System.IO;

namespace LasalMotionControlLib
{
    internal static partial class LMC_AdminFrame
    {
        internal const ushort SchemaVersion = 1;
        internal const int CommonRequestPayloadLength = 8;
        internal const int ReadParameterRequestPayloadLength = 12;
        internal const int GroupMoveLinearRelativeRequestPayloadLength = 104;

        internal static byte[] GetCapabilities(uint requestId)
        {
            return CreateCommonRequest(
                LMC_CommandId.GetAdminCapabilities,
                0,
                CommonRequestPayloadLength,
                requestId);
        }

        internal static byte[] ReadAxisParameter(
            uint requestId,
            ushort axisReference,
            LMCAxisParameterKey key)
        {
            ValidateAxisReference(axisReference);
            ValidateAxisParameterKey(key);

            var buffer = CreateCommonRequest(
                LMC_CommandId.ReadAxisParameter,
                axisReference,
                ReadParameterRequestPayloadLength,
                requestId);
            LMC_Frame.WriteUInt16(
                buffer,
                LMC_Frame.HeaderSize + CommonRequestPayloadLength,
                (ushort)key);
            LMC_Frame.WriteUInt16(
                buffer,
                LMC_Frame.HeaderSize + CommonRequestPayloadLength + 2,
                0);
            return buffer;
        }

        internal static byte[] ReadGroupParameters(
            uint requestId,
            ushort groupReference,
            LMCGroupParameterSelection selection)
        {
            ValidateGroupReference(groupReference);
            ValidateGroupSelection(selection);

            var buffer = CreateCommonRequest(
                LMC_CommandId.ReadGroupParameters,
                groupReference,
                ReadParameterRequestPayloadLength,
                requestId);
            LMC_Frame.WriteUInt32(
                buffer,
                LMC_Frame.HeaderSize + CommonRequestPayloadLength,
                (uint)selection);
            return buffer;
        }

        internal static byte[] GroupMoveLinearRelative(
            uint requestId,
            ushort groupReference,
            int[] distance,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMCGroupMotionOptions options)
        {
            ValidateGroupReference(groupReference);
            ValidateGroupLinearRelative(
                distance,
                velocity,
                acceleration,
                deceleration,
                jerk,
                options);

            var buffer = CreateCommonRequest(
                LMC_CommandId.GroupMoveLinearRelative,
                groupReference,
                GroupMoveLinearRelativeRequestPayloadLength,
                requestId);
            var motionOffset = LMC_Frame.HeaderSize
                + CommonRequestPayloadLength;
            LMC_Frame.WriteGroupLinearVector(
                buffer,
                motionOffset,
                distance);
            LMC_Frame.WriteInt32(buffer, motionOffset + 64, velocity);
            LMC_Frame.WriteInt32(buffer, motionOffset + 68, acceleration);
            LMC_Frame.WriteInt32(buffer, motionOffset + 72, deceleration);
            LMC_Frame.WriteInt32(buffer, motionOffset + 76, jerk);
            LMC_Frame.WriteInt32(
                buffer,
                motionOffset + 80,
                (int)options.CoordinateSystem);
            LMC_Frame.WriteInt32(
                buffer,
                motionOffset + 84,
                (int)options.TransitionMode);
            LMC_Frame.WriteInt32(
                buffer,
                motionOffset + 88,
                (int)options.BufferMode);
            LMC_Frame.WriteInt32(
                buffer,
                motionOffset + 92,
                options.Execute ? 1 : 0);
            return buffer;
        }

        internal static void ValidateGroupLinearRelative(
            int[] distance,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMCGroupMotionOptions options)
        {
            LMC_Frame.ValidateGroupLinearMotion(
                distance,
                "distance",
                velocity,
                acceleration,
                deceleration,
                jerk,
                options);
        }

        internal static void ValidateAxisReference(ushort axisReference)
        {
            if (axisReference < 1 || axisReference > 4)
            {
                throw new ArgumentOutOfRangeException(
                    "axisReference",
                    "LASAL-local admin commands support physical AxisReference 1 through 4 only.");
            }
        }

        internal static void ValidateAxisParameterKey(
            LMCAxisParameterKey key)
        {
            if (key < LMCAxisParameterKey.SoftwareMinPosition
                || key > LMCAxisParameterKey.ReferencePosition)
            {
                throw new ArgumentOutOfRangeException(
                    "key",
                    "The axis parameter key is outside the schema version 1 allowlist.");
            }
        }

        internal static void ValidateGroupReference(ushort groupReference)
        {
            if (groupReference != 0x0100)
            {
                throw new ArgumentOutOfRangeException(
                    "groupReference",
                    "LASAL-local admin group commands support the main group reference 0x0100 only.");
            }
        }

        internal static void ValidateGroupSelection(
            LMCGroupParameterSelection selection)
        {
            if (selection == LMCGroupParameterSelection.None
                || (selection & ~LMCGroupParameterSelection.All) != 0)
            {
                throw new ArgumentOutOfRangeException(
                    "selection",
                    "Group parameter selection must contain only schema version 1 read keys.");
            }
        }

        private static byte[] CreateCommonRequest(
            ushort command,
            ushort reference,
            int payloadLength,
            uint requestId)
        {
            if (requestId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "requestId",
                    "Admin request identifiers must be non-zero.");
            }

            var buffer = LMC_Frame.CreateRequest(
                command,
                reference,
                checked((ushort)payloadLength));
            LMC_Frame.WriteUInt16(
                buffer,
                LMC_Frame.HeaderSize,
                SchemaVersion);
            LMC_Frame.WriteUInt16(
                buffer,
                LMC_Frame.HeaderSize + 2,
                0);
            LMC_Frame.WriteUInt32(
                buffer,
                LMC_Frame.HeaderSize + 4,
                requestId);
            return buffer;
        }
    }

    internal static partial class LMC_AdminParser
    {
        internal const int CommonResponsePayloadLength = 16;
        internal const int CapabilitiesPayloadLength = 40;
        internal const int AxisParameterPayloadLength = 28;
        internal const int GroupParametersPayloadLength = 32;
        internal const short AdminErrorId = -31000;

        internal static LMCAdminCapabilities ParseCapabilities(
            byte[] raw,
            uint expectedRequestId,
            long connectionSessionGeneration)
        {
            return ParseCapabilities(
                raw,
                expectedRequestId,
                connectionSessionGeneration,
                null);
        }

        internal static LMCAdminCapabilities ParseCapabilities(
            byte[] raw,
            uint expectedRequestId,
            long connectionSessionGeneration,
            LMCConnection connectionOwner)
        {
            var transport = ParseTransport(
                raw,
                "GetAdminCapabilities",
                true);
            var response = ParseCommonResponse(transport, expectedRequestId);
            ThrowIfCommandFailed("GetAdminCapabilities", response);
            EnsurePayloadLength(
                transport,
                CapabilitiesPayloadLength,
                "GetAdminCapabilities");

            var payload = transport.Payload;
            var features = (LMCAdminFeature)LMC_Frame.ReadUInt32(payload, 16);
            var axisMask = LMC_Frame.ReadUInt32(payload, 20);
            var groupSelection =
                (LMCGroupParameterSelection)LMC_Frame.ReadUInt32(payload, 24);
            var physicalAxisCount = LMC_Frame.ReadUInt16(payload, 28);
            var maxAxisParameterCount = LMC_Frame.ReadUInt16(payload, 30);
            var groupReference = LMC_Frame.ReadUInt16(payload, 32);
            var maxGroupParameterCount = LMC_Frame.ReadUInt16(payload, 34);
            var errorCatalogVersion = LMC_Frame.ReadUInt16(payload, 36);
            var setOperationModeSupportedMask =
                LMC_Frame.ReadUInt16(payload, 38);

            const LMCAdminFeature knownFeatures =
                LMCAdminFeature.AxisParameterRead
                | LMCAdminFeature.GroupParameterRead
                | LMCAdminFeature.GroupLinearRelative
                | LMCAdminFeature.AxisSetPosition
                | LMCAdminFeature.AxisHome
                | LMCAdminFeature.AxisSetPositionOutcomeRead
                | LMCAdminFeature.AxisDs402Home
                | LMCAdminFeature.AxisSetPositionOutcomeRetirement
                | LMCAdminFeature.AxisSetOperationModeStart
                | LMCAdminFeature.AxisSetOperationModeOutcomeRead
                | LMCAdminFeature.AxisSetOperationModeOutcomeRetire
                | LMCAdminFeature.AxisDs402HomeEx;
            const uint knownAxisMask = 0x0000003Fu;
            const ushort knownSetOperationModeSupportedMask = 0x018A;

            if ((features & ~knownFeatures) != 0
                || (axisMask & ~knownAxisMask) != 0
                || (groupSelection & ~LMCGroupParameterSelection.All) != 0
                || (setOperationModeSupportedMask
                    & ~knownSetOperationModeSupportedMask) != 0)
            {
                throw new InvalidDataException(
                    "GetAdminCapabilities contains schema version 1 reserved bits.");
            }

            if (((features & LMCAdminFeature.AxisParameterRead) != 0
                    && (axisMask == 0
                        || physicalAxisCount == 0
                        || maxAxisParameterCount != 1))
                || ((features & LMCAdminFeature.GroupParameterRead) != 0
                    && (groupSelection == LMCGroupParameterSelection.None
                        || groupReference != 0x0100
                        || maxGroupParameterCount == 0))
                || ((features & LMCAdminFeature.GroupLinearRelative) != 0
                    && groupReference != 0x0100)
                || ((features & LMCAdminFeature.AxisSetPosition) != 0
                    && ((features
                            & LMCAdminFeature.AxisSetPositionOutcomeRead) == 0
                        || (features
                            & LMCAdminFeature
                                .AxisSetPositionOutcomeRetirement) == 0
                        || physicalAxisCount == 0
                        || physicalAxisCount > 4
                        || errorCatalogVersion < 2))
                || ((features & LMCAdminFeature.AxisHome) != 0
                    && (physicalAxisCount == 0
                        || physicalAxisCount > 4
                        || errorCatalogVersion < 5))
                || ((features
                        & LMCAdminFeature.AxisSetPositionOutcomeRead) != 0
                    && (physicalAxisCount == 0
                        || physicalAxisCount > 4
                        || errorCatalogVersion < 2))
                || ((features
                        & LMCAdminFeature
                            .AxisSetPositionOutcomeRetirement) != 0
                    && ((features
                            & LMCAdminFeature.AxisSetPositionOutcomeRead) == 0
                        || physicalAxisCount == 0
                        || physicalAxisCount > 4
                        || errorCatalogVersion < 2))
                || ((features & LMCAdminFeature.AxisDs402Home) != 0
                    && (physicalAxisCount == 0
                        || physicalAxisCount > 4
                        || errorCatalogVersion < 4))
                || (((features
                            & (LMCAdminFeature.AxisSetOperationModeStart
                                | LMCAdminFeature
                                    .AxisSetOperationModeOutcomeRead
                                | LMCAdminFeature
                                    .AxisSetOperationModeOutcomeRetire)) != 0)
                    && ((features
                            & (LMCAdminFeature.AxisSetOperationModeStart
                                | LMCAdminFeature
                                    .AxisSetOperationModeOutcomeRead
                                | LMCAdminFeature
                                    .AxisSetOperationModeOutcomeRetire))
                        != (LMCAdminFeature.AxisSetOperationModeStart
                            | LMCAdminFeature
                                .AxisSetOperationModeOutcomeRead
                            | LMCAdminFeature
                                .AxisSetOperationModeOutcomeRetire)
                        || physicalAxisCount == 0
                        || physicalAxisCount > 4
                        || errorCatalogVersion < 6))
                || ((features & LMCAdminFeature.AxisDs402HomeEx) != 0
                    && (physicalAxisCount == 0
                        || physicalAxisCount > 4
                        || errorCatalogVersion < 7))
                || errorCatalogVersion == 0)
            {
                throw new InvalidDataException(
                    "GetAdminCapabilities feature bits and limits are inconsistent.");
            }

            var setOperationModeFeatures = features
                & (LMCAdminFeature.AxisSetOperationModeStart
                    | LMCAdminFeature.AxisSetOperationModeOutcomeRead
                    | LMCAdminFeature.AxisSetOperationModeOutcomeRetire);
            var fullSetOperationModeTriad =
                LMCAdminFeature.AxisSetOperationModeStart
                | LMCAdminFeature.AxisSetOperationModeOutcomeRead
                | LMCAdminFeature.AxisSetOperationModeOutcomeRetire;
            if ((setOperationModeFeatures == LMCAdminFeature.None
                    && setOperationModeSupportedMask != 0)
                || (setOperationModeFeatures == fullSetOperationModeTriad
                    && setOperationModeSupportedMask == 0))
            {
                throw new InvalidDataException(
                    "GetAdminCapabilities SetOperationMode triad and supported-mode mask are inconsistent.");
            }

            return new LMCAdminCapabilities(
                response,
                connectionOwner,
                connectionSessionGeneration,
                features,
                axisMask,
                groupSelection,
                physicalAxisCount,
                maxAxisParameterCount,
                groupReference,
                maxGroupParameterCount,
                errorCatalogVersion,
                setOperationModeSupportedMask);
        }

        internal static LMCAxisParameterResult ParseAxisParameter(
            byte[] raw,
            uint expectedRequestId,
            ushort expectedAxisReference,
            LMCAxisParameterKey expectedKey)
        {
            var transport = ParseTransport(raw, "ReadAxisParameter", false);
            var response = ParseCommonResponse(transport, expectedRequestId);
            ThrowIfCommandFailed("ReadAxisParameter", response);
            EnsurePayloadLength(
                transport,
                AxisParameterPayloadLength,
                "ReadAxisParameter");

            var payload = transport.Payload;
            var key = (LMCAxisParameterKey)LMC_Frame.ReadUInt16(payload, 16);
            var valueType =
                (LMCAdminValueType)LMC_Frame.ReadUInt16(payload, 18);
            var unit = (LMCAdminUnit)LMC_Frame.ReadUInt16(payload, 20);
            var reserved = LMC_Frame.ReadUInt16(payload, 22);

            if (key != expectedKey
                || valueType != LMCAdminValueType.Int32
                || unit != ExpectedAxisUnit(key)
                || reserved != 0)
            {
                throw new InvalidDataException(
                    "ReadAxisParameter response metadata does not match the request schema.");
            }

            return new LMCAxisParameterResult(
                response,
                expectedAxisReference,
                key,
                valueType,
                unit,
                LMC_Frame.ReadInt32(payload, 24));
        }

        internal static LMCGroupParametersResult ParseGroupParameters(
            byte[] raw,
            uint expectedRequestId,
            ushort expectedGroupReference,
            LMCGroupParameterSelection expectedSelection)
        {
            var transport = ParseTransport(raw, "ReadGroupParameters", false);
            var response = ParseCommonResponse(transport, expectedRequestId);
            ThrowIfCommandFailed("ReadGroupParameters", response);
            EnsurePayloadLength(
                transport,
                GroupParametersPayloadLength,
                "ReadGroupParameters");

            var payload = transport.Payload;
            var presentSelection =
                (LMCGroupParameterSelection)LMC_Frame.ReadUInt32(payload, 16);
            if (presentSelection != expectedSelection
                || (presentSelection & ~LMCGroupParameterSelection.All) != 0)
            {
                throw new InvalidDataException(
                    "ReadGroupParameters response selection does not match the request.");
            }

            return new LMCGroupParametersResult(
                response,
                expectedGroupReference,
                presentSelection,
                LMC_Frame.ReadInt32(payload, 20),
                LMC_Frame.ReadInt32(payload, 24),
                LMC_Frame.ReadInt32(payload, 28));
        }

        internal static LMCAdminResponse ParseGroupMoveLinearRelative(
            byte[] raw,
            uint expectedRequestId)
        {
            var transport = ParseTransport(
                raw,
                "GroupMoveLinearRelative",
                false);
            var response = ParseCommonResponse(
                transport,
                expectedRequestId,
                true);
            EnsurePayloadLength(
                transport,
                CommonResponsePayloadLength,
                "GroupMoveLinearRelative");
            ThrowIfCommandFailed("GroupMoveLinearRelative", response);
            return response;
        }

        private static LMCAdminUnit ExpectedAxisUnit(LMCAxisParameterKey key)
        {
            switch (key)
            {
                case LMCAxisParameterKey.MaxVelocity:
                    return LMCAdminUnit.ApplicationUnitsPerSecond;
                case LMCAxisParameterKey.MaxAcceleration:
                    return LMCAdminUnit.ApplicationUnitsPerSecondSquared;
                default:
                    return LMCAdminUnit.ApplicationUnits;
            }
        }

        private static LMC_Response ParseTransport(
            byte[] raw,
            string operation,
            bool translateUnknownCommand)
        {
            var transport = LMCConnection.Parse(raw);
            if (!transport.IsFrameValid || transport.HeaderReserved != 0)
            {
                throw new InvalidDataException(
                    operation + " returned an invalid RPC frame.");
            }

            if (translateUnknownCommand && transport.PayloadLength == 4)
            {
                var acknowledgement = LMCConnection.ParseAcknowledgement(raw);
                if (acknowledgement.HasCommandResult
                    && acknowledgement.ErrorId == -4)
                {
                    throw new LMCAdminNotSupportedException(
                        "The connected RPC server does not support LASAL admin command 0x7D00.",
                        acknowledgement);
                }
            }

            if (transport.HeaderStatus != 0)
            {
                throw new InvalidOperationException(
                    operation
                    + " was rejected by the RPC dispatcher. HeaderStatus="
                    + transport.HeaderStatus
                    + ".");
            }

            return transport;
        }

        private static LMCAdminResponse ParseCommonResponse(
            LMC_Response transport,
            uint expectedRequestId)
        {
            return ParseCommonResponse(
                transport,
                expectedRequestId,
                false,
                false);
        }

        private static LMCAdminResponse ParseCommonResponse(
            LMC_Response transport,
            uint expectedRequestId,
            bool allowMotionFailureDetails)
        {
            return ParseCommonResponse(
                transport,
                expectedRequestId,
                allowMotionFailureDetails,
                false);
        }

        private static LMCAdminResponse ParseCommonResponse(
            LMC_Response transport,
            uint expectedRequestId,
            bool allowMotionFailureDetails,
            bool allowSetPositionFailureDetails)
        {
            return ParseCommonResponse(
                transport,
                expectedRequestId,
                allowMotionFailureDetails,
                allowSetPositionFailureDetails,
                false);
        }

        private static LMCAdminResponse ParseCommonResponse(
            LMC_Response transport,
            uint expectedRequestId,
            bool allowMotionFailureDetails,
            bool allowSetPositionFailureDetails,
            bool allowSetPositionOutcomeFailureDetails)
        {
            return ParseCommonResponse(
                transport,
                expectedRequestId,
                allowMotionFailureDetails,
                allowSetPositionFailureDetails,
                allowSetPositionOutcomeFailureDetails,
                false);
        }

        private static LMCAdminResponse ParseCommonResponse(
            LMC_Response transport,
            uint expectedRequestId,
            bool allowMotionFailureDetails,
            bool allowSetPositionFailureDetails,
            bool allowSetPositionOutcomeFailureDetails,
            bool allowDs402HomeOutcomeFailureDetails)
        {
            return ParseCommonResponse(
                transport,
                expectedRequestId,
                allowMotionFailureDetails,
                allowSetPositionFailureDetails,
                allowSetPositionOutcomeFailureDetails,
                allowDs402HomeOutcomeFailureDetails,
                false);
        }

        private static LMCAdminResponse ParseCommonResponse(
            LMC_Response transport,
            uint expectedRequestId,
            bool allowMotionFailureDetails,
            bool allowSetPositionFailureDetails,
            bool allowSetPositionOutcomeFailureDetails,
            bool allowDs402HomeOutcomeFailureDetails,
            bool allowDs402HomeStartFailureDetails)
        {
            return ParseCommonResponse(
                transport,
                expectedRequestId,
                allowMotionFailureDetails,
                allowSetPositionFailureDetails,
                allowSetPositionOutcomeFailureDetails,
                allowDs402HomeOutcomeFailureDetails,
                allowDs402HomeStartFailureDetails,
                false,
                false,
                false);
        }

        private static LMCAdminResponse ParseCommonResponse(
            LMC_Response transport,
            uint expectedRequestId,
            bool allowMotionFailureDetails,
            bool allowSetPositionFailureDetails,
            bool allowSetPositionOutcomeFailureDetails,
            bool allowDs402HomeOutcomeFailureDetails,
            bool allowDs402HomeStartFailureDetails,
            bool allowLmcHomeOutcomeFailureDetails,
            bool allowLmcHomeStartFailureDetails)
        {
            return ParseCommonResponse(
                transport,
                expectedRequestId,
                allowMotionFailureDetails,
                allowSetPositionFailureDetails,
                allowSetPositionOutcomeFailureDetails,
                allowDs402HomeOutcomeFailureDetails,
                allowDs402HomeStartFailureDetails,
                allowLmcHomeOutcomeFailureDetails,
                allowLmcHomeStartFailureDetails,
                false);
        }

        internal static LMCAdminResponse ParseSetOperationModeCommonResponse(
            LMC_Response transport,
            uint expectedRequestId)
        {
            return ParseCommonResponse(
                transport,
                expectedRequestId,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                true);
        }

        private static LMCAdminResponse ParseCommonResponse(
            LMC_Response transport,
            uint expectedRequestId,
            bool allowMotionFailureDetails,
            bool allowSetPositionFailureDetails,
            bool allowSetPositionOutcomeFailureDetails,
            bool allowDs402HomeOutcomeFailureDetails,
            bool allowDs402HomeStartFailureDetails,
            bool allowLmcHomeOutcomeFailureDetails,
            bool allowLmcHomeStartFailureDetails,
            bool allowSetOperationModeFailureDetails)
        {
            if (transport.Payload.Length < CommonResponsePayloadLength)
            {
                throw new InvalidDataException(
                    "Admin response does not contain the 16-byte common envelope.");
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
                || requestId != expectedRequestId)
            {
                throw new InvalidDataException(
                    "Admin common response schema, flags, or RequestId is invalid.");
            }

            if (commandStatus > 1
                || (commandStatus == 0
                    && (errorId != 0 || detailCode != 0))
                || (commandStatus == 1
                    && !IsValidCommandFailure(
                        errorId,
                        detailCode,
                        allowMotionFailureDetails,
                        allowSetPositionFailureDetails,
                        allowSetPositionOutcomeFailureDetails,
                        allowDs402HomeOutcomeFailureDetails,
                        allowDs402HomeStartFailureDetails,
                        allowLmcHomeOutcomeFailureDetails,
                        allowLmcHomeStartFailureDetails,
                        allowSetOperationModeFailureDetails)))
            {
                throw new InvalidDataException(
                    "Admin response contains an invalid status/error/detail combination.");
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

        private static bool IsValidCommandFailure(
            short errorId,
            uint detailCode,
            bool allowMotionFailureDetails,
            bool allowSetPositionFailureDetails,
            bool allowSetPositionOutcomeFailureDetails,
            bool allowDs402HomeOutcomeFailureDetails,
            bool allowDs402HomeStartFailureDetails,
            bool allowLmcHomeOutcomeFailureDetails,
            bool allowLmcHomeStartFailureDetails,
            bool allowSetOperationModeFailureDetails)
        {
            if (detailCode >= (uint)LMCAdminDetailCode.UnsupportedSchema
                && detailCode <= (uint)LMCAdminDetailCode.InvalidSelection)
            {
                return errorId == AdminErrorId;
            }

            if (allowMotionFailureDetails
                && (detailCode
                        == (uint)LMCAdminDetailCode
                            .InvalidMotionParameters
                    || detailCode
                        == (uint)LMCAdminDetailCode.InvalidState))
            {
                return errorId == AdminErrorId;
            }

            if (allowSetPositionFailureDetails
                && (detailCode == (uint)LMCAdminDetailCode.NonZeroVelocity
                || detailCode == (uint)LMCAdminDetailCode.ActiveAxisError
                || detailCode == (uint)LMCAdminDetailCode
                    .InvalidSetPositionSafetyConfiguration
                || detailCode == (uint)LMCAdminDetailCode
                    .CoordinatePreconditionFailed))
            {
                return errorId == AdminErrorId;
            }

            if (allowSetPositionFailureDetails
                && ((detailCode
                            >= (uint)LMCAdminDetailCode
                                .DiagnosticsBuildMismatch
                        && detailCode
                            <= (uint)LMCAdminDetailCode.MapRevisionMismatch)
                    || detailCode
                        == (uint)LMCAdminDetailCode
                            .SetPositionOutcomeIndeterminate
                    || detailCode
                        == (uint)LMCAdminDetailCode
                            .SetPositionOutcomeStoreCorrupt
                    || (detailCode
                            >= (uint)LMCAdminDetailCode
                                .SetPositionOutcomeSlotOccupied
                        && detailCode
                            <= (uint)LMCAdminDetailCode
                                .SetPositionOutcomeStorageUnavailable)))
            {
                return errorId == AdminErrorId;
            }

            if (allowSetPositionOutcomeFailureDetails
                && ((detailCode
                            >= (uint)LMCAdminDetailCode
                                .DiagnosticsBuildMismatch
                        && detailCode
                            <= (uint)LMCAdminDetailCode
                                .SetPositionOutcomeKeyMismatch)
                    || detailCode
                        == (uint)LMCAdminDetailCode
                            .SetPositionOutcomeStorageUnavailable))
            {
                return errorId == AdminErrorId;
            }

            if (allowDs402HomeOutcomeFailureDetails
                && ((detailCode
                            >= (uint)LMCAdminDetailCode
                                .DiagnosticsBuildMismatch
                        && detailCode
                            <= (uint)LMCAdminDetailCode.MapRevisionMismatch)
                    || (detailCode
                            >= (uint)LMCAdminDetailCode
                                .Ds402HomeOutcomeNotFound
                        && detailCode
                            <= (uint)LMCAdminDetailCode
                                .Ds402HomeOutcomeStorageUnavailable)))
            {
                return errorId == AdminErrorId;
            }

            if (allowDs402HomeStartFailureDetails
                && (detailCode
                        == (uint)LMCAdminDetailCode
                            .Ds402HomeOutcomeSlotOccupied
                    || detailCode
                        == (uint)LMCAdminDetailCode.AxisOwnershipConflict
                    || detailCode
                        == (uint)LMCAdminDetailCode
                            .AxisOwnershipQuarantined))
            {
                return errorId == AdminErrorId;
            }

            if (allowLmcHomeOutcomeFailureDetails
                && ((detailCode
                            >= (uint)LMCAdminDetailCode
                                .DiagnosticsBuildMismatch
                        && detailCode
                            <= (uint)LMCAdminDetailCode.MapRevisionMismatch)
                    || (detailCode
                            >= (uint)LMCAdminDetailCode
                                .LmcHomeOutcomeNotFound
                        && detailCode
                            <= (uint)LMCAdminDetailCode
                                .LmcHomeOutcomeStorageUnavailable)))
            {
                return errorId == AdminErrorId;
            }

            if (allowLmcHomeStartFailureDetails
                && (detailCode == (uint)LMCAdminDetailCode.InvalidState
                    || detailCode
                        == (uint)LMCAdminDetailCode.ActiveAxisError
                    || detailCode
                        == (uint)LMCAdminDetailCode
                            .CoordinatePreconditionFailed
                    || (detailCode
                            >= (uint)LMCAdminDetailCode
                                .DiagnosticsBuildMismatch
                        && detailCode
                            <= (uint)LMCAdminDetailCode.MapRevisionMismatch)
                    || detailCode
                        == (uint)LMCAdminDetailCode
                            .LmcHomeOutcomeSlotOccupied
                    || detailCode
                        == (uint)LMCAdminDetailCode.AxisOwnershipConflict
                    || detailCode
                        == (uint)LMCAdminDetailCode
                            .AxisOwnershipQuarantined))
            {
                return errorId == AdminErrorId;
            }

            if ((allowMotionFailureDetails
                    || allowSetPositionFailureDetails)
                && detailCode
                    == (uint)LMCAdminDetailCode.NativeCommandRejected)
            {
                return errorId > 0 || errorId == -6;
            }

            if (allowSetOperationModeFailureDetails
                && ((detailCode
                            >= (uint)LMCAdminDetailCode
                                .DiagnosticsBuildMismatch
                        && detailCode
                            <= (uint)LMCAdminDetailCode.MapRevisionMismatch)
                    || detailCode
                        == (uint)LMCAdminDetailCode.AxisOwnershipConflict
                    || detailCode
                        == (uint)LMCAdminDetailCode.AxisOwnershipQuarantined
                    || (detailCode
                            >= (uint)LMCAdminDetailCode
                                .SetOperationModeUnsupportedMode
                        && detailCode
                            <= (uint)LMCAdminDetailCode
                                .SetOperationModeOutcomeSlotOccupied)))
            {
                return errorId == AdminErrorId;
            }

            return false;
        }

        private static void ThrowIfCommandFailed(
            string operation,
            LMCAdminResponse response)
        {
            if (!response.IsSuccess)
            {
                throw new LMCAdminCommandException(
                    operation
                    + " failed. ErrorId="
                    + response.ErrorId
                    + ", DetailCode="
                    + response.DetailCodeValue
                    + ".",
                    response);
            }
        }

        private static void EnsurePayloadLength(
            LMC_Response transport,
            int expectedLength,
            string operation)
        {
            if (transport.PayloadLength != expectedLength
                || transport.Payload.Length != expectedLength)
            {
                throw new InvalidDataException(
                    operation
                    + " response must contain exactly "
                    + expectedLength
                    + " payload bytes.");
            }
        }
    }
}

using System;
using System.IO;

namespace LasalMotionControlLib
{
    internal static class LMC_AdminFrame
    {
        internal const ushort SchemaVersion = 1;
        internal const int CommonRequestPayloadLength = 8;
        internal const int ReadParameterRequestPayloadLength = 12;

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

        internal static void ValidateAxisReference(ushort axisReference)
        {
            if (axisReference < 1 || axisReference > 4)
            {
                throw new ArgumentOutOfRangeException(
                    "axisReference",
                    "Phase 1 admin reads support physical AxisReference 1 through 4 only.");
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
                    "Phase 1 admin reads support the main group reference 0x0100 only.");
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

    internal static class LMC_AdminParser
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
            var reserved = LMC_Frame.ReadUInt16(payload, 38);

            const LMCAdminFeature knownFeatures =
                LMCAdminFeature.AxisParameterRead
                | LMCAdminFeature.GroupParameterRead;
            const uint knownAxisMask = 0x0000003Fu;

            if ((features & ~knownFeatures) != 0
                || (axisMask & ~knownAxisMask) != 0
                || (groupSelection & ~LMCGroupParameterSelection.All) != 0
                || reserved != 0)
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
                || errorCatalogVersion == 0)
            {
                throw new InvalidDataException(
                    "GetAdminCapabilities feature bits and limits are inconsistent.");
            }

            return new LMCAdminCapabilities(
                response,
                connectionSessionGeneration,
                features,
                axisMask,
                groupSelection,
                physicalAxisCount,
                maxAxisParameterCount,
                groupReference,
                maxGroupParameterCount,
                errorCatalogVersion);
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
                    && (errorId != AdminErrorId
                        || detailCode == 0
                        || detailCode
                            > (uint)LMCAdminDetailCode.InvalidSelection)))
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

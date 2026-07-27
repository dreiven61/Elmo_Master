using System;
using System.IO;

namespace LasalMotionControlLib
{
    internal static partial class LMC_DiagnosticsFrame
    {
        internal const ushort SchemaVersion = 1;
        internal const int CommonRequestPayloadLength = 8;

        internal static byte[] GetDiagnosticsCapabilities(uint requestId)
        {
            if (requestId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "requestId",
                    "Diagnostics request identifiers must be non-zero.");
            }

            var buffer = LMC_Frame.CreateRequest(
                LMC_CommandId.GetDiagnosticsCapabilities,
                0,
                CommonRequestPayloadLength);

            LMC_Frame.WriteUInt16(buffer, LMC_Frame.HeaderSize, SchemaVersion);
            LMC_Frame.WriteUInt16(buffer, LMC_Frame.HeaderSize + 2, 0);
            LMC_Frame.WriteUInt32(buffer, LMC_Frame.HeaderSize + 4, requestId);

            return buffer;
        }
    }

    internal static partial class LMC_DiagnosticsParser
    {
        internal const int CommonResponsePayloadLength = 16;
        internal const int CapabilitiesPayloadLength = 68;
        internal const short DiagnosticsErrorId = -32000;

        private const ushort KnownResponseFlagsMask = 0x0003;
        private const uint MaximumDefinedDetailCode =
            (uint)LMCDiagnosticsDetailCode.RTOwnerUnavailable;
        private const uint StatefulCapabilityMask =
            (uint)(LMCDiagnosticCapability.BulkSnapshot
                | LMCDiagnosticCapability.RecorderSingleBank
                | LMCDiagnosticCapability.RecorderTrigger
                | LMCDiagnosticCapability.RecorderDoubleBank
                | LMCDiagnosticCapability.PIWrite
                | LMCDiagnosticCapability.SDORead
                | LMCDiagnosticCapability.SDOWrite
                | LMCDiagnosticCapability.ExtendedSdoResultChunk
                | LMCDiagnosticCapability.SDOReadGeneralInline
                | LMCDiagnosticCapability.DigitalIOWrite);

        internal static LMCDiagnosticCapabilities ParseCapabilities(
            byte[] raw,
            uint expectedRequestId,
            long connectionSessionGeneration)
        {
            var transportResponse = LMCConnection.Parse(raw);

            if (!transportResponse.IsFrameValid)
            {
                throw new InvalidDataException(
                    "GetDiagnosticsCapabilities returned an invalid RPC frame.");
            }

            if (transportResponse.HeaderReserved != 0)
            {
                throw new InvalidDataException(
                    "GetDiagnosticsCapabilities outer response reserved field must be zero.");
            }

            if (transportResponse.PayloadLength == 4)
            {
                var acknowledgement = LMCConnection.ParseAcknowledgement(raw);
                if (acknowledgement.HasCommandResult
                    && !acknowledgement.IsSuccess)
                {
                    if (acknowledgement.ErrorId == -4)
                    {
                        throw new LMCDiagnosticsNotSupportedException(
                            "The connected RPC server does not support LASAL diagnostics command 0x7E00.",
                            acknowledgement);
                    }

                    throw new InvalidOperationException(
                        "GetDiagnosticsCapabilities was rejected before diagnostics dispatch. HeaderStatus="
                        + acknowledgement.HeaderStatus
                        + ", CommandStatus="
                        + acknowledgement.CommandStatus
                        + ", ErrorId="
                        + acknowledgement.ErrorId
                        + ".");
                }
            }

            if (transportResponse.HeaderStatus != 0)
            {
                throw new InvalidOperationException(
                    "GetDiagnosticsCapabilities was rejected by the RPC dispatcher. HeaderStatus="
                    + transportResponse.HeaderStatus
                    + ".");
            }

            if (transportResponse.PayloadLength != CapabilitiesPayloadLength
                || transportResponse.Payload.Length != CapabilitiesPayloadLength)
            {
                throw new InvalidDataException(
                    "GetDiagnosticsCapabilities response must contain exactly 68 payload bytes.");
            }

            var response = ParseCommonResponse(
                transportResponse,
                expectedRequestId);

            if (response.ResponseFlags != LMCDiagnosticsResponseFlags.None)
            {
                throw new InvalidDataException(
                    "GetDiagnosticsCapabilities does not define response flags in schema version 1.");
            }

            if (!response.IsSuccess)
            {
                throw new LMCDiagnosticsCommandException(
                    "GetDiagnosticsCapabilities failed. ErrorId="
                    + response.ErrorId
                    + ", DetailCode="
                    + response.DetailCode
                    + ".",
                    response);
            }

            var payload = transportResponse.Payload;
            var capabilityBits = LMC_Frame.ReadUInt32(payload, 20);
            var mapRevision = LMC_Frame.ReadUInt32(payload, 24);
            var catalogEntryCount = LMC_Frame.ReadUInt16(payload, 28);
            var catalogEntryStride = LMC_Frame.ReadUInt16(payload, 50);
            var signalValueEntryStride = LMC_Frame.ReadUInt16(payload, 52);
            var diagnosticsBootId = LMC_Frame.ReadUInt32(payload, 64);
            var maxRequestPayloadBytes = LMC_Frame.ReadUInt16(payload, 44);
            var maxResponsePayloadBytes = LMC_Frame.ReadUInt16(payload, 46);
            var maxChunkDataBytes = LMC_Frame.ReadUInt16(payload, 48);
            var maxSdoDataBytes = LMC_Frame.ReadUInt16(payload, 60);

            var signalCatalogEnabled =
                (capabilityBits & (uint)LMCDiagnosticCapability.SignalCatalog) != 0;
            var piReadEnabled =
                (capabilityBits & (uint)LMCDiagnosticCapability.PIRead) != 0;
            var recorderSingleBankEnabled =
                (capabilityBits
                    & (uint)LMCDiagnosticCapability.RecorderSingleBank) != 0;
            var recorderExtensionEnabled =
                (capabilityBits
                    & (uint)(LMCDiagnosticCapability.RecorderTrigger
                        | LMCDiagnosticCapability.RecorderDoubleBank)) != 0;
            var piWriteEnabled =
                (capabilityBits & (uint)LMCDiagnosticCapability.PIWrite) != 0;
            var sdoEnabled =
                (capabilityBits
                    & (uint)(LMCDiagnosticCapability.SDORead
                        | LMCDiagnosticCapability.SDOWrite)) != 0;
            var extendedSdoResultEnabled =
                (capabilityBits
                    & (uint)LMCDiagnosticCapability.ExtendedSdoResultChunk) != 0;
            var generalInlineSdoReadEnabled =
                (capabilityBits
                    & (uint)LMCDiagnosticCapability.SDOReadGeneralInline) != 0;
            var topologyEnabled =
                (capabilityBits
                    & (uint)LMCDiagnosticCapability.EtherCATTopology) != 0;
            var topologyDependentEnabled =
                (capabilityBits
                    & (uint)(LMCDiagnosticCapability.EtherCATNodeHealth
                        | LMCDiagnosticCapability.DigitalIORead
                        | LMCDiagnosticCapability.DigitalIOWrite)) != 0;
            var nodeHealthEnabled =
                (capabilityBits
                    & (uint)LMCDiagnosticCapability.EtherCATNodeHealth) != 0;
            var digitalIOReadEnabled =
                (capabilityBits
                    & (uint)LMCDiagnosticCapability.DigitalIORead) != 0;
            var digitalIOWriteEnabled =
                (capabilityBits
                    & (uint)LMCDiagnosticCapability.DigitalIOWrite) != 0;

            if (piReadEnabled && !signalCatalogEnabled)
            {
                throw new InvalidDataException(
                    "PIRead capability requires SignalCatalog capability.");
            }

            if (recorderExtensionEnabled && !recorderSingleBankEnabled)
            {
                throw new InvalidDataException(
                    "Recorder trigger and double-bank capabilities require RecorderSingleBank capability.");
            }

            if (piWriteEnabled && !signalCatalogEnabled)
            {
                throw new InvalidDataException(
                    "PIWrite capability requires SignalCatalog capability.");
            }

            if (topologyDependentEnabled && !topologyEnabled)
            {
                throw new InvalidDataException(
                    "EtherCATNodeHealth, DigitalIORead, and DigitalIOWrite capabilities require EtherCATTopology capability.");
            }

            if (digitalIOWriteEnabled
                && (!nodeHealthEnabled || !digitalIOReadEnabled))
            {
                throw new InvalidDataException(
                    "DigitalIOWrite requires EtherCATNodeHealth and DigitalIORead capabilities.");
            }

            if (extendedSdoResultEnabled
                && ((capabilityBits
                        & (uint)LMCDiagnosticCapability.SDORead) == 0
                    || maxSdoDataBytes
                        <= LMC_DiagnosticsFrame.MaxD5InlineSdoDataBytes))
            {
                throw new InvalidDataException(
                    "ExtendedSdoResultChunk requires SDORead and MaxSdoDataBytes greater than 12.");
            }

            if (generalInlineSdoReadEnabled
                && ((capabilityBits
                        & (uint)LMCDiagnosticCapability.SDORead) == 0
                    || maxSdoDataBytes != 4))
            {
                throw new InvalidDataException(
                    "SDOReadGeneralInline requires SDORead and MaxSdoDataBytes equal to 4.");
            }

            if (sdoEnabled
                && (maxSdoDataBytes == 0
                    || (!extendedSdoResultEnabled
                        && maxSdoDataBytes
                            > LMC_DiagnosticsFrame.MaxD5InlineSdoDataBytes)))
            {
                throw new InvalidDataException(
                    "SDO capability and MaxSdoDataBytes are inconsistent.");
            }

            if (signalCatalogEnabled
                && (mapRevision == 0
                    || catalogEntryCount == 0
                    || catalogEntryStride != 80
                    || signalValueEntryStride != 16))
            {
                throw new InvalidDataException(
                    "SignalCatalog capability requires valid map revision, count, and entry strides.");
            }

            if ((capabilityBits & StatefulCapabilityMask) != 0
                && diagnosticsBootId == 0)
            {
                throw new InvalidDataException(
                    "A stateful diagnostics capability requires a non-zero DiagnosticsBootId.");
            }

            if (maxRequestPayloadBytes < LMC_DiagnosticsFrame.CommonRequestPayloadLength
                || maxResponsePayloadBytes < CapabilitiesPayloadLength
                || (maxChunkDataBytes & 3) != 0
                || maxChunkDataBytes
                    > LMC_DiagnosticsFrame.AbsoluteMaxRecorderChunkDataBytes
                || maxChunkDataBytes
                    > maxResponsePayloadBytes
                        - RecorderChunkResponseHeaderPayloadLength)
            {
                throw new InvalidDataException(
                    "Diagnostics capability payload limits are internally inconsistent for schema version 1.");
            }

            return new LMCDiagnosticCapabilities(
                response,
                connectionSessionGeneration,
                LMC_Frame.ReadUInt32(payload, 16),
                capabilityBits,
                mapRevision,
                catalogEntryCount,
                LMC_Frame.ReadUInt16(payload, 30),
                LMC_Frame.ReadUInt16(payload, 32),
                LMC_Frame.ReadUInt16(payload, 34),
                LMC_Frame.ReadUInt32(payload, 36),
                LMC_Frame.ReadUInt32(payload, 40),
                maxRequestPayloadBytes,
                maxResponsePayloadBytes,
                maxChunkDataBytes,
                catalogEntryStride,
                signalValueEntryStride,
                LMC_Frame.ReadUInt32(payload, 56),
                maxSdoDataBytes,
                diagnosticsBootId);
        }

        internal static LMCDiagnosticsResponse ParseCommonResponse(
            LMC_Response transportResponse,
            uint expectedRequestId)
        {
            if (transportResponse == null)
            {
                throw new ArgumentNullException("transportResponse");
            }

            if (!transportResponse.IsFrameValid
                || transportResponse.HeaderStatus != 0
                || transportResponse.Payload.Length < CommonResponsePayloadLength)
            {
                throw new InvalidDataException(
                    "Diagnostics response does not contain a valid 16-byte common envelope.");
            }

            var payload = transportResponse.Payload;
            var schemaVersion = LMC_Frame.ReadUInt16(payload, 0);
            var responseFlagsValue = LMC_Frame.ReadUInt16(payload, 2);
            var commandStatus = LMC_Frame.ReadUInt16(payload, 4);
            var errorId = unchecked((short)LMC_Frame.ReadUInt16(payload, 6));
            var requestId = LMC_Frame.ReadUInt32(payload, 8);
            var detailCode = LMC_Frame.ReadUInt32(payload, 12);

            if (schemaVersion != LMC_DiagnosticsFrame.SchemaVersion)
            {
                throw new InvalidDataException(
                    "Unsupported diagnostics schema version "
                    + schemaVersion
                    + ". Expected version "
                    + LMC_DiagnosticsFrame.SchemaVersion
                    + ".");
            }

            if ((responseFlagsValue & ~KnownResponseFlagsMask) != 0)
            {
                throw new InvalidDataException(
                    "Diagnostics response contains flags not defined by schema version 1.");
            }

            if (requestId != expectedRequestId)
            {
                throw new InvalidDataException(
                    "Diagnostics response RequestId does not match the request. Expected "
                    + expectedRequestId
                    + ", actual "
                    + requestId
                    + ".");
            }

            if (commandStatus > 1
                || (commandStatus == 0 && errorId != 0)
                || (commandStatus == 0 && detailCode != 0)
                || (commandStatus == 1 && errorId != DiagnosticsErrorId)
                || (commandStatus == 1
                    && (detailCode == 0
                        || detailCode > MaximumDefinedDetailCode)))
            {
                throw new InvalidDataException(
                    "Diagnostics response contains an invalid CommandStatus/ErrorId pair.");
            }

            return new LMCDiagnosticsResponse(
                transportResponse,
                schemaVersion,
                (LMCDiagnosticsResponseFlags)responseFlagsValue,
                commandStatus,
                errorId,
                requestId,
                detailCode);
        }
    }
}

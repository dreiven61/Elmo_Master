using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LasalMotionControlLib
{
    internal static partial class LMC_DiagnosticsFrame
    {
        internal const int TopologyChunkRequestPayloadLength = 16;
        internal const int NodeHealthRequestPayloadLength = 16;
        internal const int DigitalIOReadRequestPayloadLength = 20;
        internal const int DigitalOutputWriteRequestPayloadLength = 40;
        internal const ushort MaxTopologyEntriesPerChunk = 16;

        internal static byte[] GetEtherCATTopologyInfo(uint requestId)
        {
            return CreateCommonRequest(
                LMC_CommandId.GetEtherCATTopologyInfo,
                requestId,
                CommonRequestPayloadLength);
        }

        internal static byte[] GetEtherCATTopologyChunk(
            uint requestId,
            uint expectedTopologyRevision,
            ushort startIndex,
            ushort maxEntries)
        {
            if (expectedTopologyRevision == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "expectedTopologyRevision");
            }

            if (maxEntries == 0 || maxEntries > MaxTopologyEntriesPerChunk)
            {
                throw new ArgumentOutOfRangeException(
                    "maxEntries",
                    "Topology chunks must request between 1 and 16 entries.");
            }

            var buffer = CreateCommonRequest(
                LMC_CommandId.GetEtherCATTopologyChunk,
                requestId,
                TopologyChunkRequestPayloadLength);
            var payloadOffset = LMC_Frame.HeaderSize;

            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 8,
                expectedTopologyRevision);
            LMC_Frame.WriteUInt16(buffer, payloadOffset + 12, startIndex);
            LMC_Frame.WriteUInt16(buffer, payloadOffset + 14, maxEntries);

            return buffer;
        }

        internal static byte[] ReadEtherCATNodeHealth(
            uint requestId,
            uint topologyRevision,
            uint nodeId)
        {
            if (topologyRevision == 0)
            {
                throw new ArgumentOutOfRangeException("topologyRevision");
            }

            if (nodeId == 0)
            {
                throw new ArgumentOutOfRangeException("nodeId");
            }

            var buffer = CreateCommonRequest(
                LMC_CommandId.ReadEtherCATNodeHealth,
                requestId,
                NodeHealthRequestPayloadLength);
            var payloadOffset = LMC_Frame.HeaderSize;

            LMC_Frame.WriteUInt32(buffer, payloadOffset + 8, topologyRevision);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 12, nodeId);

            return buffer;
        }

        internal static byte[] ReadDigitalIO(
            uint requestId,
            LMCDigitalIOReadRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var buffer = CreateCommonRequest(
                LMC_CommandId.ReadDigitalIO,
                requestId,
                DigitalIOReadRequestPayloadLength);
            var payloadOffset = LMC_Frame.HeaderSize;

            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 8,
                request.TopologyRevision);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 12,
                request.IOReference);
            buffer[payloadOffset + 16] = (byte)request.ExpectedDirection;
            buffer[payloadOffset + 17] = request.ExpectedBitWidth;

            return buffer;
        }

        internal static byte[] SubmitDigitalOutputWrite(
            uint requestId,
            LMCDigitalOutputWriteRequest request,
            uint diagnosticsBootId)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (diagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException("diagnosticsBootId");
            }

            var buffer = CreateCommonRequest(
                LMC_CommandId.SubmitDigitalOutputWrite,
                requestId,
                DigitalOutputWriteRequestPayloadLength);
            var payloadOffset = LMC_Frame.HeaderSize;

            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 8,
                request.TopologyRevision);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 12,
                request.IOReference);
            LMC_Frame.WriteUInt64(buffer, payloadOffset + 16, request.Value);
            LMC_Frame.WriteUInt64(buffer, payloadOffset + 24, request.Mask);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 32,
                request.ExpectedOutputRevision);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 36,
                diagnosticsBootId);

            return buffer;
        }
    }

    internal static partial class LMC_DiagnosticsParser
    {
        internal const int TopologyInfoPayloadLength = 44;
        internal const int TopologyChunkHeaderPayloadLength = 28;
        internal const int TopologyEntryStride = 96;
        internal const int TopologyNameBytes = 48;
        internal const int NodeHealthPayloadLength = 72;
        internal const int DigitalIOPayloadLength = 56;

        private const uint RequiredTopologyFlags = 0x0000000Fu;
        private const ushort KnownTopologyNodeFlagsMask = 0x00FF;
        private const ushort KnownNodeHealthFlagsMask = 0x003F;
        private const ushort KnownDigitalIOStatusFlagsMask = 0x01FF;
        private const ushort KnownDigitalIOFaultCauseFlagsMask = 0x00FE;

        internal static LMCEtherCATTopologyInfo ParseEtherCATTopologyInfo(
            byte[] raw,
            uint expectedRequestId)
        {
            var response = ParseSuccessfulCommand(
                raw,
                expectedRequestId,
                "GetEtherCATTopologyInfo");
            RequireExactPayloadLength(
                response,
                TopologyInfoPayloadLength,
                "GetEtherCATTopologyInfo");
            RequireNoResponseFlags(response, "GetEtherCATTopologyInfo");

            var payload = response.TransportResponse.Payload;
            var topologyRevision = LMC_Frame.ReadUInt32(payload, 16);
            var totalNodeCount = LMC_Frame.ReadUInt16(payload, 20);
            var entryStride = LMC_Frame.ReadUInt16(payload, 22);
            var maxEntriesPerChunk = LMC_Frame.ReadUInt16(payload, 24);
            var configuredSlaveCount = LMC_Frame.ReadUInt16(payload, 26);
            var slotModuleCount = LMC_Frame.ReadUInt16(payload, 28);
            var physicalAxisCount = LMC_Frame.ReadUInt16(payload, 30);
            var topologyFlags = LMC_Frame.ReadUInt32(payload, 32);
            var crcKind = LMC_Frame.ReadUInt32(payload, 36);
            var reserved = LMC_Frame.ReadUInt32(payload, 40);

            if (topologyRevision == 0
                || totalNodeCount == 0
                || entryStride != TopologyEntryStride
                || maxEntriesPerChunk == 0
                || maxEntriesPerChunk
                    > LMC_DiagnosticsFrame.MaxTopologyEntriesPerChunk
                || configuredSlaveCount + slotModuleCount != totalNodeCount
                || physicalAxisCount > configuredSlaveCount
                || topologyFlags != RequiredTopologyFlags
                || crcKind != (uint)LMCDiagnosticsCrcKind.Crc32IsoHdlc
                || reserved != 0)
            {
                throw new InvalidDataException(
                    "GetEtherCATTopologyInfo returned an inconsistent topology contract.");
            }

            return new LMCEtherCATTopologyInfo(
                response,
                topologyRevision,
                totalNodeCount,
                entryStride,
                maxEntriesPerChunk,
                configuredSlaveCount,
                slotModuleCount,
                physicalAxisCount,
                topologyFlags,
                crcKind);
        }

        internal static LMCEtherCATTopologyChunk ParseEtherCATTopologyChunk(
            byte[] raw,
            uint expectedRequestId,
            uint expectedTopologyRevision,
            ushort expectedStartIndex,
            ushort requestedMaxEntries)
        {
            if (expectedTopologyRevision == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "expectedTopologyRevision");
            }

            if (requestedMaxEntries == 0
                || requestedMaxEntries
                    > LMC_DiagnosticsFrame.MaxTopologyEntriesPerChunk)
            {
                throw new ArgumentOutOfRangeException("requestedMaxEntries");
            }

            var response = ParseSuccessfulCommand(
                raw,
                expectedRequestId,
                "GetEtherCATTopologyChunk");
            var payload = response.TransportResponse.Payload;

            if (payload.Length < TopologyChunkHeaderPayloadLength)
            {
                throw new InvalidDataException(
                    "GetEtherCATTopologyChunk response is shorter than its header.");
            }

            var topologyRevision = LMC_Frame.ReadUInt32(payload, 16);
            var startIndex = LMC_Frame.ReadUInt16(payload, 20);
            var returnedCount = LMC_Frame.ReadUInt16(payload, 22);
            var totalNodeCount = LMC_Frame.ReadUInt16(payload, 24);
            var entryStride = LMC_Frame.ReadUInt16(payload, 26);

            if (topologyRevision != expectedTopologyRevision
                || startIndex != expectedStartIndex
                || totalNodeCount == 0
                || startIndex >= totalNodeCount
                || entryStride != TopologyEntryStride)
            {
                throw new InvalidDataException(
                    "GetEtherCATTopologyChunk returned a stale or invalid identity.");
            }

            var expectedReturnedCount = checked((ushort)Math.Min(
                requestedMaxEntries,
                totalNodeCount - startIndex));
            if (returnedCount != expectedReturnedCount)
            {
                throw new InvalidDataException(
                    "GetEtherCATTopologyChunk returned an unexpected entry count.");
            }

            var expectedPayloadLength = checked(
                TopologyChunkHeaderPayloadLength
                    + returnedCount * TopologyEntryStride);
            RequireExactPayloadLength(
                response,
                expectedPayloadLength,
                "GetEtherCATTopologyChunk");

            var isLastChunk = startIndex + returnedCount == totalNodeCount;
            var expectedFlags = isLastChunk
                ? LMCDiagnosticsResponseFlags.LastChunk
                : LMCDiagnosticsResponseFlags.None;
            if (response.ResponseFlags != expectedFlags)
            {
                throw new InvalidDataException(
                    "GetEtherCATTopologyChunk response flags do not match its range.");
            }

            var entries = new List<LMCEtherCATTopologyEntry>(returnedCount);
            for (var index = 0; index < returnedCount; index++)
            {
                entries.Add(ParseTopologyEntry(
                    payload,
                    TopologyChunkHeaderPayloadLength
                        + index * TopologyEntryStride,
                    checked((ushort)(startIndex + index))));
            }

            return new LMCEtherCATTopologyChunk(
                response,
                topologyRevision,
                startIndex,
                totalNodeCount,
                entryStride,
                entries);
        }

        internal static LMCEtherCATNodeHealth ParseEtherCATNodeHealth(
            byte[] raw,
            uint expectedRequestId,
            uint expectedTopologyRevision,
            uint expectedNodeId)
        {
            var response = ParseSuccessfulCommand(
                raw,
                expectedRequestId,
                "ReadEtherCATNodeHealth");
            RequireExactPayloadLength(
                response,
                NodeHealthPayloadLength,
                "ReadEtherCATNodeHealth");
            RequireNoResponseFlags(response, "ReadEtherCATNodeHealth");

            var payload = response.TransportResponse.Payload;
            var topologyRevision = LMC_Frame.ReadUInt32(payload, 16);
            var nodeId = LMC_Frame.ReadUInt32(payload, 20);
            var capturePhase = (LMCCapturePhase)LMC_Frame.ReadUInt16(
                payload,
                24);
            var healthFlagsValue = LMC_Frame.ReadUInt16(payload, 26);
            var healthFlags = (LMCEtherCATNodeHealthFlags)healthFlagsValue;
            var snapshotSequence = LMC_Frame.ReadUInt32(payload, 40);
            var onlineValue = payload[44];
            var etherCATState = payload[45];
            var ds402StatusWord = LMC_Frame.ReadUInt32(payload, 56);
            var axisError = LMC_Frame.ReadUInt32(payload, 60);
            var isConfigured = (healthFlags
                & LMCEtherCATNodeHealthFlags.Configured) != 0;
            var isDetected = (healthFlags
                & LMCEtherCATNodeHealthFlags.Detected) != 0;
            var isIdentityMatched = (healthFlags
                & LMCEtherCATNodeHealthFlags.IdentityMatched) != 0;
            var isDataValid = (healthFlags
                & LMCEtherCATNodeHealthFlags.DataValid) != 0;
            var isDataDefaulted = (healthFlags
                & LMCEtherCATNodeHealthFlags.DataDefaulted) != 0;
            var hasDs402Data = (healthFlags
                & LMCEtherCATNodeHealthFlags.Ds402DataPresent) != 0;

            if (expectedTopologyRevision == 0
                || expectedNodeId == 0
                || topologyRevision != expectedTopologyRevision
                || nodeId != expectedNodeId
                || capturePhase != LMCCapturePhase.InputMapped
                || (healthFlagsValue & ~KnownNodeHealthFlagsMask) != 0
                || snapshotSequence == 0
                || (snapshotSequence & 1) != 0
                || onlineValue > 1
                || !IsEtherCATState(etherCATState)
                || !isConfigured
                || (onlineValue != 0) != isDetected
                || (isDetected != (etherCATState != 0))
                || (isIdentityMatched && !isDetected)
                || isDataValid == isDataDefaulted
                || (isDataValid && (!isDetected || !isIdentityMatched))
                || (hasDs402Data && !isDataValid)
                || (!hasDs402Data
                    && (ds402StatusWord != 0 || axisError != 0)))
            {
                throw new InvalidDataException(
                    "ReadEtherCATNodeHealth returned an invalid coherent snapshot.");
            }

            return new LMCEtherCATNodeHealth(
                response,
                topologyRevision,
                nodeId,
                capturePhase,
                healthFlags,
                LMC_Frame.ReadUInt32(payload, 28),
                LMC_Frame.ReadUInt64(payload, 32),
                snapshotSequence,
                onlineValue != 0,
                etherCATState,
                LMC_Frame.ReadUInt16(payload, 46),
                LMC_Frame.ReadUInt32(payload, 48),
                LMC_Frame.ReadUInt32(payload, 52),
                ds402StatusWord,
                axisError,
                LMC_Frame.ReadUInt32(payload, 64),
                LMC_Frame.ReadUInt32(payload, 68));
        }

        internal static LMCDigitalIOValue ParseDigitalIO(
            byte[] raw,
            uint expectedRequestId,
            LMCDigitalIOReadRequest expectedRequest)
        {
            if (expectedRequest == null)
            {
                throw new ArgumentNullException("expectedRequest");
            }

            var response = ParseSuccessfulCommand(
                raw,
                expectedRequestId,
                "ReadDigitalIO");
            RequireExactPayloadLength(
                response,
                DigitalIOPayloadLength,
                "ReadDigitalIO");
            RequireNoResponseFlags(response, "ReadDigitalIO");

            var payload = response.TransportResponse.Payload;
            var topologyRevision = LMC_Frame.ReadUInt32(payload, 16);
            var ioReference = LMC_Frame.ReadUInt32(payload, 20);
            var nodeId = LMC_Frame.ReadUInt32(payload, 24);
            var direction = (LMCDigitalIODirection)payload[28];
            var bitWidth = payload[29];
            var statusFlagsValue = LMC_Frame.ReadUInt16(payload, 30);
            var statusFlags = (LMCDigitalIOStatusFlags)statusFlagsValue;
            var value = LMC_Frame.ReadUInt64(payload, 32);
            var validMask = LMC_Frame.ReadUInt64(payload, 40);
            var outputRevision = LMC_Frame.ReadUInt32(payload, 52);
            var widthMask = GetBitWidthMask(bitWidth);
            var isValid = (statusFlags & LMCDigitalIOStatusFlags.Valid) != 0;
            var isDataDefaulted = (statusFlags
                & LMCDigitalIOStatusFlags.DataDefaulted) != 0;
            var hasFaultCause = (statusFlagsValue
                & KnownDigitalIOFaultCauseFlagsMask) != 0;

            if (topologyRevision != expectedRequest.TopologyRevision
                || ioReference != expectedRequest.IOReference
                || nodeId == 0
                || direction != expectedRequest.ExpectedDirection
                || bitWidth != expectedRequest.ExpectedBitWidth
                || bitWidth == 0
                || bitWidth > 64
                || (statusFlagsValue & ~KnownDigitalIOStatusFlagsMask) != 0
                || (value & ~widthMask) != 0
                || (validMask & ~widthMask) != 0
                || (isValid
                    ? statusFlags != LMCDigitalIOStatusFlags.Valid
                        || validMask != widthMask
                    : value != 0
                        || validMask != 0
                        || !isDataDefaulted
                        || !hasFaultCause)
                || (direction == LMCDigitalIODirection.Input
                    ? outputRevision != 0
                    : outputRevision == 0))
            {
                throw new InvalidDataException(
                    "ReadDigitalIO returned an invalid or mismatched value.");
            }

            return new LMCDigitalIOValue(
                response,
                topologyRevision,
                ioReference,
                nodeId,
                direction,
                bitWidth,
                statusFlags,
                value,
                validMask,
                LMC_Frame.ReadUInt32(payload, 48),
                outputRevision);
        }

        internal static uint ComputeEtherCATTopologyRevision(
            IReadOnlyList<LMCEtherCATTopologyEntry> entries)
        {
            if (entries == null)
            {
                throw new ArgumentNullException("entries");
            }

            var canonical = new byte[checked(entries.Count * TopologyEntryStride)];
            for (var index = 0; index < entries.Count; index++)
            {
                var entry = entries[index]
                    ?? throw new InvalidDataException(
                        "EtherCAT topology contains a null entry.");
                var offset = index * TopologyEntryStride;
                var nameBytes = Encoding.ASCII.GetBytes(entry.Name);

                if (entry.TopologyIndex != index
                    || nameBytes.Length == 0
                    || nameBytes.Length >= TopologyNameBytes)
                {
                    throw new InvalidDataException(
                        "EtherCAT topology cannot be serialized canonically.");
                }

                LMC_Frame.WriteUInt32(canonical, offset, entry.NodeId);
                LMC_Frame.WriteUInt32(canonical, offset + 4, entry.ParentNodeId);
                LMC_Frame.WriteUInt16(canonical, offset + 8, entry.TopologyIndex);
                LMC_Frame.WriteUInt16(canonical, offset + 10, entry.MasterSlaveIndex);
                canonical[offset + 12] = (byte)entry.NodeKind;
                LMC_Frame.WriteUInt16(canonical, offset + 14, (ushort)entry.NodeFlags);
                LMC_Frame.WriteUInt16(canonical, offset + 16, entry.SdoSlaveReference);
                LMC_Frame.WriteUInt16(canonical, offset + 18, entry.PhysicalAxisReference);
                LMC_Frame.WriteUInt16(canonical, offset + 20, entry.SlotIndex);
                LMC_Frame.WriteUInt32(canonical, offset + 24, entry.VendorId);
                LMC_Frame.WriteUInt32(canonical, offset + 28, entry.ProductCode);
                LMC_Frame.WriteUInt32(canonical, offset + 32, entry.RevisionNumber);
                LMC_Frame.WriteUInt32(canonical, offset + 36, entry.SerialNumber);
                LMC_Frame.WriteUInt16(canonical, offset + 40, entry.InputBytes);
                LMC_Frame.WriteUInt16(canonical, offset + 42, entry.OutputBytes);
                Buffer.BlockCopy(
                    nameBytes,
                    0,
                    canonical,
                    offset + 44,
                    nameBytes.Length);
                LMC_Frame.WriteUInt32(canonical, offset + 92, entry.IOReference);
            }

            var crc = 0xFFFFFFFFu;
            foreach (var octet in canonical)
            {
                crc ^= octet;
                for (var bit = 0; bit < 8; bit++)
                {
                    crc = (crc & 1u) != 0
                        ? (crc >> 1) ^ 0xEDB88320u
                        : crc >> 1;
                }
            }

            var result = crc ^ 0xFFFFFFFFu;
            return result == 0 ? 0xFFFFFFFFu : result;
        }

        private static LMCEtherCATTopologyEntry ParseTopologyEntry(
            byte[] payload,
            int offset,
            ushort expectedTopologyIndex)
        {
            var nodeId = LMC_Frame.ReadUInt32(payload, offset);
            var parentNodeId = LMC_Frame.ReadUInt32(payload, offset + 4);
            var topologyIndex = LMC_Frame.ReadUInt16(payload, offset + 8);
            var masterSlaveIndex = LMC_Frame.ReadUInt16(payload, offset + 10);
            var nodeKind = (LMCEtherCATTopologyNodeKind)payload[offset + 12];
            var nodeFlagsValue = LMC_Frame.ReadUInt16(payload, offset + 14);
            var nodeFlags = (LMCEtherCATTopologyNodeFlags)nodeFlagsValue;
            var sdoSlaveReference = LMC_Frame.ReadUInt16(payload, offset + 16);
            var physicalAxisReference = LMC_Frame.ReadUInt16(payload, offset + 18);
            var slotIndex = LMC_Frame.ReadUInt16(payload, offset + 20);
            var inputBytes = LMC_Frame.ReadUInt16(payload, offset + 40);
            var outputBytes = LMC_Frame.ReadUInt16(payload, offset + 42);
            var ioReference = LMC_Frame.ReadUInt32(payload, offset + 92);
            var hasMasterIndex = masterSlaveIndex != ushort.MaxValue;
            var supportsSdo = sdoSlaveReference != 0;
            var isPhysicalAxis = physicalAxisReference != 0;
            var name = ReadFixedTopologyName(payload, offset + 44);

            if (nodeId == 0
                || topologyIndex != expectedTopologyIndex
                || (nodeKind != LMCEtherCATTopologyNodeKind.EtherCATSlave
                    && nodeKind != LMCEtherCATTopologyNodeKind.SlotModule)
                || payload[offset + 13] != 0
                || LMC_Frame.ReadUInt16(payload, offset + 22) != 0
                || (nodeFlagsValue & ~KnownTopologyNodeFlagsMask) != 0
                || HasFlag(nodeFlags, LMCEtherCATTopologyNodeFlags.HasMasterSlaveIndex)
                    != hasMasterIndex
                || HasFlag(nodeFlags, LMCEtherCATTopologyNodeFlags.SupportsSdo)
                    != supportsSdo
                || HasFlag(nodeFlags, LMCEtherCATTopologyNodeFlags.PhysicalAxis)
                    != isPhysicalAxis
                || HasFlag(nodeFlags, LMCEtherCATTopologyNodeFlags.HasInputs)
                    != (inputBytes != 0)
                || HasFlag(nodeFlags, LMCEtherCATTopologyNodeFlags.HasOutputs)
                    != (outputBytes != 0)
                || HasFlag(nodeFlags, LMCEtherCATTopologyNodeFlags.HasDigitalIO)
                    != (ioReference != 0)
                || (ioReference != 0
                    && ((inputBytes == 0 && outputBytes == 0)
                        || inputBytes > sizeof(ulong)
                        || outputBytes > sizeof(ulong)))
                || (HasFlag(nodeFlags, LMCEtherCATTopologyNodeFlags.Ds402Drive)
                    && !isPhysicalAxis)
                || (isPhysicalAxis
                    && nodeKind
                        != LMCEtherCATTopologyNodeKind.EtherCATSlave)
                || LMC_Frame.ReadUInt32(payload, offset + 24) == 0
                || LMC_Frame.ReadUInt32(payload, offset + 28) == 0
                || (nodeKind == LMCEtherCATTopologyNodeKind.EtherCATSlave
                    && (parentNodeId != 0
                        || slotIndex != ushort.MaxValue
                        || !hasMasterIndex))
                || (nodeKind == LMCEtherCATTopologyNodeKind.SlotModule
                    && (parentNodeId == 0
                        || slotIndex == ushort.MaxValue
                        || hasMasterIndex)))
            {
                throw new InvalidDataException(
                    "EtherCAT topology entry is not canonical or self-consistent.");
            }

            return new LMCEtherCATTopologyEntry(
                nodeId,
                parentNodeId,
                topologyIndex,
                masterSlaveIndex,
                nodeKind,
                nodeFlags,
                sdoSlaveReference,
                physicalAxisReference,
                slotIndex,
                LMC_Frame.ReadUInt32(payload, offset + 24),
                LMC_Frame.ReadUInt32(payload, offset + 28),
                LMC_Frame.ReadUInt32(payload, offset + 32),
                LMC_Frame.ReadUInt32(payload, offset + 36),
                inputBytes,
                outputBytes,
                name,
                ioReference);
        }

        private static string ReadFixedTopologyName(byte[] payload, int offset)
        {
            var length = 0;
            var terminated = false;

            for (var index = 0; index < TopologyNameBytes; index++)
            {
                var value = payload[offset + index];
                if (value > 0x7F)
                {
                    throw new InvalidDataException(
                        "EtherCAT topology name contains non-ASCII data.");
                }

                if (terminated)
                {
                    if (value != 0)
                    {
                        throw new InvalidDataException(
                            "EtherCAT topology name is not canonically NUL padded.");
                    }
                }
                else if (value == 0)
                {
                    terminated = true;
                }
                else
                {
                    length++;
                }
            }

            if (!terminated || length == 0)
            {
                throw new InvalidDataException(
                    "EtherCAT topology name must be non-empty and NUL terminated.");
            }

            return Encoding.ASCII.GetString(payload, offset, length);
        }

        private static ulong GetBitWidthMask(byte bitWidth)
        {
            if (bitWidth == 0 || bitWidth > 64)
            {
                return 0;
            }

            return bitWidth == 64
                ? ulong.MaxValue
                : (1UL << bitWidth) - 1UL;
        }

        private static bool HasFlag(
            LMCEtherCATTopologyNodeFlags value,
            LMCEtherCATTopologyNodeFlags flag)
        {
            return (value & flag) != 0;
        }
    }
}

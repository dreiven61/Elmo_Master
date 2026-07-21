using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LasalMotionControlLib
{
    internal static partial class LMC_DiagnosticsFrame
    {
        internal const int CatalogChunkRequestPayloadLength = 16;
        internal const int ReadPIRequestPayloadLength = 20;
        internal const ushort MaxCatalogEntriesPerChunk = 16;

        internal static byte[] GetSignalCatalogInfo(uint requestId)
        {
            return CreateCommonRequest(
                LMC_CommandId.GetSignalCatalogInfo,
                requestId,
                CommonRequestPayloadLength);
        }

        internal static byte[] GetSignalCatalogChunk(
            uint requestId,
            uint expectedMapRevision,
            ushort startIndex,
            ushort maxEntries)
        {
            if (maxEntries == 0 || maxEntries > MaxCatalogEntriesPerChunk)
            {
                throw new ArgumentOutOfRangeException(
                    "maxEntries",
                    "Catalog chunks must request between 1 and 16 entries.");
            }

            var buffer = CreateCommonRequest(
                LMC_CommandId.GetSignalCatalogChunk,
                requestId,
                CatalogChunkRequestPayloadLength);
            var payloadOffset = LMC_Frame.HeaderSize;

            LMC_Frame.WriteUInt32(buffer, payloadOffset + 8, expectedMapRevision);
            LMC_Frame.WriteUInt16(buffer, payloadOffset + 12, startIndex);
            LMC_Frame.WriteUInt16(buffer, payloadOffset + 14, maxEntries);

            return buffer;
        }

        internal static byte[] ReadEtherCATHealth(uint requestId)
        {
            return CreateCommonRequest(
                LMC_CommandId.ReadEtherCATHealth,
                requestId,
                CommonRequestPayloadLength);
        }

        internal static byte[] ReadPI(
            uint requestId,
            uint expectedMapRevision,
            uint signalId,
            LMCSignalValueType expectedType)
        {
            if (signalId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "signalId",
                    "SignalId must be non-zero.");
            }

            ValidateExpectedValueType(expectedType);

            var buffer = CreateCommonRequest(
                LMC_CommandId.ReadPI,
                requestId,
                ReadPIRequestPayloadLength);
            var payloadOffset = LMC_Frame.HeaderSize;

            LMC_Frame.WriteUInt32(buffer, payloadOffset + 8, expectedMapRevision);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 12, signalId);
            buffer[payloadOffset + 16] = (byte)expectedType;

            return buffer;
        }

        private static byte[] CreateCommonRequest(
            ushort commandId,
            uint requestId,
            int payloadLength)
        {
            if (requestId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "requestId",
                    "Diagnostics request identifiers must be non-zero.");
            }

            var buffer = LMC_Frame.CreateRequest(
                commandId,
                0,
                checked((ushort)payloadLength));
            var payloadOffset = LMC_Frame.HeaderSize;

            LMC_Frame.WriteUInt16(buffer, payloadOffset, SchemaVersion);
            LMC_Frame.WriteUInt16(buffer, payloadOffset + 2, 0);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 4, requestId);

            return buffer;
        }

        private static void ValidateExpectedValueType(
            LMCSignalValueType expectedType)
        {
            if (expectedType < LMCSignalValueType.Invalid
                || expectedType > LMCSignalValueType.BitField32)
            {
                throw new ArgumentOutOfRangeException(
                    "expectedType",
                    "Expected PI type is not defined by diagnostics schema version 1.");
            }
        }
    }

    internal static partial class LMC_DiagnosticsParser
    {
        internal const int CatalogInfoPayloadLength = 36;
        internal const int CatalogChunkHeaderPayloadLength = 28;
        internal const int CatalogEntryStride = 80;
        internal const int CatalogAliasBytes = 40;
        internal const int HealthHeaderPayloadLength = 72;
        internal const int SlaveHealthEntryStride = 32;
        internal const int ReadPIPayloadLength = 52;
        internal const int SignalValueEntryStride = 16;

        private const uint D1CatalogFlags = 0x0000000Fu;
        private const ushort KnownMasterFlagsMask = 0x0003;
        private const ushort KnownAccessFlagsMask = 0x000F;
        private const ushort KnownSignalFlagsMask = 0x003F;
        private const ushort D1HealthSlaveCount = 4;

        internal static LMCSignalCatalogInfo ParseSignalCatalogInfo(
            byte[] raw,
            uint expectedRequestId)
        {
            var response = ParseSuccessfulCommand(
                raw,
                expectedRequestId,
                "GetSignalCatalogInfo");
            RequireExactPayloadLength(
                response,
                CatalogInfoPayloadLength,
                "GetSignalCatalogInfo");
            RequireNoResponseFlags(response, "GetSignalCatalogInfo");

            var payload = response.TransportResponse.Payload;
            var entryStride = LMC_Frame.ReadUInt16(payload, 22);
            var aliasBytes = LMC_Frame.ReadUInt16(payload, 24);
            var signalIdBytes = LMC_Frame.ReadUInt16(payload, 26);
            var catalogFlags = LMC_Frame.ReadUInt32(payload, 28);
            var crcKind = LMC_Frame.ReadUInt32(payload, 32);

            if (entryStride != CatalogEntryStride
                || aliasBytes != CatalogAliasBytes
                || signalIdBytes != 4
                || catalogFlags != D1CatalogFlags
                || crcKind != (uint)LMCDiagnosticsCrcKind.Crc32IsoHdlc)
            {
                throw new InvalidDataException(
                    "GetSignalCatalogInfo returned a Catalog schema not defined by diagnostics version 1.");
            }

            var mapRevision = LMC_Frame.ReadUInt32(payload, 16);
            if (mapRevision == 0)
            {
                throw new InvalidDataException(
                    "GetSignalCatalogInfo returned reserved MapRevision zero.");
            }

            return new LMCSignalCatalogInfo(
                response,
                mapRevision,
                LMC_Frame.ReadUInt16(payload, 20),
                entryStride,
                aliasBytes,
                signalIdBytes,
                catalogFlags,
                crcKind);
        }

        internal static LMCSignalCatalogChunk ParseSignalCatalogChunk(
            byte[] raw,
            uint expectedRequestId,
            uint expectedMapRevision,
            ushort expectedStartIndex,
            ushort requestedMaxEntries)
        {
            if (requestedMaxEntries == 0
                || requestedMaxEntries > LMC_DiagnosticsFrame.MaxCatalogEntriesPerChunk)
            {
                throw new ArgumentOutOfRangeException("requestedMaxEntries");
            }

            var response = ParseSuccessfulCommand(
                raw,
                expectedRequestId,
                "GetSignalCatalogChunk");

            if (response.TransportResponse.PayloadLength
                < CatalogChunkHeaderPayloadLength)
            {
                throw new InvalidDataException(
                    "GetSignalCatalogChunk response is shorter than its 28-byte header.");
            }

            if ((response.ResponseFlags & LMCDiagnosticsResponseFlags.Partial) != 0)
            {
                throw new InvalidDataException(
                    "GetSignalCatalogChunk must report per-chunk results, not Partial response status.");
            }

            var payload = response.TransportResponse.Payload;
            var mapRevision = LMC_Frame.ReadUInt32(payload, 16);
            var startIndex = LMC_Frame.ReadUInt16(payload, 20);
            var returnedCount = LMC_Frame.ReadUInt16(payload, 22);
            var totalCount = LMC_Frame.ReadUInt16(payload, 24);
            var entryStride = LMC_Frame.ReadUInt16(payload, 26);

            if (mapRevision == 0
                || (expectedMapRevision != 0 && mapRevision != expectedMapRevision))
            {
                throw new InvalidDataException(
                    "GetSignalCatalogChunk MapRevision does not match the request.");
            }

            if (startIndex != expectedStartIndex
                || entryStride != CatalogEntryStride
                || startIndex > totalCount
                || returnedCount != Math.Min(
                    requestedMaxEntries,
                    totalCount - startIndex))
            {
                throw new InvalidDataException(
                    "GetSignalCatalogChunk returned invalid bounds or schema fields.");
            }

            var expectedLength = checked(
                CatalogChunkHeaderPayloadLength
                + returnedCount * CatalogEntryStride);
            RequireExactPayloadLength(
                response,
                expectedLength,
                "GetSignalCatalogChunk");

            var isFinalChunk = startIndex + returnedCount == totalCount;
            var hasLastChunkFlag =
                (response.ResponseFlags & LMCDiagnosticsResponseFlags.LastChunk) != 0;
            if (hasLastChunkFlag != isFinalChunk)
            {
                throw new InvalidDataException(
                    "GetSignalCatalogChunk LastChunk must be set if and only if the returned range is final.");
            }

            var entries = new List<LMCSignalCatalogEntry>(returnedCount);
            var signalIds = new HashSet<uint>();

            for (var index = 0; index < returnedCount; index++)
            {
                var entryOffset = CatalogChunkHeaderPayloadLength
                    + index * CatalogEntryStride;
                var catalogIndex = checked((ushort)(startIndex + index));
                var entry = ParseCatalogEntry(payload, entryOffset, catalogIndex);

                if (!signalIds.Add(entry.SignalId))
                {
                    throw new InvalidDataException(
                        "GetSignalCatalogChunk contains duplicate SignalId values.");
                }

                entries.Add(entry);
            }

            return new LMCSignalCatalogChunk(
                response,
                mapRevision,
                startIndex,
                totalCount,
                entryStride,
                entries);
        }

        internal static LMCEtherCATHealth ParseEtherCATHealth(
            byte[] raw,
            uint expectedRequestId)
        {
            var response = ParseSuccessfulCommand(
                raw,
                expectedRequestId,
                "ReadEtherCATHealth");

            if (response.TransportResponse.PayloadLength
                < HealthHeaderPayloadLength)
            {
                throw new InvalidDataException(
                    "ReadEtherCATHealth response is shorter than its 72-byte header.");
            }

            RequireNoResponseFlags(response, "ReadEtherCATHealth");

            var payload = response.TransportResponse.Payload;
            var capturePhase = (LMCCapturePhase)LMC_Frame.ReadUInt16(payload, 20);
            var slaveCount = LMC_Frame.ReadUInt16(payload, 22);
            var masterFlagsValue = LMC_Frame.ReadUInt16(payload, 38);
            var snapshotSequence = LMC_Frame.ReadUInt32(payload, 64);
            var slaveEntryStride = LMC_Frame.ReadUInt16(payload, 68);
            var mapRevision = LMC_Frame.ReadUInt32(payload, 16);

            if (mapRevision == 0
                || capturePhase != LMCCapturePhase.InputMapped
                || slaveCount != D1HealthSlaveCount
                || slaveEntryStride != SlaveHealthEntryStride
                || (masterFlagsValue & ~KnownMasterFlagsMask) != 0
                || (snapshotSequence & 1u) != 0
                || LMC_Frame.ReadUInt16(payload, 70) != 0
                || !IsEtherCATState(LMC_Frame.ReadUInt16(payload, 36)))
            {
                throw new InvalidDataException(
                    "ReadEtherCATHealth returned invalid capture, stride, flags, count, or seqlock fields.");
            }

            var expectedLength = checked(
                HealthHeaderPayloadLength
                + slaveCount * SlaveHealthEntryStride);
            RequireExactPayloadLength(
                response,
                expectedLength,
                "ReadEtherCATHealth");

            var slaves = new List<LMCEtherCATSlaveHealth>(slaveCount);
            for (var index = 0; index < slaveCount; index++)
            {
                var entryOffset = HealthHeaderPayloadLength
                    + index * SlaveHealthEntryStride;
                var slaveIndex = LMC_Frame.ReadUInt16(payload, entryOffset);
                var physicalAxis = LMC_Frame.ReadUInt16(payload, entryOffset + 2);
                var online = payload[entryOffset + 4];
                var etherCATState = payload[entryOffset + 5];

                if (online > 1
                    || slaveIndex != index
                    || physicalAxis != index + 1
                    || !IsEtherCATState(etherCATState))
                {
                    throw new InvalidDataException(
                        "ReadEtherCATHealth must contain SlaveIndex 0..3 and PhysicalAxis 1..4 in order.");
                }

                slaves.Add(
                    new LMCEtherCATSlaveHealth(
                        slaveIndex,
                        physicalAxis,
                        online != 0,
                        etherCATState,
                        LMC_Frame.ReadUInt16(payload, entryOffset + 6),
                        LMC_Frame.ReadUInt32(payload, entryOffset + 8),
                        LMC_Frame.ReadUInt32(payload, entryOffset + 12),
                        LMC_Frame.ReadUInt32(payload, entryOffset + 16),
                        LMC_Frame.ReadUInt32(payload, entryOffset + 20),
                        LMC_Frame.ReadUInt32(payload, entryOffset + 24),
                        LMC_Frame.ReadUInt32(payload, entryOffset + 28)));
            }

            return new LMCEtherCATHealth(
                response,
                mapRevision,
                capturePhase,
                LMC_Frame.ReadUInt32(payload, 24),
                LMC_Frame.ReadUInt32(payload, 28),
                LMC_Frame.ReadUInt32(payload, 32),
                LMC_Frame.ReadUInt16(payload, 36),
                (LMCEtherCATMasterFlags)masterFlagsValue,
                LMC_Frame.ReadUInt32(payload, 40),
                LMC_Frame.ReadUInt32(payload, 44),
                LMC_Frame.ReadUInt32(payload, 48),
                LMC_Frame.ReadUInt32(payload, 52),
                LMC_Frame.ReadUInt32(payload, 56),
                LMC_Frame.ReadUInt32(payload, 60),
                snapshotSequence,
                slaves);
        }

        internal static LMCSignalValue ParsePI(
            byte[] raw,
            uint expectedRequestId,
            uint expectedMapRevision,
            uint expectedSignalId,
            LMCSignalValueType expectedType)
        {
            var response = ParseSuccessfulCommand(
                raw,
                expectedRequestId,
                "ReadPI");
            RequireExactPayloadLength(response, ReadPIPayloadLength, "ReadPI");
            RequireNoResponseFlags(response, "ReadPI");

            var payload = response.TransportResponse.Payload;
            var mapRevision = LMC_Frame.ReadUInt32(payload, 16);
            var capturePhase = (LMCCapturePhase)LMC_Frame.ReadUInt16(payload, 20);

            if (mapRevision == 0
                || (expectedMapRevision != 0 && mapRevision != expectedMapRevision))
            {
                throw new InvalidDataException(
                    "ReadPI MapRevision does not match the request.");
            }

            if (capturePhase != LMCCapturePhase.InputMapped
                && capturePhase != LMCCapturePhase.PreOutput)
            {
                throw new InvalidDataException(
                    "ReadPI returned an invalid CapturePhase.");
            }

            if (LMC_Frame.ReadUInt16(payload, 22) != 0)
            {
                throw new InvalidDataException(
                    "ReadPI reserved header field must be zero.");
            }

            var entry = ParseSignalValueEntry(payload, 36);
            if (entry.SignalId != expectedSignalId)
            {
                throw new InvalidDataException(
                    "ReadPI returned a different SignalId than requested.");
            }

            if (expectedType != LMCSignalValueType.Invalid
                && entry.ValueType != expectedType)
            {
                throw new InvalidDataException(
                    "ReadPI returned a ValueType different from ExpectedType.");
            }

            return new LMCSignalValue(
                response,
                mapRevision,
                capturePhase,
                LMC_Frame.ReadUInt32(payload, 24),
                LMC_Frame.ReadUInt32(payload, 28),
                LMC_Frame.ReadUInt32(payload, 32),
                entry);
        }

        private static LMCSignalCatalogEntry ParseCatalogEntry(
            byte[] payload,
            int offset,
            ushort expectedCatalogIndex)
        {
            var signalId = LMC_Frame.ReadUInt32(payload, offset);
            var catalogIndex = LMC_Frame.ReadUInt16(payload, offset + 4);
            var sourceKind = (LMCSignalSourceKind)payload[offset + 6];
            var dataType = (LMCSignalValueType)payload[offset + 8];
            var byteWidth = payload[offset + 9];
            var accessFlagsValue = LMC_Frame.ReadUInt16(payload, offset + 12);
            var signalFlagsValue = LMC_Frame.ReadUInt16(payload, offset + 14);
            var pdoDirection = (LMCPdoDirection)payload[offset + 19];
            var scaleDenominator = LMC_Frame.ReadInt32(payload, offset + 24);
            var minimumRaw = LMC_Frame.ReadInt32(payload, offset + 28);
            var maximumRaw = LMC_Frame.ReadInt32(payload, offset + 32);
            var pdoIndex = LMC_Frame.ReadUInt16(payload, offset + 16);
            var pdoSubIndex = payload[offset + 18];
            var unitCode = LMC_Frame.ReadUInt16(payload, offset + 10);

            if (signalId == 0
                || catalogIndex != expectedCatalogIndex
                || sourceKind < LMCSignalSourceKind.System
                || sourceKind > LMCSignalSourceKind.PlcApplication
                || dataType < LMCSignalValueType.Bool
                || dataType > LMCSignalValueType.BitField32
                || byteWidth != ExpectedByteWidth(dataType)
                || (accessFlagsValue & ~KnownAccessFlagsMask) != 0
                || (signalFlagsValue & ~KnownSignalFlagsMask) != 0
                || pdoDirection < LMCPdoDirection.None
                || pdoDirection > LMCPdoDirection.DriveToMaster
                || scaleDenominator == 0
                || LMC_Frame.ReadUInt32(payload, offset + 76) != 0
                || (accessFlagsValue & (ushort)LMCSignalAccessFlags.Readable) == 0
                || unitCode > 1
                || !IsRawRangeValid(dataType, minimumRaw, maximumRaw))
            {
                throw new InvalidDataException(
                    "Signal Catalog entry contains a field not defined by diagnostics version 1.");
            }

            var signalFlags = (LMCSignalFlags)signalFlagsValue;
            if ((signalFlags & LMCSignalFlags.PhysicalAxis) != 0
                && (signalFlags & LMCSignalFlags.SoftwareAxis) != 0)
            {
                throw new InvalidDataException(
                    "Signal Catalog entry cannot be both a physical and software axis signal.");
            }

            if ((signalFlags & LMCSignalFlags.InputMappedPhase) != 0
                && (signalFlags & LMCSignalFlags.PreOutputPhase) != 0)
            {
                throw new InvalidDataException(
                    "Signal Catalog entry cannot declare two capture phases.");
            }

            if ((signalFlags & (LMCSignalFlags.InputMappedPhase
                    | LMCSignalFlags.PreOutputPhase)) == 0)
            {
                throw new InvalidDataException(
                    "Signal Catalog entry must declare exactly one capture phase.");
            }

            var isPdoInput = sourceKind == LMCSignalSourceKind.PdoInput;
            var isPdoOutput = sourceKind == LMCSignalSourceKind.PdoOutputLastTx;
            var isPdo = isPdoInput || isPdoOutput;
            if (isPdo)
            {
                if ((signalFlags & LMCSignalFlags.ActivePdo) == 0
                    || (signalFlags & LMCSignalFlags.SoftwareAxis) != 0
                    || (signalFlags & LMCSignalFlags.InputMappedPhase) == 0
                    || (signalFlags & LMCSignalFlags.PreOutputPhase) != 0
                    || pdoIndex == 0
                    || (isPdoInput && pdoDirection != LMCPdoDirection.DriveToMaster)
                    || (isPdoOutput && pdoDirection != LMCPdoDirection.MasterToDrive))
                {
                    throw new InvalidDataException(
                        "Signal Catalog PDO metadata is internally inconsistent.");
                }
            }
            else if ((signalFlags & LMCSignalFlags.ActivePdo) != 0
                || pdoIndex != 0
                || pdoSubIndex != 0
                || pdoDirection != LMCPdoDirection.None)
            {
                throw new InvalidDataException(
                    "Non-PDO Signal Catalog entries must not expose PDO metadata.");
            }

            if (unitCode == 1 && dataType != LMCSignalValueType.Int32)
            {
                throw new InvalidDataException(
                    "PositionCounts UnitCode requires an Int32 signal value.");
            }

            return new LMCSignalCatalogEntry(
                signalId,
                catalogIndex,
                sourceKind,
                payload[offset + 7],
                dataType,
                byteWidth,
                unitCode,
                (LMCSignalAccessFlags)accessFlagsValue,
                signalFlags,
                pdoIndex,
                pdoSubIndex,
                pdoDirection,
                LMC_Frame.ReadInt32(payload, offset + 20),
                scaleDenominator,
                minimumRaw,
                maximumRaw,
                ReadFixedAsciiAlias(payload, offset + 36));
        }

        private static LMCSignalValueEntry ParseSignalValueEntry(
            byte[] payload,
            int offset)
        {
            var valueType = (LMCSignalValueType)payload[offset + 8];
            var entryStatus = (LMCSignalEntryStatus)payload[offset + 9];
            var detailCode = LMC_Frame.ReadUInt32(payload, offset + 12);
            var rawValue = LMC_Frame.ReadUInt32(payload, offset + 4);

            if (valueType < LMCSignalValueType.Bool
                || valueType > LMCSignalValueType.BitField32
                || entryStatus == LMCSignalEntryStatus.None
                || LMC_Frame.ReadUInt16(payload, offset + 10) != 0
                || ((entryStatus & LMCSignalEntryStatus.Valid) != 0
                    && entryStatus != LMCSignalEntryStatus.Valid)
                || (entryStatus == LMCSignalEntryStatus.Valid && detailCode != 0))
            {
                throw new InvalidDataException(
                    "Signal value entry contains invalid type, status, or detail fields.");
            }

            if ((valueType == LMCSignalValueType.Bool
                    || valueType == LMCSignalValueType.UInt16
                    || valueType == LMCSignalValueType.BitField16)
                && (rawValue & 0xFFFF0000u) != 0)
            {
                throw new InvalidDataException(
                    "A 16-bit unsigned signal value must be zero-extended to 32 bits.");
            }

            if (valueType == LMCSignalValueType.Bool && rawValue > 1)
            {
                throw new InvalidDataException(
                    "A Boolean signal value must be encoded as zero or one.");
            }

            if (valueType == LMCSignalValueType.Int16
                && rawValue != (uint)(int)(short)rawValue)
            {
                throw new InvalidDataException(
                    "A signed 16-bit signal value must be sign-extended to 32 bits.");
            }

            return new LMCSignalValueEntry(
                LMC_Frame.ReadUInt32(payload, offset),
                rawValue,
                valueType,
                entryStatus,
                detailCode);
        }

        private static LMCDiagnosticsResponse ParseSuccessfulCommand(
            byte[] raw,
            uint expectedRequestId,
            string commandName)
        {
            var transportResponse = LMCConnection.Parse(raw);

            if (!transportResponse.IsFrameValid)
            {
                throw new InvalidDataException(
                    commandName + " returned an invalid RPC frame.");
            }

            if (transportResponse.HeaderReserved != 0)
            {
                throw new InvalidDataException(
                    commandName + " outer response reserved field must be zero.");
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
                            "The connected RPC server does not support "
                            + commandName
                            + ".",
                            acknowledgement);
                    }

                    throw new InvalidOperationException(
                        commandName
                        + " was rejected before diagnostics dispatch. HeaderStatus="
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
                    commandName
                    + " was rejected by the RPC dispatcher. HeaderStatus="
                    + transportResponse.HeaderStatus
                    + ".");
            }

            var response = ParseCommonResponse(
                transportResponse,
                expectedRequestId);

            if (!response.IsSuccess)
            {
                if (transportResponse.PayloadLength != CommonResponsePayloadLength
                    || transportResponse.Payload.Length != CommonResponsePayloadLength
                    || response.ResponseFlags != LMCDiagnosticsResponseFlags.None)
                {
                    throw new InvalidDataException(
                        commandName
                        + " domain error must contain exactly the 16-byte common response with no flags.");
                }

                throw new LMCDiagnosticsCommandException(
                    commandName
                    + " failed. ErrorId="
                    + response.ErrorId
                    + ", DetailCode="
                    + response.DetailCode
                    + ".",
                    response);
            }

            return response;
        }

        private static void RequireExactPayloadLength(
            LMCDiagnosticsResponse response,
            int expectedLength,
            string commandName)
        {
            if (response.TransportResponse.PayloadLength != expectedLength
                || response.TransportResponse.Payload.Length != expectedLength)
            {
                throw new InvalidDataException(
                    commandName
                    + " response must contain exactly "
                    + expectedLength
                    + " payload bytes.");
            }
        }

        private static void RequireNoResponseFlags(
            LMCDiagnosticsResponse response,
            string commandName)
        {
            if (response.ResponseFlags != LMCDiagnosticsResponseFlags.None)
            {
                throw new InvalidDataException(
                    commandName + " does not define response flags in schema version 1.");
            }
        }

        private static byte ExpectedByteWidth(LMCSignalValueType valueType)
        {
            switch (valueType)
            {
                case LMCSignalValueType.Bool:
                    return 1;
                case LMCSignalValueType.Int16:
                case LMCSignalValueType.UInt16:
                case LMCSignalValueType.BitField16:
                    return 2;
                default:
                    return 4;
            }
        }

        private static string ReadFixedAsciiAlias(byte[] payload, int offset)
        {
            var length = 0;
            var foundTerminator = false;

            for (var index = 0; index < CatalogAliasBytes; index++)
            {
                var value = payload[offset + index];
                if (value > 0x7F)
                {
                    throw new InvalidDataException(
                        "Signal Catalog alias contains non-ASCII data.");
                }

                if (foundTerminator)
                {
                    if (value != 0)
                    {
                        throw new InvalidDataException(
                            "Signal Catalog alias is not canonically NUL padded.");
                    }

                    continue;
                }

                if (value == 0)
                {
                    foundTerminator = true;
                }
                else
                {
                    length++;
                }
            }

            if (length == 0)
            {
                throw new InvalidDataException(
                    "Signal Catalog alias must not be empty.");
            }

            return Encoding.ASCII.GetString(payload, offset, length);
        }

        internal static uint ComputeCatalogMapRevision(
            IReadOnlyList<LMCSignalCatalogEntry> entries)
        {
            if (entries == null)
            {
                throw new ArgumentNullException("entries");
            }

            var canonical = new byte[checked(entries.Count * CatalogEntryStride)];
            for (var index = 0; index < entries.Count; index++)
            {
                var entry = entries[index]
                    ?? throw new InvalidDataException(
                        "Signal Catalog contains a null entry.");
                var offset = index * CatalogEntryStride;
                var aliasBytes = Encoding.ASCII.GetBytes(entry.Alias);

                if (entry.CatalogIndex != index
                    || aliasBytes.Length == 0
                    || aliasBytes.Length > CatalogAliasBytes)
                {
                    throw new InvalidDataException(
                        "Signal Catalog cannot be serialized canonically.");
                }

                LMC_Frame.WriteUInt32(canonical, offset, entry.SignalId);
                LMC_Frame.WriteUInt16(canonical, offset + 4, entry.CatalogIndex);
                canonical[offset + 6] = (byte)entry.SourceKind;
                canonical[offset + 7] = entry.SourceIndex;
                canonical[offset + 8] = (byte)entry.DataType;
                canonical[offset + 9] = entry.ByteWidth;
                LMC_Frame.WriteUInt16(canonical, offset + 10, entry.UnitCode);
                LMC_Frame.WriteUInt16(canonical, offset + 12, (ushort)entry.AccessFlags);
                LMC_Frame.WriteUInt16(canonical, offset + 14, (ushort)entry.SignalFlags);
                LMC_Frame.WriteUInt16(canonical, offset + 16, entry.PdoIndex);
                canonical[offset + 18] = entry.PdoSubIndex;
                canonical[offset + 19] = (byte)entry.PdoDirection;
                LMC_Frame.WriteInt32(canonical, offset + 20, entry.ScaleNumerator);
                LMC_Frame.WriteInt32(canonical, offset + 24, entry.ScaleDenominator);
                LMC_Frame.WriteInt32(canonical, offset + 28, entry.MinimumRaw);
                LMC_Frame.WriteInt32(canonical, offset + 32, entry.MaximumRaw);
                Buffer.BlockCopy(
                    aliasBytes,
                    0,
                    canonical,
                    offset + 36,
                    aliasBytes.Length);
            }

            var crc = 0xFFFFFFFFu;
            foreach (var value in canonical)
            {
                crc ^= value;
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

        private static bool IsEtherCATState(uint value)
        {
            return value == 0
                || value == 1
                || value == 2
                || value == 3
                || value == 4
                || value == 8;
        }

        private static bool IsRawRangeValid(
            LMCSignalValueType dataType,
            int minimumRaw,
            int maximumRaw)
        {
            switch (dataType)
            {
                case LMCSignalValueType.Bool:
                    return minimumRaw >= 0
                        && maximumRaw <= 1
                        && minimumRaw <= maximumRaw;
                case LMCSignalValueType.Int16:
                    return minimumRaw >= short.MinValue
                        && maximumRaw <= short.MaxValue
                        && minimumRaw <= maximumRaw;
                case LMCSignalValueType.UInt16:
                case LMCSignalValueType.BitField16:
                    return minimumRaw >= 0
                        && maximumRaw <= ushort.MaxValue
                        && minimumRaw <= maximumRaw;
                case LMCSignalValueType.Int32:
                    return minimumRaw <= maximumRaw;
                case LMCSignalValueType.UInt32:
                case LMCSignalValueType.BitField32:
                    return unchecked((uint)minimumRaw)
                        <= unchecked((uint)maximumRaw);
                case LMCSignalValueType.Real32:
                    return true;
                default:
                    return false;
            }
        }
    }
}

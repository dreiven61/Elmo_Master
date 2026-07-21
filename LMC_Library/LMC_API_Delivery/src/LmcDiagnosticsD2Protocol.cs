using System;
using System.Collections.Generic;
using System.IO;

namespace LasalMotionControlLib
{
    internal static partial class LMC_DiagnosticsFrame
    {
        internal const ushort MaxBulkSignalCount = 32;
        internal const int BulkIdentityRequestPayloadLength = 20;
        internal const int ConfigureBulkRequestHeaderPayloadLength = 20;

        internal static byte[] ConfigureBulk(
            uint requestId,
            uint expectedMapRevision,
            uint requestedBulkId,
            IReadOnlyList<uint> signalIds)
        {
            RequireExactBulkMapRevision(expectedMapRevision);
            ValidateBulkSignalIds(signalIds);

            var payloadLength = checked(
                ConfigureBulkRequestHeaderPayloadLength
                + signalIds.Count * sizeof(uint));
            var buffer = CreateCommonRequest(
                LMC_CommandId.ConfigureBulk,
                requestId,
                payloadLength);
            var payloadOffset = LMC_Frame.HeaderSize;

            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 8,
                expectedMapRevision);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 12,
                requestedBulkId);
            LMC_Frame.WriteUInt16(
                buffer,
                payloadOffset + 16,
                checked((ushort)signalIds.Count));

            for (var index = 0; index < signalIds.Count; index++)
            {
                LMC_Frame.WriteUInt32(
                    buffer,
                    payloadOffset + 20 + index * sizeof(uint),
                    signalIds[index]);
            }

            return buffer;
        }

        internal static byte[] ReadBulkStatus(
            uint requestId,
            uint bulkId,
            uint configRevision,
            uint mapRevision)
        {
            return CreateBulkIdentityRequest(
                LMC_CommandId.ReadBulkStatus,
                requestId,
                bulkId,
                configRevision,
                mapRevision);
        }

        internal static byte[] ReadBulkSnapshot(
            uint requestId,
            uint bulkId,
            uint configRevision,
            uint mapRevision)
        {
            return CreateBulkIdentityRequest(
                LMC_CommandId.ReadBulkSnapshot,
                requestId,
                bulkId,
                configRevision,
                mapRevision);
        }

        internal static byte[] ReleaseBulk(
            uint requestId,
            uint bulkId,
            uint configRevision,
            uint mapRevision)
        {
            return CreateBulkIdentityRequest(
                LMC_CommandId.ReleaseBulk,
                requestId,
                bulkId,
                configRevision,
                mapRevision);
        }

        internal static uint[] CopyAndValidateBulkSignalIds(
            IReadOnlyList<uint> signalIds)
        {
            ValidateBulkSignalIds(signalIds);

            var copy = new uint[signalIds.Count];
            for (var index = 0; index < copy.Length; index++)
            {
                copy[index] = signalIds[index];
            }

            return copy;
        }

        private static byte[] CreateBulkIdentityRequest(
            ushort commandId,
            uint requestId,
            uint bulkId,
            uint configRevision,
            uint mapRevision)
        {
            if (bulkId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "bulkId",
                    "BulkId must be non-zero.");
            }

            if (configRevision == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "configRevision",
                    "ConfigRevision must be non-zero.");
            }

            RequireExactBulkMapRevision(mapRevision);

            var buffer = CreateCommonRequest(
                commandId,
                requestId,
                BulkIdentityRequestPayloadLength);
            var payloadOffset = LMC_Frame.HeaderSize;

            LMC_Frame.WriteUInt32(buffer, payloadOffset + 8, bulkId);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 12, configRevision);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 16, mapRevision);

            return buffer;
        }

        private static void ValidateBulkSignalIds(
            IReadOnlyList<uint> signalIds)
        {
            if (signalIds == null)
            {
                throw new ArgumentNullException("signalIds");
            }

            if (signalIds.Count == 0
                || signalIds.Count > MaxBulkSignalCount)
            {
                throw new ArgumentOutOfRangeException(
                    "signalIds",
                    "Bulk configurations require between 1 and 32 signals.");
            }

            var uniqueSignalIds = new HashSet<uint>();
            for (var index = 0; index < signalIds.Count; index++)
            {
                var signalId = signalIds[index];
                if (signalId == 0)
                {
                    throw new ArgumentOutOfRangeException(
                        "signalIds",
                        "Bulk SignalId values must be non-zero.");
                }

                if (!uniqueSignalIds.Add(signalId))
                {
                    throw new ArgumentException(
                        "Bulk configurations do not allow duplicate SignalId values.",
                        "signalIds");
                }
            }
        }

        private static void RequireExactBulkMapRevision(uint mapRevision)
        {
            if (mapRevision == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "mapRevision",
                    "Bulk operations require a non-zero exact Catalog revision.");
            }
        }
    }

    internal static partial class LMC_DiagnosticsParser
    {
        internal const int BulkStatusPayloadLength = 36;
        internal const int BulkSnapshotHeaderPayloadLength = 56;

        private const uint KnownBulkSnapshotFlagsMask = 0x00000007u;

        internal static LMCBulkStatus ParseConfigureBulk(
            byte[] raw,
            uint expectedRequestId,
            uint expectedMapRevision,
            uint requestedBulkId,
            ushort expectedSignalCount)
        {
            return ParseBulkStatusCore(
                raw,
                expectedRequestId,
                expectedMapRevision,
                requestedBulkId,
                0,
                expectedSignalCount,
                true,
                "ConfigureBulk");
        }

        internal static LMCBulkStatus ParseBulkStatus(
            byte[] raw,
            uint expectedRequestId,
            uint expectedBulkId,
            uint expectedConfigRevision,
            uint expectedMapRevision,
            ushort expectedSignalCount)
        {
            return ParseBulkStatusCore(
                raw,
                expectedRequestId,
                expectedMapRevision,
                expectedBulkId,
                expectedConfigRevision,
                expectedSignalCount,
                false,
                "ReadBulkStatus");
        }

        internal static LMCBulkSnapshot ParseBulkSnapshot(
            byte[] raw,
            uint expectedRequestId,
            uint expectedBulkId,
            uint expectedConfigRevision,
            uint expectedMapRevision,
            IReadOnlyList<uint> expectedSignalIds)
        {
            if (expectedSignalIds == null)
            {
                throw new ArgumentNullException("expectedSignalIds");
            }

            if (expectedSignalIds.Count == 0
                || expectedSignalIds.Count > LMC_DiagnosticsFrame.MaxBulkSignalCount)
            {
                throw new ArgumentOutOfRangeException("expectedSignalIds");
            }

            var response = ParseSuccessfulCommand(
                raw,
                expectedRequestId,
                "ReadBulkSnapshot");

            if (response.TransportResponse.PayloadLength
                < BulkSnapshotHeaderPayloadLength)
            {
                throw new InvalidDataException(
                    "ReadBulkSnapshot response is shorter than its 56-byte header.");
            }

            if ((response.ResponseFlags & LMCDiagnosticsResponseFlags.LastChunk) != 0)
            {
                throw new InvalidDataException(
                    "ReadBulkSnapshot does not define the LastChunk response flag.");
            }

            var payload = response.TransportResponse.Payload;
            var bulkId = LMC_Frame.ReadUInt32(payload, 16);
            var configRevision = LMC_Frame.ReadUInt32(payload, 20);
            var mapRevision = LMC_Frame.ReadUInt32(payload, 24);
            var entryCount = LMC_Frame.ReadUInt16(payload, 40);
            var entryStride = LMC_Frame.ReadUInt16(payload, 42);
            var capturePhase = (LMCCapturePhase)payload[44];
            var snapshotSequence = LMC_Frame.ReadUInt32(payload, 48);
            var snapshotFlagsValue = LMC_Frame.ReadUInt32(payload, 52);

            if (bulkId == 0
                || bulkId != expectedBulkId
                || configRevision == 0
                || configRevision != expectedConfigRevision
                || mapRevision == 0
                || mapRevision != expectedMapRevision)
            {
                throw new InvalidDataException(
                    "ReadBulkSnapshot response identity does not match the Bulk configuration.");
            }

            if (entryCount != expectedSignalIds.Count
                || entryStride != SignalValueEntryStride)
            {
                throw new InvalidDataException(
                    "ReadBulkSnapshot returned an unexpected entry count or stride.");
            }

            if (capturePhase != LMCCapturePhase.InputMapped
                && capturePhase != LMCCapturePhase.PreOutput)
            {
                throw new InvalidDataException(
                    "ReadBulkSnapshot returned an invalid CapturePhase.");
            }

            if (payload[45] != 0 || payload[46] != 0 || payload[47] != 0)
            {
                throw new InvalidDataException(
                    "ReadBulkSnapshot reserved header bytes must be zero.");
            }

            if ((snapshotSequence & 1u) != 0)
            {
                throw new InvalidDataException(
                    "ReadBulkSnapshot SnapshotSequence must be a stable even seqlock value.");
            }

            ValidateBulkSnapshotFlags(capturePhase, snapshotFlagsValue);

            var expectedLength = checked(
                BulkSnapshotHeaderPayloadLength
                + entryCount * SignalValueEntryStride);
            RequireExactPayloadLength(
                response,
                expectedLength,
                "ReadBulkSnapshot");

            var entries = new List<LMCSignalValueEntry>(entryCount);
            var hasInvalidEntry = false;

            for (var index = 0; index < entryCount; index++)
            {
                var entry = ParseSignalValueEntry(
                    payload,
                    BulkSnapshotHeaderPayloadLength
                        + index * SignalValueEntryStride);

                if (entry.SignalId != expectedSignalIds[index])
                {
                    throw new InvalidDataException(
                        "ReadBulkSnapshot SignalId order does not match the configured order.");
                }

                hasInvalidEntry |= !entry.IsValid;
                entries.Add(entry);
            }

            var reportsPartial = (response.ResponseFlags
                & LMCDiagnosticsResponseFlags.Partial) != 0;
            if (reportsPartial != hasInvalidEntry)
            {
                throw new InvalidDataException(
                    "ReadBulkSnapshot Partial must be set if and only if an entry is invalid.");
            }

            return new LMCBulkSnapshot(
                response,
                bulkId,
                configRevision,
                mapRevision,
                LMC_Frame.ReadUInt32(payload, 28),
                LMC_Frame.ReadUInt32(payload, 32),
                LMC_Frame.ReadUInt32(payload, 36),
                entryStride,
                capturePhase,
                snapshotSequence,
                (LMCBulkSnapshotFlags)snapshotFlagsValue,
                entries);
        }

        internal static LMCDiagnosticsResponse ParseReleaseBulk(
            byte[] raw,
            uint expectedRequestId)
        {
            var response = ParseSuccessfulCommand(
                raw,
                expectedRequestId,
                "ReleaseBulk");
            RequireExactPayloadLength(
                response,
                CommonResponsePayloadLength,
                "ReleaseBulk");
            RequireNoResponseFlags(response, "ReleaseBulk");
            return response;
        }

        private static LMCBulkStatus ParseBulkStatusCore(
            byte[] raw,
            uint expectedRequestId,
            uint expectedMapRevision,
            uint expectedBulkId,
            uint expectedConfigRevision,
            ushort expectedSignalCount,
            bool isConfigureResponse,
            string commandName)
        {
            var response = ParseSuccessfulCommand(
                raw,
                expectedRequestId,
                commandName);
            RequireExactPayloadLength(
                response,
                BulkStatusPayloadLength,
                commandName);
            RequireNoResponseFlags(response, commandName);

            var payload = response.TransportResponse.Payload;
            var bulkId = LMC_Frame.ReadUInt32(payload, 16);
            var configRevision = LMC_Frame.ReadUInt32(payload, 20);
            var mapRevision = LMC_Frame.ReadUInt32(payload, 24);
            var state = (LMCBulkState)LMC_Frame.ReadUInt16(payload, 28);
            var signalCount = LMC_Frame.ReadUInt16(payload, 30);

            if (bulkId == 0
                || (expectedBulkId != 0 && bulkId != expectedBulkId)
                || configRevision == 0
                || (expectedConfigRevision != 0
                    && configRevision != expectedConfigRevision)
                || mapRevision == 0
                || mapRevision != expectedMapRevision
                || signalCount == 0
                || signalCount > LMC_DiagnosticsFrame.MaxBulkSignalCount
                || signalCount != expectedSignalCount)
            {
                throw new InvalidDataException(
                    commandName + " returned an invalid or mismatched Bulk identity.");
            }

            if (state < LMCBulkState.Pending
                || state > LMCBulkState.Failed
                || (isConfigureResponse && state == LMCBulkState.Failed))
            {
                throw new InvalidDataException(
                    commandName + " returned a BulkState not valid for a successful response.");
            }

            return new LMCBulkStatus(
                response,
                bulkId,
                configRevision,
                mapRevision,
                state,
                signalCount,
                LMC_Frame.ReadUInt32(payload, 32));
        }

        private static void ValidateBulkSnapshotFlags(
            LMCCapturePhase capturePhase,
            uint snapshotFlagsValue)
        {
            if ((snapshotFlagsValue & ~KnownBulkSnapshotFlagsMask) != 0
                || (snapshotFlagsValue
                    & (uint)LMCBulkSnapshotFlags.SameCycle) == 0)
            {
                throw new InvalidDataException(
                    "ReadBulkSnapshot returned invalid SnapshotFlags.");
            }

            var phaseFlags = snapshotFlagsValue
                & ((uint)LMCBulkSnapshotFlags.InputMappedPhase
                    | (uint)LMCBulkSnapshotFlags.PreOutputPhase);
            var expectedPhaseFlag = capturePhase == LMCCapturePhase.InputMapped
                ? (uint)LMCBulkSnapshotFlags.InputMappedPhase
                : (uint)LMCBulkSnapshotFlags.PreOutputPhase;

            if (phaseFlags != expectedPhaseFlag)
            {
                throw new InvalidDataException(
                    "ReadBulkSnapshot CapturePhase does not match SnapshotFlags.");
            }
        }
    }
}

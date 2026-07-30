using System;
using System.Security.Cryptography;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal sealed class RecorderHeaderSemanticEvidence
    {
        private readonly byte[] canonicalBytes;

        internal RecorderHeaderSemanticEvidence(LMCRecorderHeader header)
        {
            canonicalBytes = RecorderHeaderSemanticCanonicalizer
                .Serialize(header);

            using (var sha256 = SHA256.Create())
            {
                Sha256 = ToUpperHex(
                    sha256.ComputeHash(this.canonicalBytes));
            }
        }

        internal string FormatId
        {
            get { return RecorderHeaderSemanticCanonicalizer.FormatId; }
        }

        internal int CanonicalByteCount
        {
            get { return canonicalBytes.Length; }
        }

        internal string Sha256 { get; private set; }

        internal byte[] CopyCanonicalBytes()
        {
            return (byte[])canonicalBytes.Clone();
        }

        private static string ToUpperHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes)
                .Replace("-", string.Empty);
        }
    }

    internal static class RecorderHeaderSemanticCanonicalizer
    {
        // Qualification-only V1 format. Multi-byte fields are little-endian,
        // signal IDs retain configured order, and Response/raw/request data is
        // intentionally excluded.
        internal const string FormatId = "LMCRHDR1";
        internal const int FixedByteCount = 103;

        private static readonly byte[] FormatTag =
        {
            0x4C, 0x4D, 0x43, 0x52, 0x48, 0x44, 0x52, 0x31
        };

        internal static RecorderHeaderSemanticEvidence CreateEvidence(
            LMCRecorderHeader header)
        {
            return new RecorderHeaderSemanticEvidence(header);
        }

        internal static byte[] Serialize(LMCRecorderHeader header)
        {
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }

            var signalCount = header.SignalIds.Count;
            var bytes = new byte[checked(
                FixedByteCount + (signalCount * sizeof(uint)))];
            var offset = 0;

            Buffer.BlockCopy(
                FormatTag,
                0,
                bytes,
                offset,
                FormatTag.Length);
            offset += FormatTag.Length;

            WriteUInt32(bytes, ref offset, header.DiagnosticsBootId);
            WriteUInt32(bytes, ref offset, header.RecordId);
            WriteUInt32(bytes, ref offset, header.BufferId);
            WriteUInt32(bytes, ref offset, header.ConfigId);
            WriteUInt32(bytes, ref offset, header.ConfigRevision);
            WriteUInt32(bytes, ref offset, header.MapRevision);
            WriteUInt16(
                bytes,
                ref offset,
                checked((ushort)header.CapturePhase));
            WriteByte(bytes, ref offset, checked((byte)header.StopReason));
            WriteUInt16(
                bytes,
                ref offset,
                checked((ushort)header.HeaderFlags));
            WriteUInt32(bytes, ref offset, header.SampleCount);
            WriteUInt32(bytes, ref offset, header.Capacity);
            WriteUInt16(bytes, ref offset, header.ChannelCount);
            WriteUInt16(bytes, ref offset, header.SampleStrideBytes);
            WriteUInt32(bytes, ref offset, header.SamplePeriodUs);
            WriteByte(bytes, ref offset, checked((byte)header.DataEncoding));
            WriteByte(bytes, ref offset, checked((byte)header.DataCrcPolicy));
            WriteUInt32(bytes, ref offset, header.TriggerIndex);
            WriteUInt32(bytes, ref offset, header.StartCycle);
            WriteUInt32(bytes, ref offset, header.TriggerCycle);
            WriteUInt32(bytes, ref offset, header.EndCycle);
            WriteUInt32(bytes, ref offset, header.StartTimestampLow);
            WriteUInt32(bytes, ref offset, header.StartTimestampHigh);
            WriteUInt32(bytes, ref offset, header.TriggerTimestampLow);
            WriteUInt32(bytes, ref offset, header.TriggerTimestampHigh);
            WriteUInt32(bytes, ref offset, header.EndTimestampLow);
            WriteUInt32(bytes, ref offset, header.EndTimestampHigh);
            WriteUInt32(bytes, ref offset, header.DroppedSamples);
            WriteUInt32(bytes, ref offset, header.OverflowCount);

            for (var index = 0; index < signalCount; index++)
            {
                WriteUInt32(bytes, ref offset, header.SignalIds[index]);
            }

            if (offset != bytes.Length)
            {
                throw new InvalidOperationException(
                    "Recorder header canonical byte count is inconsistent.");
            }

            return bytes;
        }

        private static void WriteByte(
            byte[] destination,
            ref int offset,
            byte value)
        {
            destination[offset++] = value;
        }

        private static void WriteUInt16(
            byte[] destination,
            ref int offset,
            ushort value)
        {
            destination[offset++] = (byte)value;
            destination[offset++] = (byte)(value >> 8);
        }

        private static void WriteUInt32(
            byte[] destination,
            ref int offset,
            uint value)
        {
            destination[offset++] = (byte)value;
            destination[offset++] = (byte)(value >> 8);
            destination[offset++] = (byte)(value >> 16);
            destination[offset++] = (byte)(value >> 24);
        }
    }
}

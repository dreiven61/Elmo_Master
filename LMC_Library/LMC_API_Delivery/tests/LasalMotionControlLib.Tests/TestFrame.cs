using System;
using System.Globalization;
using System.Text;

namespace LasalMotionControlLib.Tests
{
    internal static class TestFrame
    {
        internal static byte[] Hex(string value)
        {
            var compact = value
                .Replace(" ", string.Empty)
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty)
                .Replace("\t", string.Empty);

            if ((compact.Length & 1) != 0)
            {
                throw new ArgumentException("Hex text must contain complete bytes.", "value");
            }

            var bytes = new byte[compact.Length / 2];

            for (var index = 0; index < bytes.Length; index++)
            {
                bytes[index] = byte.Parse(
                    compact.Substring(index * 2, 2),
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture);
            }

            return bytes;
        }

        internal static string ToHex(byte[] value)
        {
            if (value == null)
            {
                return "<null>";
            }

            var builder = new StringBuilder(value.Length * 3);

            for (var index = 0; index < value.Length; index++)
            {
                if (index > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(value[index].ToString("X2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        internal static byte[] Response(
            ushort headerStatus,
            byte[] payload,
            uint headerReserved = 0)
        {
            var safePayload = payload ?? new byte[0];
            var response = new byte[8 + safePayload.Length];

            WriteUInt16(response, 0, headerStatus);
            WriteUInt16(response, 2, checked((ushort)safePayload.Length));
            WriteUInt32(response, 4, headerReserved);

            if (safePayload.Length > 0)
            {
                Buffer.BlockCopy(safePayload, 0, response, 8, safePayload.Length);
            }

            return response;
        }

        internal static byte[] Request(
            ushort command,
            ushort reference,
            byte[] payload)
        {
            var safePayload = payload ?? new byte[0];
            var request = new byte[8 + safePayload.Length];

            WriteUInt16(request, 0, command);
            WriteUInt16(request, 4, checked((ushort)safePayload.Length));
            WriteUInt16(request, 6, reference);

            if (safePayload.Length > 0)
            {
                Buffer.BlockCopy(safePayload, 0, request, 8, safePayload.Length);
            }

            return request;
        }

        internal static ushort ReadUInt16(byte[] buffer, int offset)
        {
            return (ushort)(buffer[offset] | (buffer[offset + 1] << 8));
        }

        internal static int ReadInt32(byte[] buffer, int offset)
        {
            return unchecked((int)ReadUInt32(buffer, offset));
        }

        internal static uint ReadUInt32(byte[] buffer, int offset)
        {
            return
                buffer[offset]
                | ((uint)buffer[offset + 1] << 8)
                | ((uint)buffer[offset + 2] << 16)
                | ((uint)buffer[offset + 3] << 24);
        }

        internal static ulong ReadUInt64(byte[] buffer, int offset)
        {
            return ReadUInt32(buffer, offset)
                | ((ulong)ReadUInt32(buffer, offset + 4) << 32);
        }

        internal static void WriteUInt16(byte[] buffer, int offset, ushort value)
        {
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
        }

        internal static void WriteInt16(byte[] buffer, int offset, short value)
        {
            WriteUInt16(buffer, offset, unchecked((ushort)value));
        }

        internal static void WriteUInt32(byte[] buffer, int offset, uint value)
        {
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 2] = (byte)(value >> 16);
            buffer[offset + 3] = (byte)(value >> 24);
        }

        internal static void WriteUInt64(byte[] buffer, int offset, ulong value)
        {
            WriteUInt32(buffer, offset, unchecked((uint)value));
            WriteUInt32(buffer, offset + 4, unchecked((uint)(value >> 32)));
        }

        internal static void WriteInt32(byte[] buffer, int offset, int value)
        {
            WriteUInt32(buffer, offset, unchecked((uint)value));
        }

        internal static void WriteDouble(byte[] buffer, int offset, double value)
        {
            var bits = unchecked((ulong)BitConverter.DoubleToInt64Bits(value));

            for (var index = 0; index < 8; index++)
            {
                buffer[offset + index] = (byte)(bits >> (index * 8));
            }
        }
    }
}

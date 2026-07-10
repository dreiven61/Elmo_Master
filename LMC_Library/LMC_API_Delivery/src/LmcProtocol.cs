using System;
using System.Net;
using System.Text;

namespace LasalMotionControlLib
{
    public enum LMC_DIRECTION : int
    {
        None = 0,
        Positive = 1,
        Shortest = 2,
        Negative = 3,
        Current = 4
    }

    public sealed class LMC_Response
    {
        private const ushort FunctionStatusCommandErrorMask = 0x0010;

        public byte[] Raw { get; internal set; }
        public ushort HeaderStatus { get; internal set; }
        public ushort PayloadLength { get; internal set; }
        public uint HeaderReserved { get; internal set; }
        public byte[] Payload { get; internal set; }
        public bool IsFrameValid { get; internal set; }
        public bool HasCommandResult { get; internal set; }
        public ushort CommandStatus { get; internal set; }
        public short ErrorId { get; internal set; }
        internal bool CommandStatusIsBitField { get; set; }

        public ushort Status
        {
            get { return HasCommandResult ? CommandStatus : HeaderStatus; }
        }

        public bool IsSuccess
        {
            get
            {
                return IsFrameValid
                    && HeaderStatus == 0
                    && (!HasCommandResult || IsCommandResultSuccess());
            }
        }

        private bool IsCommandResultSuccess()
        {
            if (ErrorId != 0)
            {
                return false;
            }

            if (CommandStatusIsBitField)
            {
                return (CommandStatus & FunctionStatusCommandErrorMask) == 0;
            }

            return CommandStatus == 0;
        }
    }

    public sealed class LMCCallbackEventArgs : EventArgs
    {
        internal LMCCallbackEventArgs(
            byte[] payload,
            IPEndPoint remoteEndPoint,
            DateTime receivedAtUtc)
        {
            Payload = payload ?? new byte[0];
            RemoteEndPoint = remoteEndPoint;
            ReceivedAtUtc = receivedAtUtc;
        }

        public byte[] Payload { get; private set; }
        public IPEndPoint RemoteEndPoint { get; private set; }
        public DateTime ReceivedAtUtc { get; private set; }
    }

    public sealed class LMCCallbackErrorEventArgs : EventArgs
    {
        internal LMCCallbackErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; private set; }
    }

    internal static class LMC_CommandId
    {
        internal const ushort RpcSessionInit = 0x8080;
        internal const ushort RpcCallbackRegistration = 0x405C;
        internal const ushort CloseConnection = 0x405D;

        internal const ushort GetAxisByName = 0x103C;
        internal const ushort GetGroupByName = 0x1042;

        internal const ushort Power = 0x2023;
        internal const ushort Reset = 0x2024;
        internal const ushort Stop = 0x2022;

        internal const ushort AxisInfo = 0x202B;
        internal const ushort ReadStatus = 0x2028;
        internal const ushort ReadPosition = 0x202E;

        internal const ushort MoveAbsolute = 0x209F;
        internal const ushort MoveRelative = 0x20A0;
        internal const ushort MoveVelocity = 0x20A2;

        internal const ushort GetMembers = 0x20D2;
        internal const ushort GroupStatus = 0x2045;
        internal const ushort GroupEnable = 0x2047;
        internal const ushort GroupDisable = 0x2048;
        internal const ushort GroupReset = 0x2049;
        internal const ushort GroupStop = 0x2085;
        internal const ushort GroupPosition = 0x2051;
        internal const ushort MoveLinear = 0x20A4;
    }

    internal static class LMC_Frame
    {
        internal const int HeaderSize = 8;

        private const int CommandOffset = 0;
        private const int ResponsePayloadLengthOffset = 2;
        private const int RequestPayloadLengthOffset = 4;
        private const int ReferenceOffset = 6;

        private const int NamePayloadLength = 0x50;
        private const int NameMaxBytes = 79;
        private const int MaxLinearAxes = 16;
        private const int IPv4ByteLength = 4;

        private const int AxisInfoModeOffset = HeaderSize;
        private const int AxisInfoEnableOffset = HeaderSize + 8;

        internal static byte[] CreateRequest(ushort command, ushort reference, ushort payloadLength)
        {
            var buffer = new byte[HeaderSize + payloadLength];

            WriteUInt16(buffer, CommandOffset, command);
            WriteUInt16(buffer, RequestPayloadLengthOffset, payloadLength);
            WriteUInt16(buffer, ReferenceOffset, reference);

            return buffer;
        }

        internal static ushort GetResponsePayloadLength(byte[] header)
        {
            return ReadUInt16(header, ResponsePayloadLengthOffset);
        }

        internal static byte[] RpcSessionInit()
        {
            return CreateRequest(LMC_CommandId.RpcSessionInit, 0, 1);
        }

        internal static byte[] RpcCallbackRegistration(
            uint eventMask,
            int callbackPort,
            byte[] localAddressBytes)
        {
            if (localAddressBytes == null || localAddressBytes.Length != IPv4ByteLength)
            {
                throw new ArgumentException("RPC callback registration requires an IPv4 address.");
            }

            var buffer = CreateRequest(LMC_CommandId.RpcCallbackRegistration, 0, 12);

            WriteUInt32(buffer, HeaderSize, eventMask);
            WriteInt32(buffer, HeaderSize + 4, callbackPort);
            Buffer.BlockCopy(localAddressBytes, 0, buffer, HeaderSize + 8, IPv4ByteLength);

            return buffer;
        }

        internal static byte[] CloseConnection()
        {
            return CreateRequest(LMC_CommandId.CloseConnection, 0, 1);
        }

        internal static byte[] LMCAxisGetByName(string axisName)
        {
            return NameLookup(LMC_CommandId.GetAxisByName, axisName);
        }

        internal static byte[] LMCGroupGetByName(string groupName)
        {
            return NameLookup(LMC_CommandId.GetGroupByName, groupName);
        }

        internal static byte[] LMCAxisInfo(ushort reference)
        {
            var buffer = CreateRequest(LMC_CommandId.AxisInfo, reference, 12);

            WriteInt32(buffer, AxisInfoModeOffset, 5);
            WriteInt32(buffer, AxisInfoEnableOffset, 1);

            return buffer;
        }

        internal static byte[] LMCAxisPower(ushort reference, bool enabled)
        {
            var buffer = CreateRequest(LMC_CommandId.Power, reference, 8);

            WriteInt32(buffer, HeaderSize, 1);
            buffer[HeaderSize + 4] = enabled ? (byte)1 : (byte)0;
            buffer[HeaderSize + 5] = 1;
            buffer[HeaderSize + 7] = 1;

            return buffer;
        }

        internal static byte[] LMCAxisReset(ushort reference)
        {
            return ExecuteOnly(LMC_CommandId.Reset, reference);
        }

        internal static byte[] LMCAxisReadStatus(ushort reference)
        {
            var buffer = CreateRequest(LMC_CommandId.ReadStatus, reference, 8);

            WriteInt32(buffer, HeaderSize, reference);
            WriteInt32(buffer, HeaderSize + 4, 1);

            return buffer;
        }

        internal static byte[] LMCAxisReadPosition(ushort reference)
        {
            return CreateRequest(LMC_CommandId.ReadPosition, reference, 1);
        }

        internal static byte[] LMCAxisStop(
            ushort reference,
            int deceleration,
            int jerk)
        {
            return StopWithMotionParameters(LMC_CommandId.Stop, reference, deceleration, jerk);
        }

        internal static byte[] LMCAxisMoveAbsolute(
            ushort reference,
            int position,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction)
        {
            return LMCAxisMove(
                LMC_CommandId.MoveAbsolute,
                reference,
                position,
                velocity,
                acceleration,
                deceleration,
                jerk,
                direction);
        }

        internal static byte[] LMCAxisMoveRelative(
            ushort reference,
            int distance,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction)
        {
            return LMCAxisMove(
                LMC_CommandId.MoveRelative,
                reference,
                distance,
                velocity,
                acceleration,
                deceleration,
                jerk,
                direction);
        }

        internal static byte[] LMCAxisMoveVelocity(
            ushort reference,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction)
        {
            if (deceleration != 0)
            {
                throw new ArgumentException(
                    "LASAL MoveEndless has no deceleration input; pass 0 and use Stop for controlled deceleration.",
                    "deceleration");
            }

            var signedVelocity = ApplyVelocityDirection(velocity, direction);
            var buffer = CreateRequest(LMC_CommandId.MoveVelocity, reference, 24);

            WriteInt32(buffer, HeaderSize, signedVelocity);
            WriteInt32(buffer, HeaderSize + 4, acceleration);
            WriteInt32(buffer, HeaderSize + 8, deceleration);
            WriteInt32(buffer, HeaderSize + 12, jerk);
            WriteInt32(buffer, HeaderSize + 16, (int)direction);
            WriteInt32(buffer, HeaderSize + 20, 1);

            return buffer;
        }

        internal static byte[] LMCGroupGetMembersInfo(ushort reference)
        {
            return ExecuteOnly(LMC_CommandId.GetMembers, reference);
        }

        internal static byte[] LMCGroupEnable(ushort reference)
        {
            return ExecuteOnly(LMC_CommandId.GroupEnable, reference);
        }

        internal static byte[] LMCGroupDisable(ushort reference)
        {
            return ExecuteOnly(LMC_CommandId.GroupDisable, reference);
        }

        internal static byte[] LMCGroupReset(ushort reference)
        {
            return ExecuteOnly(LMC_CommandId.GroupReset, reference);
        }

        internal static byte[] LMCGroupReadStatus(ushort reference)
        {
            var buffer = CreateRequest(LMC_CommandId.GroupStatus, reference, 8);

            WriteInt32(buffer, HeaderSize, reference);
            WriteInt32(buffer, HeaderSize + 4, 1);

            return buffer;
        }

        internal static byte[] LMCGroupStop(
            ushort reference,
            int deceleration,
            int jerk)
        {
            return StopWithMotionParameters(LMC_CommandId.GroupStop, reference, deceleration, jerk);
        }

        internal static byte[] LMCGroupMoveLinearAbsolute(
            ushort reference,
            int[] position,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk)
        {
            var buffer = CreateRequest(LMC_CommandId.MoveLinear, reference, 96);

            WriteLinearPositions(buffer, position);
            WriteInt32(buffer, HeaderSize + 64, velocity);
            WriteInt32(buffer, HeaderSize + 68, acceleration);
            WriteInt32(buffer, HeaderSize + 72, deceleration);
            WriteInt32(buffer, HeaderSize + 76, jerk);
            WriteInt32(buffer, HeaderSize + 80, 0);
            WriteInt32(buffer, HeaderSize + 84, 0);
            WriteInt32(buffer, HeaderSize + 88, 1);
            WriteInt32(buffer, HeaderSize + 92, 1);

            return buffer;
        }

        private static byte[] NameLookup(ushort command, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(
                    "A non-empty LASAL object name is required.",
                    "name");
            }

            for (var index = 0; index < name.Length; index++)
            {
                if (name[index] < 0x20 || name[index] > 0x7E)
                {
                    throw new ArgumentException(
                        "LASAL object names must contain printable ASCII characters only.",
                        "name");
                }
            }

            if (name.Length > NameMaxBytes)
            {
                throw new ArgumentOutOfRangeException(
                    "name",
                    "LASAL object names must be 79 ASCII bytes or fewer.");
            }

            var buffer = CreateRequest(command, 0, NamePayloadLength);
            var encodedName = Encoding.ASCII.GetBytes(name);

            Buffer.BlockCopy(encodedName, 0, buffer, HeaderSize, encodedName.Length);
            return buffer;
        }

        private static byte[] ExecuteOnly(ushort command, ushort reference)
        {
            var buffer = CreateRequest(command, reference, 1);
            buffer[HeaderSize] = 1;
            return buffer;
        }

        private static byte[] LMCAxisMove(
            ushort command,
            ushort reference,
            int positionOrDistance,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction)
        {
            if (direction != LMC_DIRECTION.Shortest)
            {
                throw new ArgumentOutOfRangeException(
                    "direction",
                    "The current LASAL absolute/relative contract supports Shortest only; relative direction is selected by the signed distance.");
            }

            var buffer = CreateRequest(command, reference, 32);

            WriteInt32(buffer, HeaderSize, positionOrDistance);
            WriteInt32(buffer, HeaderSize + 4, velocity);
            WriteInt32(buffer, HeaderSize + 8, acceleration);
            WriteInt32(buffer, HeaderSize + 12, deceleration);
            WriteInt32(buffer, HeaderSize + 16, jerk);
            WriteInt32(buffer, HeaderSize + 20, (int)direction);
            WriteInt32(buffer, HeaderSize + 24, 1);
            WriteInt32(buffer, HeaderSize + 28, 1);

            return buffer;
        }

        private static int ApplyVelocityDirection(
            int velocity,
            LMC_DIRECTION direction)
        {
            if (direction == LMC_DIRECTION.Positive)
            {
                if (velocity == int.MinValue)
                {
                    throw new ArgumentOutOfRangeException(
                        "velocity",
                        "DINT minimum cannot be converted to a positive velocity.");
                }

                return velocity < 0 ? -velocity : velocity;
            }

            if (direction == LMC_DIRECTION.Negative)
            {
                return velocity > 0 ? -velocity : velocity;
            }

            throw new ArgumentOutOfRangeException(
                "direction",
                "LASAL MoveVelocity supports Positive or Negative direction only.");
        }

        internal static ushort ReadUInt16(byte[] buffer, int offset)
        {
            return (ushort)(buffer[offset] | (buffer[offset + 1] << 8));
        }

        internal static uint ReadUInt32(byte[] buffer, int offset)
        {
            return
                buffer[offset]
                | ((uint)buffer[offset + 1] << 8)
                | ((uint)buffer[offset + 2] << 16)
                | ((uint)buffer[offset + 3] << 24);
        }

        internal static int ReadInt32(byte[] buffer, int offset)
        {
            return unchecked((int)ReadUInt32(buffer, offset));
        }

        internal static void WriteUInt16(byte[] buffer, int offset, ushort value)
        {
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
        }

        internal static void WriteUInt32(byte[] buffer, int offset, uint value)
        {
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 2] = (byte)(value >> 16);
            buffer[offset + 3] = (byte)(value >> 24);
        }

        internal static void WriteInt32(byte[] buffer, int offset, int value)
        {
            var unsignedValue = unchecked((uint)value);

            buffer[offset] = (byte)unsignedValue;
            buffer[offset + 1] = (byte)(unsignedValue >> 8);
            buffer[offset + 2] = (byte)(unsignedValue >> 16);
            buffer[offset + 3] = (byte)(unsignedValue >> 24);
        }

        private static byte[] StopWithMotionParameters(
            ushort command,
            ushort reference,
            int deceleration,
            int jerk)
        {
            var buffer = CreateRequest(command, reference, 16);

            WriteInt32(buffer, HeaderSize, deceleration);
            WriteInt32(buffer, HeaderSize + 4, jerk);
            WriteInt32(buffer, HeaderSize + 8, 1);
            WriteInt32(buffer, HeaderSize + 12, 1);

            return buffer;
        }

        private static void WriteLinearPositions(
            byte[] buffer,
            int[] position)
        {
            for (var axisIndex = 0; axisIndex < MaxLinearAxes; axisIndex++)
            {
                var internalPosition = GetPosition(position, axisIndex);
                var offset = HeaderSize + axisIndex * 4;

                WriteInt32(buffer, offset, internalPosition);
            }
        }

        private static int GetPosition(int[] position, int axisIndex)
        {
            if (position == null || axisIndex >= position.Length)
            {
                return 0;
            }

            return position[axisIndex];
        }
    }
}

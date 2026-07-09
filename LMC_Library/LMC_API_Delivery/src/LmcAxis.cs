using System;

namespace LasalMotionControlLib
{
    public class LMCSingleAxis
    {
        private const int LookupReferenceOffset = 12;
        private const int MinimumLookupResponseLength = 14;
        private const int ResponseValueOffset = 8;
        private const int MinimumValueResponseLength = 12;

        private readonly LMCConnection connection;

        public string AxisName { get; private set; }
        public ushort AxisReference { get; private set; }

        public LMCSingleAxis(LMCConnection connection, string axisName)
        {
            this.connection = connection ?? throw new ArgumentNullException(nameof(connection));

            AxisName = axisName;
            AxisReference = ResolveAxisReference(axisName);

            connection.Exchange(LMC_Frame.AxisInfo(AxisReference));
        }

        public LMC_Response PowerOn()
        {
            return Send(LMC_Frame.Power(AxisReference, true));
        }

        public LMC_Response PowerOff()
        {
            return Send(LMC_Frame.Power(AxisReference, false));
        }

        public LMC_Response LMC_PowerCmd(bool enable)
        {
            return enable ? PowerOn() : PowerOff();
        }

        public LMC_Response Reset()
        {
            return Send(LMC_Frame.Simple(LMC_CommandId.Reset, AxisReference));
        }

        public LMC_Response LMC_Reset()
        {
            return Reset();
        }

        public LMC_Response Stop(int deceleration, int jerk)
        {
            return Send(LMC_Frame.Stop(AxisReference, deceleration, jerk));
        }

        public LMC_Response LMC_StopCmd(int deceleration, int jerk)
        {
            return Stop(deceleration, jerk);
        }

        public LMC_Response MoveAbsoluteEx(
            int position,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction = LMC_DIRECTION.Shortest)
        {
            return Move(
                LMC_CommandId.MoveAbsolute,
                position,
                velocity,
                acceleration,
                deceleration,
                jerk,
                direction);
        }

        public LMC_Response LMC_MoveAbsoluteExCmd(
            int position,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction = LMC_DIRECTION.Shortest)
        {
            return MoveAbsoluteEx(
                position,
                velocity,
                acceleration,
                deceleration,
                jerk,
                direction);
        }

        public LMC_Response MoveRelativeEx(
            int distance,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction = LMC_DIRECTION.Shortest)
        {
            return Move(
                LMC_CommandId.MoveRelative,
                distance,
                velocity,
                acceleration,
                deceleration,
                jerk,
                direction);
        }

        public LMC_Response LMC_MoveRelativeExCmd(
            int distance,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction = LMC_DIRECTION.Shortest)
        {
            return MoveRelativeEx(
                distance,
                velocity,
                acceleration,
                deceleration,
                jerk,
                direction);
        }

        public LMC_Response MoveVelocityEx(
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction)
        {
            return Send(
                LMC_Frame.Velocity(
                    AxisReference,
                    velocity,
                    acceleration,
                    deceleration,
                    jerk,
                    direction));
        }

        public LMC_Response LMC_MoveVelocityExCmd(
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction)
        {
            return MoveVelocityEx(
                velocity,
                acceleration,
                deceleration,
                jerk,
                direction);
        }

        public uint ReadStatus()
        {
            LMC_Response response;
            return ReadStatus(out response);
        }

        public uint ReadStatus(out LMC_Response response)
        {
            var raw = connection.Exchange(LMC_Frame.ReadStatus(AxisReference));
            response = new LMC_Response { Raw = raw };

            if (raw.Length < MinimumValueResponseLength)
            {
                return 0;
            }

            return LMC_Frame.ReadUInt32(raw, ResponseValueOffset);
        }

        public uint LMC_ReadStatusCmd(out LMC_Response response)
        {
            return ReadStatus(out response);
        }

        public int GetActualPosition()
        {
            LMC_Response response;
            return GetActualPosition(out response);
        }

        public int GetActualPosition(out LMC_Response response)
        {
            var raw = connection.Exchange(LMC_Frame.ReadPosition(AxisReference));
            response = new LMC_Response { Raw = raw };

            if (raw.Length < MinimumValueResponseLength)
            {
                return 0;
            }

            return LMC_Frame.ReadInt32(raw, ResponseValueOffset);
        }

        public int LMC_ReadActualPositionCmd(out LMC_Response response)
        {
            return GetActualPosition(out response);
        }

        private ushort ResolveAxisReference(string axisName)
        {
            var raw = connection.Exchange(
                LMC_Frame.Name(LMC_CommandId.GetAxisByName, axisName));

            if (raw.Length < MinimumLookupResponseLength)
            {
                throw new InvalidOperationException("Invalid axis lookup response.");
            }

            return LMC_Frame.ReadUInt16(raw, LookupReferenceOffset);
        }

        private LMC_Response Move(
            ushort command,
            int positionOrDistance,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction)
        {
            return Send(
                LMC_Frame.AxisMove(
                    command,
                    AxisReference,
                    positionOrDistance,
                    velocity,
                    acceleration,
                    deceleration,
                    jerk,
                    direction));
        }

        private LMC_Response Send(byte[] request)
        {
            return LMCConnection.Parse(connection.Exchange(request));
        }
    }

    public sealed class LMCAxis : LMCSingleAxis
    {
        public LMCAxis(LMCConnection connection, string axisName)
            : base(connection, axisName)
        {
        }
    }
}

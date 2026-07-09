using System;

namespace LasalMotionControlLib
{
    public class LMCSingleAxis
    {
        private readonly LMCConnection connection;

        public string AxisName { get; private set; }
        public ushort AxisReference { get; private set; }

        public LMCSingleAxis(LMCConnection connection, string axisName)
        {
            this.connection = connection ?? throw new ArgumentNullException(nameof(connection));

            AxisName = axisName;
            AxisReference = ResolveAxisReference(axisName);

            connection.Exchange(LMC_Frame.LMCAxisInfo(AxisReference));
        }

        public LMC_Response PowerOn()
        {
            return SendPower(true);
        }

        public LMC_Response PowerOff()
        {
            return SendPower(false);
        }

        public LMC_Response LMC_PowerCmd(bool enable)
        {
            return SendPower(enable);
        }

        public LMC_Response Reset()
        {
            return SendReset();
        }

        public LMC_Response LMC_Reset()
        {
            return SendReset();
        }

        public LMC_Response Stop(int deceleration, int jerk)
        {
            return SendStop(deceleration, jerk);
        }

        public LMC_Response LMC_StopCmd(int deceleration, int jerk)
        {
            return SendStop(deceleration, jerk);
        }

        public LMC_Response MoveAbsoluteEx(
            int position,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction = LMC_DIRECTION.Shortest)
        {
            return SendMoveAbsolute(position, velocity, acceleration, deceleration, jerk, direction);
        }

        public LMC_Response LMC_MoveAbsoluteExCmd(
            int position,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction = LMC_DIRECTION.Shortest)
        {
            return SendMoveAbsolute(position, velocity, acceleration, deceleration, jerk, direction);
        }

        public LMC_Response MoveRelativeEx(
            int distance,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction = LMC_DIRECTION.Shortest)
        {
            return SendMoveRelative(distance, velocity, acceleration, deceleration, jerk, direction);
        }

        public LMC_Response LMC_MoveRelativeExCmd(
            int distance,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction = LMC_DIRECTION.Shortest)
        {
            return SendMoveRelative(distance, velocity, acceleration, deceleration, jerk, direction);
        }

        public LMC_Response MoveVelocityEx(
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction)
        {
            return SendMoveVelocity(velocity, acceleration, deceleration, jerk, direction);
        }

        public LMC_Response LMC_MoveVelocityExCmd(
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction)
        {
            return SendMoveVelocity(velocity, acceleration, deceleration, jerk, direction);
        }

        public uint ReadStatus()
        {
            LMC_Response response;
            return ReadStatusValue(out response);
        }

        public uint ReadStatus(out LMC_Response response)
        {
            return ReadStatusValue(out response);
        }

        public uint LMC_ReadStatusCmd(out LMC_Response response)
        {
            return ReadStatusValue(out response);
        }

        public int GetActualPosition()
        {
            LMC_Response response;
            return ReadActualPositionValue(out response);
        }

        public int GetActualPosition(out LMC_Response response)
        {
            return ReadActualPositionValue(out response);
        }

        public int LMC_ReadActualPositionCmd(out LMC_Response response)
        {
            return ReadActualPositionValue(out response);
        }

        private ushort ResolveAxisReference(string axisName)
        {
            ushort axisReference;

            if (!LMCConnection.TryParseLookupReference(
                connection.Exchange(LMC_Frame.LMCAxisGetByName(axisName)),
                out _,
                out axisReference))
            {
                throw new InvalidOperationException("Invalid axis lookup response.");
            }

            return axisReference;
        }

        private LMC_Response SendPower(bool enable)
        {
            return Send(LMC_Frame.LMCAxisPower(AxisReference, enable));
        }

        private LMC_Response SendReset()
        {
            return Send(LMC_Frame.LMCAxisReset(AxisReference));
        }

        private LMC_Response SendStop(int deceleration, int jerk)
        {
            return Send(LMC_Frame.LMCAxisStop(AxisReference, deceleration, jerk));
        }

        private LMC_Response SendMoveAbsolute(
            int position,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction)
        {
            return Send(
                LMC_Frame.LMCAxisMoveAbsolute(
                    AxisReference,
                    position,
                    velocity,
                    acceleration,
                    deceleration,
                    jerk,
                    direction));
        }

        private LMC_Response SendMoveRelative(
            int distance,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction)
        {
            return Send(
                LMC_Frame.LMCAxisMoveRelative(
                    AxisReference,
                    distance,
                    velocity,
                    acceleration,
                    deceleration,
                    jerk,
                    direction));
        }

        private LMC_Response SendMoveVelocity(
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction)
        {
            return Send(
                LMC_Frame.LMCAxisMoveVelocity(
                    AxisReference,
                    velocity,
                    acceleration,
                    deceleration,
                    jerk,
                    direction));
        }

        private uint ReadStatusValue(out LMC_Response response)
        {
            return LMCConnection.ParseUInt32Value(
                connection.Exchange(LMC_Frame.LMCAxisReadStatus(AxisReference)),
                out response);
        }

        private int ReadActualPositionValue(out LMC_Response response)
        {
            return LMCConnection.ParseInt32Value(
                connection.Exchange(LMC_Frame.LMCAxisReadPosition(AxisReference)),
                out response);
        }

        private LMC_Response Send(byte[] request)
        {
            return LMCConnection.ParseAcknowledgement(connection.Exchange(request));
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

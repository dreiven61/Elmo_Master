using System;

namespace LmcLasalMotionApi
{
    public sealed class LMCAxis
    {
        private const int LookupReferenceOffset = 12;
        private const int MinimumLookupResponseLength = 14;
        private const int ResponseValueOffset = 8;
        private const int MinimumValueResponseLength = 12;

        private readonly LMCConnection connection;
        private readonly LMC_UnitConverter units;

        public string AxisName { get; private set; }
        public ushort AxisReference { get; private set; }

        public LMCAxis(
            LMCConnection connection,
            string axisName,
            LMC_UnitConverter units = null)
        {
            this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
            this.units = units ?? new LMC_UnitConverter();

            AxisName = axisName;
            AxisReference = ResolveAxisReference(axisName);

            connection.Exchange(LMC_Frame.AxisInfo(AxisReference));
        }

        public LMC_Response LMC_PowerCmd(bool enable)
        {
            return Send(LMC_Frame.Power(AxisReference, enable));
        }

        public LMC_Response LMC_Reset()
        {
            return Send(LMC_Frame.Simple(LMC_CommandId.Reset, AxisReference));
        }

        public LMC_Response LMC_StopCmd(double deceleration, double jerk)
        {
            return Send(
                LMC_Frame.Stop(
                    AxisReference,
                    units.DecelerationToInternal(deceleration),
                    units.JerkToInternal(jerk)));
        }

        public LMC_Response LMC_MoveAbsoluteExCmd(
            double position,
            double velocity,
            double acceleration,
            double deceleration,
            double jerk,
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

        public LMC_Response LMC_MoveRelativeExCmd(
            double distance,
            double velocity,
            double acceleration,
            double deceleration,
            double jerk,
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

        public LMC_Response LMC_MoveVelocityExCmd(
            double velocity,
            double acceleration,
            double deceleration,
            double jerk,
            LMC_DIRECTION direction)
        {
            return Send(
                LMC_Frame.Velocity(
                    AxisReference,
                    units.VelocityToInternal(velocity),
                    units.AccelerationToInternal(acceleration),
                    units.DecelerationToInternal(deceleration),
                    units.JerkToInternal(jerk),
                    direction));
        }

        public uint LMC_ReadStatusCmd(out LMC_Response response)
        {
            var raw = connection.Exchange(LMC_Frame.ReadStatus(AxisReference));
            response = new LMC_Response { Raw = raw };

            if (raw.Length < MinimumValueResponseLength)
            {
                return 0;
            }

            return LMC_Frame.ReadUInt32(raw, ResponseValueOffset);
        }

        public double LMC_ReadActualPositionCmd(out LMC_Response response)
        {
            var raw = connection.Exchange(LMC_Frame.ReadPosition(AxisReference));
            response = new LMC_Response { Raw = raw };

            if (raw.Length < MinimumValueResponseLength)
            {
                return 0;
            }

            var internalPosition = LMC_Frame.ReadInt32(raw, ResponseValueOffset);
            return units.InternalToPosition(internalPosition);
        }

        private ushort ResolveAxisReference(string axisName)
        {
            var raw = connection.Exchange(
                LMC_Frame.Name(LMC_CommandId.GetAxisByName, axisName));

            if (raw.Length < MinimumLookupResponseLength)
            {
                throw new InvalidOperationException("Invalid LASAL axis lookup response.");
            }

            return LMC_Frame.ReadUInt16(raw, LookupReferenceOffset);
        }

        private LMC_Response Move(
            ushort command,
            double positionOrDistance,
            double velocity,
            double acceleration,
            double deceleration,
            double jerk,
            LMC_DIRECTION direction)
        {
            return Send(
                LMC_Frame.AxisMove(
                    command,
                    AxisReference,
                    units.PositionToInternal(positionOrDistance),
                    units.VelocityToInternal(velocity),
                    units.AccelerationToInternal(acceleration),
                    units.DecelerationToInternal(deceleration),
                    units.JerkToInternal(jerk),
                    direction));
        }

        private LMC_Response Send(byte[] request)
        {
            return LMCConnection.Parse(connection.Exchange(request));
        }
    }
}

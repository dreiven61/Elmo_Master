using LasalMotionControlLib;

public static class BasicUsage
{
    public static void Run()
    {
        var units = new UnitConverter(
            Units.MM,
            Units.MMPSEC,
            Units.MMPSEC2,
            Units.MMPSEC2,
            Units.MMPSEC2);

        var position = units.PositionToInternal(1.0);
        var velocity = units.VelocityToInternal(1.0);
        var acceleration = units.AccelerationToInternal(1.0);
        var deceleration = units.DecelerationToInternal(1.0);
        var jerk = units.JerkToInternal(0.0);

        using (var connection = new LMCConnection())
        {
            connection.RpcInitConnection(
                "10.10.150.1", 4000, "10.10.150.14");

            var axis = new MMCSingleAxis(connection, "a01");
            axis.PowerOn();

            // API methods receive already-converted LASAL/internal DINT values.
            axis.MoveAbsoluteEx(position, velocity, acceleration, deceleration, jerk);

            var actualPosition = axis.GetActualPosition();
            var actualPositionMm = units.InternalToPosition(actualPosition);
            System.Console.WriteLine("Actual position mm = " + actualPositionMm);

            var group = new MMCGroupAxis(connection, "v01");
            group.GroupEnable();
            group.MoveLinearAbsoluteEx(
                new[] { position, position, position, position },
                velocity,
                acceleration,
                deceleration,
                jerk);
        }
    }
}

using System;
using LasalMotionControlLib;

public static class BasicUsage
{
    // User application owns unit constants and conversion policy.
    private const int MM = 10000;
    private const int MMPSEC = 10000;
    private const int MMPSEC2 = 1;

    public static void Run()
    {
        var position = ToDint(1.0, MM);
        var velocity = ToDint(1.0, MMPSEC);
        var acceleration = ToDint(1.0, MMPSEC2);
        var deceleration = ToDint(1.0, MMPSEC2);
        var jerk = ToDint(0.0, MMPSEC2);

        using (var connection = new LMCConnection())
        {
            connection.RpcInitConnection(
                "10.10.150.1",
                4000,
                "10.10.150.14",
                LMCConnection.DefaultCallbackPort,
                LMCConnection.DefaultEventMask);

            var axis = new LMCSingleAxis(connection, "a01");
            axis.PowerOn();

            // API methods receive already-converted LASAL/internal DINT values.
            axis.MoveAbsoluteEx(position, velocity, acceleration, deceleration, jerk);

            var actualPosition = axis.GetActualPosition();
            var actualPositionMm = FromDint(actualPosition, MM);
            System.Console.WriteLine("Actual position mm = " + actualPositionMm);

            var group = new LMCGroupAxis(connection, "v01");
            group.GroupEnable();
            group.MoveLinearAbsoluteEx(
                new[] { position, position, position, position },
                velocity,
                acceleration,
                deceleration,
                jerk);
        }
    }

    private static int ToDint(double value, int unit)
    {
        return checked((int)Math.Round(value * unit, MidpointRounding.AwayFromZero));
    }

    private static double FromDint(int value, int unit)
    {
        return (double)value / unit;
    }
}

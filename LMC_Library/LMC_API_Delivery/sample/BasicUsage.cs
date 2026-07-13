using System;
using LasalMotionControlLib;

public static class BasicUsage
{
    public static void Run()
    {
        var options = new LMCConnectionOptions
        {
            ConnectTimeoutMilliseconds = 3000,
            ReceiveTimeoutMilliseconds = 3000,
            SendTimeoutMilliseconds = 3000,
            ValidateCallbackSourceAddress = true
        };

        using (var connection = new LMCConnection(options))
        {
            connection.ConnectionStateChanged += delegate(
                object sender,
                LMCConnectionStateChangedEventArgs e)
            {
                Console.WriteLine(
                    "Connection "
                    + e.PreviousState
                    + " -> "
                    + e.CurrentState);
            };

            connection.CallbackReceived += delegate(object sender, LMCCallbackEventArgs e)
            {
                // Raw datagram only. Do not infer a typed event until an actual
                // LASAL callback payload contract is captured.
                Console.WriteLine(
                    "Callback bytes = "
                    + e.Payload.Length
                    + ", remote = "
                    + e.RemoteEndPoint);
            };

            connection.RpcInitConnection(
                "10.10.150.1",
                4000,
                "10.10.150.14",
                LMCConnection.DefaultCallbackPort,
                LMCConnection.DefaultEventMask);

            var axis = new LMCSingleAxis(connection, "_LMCAxis1");

            // This default sample does not enable the drive. After application
            // safety checks, call PowerOn(), verify its response, poll
            // ReadStatus with the approved ready predicate, and only then call
            // MoveAfterPowerReadyConfirmed(axis). Always PowerOff/Stop according
            // to the machine shutdown policy before closing the connection.
            Console.WriteLine(
                "RPC and axis lookup completed. Drive remains disabled.");

            // MoveAfterPowerReadyConfirmed(axis);
        }
    }

    public static void MoveAfterPowerReadyConfirmed(LMCSingleAxis axis)
    {
        if (axis == null)
        {
            throw new ArgumentNullException("axis");
        }

        // Caller selects a UNIT and converts before calling the API.
        var position = ToDint(1.0, LMC_Units.DEG);
        var velocity = ToDint(1.0, LMC_Units.DEG);
        var acceleration = ToDint(1.0, LMC_Units.DEG);
        var deceleration = ToDint(1.0, LMC_Units.DEG);
        var jerk = 0; // Nonzero jerk conversion requires an approved profile.

        EnsureSuccess(
            "MoveAbsoluteEx",
            axis.MoveAbsoluteEx(
                position,
                velocity,
                acceleration,
                deceleration,
                jerk));

        LMC_Response positionResponse;
        var actualPosition = axis.GetActualPosition(out positionResponse);
        EnsureSuccess("GetActualPosition", positionResponse);

        var actualPositionDeg = FromDint(actualPosition, LMC_Units.DEG);
        Console.WriteLine("Actual position deg = " + actualPositionDeg);

        // GroupEnable, GroupDisable, GroupReadStatus, and group-member lookup
        // have an active LASAL path but are omitted from this axis-only sample.
        // GroupReset, GroupStop, MoveLinearAbsoluteEx,
        // GroupReadActualPosition, and SetKinTransformCartesian4Axis have public
        // PC methods but currently return unsupported error -5 from LASAL.
    }

    private static void EnsureSuccess(string operation, LMC_Response response)
    {
        if (response == null || !response.IsSuccess)
        {
            throw new InvalidOperationException(
                operation
                + " failed. FrameValid="
                + (response != null && response.IsFrameValid)
                + ", Status="
                + (response == null ? 0 : response.Status)
                + ", ErrorId="
                + (response == null ? 0 : response.ErrorId));
        }
    }

    private static int ToDint(double value, int unit)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentOutOfRangeException("value");
        }

        return checked((int)Math.Round(value * unit, MidpointRounding.AwayFromZero));
    }

    private static double FromDint(int value, int unit)
    {
        if (unit == 0)
        {
            throw new ArgumentOutOfRangeException("unit");
        }

        return (double)value / unit;
    }
}

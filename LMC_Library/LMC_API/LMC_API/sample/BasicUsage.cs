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
            connection.ConnectionStateChanged += delegate(object sender, LMCConnectionStateChangedEventArgs e)
            {
                Console.WriteLine("Connection: " + e.PreviousState + " -> " + e.CurrentState);
            };

            connection.CallbackReceived += delegate(object sender, LMCCallbackEventArgs e)
            {
                // Payload is raw until an actual callback datagram contract is captured.
                Console.WriteLine("Raw callback bytes=" + e.Payload.Length + ", remote=" + e.RemoteEndPoint);
            };

            connection.RpcInitConnection(
                "10.10.150.1",
                4000,
                "10.10.150.14",
                0,
                LMCConnection.DefaultEventMask);

            var axisX = new LMCSingleAxis(connection, "_LMCAxis1");

            // The caller, not the DLL, multiplies by the PLC UNIT.
            var oneDegreeRaw = ToDint(1.0, LMC_Units.DEG);
            Console.WriteLine("1 degree raw DINT=" + oneDegreeRaw);

            // Keep motion disabled until machine safety checks, the LASAL RtWork
            // design, IDE build and PLC packet verification are complete.
            Console.WriteLine("RPC and axis lookup completed; no motion command sent.");

            // After 0x2051/0x20E7 LASAL handlers and safety validation:
            // ConfigureFourAxisGroup(connection, axisX);
        }
    }

    private static void ConfigureFourAxisGroup(
        LMCConnection connection,
        LMCSingleAxis axisX)
    {
        var axisY = new LMCSingleAxis(connection, "_LMCAxis2");
        var axisZ = new LMCSingleAxis(connection, "_LMCAxis3");
        var axisU = new LMCSingleAxis(connection, "_LMCAxis4");
        var group = new LMCGroupAxis(connection, "_LMCRobotBase1");

        EnsureSuccess(
            "SetKinTransformCartesian4Axis",
            group.SetKinTransformCartesian4Axis(axisX, axisY, axisZ, axisU));

        var position = group.GroupReadActualPosition(LMC_COORD_SYSTEM.Mcs);
        if (!position.IsSuccess)
        {
            throw new InvalidOperationException(
                "GroupReadActualPosition failed. Status="
                + position.FunctionStatus
                + ", ErrorId="
                + position.ErrorId);
        }

        var moveOptions = new LMCGroupMotionOptions
        {
            CoordinateSystem = LMC_COORD_SYSTEM.Mcs,
            TransitionMode = LMC_GROUP_TRANSITION_MODE.ExactStop,
            BufferMode = LMC_BUFFER_MODE.Aborting,
            Execute = true
        };

        // Example only: issue after Power/GroupEnable and approved safety checks.
        EnsureSuccess(
            "MoveLinearAbsoluteEx",
            group.MoveLinearAbsoluteEx(
                new[] { ToDint(1.0, LMC_Units.DEG), 0, 0, 0 },
                ToDint(1.0, LMC_Units.DEG),
                ToDint(1.0, LMC_Units.DEG),
                ToDint(1.0, LMC_Units.DEG),
                0,
                moveOptions));
    }

    private static void EnsureSuccess(string operation, LMC_Response response)
    {
        if (response == null || !response.IsSuccess)
        {
            throw new InvalidOperationException(
                operation
                + " failed. Status="
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
}

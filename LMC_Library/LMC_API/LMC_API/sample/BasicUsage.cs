using LmcMotionApi;

public static class BasicUsage
{
    public static void Run()
    {
        using (var connection = new LMCConnection())
        {
            connection.LMC_RpcInitConnection(
                "192.168.99.20", 5000,
                "192.168.99.14", 5003,
                0xFFFFFFFF, true);

            // Single axis: API inputs stay in PMAS/controller counts.
            // The DLL converts motion fields to LASAL internal units before TCP send.
            var axis = new LMCAxis(connection, "a01");
            axis.LMC_PowerCmd(true);
            axis.LMC_MoveAbsoluteExCmd(8388608, 8388608, 8388608, 8388608, 8388608);

            // Four-axis group: a01/a02/a03/a04 = X/Y/Z/U.
            // 8388608 count is sent as 3600000 LASAL internal.
            var group = new LMCGroup(connection, "v01");
            var axes = new[]
            {
                new LMCAxis(connection, "a01"),
                new LMCAxis(connection, "a02"),
                new LMCAxis(connection, "a03"),
                new LMCAxis(connection, "a04")
            };

            group.LMC_PowerMembers(axes, true);
            group.LMC_SetKinTransformCartesian4Axis();
            group.LMC_GroupEnableCmd();
            group.LMC_MoveLinearAbsoluteExCmd(
                new[] { 8388608.0, 8388608.0, 8388608.0, 8388608.0 },
                8388608, 8388608, 8388608, 8388608);

            // Release coordinated control before using a single axis again.
            group.LMC_GroupStopCmd(8388608, 8388608);
            group.LMC_GroupDisableCmd();
            group.LMC_PowerMembers(axes, false);
        }
    }
}

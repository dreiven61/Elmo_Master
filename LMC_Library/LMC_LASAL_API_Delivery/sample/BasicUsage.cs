using LmcLasalMotionApi;

public static class BasicUsage
{
    public static void Run()
    {
        var units = new LMC_UnitConverter();

        using (var connection = new LMCConnection())
        {
            connection.LMC_RpcInitConnection(
                "10.10.150.1", 4000, "10.10.150.14");

            var axis = new LMCAxis(connection, "a01", units);
            axis.LMC_PowerCmd(true);

            // 1 rev is sent as 3,600,000 LASAL internal DINT.
            axis.LMC_MoveAbsoluteExCmd(1, 1, 1, 1, 0);

            var group = new LMCGroup(connection, "v01", units);
            group.LMC_GroupEnableCmd();
            group.LMC_MoveLinearAbsoluteExCmd(
                new[] { 1.0, 1.0, 1.0, 1.0 },
                1, 1, 1, 0);
        }
    }
}

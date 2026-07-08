using LmcLasalMotionApi;

public static class BasicUsage
{
    public static void Run()
    {
        var units = new LMC_UnitConverter(
            LMC_Units.LMC_MM,
            LMC_Units.LMC_MMPSEC,
            LMC_Units.LMC_MMPSEC2,
            LMC_Units.LMC_MMPSEC2,
            LMC_Units.LMC_MMPSEC2);

        using (var connection = new LMCConnection())
        {
            connection.LMC_RpcInitConnection(
                "10.10.150.1", 4000, "10.10.150.14");

            var axis = new LMCAxis(connection, "a01", units);
            axis.LMC_PowerCmd(true);

            // Default profile: 1.0 mm -> 10000, 1.0 mm/s -> 10000, 1.0 mm/s2 -> 1.
            axis.LMC_MoveAbsoluteExCmd(1, 1, 1, 1, 0);

            var group = new LMCGroup(connection, "v01", units);
            group.LMC_GroupEnableCmd();
            group.LMC_MoveLinearAbsoluteExCmd(
                new[] { 1.0, 1.0, 1.0, 1.0 },
                1, 1, 1, 0);
        }
    }
}

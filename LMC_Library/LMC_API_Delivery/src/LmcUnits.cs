namespace LasalMotionControlLib
{
    public static class LMC_Units
    {
        // Mirrors Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Include/unit.h.
        // Caller code multiplies/divides with these constants. Packet builders
        // never apply them automatically.
        public const int MMPSEC2 = 1;
        public const int DEG = 10000;
        public const int MM2 = 10;
        public const int KN = 1000;
        public const int N = 1;
        public const int M = 1000 * 10000;
        public const int MM = 10000;
        public const int GB = 1024 * 1024 * 1024;
        public const int KB = 1024;
        public const int MB = 1024 * 1024;
        public const int BAR = 1000;
        public const int RPM = 1000;
        public const int MLPMIN = 1;
        public const int MMPSEC = 10000;
        public const int HOURS = 60 * 60 * 1000;
        public const int MIN = 60 * 1000;
        public const int MS = 1;
        public const int SEC = 1000;
        public const int SECS = 1000;
        public const int CCM = 1;
    }
}

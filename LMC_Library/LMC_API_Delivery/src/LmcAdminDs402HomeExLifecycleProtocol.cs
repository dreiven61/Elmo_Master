namespace LasalMotionControlLib
{
    internal static partial class LMC_AdminParser
    {
        internal static LMCParsedAxisDs402HomeExOutcome
            ParseRetireAxisDs402HomeExOutcome(
                byte[] raw,
                uint expectedRetireRequestId,
                LMCAxisDs402HomeExRecoveryKey expectedRecoveryKey,
                uint expectedRecordGeneration)
        {
            return ParseAxisDs402HomeExOutcomeRetirement(
                raw,
                expectedRetireRequestId,
                expectedRecoveryKey,
                expectedRecordGeneration);
        }
    }
}

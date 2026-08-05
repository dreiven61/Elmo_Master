using System;

namespace LasalMotionControlLib
{
    /// <summary>
    /// Parameters for LMC_Home CurrentPositionZero. The command applies zero
    /// to the current coordinate without reference or limit switch motion.
    /// </summary>
    public sealed class LMCHomeParameters
    {
        public LMCHomeParameters(
            int expectedActualPosition,
            int timeoutMilliseconds)
        {
            LMC_AdminFrame.ValidateLmcHome(
                LMCHomeSemanticMode.CurrentPositionZero,
                timeoutMilliseconds);
            ExpectedActualPosition = expectedActualPosition;
            TimeoutMilliseconds = timeoutMilliseconds;
        }

        public LMCHomeSemanticMode SemanticMode
        {
            get { return LMCHomeSemanticMode.CurrentPositionZero; }
        }
        public int ExpectedActualPosition { get; private set; }
        public int TargetPosition { get { return 0; } }
        public int TimeoutMilliseconds { get; private set; }
    }

    public partial class LMCSingleAxis
    {
        public LMCPreparedHome PrepareLMC_Home(
            LMCHomeParameters parameters,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            LMCHomeExecuteToken executeToken)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            return PrepareLMC_Home(
                parameters.ExpectedActualPosition,
                parameters.TimeoutMilliseconds,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                executeToken);
        }
    }
}

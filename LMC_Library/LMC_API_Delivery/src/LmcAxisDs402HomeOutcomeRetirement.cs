using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public partial class LMCSingleAxis
    {
        /// <summary>
        /// Retires an exact terminal DS402 Home outcome. Repeating the same
        /// key and generation is safe because the PLC retains a tombstone.
        /// </summary>
        public LMCAxisDs402HomeOutcomeRetirementResult
            RetireDs402HomeOutcome(
                LMCAxisDs402HomeRecoveryKey recoveryKey,
                uint recordGeneration,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.RetireAxisDs402HomeOutcome(
                this,
                recoveryKey,
                recordGeneration,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities);
        }

        /// <summary>
        /// Convenience overload that accepts a successfully queried terminal
        /// outcome and derives its exact key and generation.
        /// </summary>
        public LMCAxisDs402HomeOutcomeRetirementResult
            RetireDs402HomeOutcome(
                LMCAxisDs402HomeOutcomeResult terminalOutcome,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.RetireAxisDs402HomeOutcome(
                this,
                terminalOutcome,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities);
        }

        public Task<LMCAxisDs402HomeOutcomeRetirementResult>
            RetireDs402HomeOutcomeAsync(
                LMCAxisDs402HomeRecoveryKey recoveryKey,
                uint recordGeneration,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
                CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.RetireAxisDs402HomeOutcomeAsync(
                this,
                recoveryKey,
                recordGeneration,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                cancellationToken);
        }

        public Task<LMCAxisDs402HomeOutcomeRetirementResult>
            RetireDs402HomeOutcomeAsync(
                LMCAxisDs402HomeOutcomeResult terminalOutcome,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
                CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.RetireAxisDs402HomeOutcomeAsync(
                this,
                terminalOutcome,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                cancellationToken);
        }
    }
}

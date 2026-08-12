using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public partial class LMCSingleAxis
    {
        /// <summary>
        /// Retires an exact terminal SetPosition outcome. Repeating the same
        /// key and generation is safe only when the paired PLC advertises the
        /// dedicated retirement capability and retains an exact tombstone.
        /// </summary>
        public LMCAxisSetPositionOutcomeRetirementResult
            RetireSetPositionOutcome(
                LMCAxisSetPositionRecoveryKey recoveryKey,
                uint recordGeneration,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.RetireAxisSetPositionOutcome(
                this,
                recoveryKey,
                recordGeneration,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities);
        }

        /// <summary>
        /// Convenience overload for a successfully queried exact terminal
        /// outcome and its nonzero generation.
        /// </summary>
        public LMCAxisSetPositionOutcomeRetirementResult
            RetireSetPositionOutcome(
                LMCAxisSetPositionOutcomeResult terminalOutcome,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.RetireAxisSetPositionOutcome(
                this,
                terminalOutcome,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities);
        }

        public Task<LMCAxisSetPositionOutcomeRetirementResult>
            RetireSetPositionOutcomeAsync(
                LMCAxisSetPositionRecoveryKey recoveryKey,
                uint recordGeneration,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
                CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.RetireAxisSetPositionOutcomeAsync(
                this,
                recoveryKey,
                recordGeneration,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                cancellationToken);
        }

        public Task<LMCAxisSetPositionOutcomeRetirementResult>
            RetireSetPositionOutcomeAsync(
                LMCAxisSetPositionOutcomeResult terminalOutcome,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
                CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.RetireAxisSetPositionOutcomeAsync(
                this,
                terminalOutcome,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                cancellationToken);
        }
    }
}

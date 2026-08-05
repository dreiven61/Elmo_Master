using System;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public partial class LMCSingleAxis
    {
        /// <summary>
        /// Prepares LMC_Home CurrentPositionZero. The supplied actual position
        /// is an exact stale-read guard. No axis motion or switch is requested.
        /// </summary>
        public LMCPreparedHome PrepareLMC_Home(
            int expectedActualPosition,
            int timeoutMilliseconds,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            LMCHomeExecuteToken executeToken)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.PrepareLmcHome(
                this,
                expectedActualPosition,
                timeoutMilliseconds,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                executeToken);
        }

        /// <summary>
        /// Sends one prepared LMC_Home request. The return value is only the
        /// start ACK; use ReadLMC_HomeOutcome for terminal proof.
        /// </summary>
        public LMCHomeStartAcknowledgement LMC_Home(
            LMCPreparedHome preparedCommand)
        {
            EnsurePreparedLmcHomeOwner(preparedCommand);
            return connection.Admin.StartLmcHome(preparedCommand);
        }

        public Task<LMCHomeStartAcknowledgement> LMC_HomeAsync(
            LMCPreparedHome preparedCommand,
            CancellationToken cancellationToken)
        {
            EnsurePreparedLmcHomeOwner(preparedCommand);
            return connection.Admin.StartLmcHomeAsync(
                preparedCommand,
                cancellationToken);
        }

        public LMCHomeOutcomeResult ReadLMC_HomeOutcome(
            LMCHomeRecoveryKey recoveryKey,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.ReadLmcHomeOutcome(
                this,
                recoveryKey,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities);
        }

        public Task<LMCHomeOutcomeResult> ReadLMC_HomeOutcomeAsync(
            LMCHomeRecoveryKey recoveryKey,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.ReadLmcHomeOutcomeAsync(
                this,
                recoveryKey,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                cancellationToken);
        }

        public LMCHomeOutcomeRetirementResult
            RetireLMC_HomeOutcome(
                LMCHomeRecoveryKey recoveryKey,
                uint recordGeneration,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.RetireLmcHomeOutcome(
                this,
                recoveryKey,
                recordGeneration,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities);
        }

        public LMCHomeOutcomeRetirementResult
            RetireLMC_HomeOutcome(
                LMCHomeOutcomeResult terminalOutcome,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.RetireLmcHomeOutcome(
                this,
                terminalOutcome,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities);
        }

        public Task<LMCHomeOutcomeRetirementResult>
            RetireLMC_HomeOutcomeAsync(
                LMCHomeRecoveryKey recoveryKey,
                uint recordGeneration,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
                CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.RetireLmcHomeOutcomeAsync(
                this,
                recoveryKey,
                recordGeneration,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                cancellationToken);
        }

        public Task<LMCHomeOutcomeRetirementResult>
            RetireLMC_HomeOutcomeAsync(
                LMCHomeOutcomeResult terminalOutcome,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
                CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.RetireLmcHomeOutcomeAsync(
                this,
                terminalOutcome,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                cancellationToken);
        }

        internal void EnsureAdminStartLmcHomeMutationAdmission(
            byte[] request)
        {
            if (request == null
                || LMC_Frame.GetRequestCommand(request)
                    != LMC_AdminFrame.StartLmcHomeCommandId)
            {
                throw new ArgumentException(
                    "The axis mutation request is not LMC_Home.",
                    "request");
            }

            EnsureCurrentSessionForUse();
            EnsureAxisMutationAdmission(request);
        }

        private void EnsurePreparedLmcHomeOwner(
            LMCPreparedHome preparedCommand)
        {
            if (preparedCommand == null)
            {
                throw new ArgumentNullException("preparedCommand");
            }

            if (!ReferenceEquals(preparedCommand.Axis, this))
            {
                throw new InvalidOperationException(
                    "The prepared LMC_Home command belongs to another axis handle.");
            }

            EnsureCurrentSessionForUse();
        }
    }
}

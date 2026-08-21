using System;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public partial class LMCSingleAxis
    {
        public LMCPreparedAxisSetOperationMode PrepareSetOperationMode(
            LMCDriveOperationMode requestedMode,
            uint timeoutMilliseconds,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            LMCAxisSetOperationModeExecuteToken executeToken)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.PrepareAxisSetOperationMode(
                this,
                requestedMode,
                timeoutMilliseconds,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                executeToken);
        }

        /// <summary>
        /// Sends one prepared Start request. A successful return is only an
        /// acceptance acknowledgement; query the exact outcome for completion.
        /// </summary>
        public LMCAxisSetOperationModeStartAcknowledgement SetOperationMode(
            LMCPreparedAxisSetOperationMode preparedCommand)
        {
            EnsurePreparedSetOperationModeOwner(preparedCommand);
            return connection.Admin.StartAxisSetOperationMode(
                preparedCommand);
        }

        public Task<LMCAxisSetOperationModeStartAcknowledgement>
            SetOperationModeAsync(
                LMCPreparedAxisSetOperationMode preparedCommand,
                CancellationToken cancellationToken)
        {
            EnsurePreparedSetOperationModeOwner(preparedCommand);
            return connection.Admin.StartAxisSetOperationModeAsync(
                preparedCommand,
                cancellationToken);
        }

        public LMCAxisSetOperationModeOutcomeResult
            ReadSetOperationModeOutcome(
                LMCAxisSetOperationModeRecoveryKey recoveryKey,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.ReadAxisSetOperationModeOutcome(
                this,
                recoveryKey,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities);
        }

        public Task<LMCAxisSetOperationModeOutcomeResult>
            ReadSetOperationModeOutcomeAsync(
                LMCAxisSetOperationModeRecoveryKey recoveryKey,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
                CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.ReadAxisSetOperationModeOutcomeAsync(
                this,
                recoveryKey,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                cancellationToken);
        }

        public LMCAxisSetOperationModeOutcomeRetirementResult
            RetireSetOperationModeOutcome(
                LMCAxisSetOperationModeRecoveryKey recoveryKey,
                uint recordGeneration,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.RetireAxisSetOperationModeOutcome(
                this,
                recoveryKey,
                recordGeneration,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities);
        }

        public LMCAxisSetOperationModeOutcomeRetirementResult
            RetireSetOperationModeOutcome(
                LMCAxisSetOperationModeOutcomeResult terminalOutcome,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.RetireAxisSetOperationModeOutcome(
                this,
                terminalOutcome,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities);
        }

        public Task<LMCAxisSetOperationModeOutcomeRetirementResult>
            RetireSetOperationModeOutcomeAsync(
                LMCAxisSetOperationModeRecoveryKey recoveryKey,
                uint recordGeneration,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
                CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.RetireAxisSetOperationModeOutcomeAsync(
                this,
                recoveryKey,
                recordGeneration,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                cancellationToken);
        }

        public Task<LMCAxisSetOperationModeOutcomeRetirementResult>
            RetireSetOperationModeOutcomeAsync(
                LMCAxisSetOperationModeOutcomeResult terminalOutcome,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
                CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.RetireAxisSetOperationModeOutcomeAsync(
                this,
                terminalOutcome,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                cancellationToken);
        }

        internal void EnsureAdminSetOperationModeMutationAdmission(
            byte[] request)
        {
            if (request == null
                || LMC_Frame.GetRequestCommand(request)
                    != LMC_CommandId.StartAxisSetOperationMode)
            {
                throw new ArgumentException(
                    "The axis mutation request is not StartAxisSetOperationMode.",
                    "request");
            }

            EnsureCurrentSessionForUse();
            EnsureAxisMutationAdmission(request);
        }

        private void EnsurePreparedSetOperationModeOwner(
            LMCPreparedAxisSetOperationMode preparedCommand)
        {
            if (preparedCommand == null)
            {
                throw new ArgumentNullException("preparedCommand");
            }

            if (!ReferenceEquals(preparedCommand.Axis, this))
            {
                throw new InvalidOperationException(
                    "The prepared SetOperationMode command belongs to another axis handle.");
            }

            EnsureCurrentSessionForUse();
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public partial class LMCSingleAxis
    {
        /// <summary>
        /// Prepares one LMC_HomeDS402 call from the typed DS402 homing
        /// parameters. Preparation performs admission and identity checks but
        /// does not send the Start command.
        /// </summary>
        public LMCPreparedAxisDs402Home PrepareLMC_HomeDS402(
            LMCAxisDs402HomeParameters parameters,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            LMCAxisDs402HomeExecuteToken executeToken)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.PrepareAxisDs402Home(
                this,
                parameters,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                executeToken);
        }

        /// <summary>
        /// Compatibility name for PrepareLMC_HomeDS402.
        /// </summary>
        public LMCPreparedAxisDs402Home PrepareDs402Home(
            LMCAxisDs402HomeParameters parameters,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            LMCAxisDs402HomeExecuteToken executeToken)
        {
            return PrepareLMC_HomeDS402(
                parameters,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                executeToken);
        }

        /// <summary>
        /// Sends one prepared LMC_HomeDS402 call through command 0x7D15. The
        /// result is a Start acknowledgement only; use
        /// ReadDs402HomeOutcome for terminal completion evidence.
        /// </summary>
        public LMCAxisDs402HomeStartAcknowledgement LMC_HomeDS402(
            LMCPreparedAxisDs402Home preparedCommand)
        {
            EnsurePreparedDs402HomeOwner(preparedCommand);
            return connection.Admin.StartAxisDs402Home(preparedCommand);
        }

        /// <summary>
        /// Compatibility name for LMC_HomeDS402.
        /// </summary>
        public LMCAxisDs402HomeStartAcknowledgement Ds402Home(
            LMCPreparedAxisDs402Home preparedCommand)
        {
            return LMC_HomeDS402(preparedCommand);
        }

        /// <summary>
        /// Asynchronously sends one prepared LMC_HomeDS402 call through
        /// command 0x7D15. The result is a Start acknowledgement only.
        /// </summary>
        public Task<LMCAxisDs402HomeStartAcknowledgement> LMC_HomeDS402Async(
            LMCPreparedAxisDs402Home preparedCommand,
            CancellationToken cancellationToken)
        {
            EnsurePreparedDs402HomeOwner(preparedCommand);
            return connection.Admin.StartAxisDs402HomeAsync(
                preparedCommand,
                cancellationToken);
        }

        /// <summary>
        /// Compatibility name for LMC_HomeDS402Async.
        /// </summary>
        public Task<LMCAxisDs402HomeStartAcknowledgement> Ds402HomeAsync(
            LMCPreparedAxisDs402Home preparedCommand,
            CancellationToken cancellationToken)
        {
            return LMC_HomeDS402Async(preparedCommand, cancellationToken);
        }

        public LMCAxisDs402HomeOutcomeResult ReadDs402HomeOutcome(
            LMCAxisDs402HomeRecoveryKey recoveryKey,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.ReadAxisDs402HomeOutcome(
                this,
                recoveryKey,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities);
        }

        public Task<LMCAxisDs402HomeOutcomeResult>
            ReadDs402HomeOutcomeAsync(
                LMCAxisDs402HomeRecoveryKey recoveryKey,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
                CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.ReadAxisDs402HomeOutcomeAsync(
                this,
                recoveryKey,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                cancellationToken);
        }

        internal void EnsureAdminStartAxisDs402HomeMutationAdmission(
            byte[] request)
        {
            if (request == null
                || LMC_Frame.GetRequestCommand(request)
                    != LMC_CommandId.StartAxisDs402Home)
            {
                throw new ArgumentException(
                    "The axis mutation request is not StartAxisDs402Home.",
                    "request");
            }

            EnsureCurrentSessionForUse();
            EnsureAxisMutationAdmission(request);
        }

        private void EnsurePreparedDs402HomeOwner(
            LMCPreparedAxisDs402Home preparedCommand)
        {
            if (preparedCommand == null)
            {
                throw new ArgumentNullException("preparedCommand");
            }

            if (!ReferenceEquals(preparedCommand.Axis, this))
            {
                throw new InvalidOperationException(
                    "The prepared DS402 Home command belongs to another axis handle.");
            }

            EnsureCurrentSessionForUse();
        }
    }
}

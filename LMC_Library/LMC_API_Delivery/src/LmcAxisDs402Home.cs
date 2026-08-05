using System;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public partial class LMCSingleAxis
    {
        public LMCPreparedAxisDs402Home PrepareDs402Home(
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
        /// Project-facing LMC_HomeDS402 name for the fixed method 37,
        /// Home-offset-zero operation.
        /// </summary>
        public LMCPreparedAxisDs402Home PrepareLMC_HomeDS402(
            LMCAxisDs402HomeParameters parameters,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            LMCAxisDs402HomeExecuteToken executeToken)
        {
            return PrepareDs402Home(
                parameters,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                executeToken);
        }

        /// <summary>
        /// Sends the separate 0x7D15 LMC_HomeDS402 command. The result is a
        /// start acknowledgement only, not homing completion evidence.
        /// </summary>
        public LMCAxisDs402HomeStartAcknowledgement Ds402Home(
            LMCPreparedAxisDs402Home preparedCommand)
        {
            EnsurePreparedDs402HomeOwner(preparedCommand);
            return connection.Admin.StartAxisDs402Home(preparedCommand);
        }

        /// <summary>
        /// Project-facing LMC_HomeDS402 name. The return value proves start
        /// acknowledgement only; use ReadDs402HomeOutcome for completion.
        /// </summary>
        public LMCAxisDs402HomeStartAcknowledgement LMC_HomeDS402(
            LMCPreparedAxisDs402Home preparedCommand)
        {
            return Ds402Home(preparedCommand);
        }

        /// <summary>
        /// Sends the separate 0x7D15 LMC_HomeDS402 command. The result is a
        /// start acknowledgement only, not homing completion evidence.
        /// </summary>
        public Task<LMCAxisDs402HomeStartAcknowledgement> Ds402HomeAsync(
            LMCPreparedAxisDs402Home preparedCommand,
            CancellationToken cancellationToken)
        {
            EnsurePreparedDs402HomeOwner(preparedCommand);
            return connection.Admin.StartAxisDs402HomeAsync(
                preparedCommand,
                cancellationToken);
        }

        /// <summary>
        /// Async project-facing LMC_HomeDS402 name. The return value proves
        /// start acknowledgement only, not successful homing.
        /// </summary>
        public Task<LMCAxisDs402HomeStartAcknowledgement> LMC_HomeDS402Async(
            LMCPreparedAxisDs402Home preparedCommand,
            CancellationToken cancellationToken)
        {
            return Ds402HomeAsync(preparedCommand, cancellationToken);
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

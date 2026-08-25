using System;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public partial class LMCSingleAxis
    {
        /// <summary>
        /// Internal bridge for a HomeDS402Ex execution plan that has already
        /// passed the engineering-profile conversion and approval boundary.
        /// Keeping this preparation path internal prevents public callers from
        /// supplying unqualified raw DINT motion values.
        /// </summary>
        internal LMCPreparedAxisDs402HomeEx PrepareDs402HomeExApprovedPlan(
            LMCAxisDs402HomeExExecutionPlan executionPlan,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            LMCAxisDs402HomeExExecuteToken executeToken)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.PrepareAxisDs402HomeExApprovedPlan(
                this,
                executionPlan,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                executeToken);
        }

        /// <summary>
        /// Sends the one-shot HomeDS402Ex Start command for a previously
        /// prepared and approved plan. The returned acknowledgement proves only
        /// Start acceptance; completion is established through the retained
        /// outcome lifecycle.
        /// </summary>
        public LMCAxisDs402HomeExStartAcknowledgement Ds402HomeEx(
            LMCPreparedAxisDs402HomeEx preparedCommand)
        {
            EnsurePreparedDs402HomeExOwner(preparedCommand);
            return connection.Admin.StartAxisDs402HomeEx(preparedCommand);
        }

        /// <summary>
        /// Project-facing LMC_HomeDS402Ex alias. This is a one-shot Start only;
        /// use ReadDs402HomeExOutcome for terminal evidence.
        /// </summary>
        public LMCAxisDs402HomeExStartAcknowledgement LMC_HomeDS402Ex(
            LMCPreparedAxisDs402HomeEx preparedCommand)
        {
            return Ds402HomeEx(preparedCommand);
        }

        public Task<LMCAxisDs402HomeExStartAcknowledgement> Ds402HomeExAsync(
            LMCPreparedAxisDs402HomeEx preparedCommand,
            CancellationToken cancellationToken)
        {
            EnsurePreparedDs402HomeExOwner(preparedCommand);
            return connection.Admin.StartAxisDs402HomeExAsync(
                preparedCommand,
                cancellationToken);
        }

        public Task<LMCAxisDs402HomeExStartAcknowledgement>
            LMC_HomeDS402ExAsync(
                LMCPreparedAxisDs402HomeEx preparedCommand,
                CancellationToken cancellationToken)
        {
            return Ds402HomeExAsync(preparedCommand, cancellationToken);
        }

        /// <summary>
        /// Read-only lookup of the exact retained HomeDS402Ex record. This path
        /// never replays Start or any Home parameter write.
        /// </summary>
        public LMCAxisDs402HomeExOutcomeResult ReadDs402HomeExOutcome(
            LMCAxisDs402HomeExRecoveryKey recoveryKey,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.ReadAxisDs402HomeExOutcome(
                this,
                recoveryKey,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities);
        }

        public Task<LMCAxisDs402HomeExOutcomeResult>
            ReadDs402HomeExOutcomeAsync(
                LMCAxisDs402HomeExRecoveryKey recoveryKey,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
                CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.ReadAxisDs402HomeExOutcomeAsync(
                this,
                recoveryKey,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                cancellationToken);
        }

        /// <summary>
        /// Retires only the exact terminal record generation already proven by
        /// ReadDs402HomeExOutcome. Running records cannot be retired.
        /// </summary>
        public LMCAxisDs402HomeExOutcomeRetirementResult
            RetireDs402HomeExOutcome(
                LMCAxisDs402HomeExOutcomeResult terminalOutcome,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.RetireAxisDs402HomeExOutcome(
                this,
                terminalOutcome,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities);
        }

        public Task<LMCAxisDs402HomeExOutcomeRetirementResult>
            RetireDs402HomeExOutcomeAsync(
                LMCAxisDs402HomeExOutcomeResult terminalOutcome,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
                CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.RetireAxisDs402HomeExOutcomeAsync(
                this,
                terminalOutcome,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                cancellationToken);
        }

        internal void EnsureAdminStartAxisDs402HomeExMutationAdmission(
            byte[] request)
        {
            if (request == null
                || LMC_Frame.GetRequestCommand(request)
                    != LMC_CommandId.StartAxisDs402HomeEx)
            {
                throw new ArgumentException(
                    "The axis mutation request is not StartAxisDs402HomeEx.",
                    "request");
            }

            EnsureCurrentSessionForUse();
            EnsureAxisMutationAdmission(request);
        }

        private void EnsurePreparedDs402HomeExOwner(
            LMCPreparedAxisDs402HomeEx preparedCommand)
        {
            if (preparedCommand == null)
            {
                throw new ArgumentNullException("preparedCommand");
            }

            if (!ReferenceEquals(preparedCommand.Axis, this))
            {
                throw new InvalidOperationException(
                    "The prepared HomeDS402Ex command belongs to another axis handle.");
            }

            EnsureCurrentSessionForUse();
        }
    }
}

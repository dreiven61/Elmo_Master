using System;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public partial class LMCSingleAxis
    {
        public LMCPreparedAxisSetPosition PrepareSetPosition(
            int targetPosition,
            int expectedActualPosition,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            LMCAxisSetPositionExecuteToken executeToken)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.PrepareAxisSetPosition(
                this,
                targetPosition,
                expectedActualPosition,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                executeToken);
        }

        public LMCAxisSetPositionResult SetPositionEx(
            LMCPreparedAxisSetPosition preparedCommand)
        {
            EnsurePreparedSetPositionOwner(preparedCommand);
            return connection.Admin.SetAxisPosition(preparedCommand);
        }

        public Task<LMCAxisSetPositionResult> SetPositionExAsync(
            LMCPreparedAxisSetPosition preparedCommand,
            CancellationToken cancellationToken)
        {
            EnsurePreparedSetPositionOwner(preparedCommand);
            return connection.Admin.SetAxisPositionAsync(
                preparedCommand,
                cancellationToken);
        }

        public LMCAxisSetPositionOutcomeResult ReadSetPositionOutcome(
            LMCAxisSetPositionRecoveryKey recoveryKey,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.ReadAxisSetPositionOutcome(
                this,
                recoveryKey,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities);
        }

        public Task<LMCAxisSetPositionOutcomeResult>
            ReadSetPositionOutcomeAsync(
                LMCAxisSetPositionRecoveryKey recoveryKey,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
                CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            return connection.Admin.ReadAxisSetPositionOutcomeAsync(
                this,
                recoveryKey,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                cancellationToken);
        }

        internal void EnsureAdminSetPositionMutationAdmission(byte[] request)
        {
            if (request == null
                || LMC_Frame.GetRequestCommand(request)
                    != LMC_CommandId.SetAxisPosition)
            {
                throw new ArgumentException(
                    "The axis mutation request is not SetAxisPosition.",
                    "request");
            }

            EnsureCurrentSessionForUse();
            EnsureAxisMutationAdmission(request);
        }

        private void EnsurePreparedSetPositionOwner(
            LMCPreparedAxisSetPosition preparedCommand)
        {
            if (preparedCommand == null)
            {
                throw new ArgumentNullException("preparedCommand");
            }

            if (!ReferenceEquals(preparedCommand.Axis, this))
            {
                throw new InvalidOperationException(
                    "The prepared SetAxisPosition command belongs to another axis handle.");
            }

            EnsureCurrentSessionForUse();
        }
    }
}

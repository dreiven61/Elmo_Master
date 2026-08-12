using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public sealed partial class LMCAdmin
    {
        public LMCAxisSetPositionOutcomeRetirementResult
            RetireAxisSetPositionOutcome(
                LMCSingleAxis axis,
                LMCAxisSetPositionRecoveryKey recoveryKey,
                uint recordGeneration,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            var sessionGeneration = ValidateAxisOwner(axis);
            ValidateAxisSetPositionOutcomeRetirement(
                axis,
                recoveryKey,
                recordGeneration,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                sessionGeneration);

            var retireRequestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_AdminFrame.RetireAxisSetPositionOutcome(
                    retireRequestId,
                    recoveryKey,
                    recordGeneration),
                sessionGeneration);
            var parsed = ParseAxisSetPositionOutcomeRetirementAndFaultSession(
                raw,
                retireRequestId,
                recoveryKey,
                recordGeneration,
                sessionGeneration);
            connection.EnsureSessionGeneration(sessionGeneration);
            return CreateAxisSetPositionOutcomeRetirementResult(
                parsed,
                recoveryKey);
        }

        public LMCAxisSetPositionOutcomeRetirementResult
            RetireAxisSetPositionOutcome(
                LMCSingleAxis axis,
                LMCAxisSetPositionOutcomeResult terminalOutcome,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            ValidateTerminalAxisSetPositionOutcome(terminalOutcome);
            return RetireAxisSetPositionOutcome(
                axis,
                terminalOutcome.RecoveryKey,
                terminalOutcome.RecordGeneration,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities);
        }

        public async Task<LMCAxisSetPositionOutcomeRetirementResult>
            RetireAxisSetPositionOutcomeAsync(
                LMCSingleAxis axis,
                LMCAxisSetPositionRecoveryKey recoveryKey,
                uint recordGeneration,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
                CancellationToken cancellationToken)
        {
            var sessionGeneration = ValidateAxisOwner(axis);
            ValidateAxisSetPositionOutcomeRetirement(
                axis,
                recoveryKey,
                recordGeneration,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                sessionGeneration);
            cancellationToken.ThrowIfCancellationRequested();

            var retireRequestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_AdminFrame.RetireAxisSetPositionOutcome(
                    retireRequestId,
                    recoveryKey,
                    recordGeneration),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var parsed = ParseAxisSetPositionOutcomeRetirementAndFaultSession(
                raw,
                retireRequestId,
                recoveryKey,
                recordGeneration,
                sessionGeneration);
            connection.EnsureSessionGeneration(sessionGeneration);
            return CreateAxisSetPositionOutcomeRetirementResult(
                parsed,
                recoveryKey);
        }

        public Task<LMCAxisSetPositionOutcomeRetirementResult>
            RetireAxisSetPositionOutcomeAsync(
                LMCSingleAxis axis,
                LMCAxisSetPositionOutcomeResult terminalOutcome,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
                CancellationToken cancellationToken)
        {
            ValidateTerminalAxisSetPositionOutcome(terminalOutcome);
            return RetireAxisSetPositionOutcomeAsync(
                axis,
                terminalOutcome.RecoveryKey,
                terminalOutcome.RecordGeneration,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                cancellationToken);
        }

        private static void ValidateTerminalAxisSetPositionOutcome(
            LMCAxisSetPositionOutcomeResult terminalOutcome)
        {
            if (terminalOutcome == null)
            {
                throw new ArgumentNullException("terminalOutcome");
            }

            if (terminalOutcome.Response == null
                || !terminalOutcome.Response.IsSuccess
                || terminalOutcome.RecoveryKey == null
                || terminalOutcome.RecordGeneration == 0
                || (terminalOutcome.RecordState
                        != LMCAxisSetPositionOutcomeRecordState.Succeeded
                    && terminalOutcome.RecordState
                        != LMCAxisSetPositionOutcomeRecordState.Rejected))
            {
                throw new InvalidOperationException(
                    "Only a successfully queried terminal SetPosition outcome with a nonzero record generation may be retired.");
            }
        }

        private void ValidateAxisSetPositionOutcomeRetirement(
            LMCSingleAxis axis,
            LMCAxisSetPositionRecoveryKey recoveryKey,
            uint recordGeneration,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            long expectedSessionGeneration)
        {
            if (recordGeneration == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "recordGeneration",
                    "RecordGeneration must be nonzero.");
            }

            ValidateAxisSetPositionOutcomeQuery(
                axis,
                recoveryKey,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                expectedSessionGeneration);
            if (!verifiedCapabilities.Supports(
                    LMCAdminFeature.AxisSetPositionOutcomeRetirement))
            {
                throw new NotSupportedException(
                    "The connected PLC does not advertise exact SetPosition outcome retirement.");
            }
            connection.EnsureSessionGeneration(expectedSessionGeneration);
        }

        private LMCParsedAxisSetPositionOutcome
            ParseAxisSetPositionOutcomeRetirementAndFaultSession(
                byte[] raw,
                uint retireRequestId,
                LMCAxisSetPositionRecoveryKey recoveryKey,
                uint recordGeneration,
                long expectedSessionGeneration)
        {
            try
            {
                return LMC_AdminParser.ParseAxisSetPositionOutcomeRetirement(
                    raw,
                    retireRequestId,
                    recoveryKey,
                    recordGeneration);
            }
            catch (InvalidDataException ex)
            {
                connection.TryInvalidateSessionAfterUncertainMutation(
                    expectedSessionGeneration,
                    ex);
                throw;
            }
        }

        private static LMCAxisSetPositionOutcomeRetirementResult
            CreateAxisSetPositionOutcomeRetirementResult(
                LMCParsedAxisSetPositionOutcome parsed,
                LMCAxisSetPositionRecoveryKey recoveryKey)
        {
            return new LMCAxisSetPositionOutcomeRetirementResult(
                parsed.Response,
                recoveryKey,
                parsed.RecordState,
                parsed.AppliedPosition,
                parsed.OriginalCommandStatus,
                parsed.OriginalErrorId,
                parsed.OriginalDetailCode,
                parsed.NativeCommandState,
                parsed.RecordGeneration);
        }
    }
}

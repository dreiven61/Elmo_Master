using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public sealed partial class LMCAdmin
    {
        public LMCAxisDs402HomeOutcomeRetirementResult
            RetireAxisDs402HomeOutcome(
                LMCSingleAxis axis,
                LMCAxisDs402HomeRecoveryKey recoveryKey,
                uint recordGeneration,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            var sessionGeneration = ValidateAxisOwner(axis);
            ValidateAxisDs402HomeOutcomeRetirement(
                axis,
                recoveryKey,
                recordGeneration,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                sessionGeneration);

            var retireRequestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_AdminFrame.RetireAxisDs402HomeOutcome(
                    retireRequestId,
                    recoveryKey,
                    recordGeneration),
                sessionGeneration);
            var parsed = ParseAxisDs402HomeOutcomeRetirementAndFaultSession(
                raw,
                retireRequestId,
                recoveryKey,
                recordGeneration,
                sessionGeneration);
            connection.EnsureSessionGeneration(sessionGeneration);
            return CreateAxisDs402HomeOutcomeRetirementResult(
                parsed,
                recoveryKey);
        }

        public LMCAxisDs402HomeOutcomeRetirementResult
            RetireAxisDs402HomeOutcome(
                LMCSingleAxis axis,
                LMCAxisDs402HomeOutcomeResult terminalOutcome,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            ValidateTerminalAxisDs402HomeOutcome(terminalOutcome);
            return RetireAxisDs402HomeOutcome(
                axis,
                terminalOutcome.RecoveryKey,
                terminalOutcome.RecordGeneration,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities);
        }

        public async Task<LMCAxisDs402HomeOutcomeRetirementResult>
            RetireAxisDs402HomeOutcomeAsync(
                LMCSingleAxis axis,
                LMCAxisDs402HomeRecoveryKey recoveryKey,
                uint recordGeneration,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
                CancellationToken cancellationToken)
        {
            var sessionGeneration = ValidateAxisOwner(axis);
            ValidateAxisDs402HomeOutcomeRetirement(
                axis,
                recoveryKey,
                recordGeneration,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                sessionGeneration);
            cancellationToken.ThrowIfCancellationRequested();

            var retireRequestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_AdminFrame.RetireAxisDs402HomeOutcome(
                    retireRequestId,
                    recoveryKey,
                    recordGeneration),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var parsed = ParseAxisDs402HomeOutcomeRetirementAndFaultSession(
                raw,
                retireRequestId,
                recoveryKey,
                recordGeneration,
                sessionGeneration);
            connection.EnsureSessionGeneration(sessionGeneration);
            return CreateAxisDs402HomeOutcomeRetirementResult(
                parsed,
                recoveryKey);
        }

        public Task<LMCAxisDs402HomeOutcomeRetirementResult>
            RetireAxisDs402HomeOutcomeAsync(
                LMCSingleAxis axis,
                LMCAxisDs402HomeOutcomeResult terminalOutcome,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
                CancellationToken cancellationToken)
        {
            ValidateTerminalAxisDs402HomeOutcome(terminalOutcome);
            return RetireAxisDs402HomeOutcomeAsync(
                axis,
                terminalOutcome.RecoveryKey,
                terminalOutcome.RecordGeneration,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                cancellationToken);
        }

        private static void ValidateTerminalAxisDs402HomeOutcome(
            LMCAxisDs402HomeOutcomeResult terminalOutcome)
        {
            if (terminalOutcome == null)
            {
                throw new ArgumentNullException("terminalOutcome");
            }

            if (!terminalOutcome.IsTerminal
                || terminalOutcome.Response == null
                || !terminalOutcome.Response.IsSuccess
                || terminalOutcome.RecoveryKey == null
                || terminalOutcome.RecordGeneration == 0)
            {
                throw new InvalidOperationException(
                    "Only a successfully queried terminal DS402 Home outcome with a nonzero record generation may be retired.");
            }
        }

        private void ValidateAxisDs402HomeOutcomeRetirement(
            LMCSingleAxis axis,
            LMCAxisDs402HomeRecoveryKey recoveryKey,
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

            ValidateAxisDs402HomeOutcomeQuery(
                axis,
                recoveryKey,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                expectedSessionGeneration);
        }

        private LMCParsedAxisDs402HomeOutcome
            ParseAxisDs402HomeOutcomeRetirementAndFaultSession(
                byte[] raw,
                uint retireRequestId,
                LMCAxisDs402HomeRecoveryKey recoveryKey,
                uint recordGeneration,
                long expectedSessionGeneration)
        {
            try
            {
                return LMC_AdminParser.ParseAxisDs402HomeOutcomeRetirement(
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

        private static LMCAxisDs402HomeOutcomeRetirementResult
            CreateAxisDs402HomeOutcomeRetirementResult(
                LMCParsedAxisDs402HomeOutcome parsed,
                LMCAxisDs402HomeRecoveryKey recoveryKey)
        {
            return new LMCAxisDs402HomeOutcomeRetirementResult(
                parsed.Response,
                recoveryKey,
                parsed.RecordState,
                parsed.OriginalCommandStatus,
                parsed.OriginalErrorId,
                parsed.OriginalDetailCode,
                parsed.Ds402StatusWord,
                parsed.ActualPosition,
                parsed.StartCycle,
                parsed.CompletionCycle,
                parsed.NativeCommandState,
                parsed.RecordGeneration);
        }
    }
}

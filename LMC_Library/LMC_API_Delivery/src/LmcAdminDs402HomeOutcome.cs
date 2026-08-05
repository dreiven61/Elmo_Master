using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public sealed partial class LMCAdmin
    {
        public LMCAxisDs402HomeOutcomeResult ReadAxisDs402HomeOutcome(
            LMCSingleAxis axis,
            LMCAxisDs402HomeRecoveryKey recoveryKey,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            var sessionGeneration = ValidateAxisOwner(axis);
            ValidateAxisDs402HomeOutcomeQuery(
                axis,
                recoveryKey,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                sessionGeneration);

            var queryRequestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_AdminFrame.ReadAxisDs402HomeOutcome(
                    queryRequestId,
                    recoveryKey),
                sessionGeneration);
            var parsed = ParseAxisDs402HomeOutcomeAndFaultMalformedSession(
                raw,
                queryRequestId,
                recoveryKey,
                sessionGeneration);
            connection.EnsureSessionGeneration(sessionGeneration);
            return CreateAxisDs402HomeOutcomeResult(parsed, recoveryKey);
        }

        public async Task<LMCAxisDs402HomeOutcomeResult>
            ReadAxisDs402HomeOutcomeAsync(
                LMCSingleAxis axis,
                LMCAxisDs402HomeRecoveryKey recoveryKey,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
                CancellationToken cancellationToken)
        {
            var sessionGeneration = ValidateAxisOwner(axis);
            ValidateAxisDs402HomeOutcomeQuery(
                axis,
                recoveryKey,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                sessionGeneration);
            cancellationToken.ThrowIfCancellationRequested();

            var queryRequestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_AdminFrame.ReadAxisDs402HomeOutcome(
                    queryRequestId,
                    recoveryKey),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var parsed = ParseAxisDs402HomeOutcomeAndFaultMalformedSession(
                raw,
                queryRequestId,
                recoveryKey,
                sessionGeneration);
            connection.EnsureSessionGeneration(sessionGeneration);
            return CreateAxisDs402HomeOutcomeResult(parsed, recoveryKey);
        }

        private static LMCAxisDs402HomeOutcomeResult
            CreateAxisDs402HomeOutcomeResult(
                LMCParsedAxisDs402HomeOutcome parsed,
                LMCAxisDs402HomeRecoveryKey recoveryKey)
        {
            return new LMCAxisDs402HomeOutcomeResult(
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

        private LMCParsedAxisDs402HomeOutcome
            ParseAxisDs402HomeOutcomeAndFaultMalformedSession(
                byte[] raw,
                uint queryRequestId,
                LMCAxisDs402HomeRecoveryKey recoveryKey,
                long expectedSessionGeneration)
        {
            try
            {
                return LMC_AdminParser.ParseAxisDs402HomeOutcome(
                    raw,
                    queryRequestId,
                    recoveryKey);
            }
            catch (InvalidDataException ex)
            {
                connection.TryInvalidateSessionAfterUncertainMutation(
                    expectedSessionGeneration,
                    ex);
                throw;
            }
        }

        private void ValidateAxisDs402HomeOutcomeQuery(
            LMCSingleAxis axis,
            LMCAxisDs402HomeRecoveryKey recoveryKey,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            long expectedSessionGeneration)
        {
            if (recoveryKey == null)
            {
                throw new ArgumentNullException("recoveryKey");
            }

            ValidateAxisDs402HomeCapabilities(
                verifiedCapabilities,
                expectedSessionGeneration,
                axis.AxisReference,
                true);
            ValidateAxisDs402HomeDiagnosticCapabilities(
                verifiedDiagnosticCapabilities,
                expectedSessionGeneration,
                true);

            if (recoveryKey.SchemaVersion != ProtocolSchemaVersion
                || recoveryKey.AxisReference != axis.AxisReference
                || recoveryKey.DiagnosticsBuild
                    != verifiedDiagnosticCapabilities.DiagnosticsBuild
                || recoveryKey.DiagnosticsBootId
                    != verifiedDiagnosticCapabilities.DiagnosticsBootId
                || recoveryKey.MapRevision
                    != verifiedDiagnosticCapabilities.MapRevision)
            {
                throw new InvalidOperationException(
                    "The DS402 Home recovery key does not match the current axis and fresh diagnostics identity.");
            }

            connection.EnsureSessionGeneration(expectedSessionGeneration);
        }
    }
}

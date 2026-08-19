using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public sealed partial class LMCAdmin
    {
        public LMCAxisSetPositionOutcomeResult ReadAxisSetPositionOutcome(
            LMCSingleAxis axis,
            LMCAxisSetPositionRecoveryKey recoveryKey,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            var sessionGeneration = ValidateAxisOwner(axis);
            ValidateAxisSetPositionOutcomeQuery(
                axis,
                recoveryKey,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                sessionGeneration);

            var queryRequestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_AdminFrame.ReadAxisSetPositionOutcome(
                    queryRequestId,
                    verifiedDiagnosticCapabilities.DiagnosticsBootId,
                    recoveryKey),
                sessionGeneration);
            var parsed = ParseAxisSetPositionOutcomeAndFaultMalformedSession(
                raw,
                queryRequestId,
                recoveryKey,
                sessionGeneration);
            connection.EnsureSessionGeneration(sessionGeneration);
            return CreateAxisSetPositionOutcomeResult(parsed, recoveryKey);
        }

        public async Task<LMCAxisSetPositionOutcomeResult>
            ReadAxisSetPositionOutcomeAsync(
                LMCSingleAxis axis,
                LMCAxisSetPositionRecoveryKey recoveryKey,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
                CancellationToken cancellationToken)
        {
            var sessionGeneration = ValidateAxisOwner(axis);
            ValidateAxisSetPositionOutcomeQuery(
                axis,
                recoveryKey,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                sessionGeneration);
            cancellationToken.ThrowIfCancellationRequested();

            var queryRequestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_AdminFrame.ReadAxisSetPositionOutcome(
                    queryRequestId,
                    verifiedDiagnosticCapabilities.DiagnosticsBootId,
                    recoveryKey),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var parsed = ParseAxisSetPositionOutcomeAndFaultMalformedSession(
                raw,
                queryRequestId,
                recoveryKey,
                sessionGeneration);
            connection.EnsureSessionGeneration(sessionGeneration);
            return CreateAxisSetPositionOutcomeResult(parsed, recoveryKey);
        }

        private static LMCAxisSetPositionOutcomeResult
            CreateAxisSetPositionOutcomeResult(
                LMCParsedAxisSetPositionOutcome parsed,
                LMCAxisSetPositionRecoveryKey recoveryKey)
        {
            return new LMCAxisSetPositionOutcomeResult(
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

        private LMCParsedAxisSetPositionOutcome
            ParseAxisSetPositionOutcomeAndFaultMalformedSession(
                byte[] raw,
                uint queryRequestId,
                LMCAxisSetPositionRecoveryKey recoveryKey,
                long expectedSessionGeneration)
        {
            try
            {
                return LMC_AdminParser.ParseAxisSetPositionOutcome(
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

        private void ValidateAxisSetPositionOutcomeQuery(
            LMCSingleAxis axis,
            LMCAxisSetPositionRecoveryKey recoveryKey,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            long expectedSessionGeneration)
        {
            if (recoveryKey == null)
            {
                throw new ArgumentNullException("recoveryKey");
            }

            ValidateAxisSetPositionOutcomeCapabilities(
                verifiedCapabilities,
                expectedSessionGeneration,
                axis.AxisReference);
            ValidateAxisSetPositionDiagnosticCapabilities(
                verifiedDiagnosticCapabilities,
                expectedSessionGeneration,
                true);

            if (recoveryKey.SchemaVersion != ProtocolSchemaVersion
                || recoveryKey.AxisReference != axis.AxisReference
                || recoveryKey.DiagnosticsBootId == 0
                || recoveryKey.DiagnosticsBuild
                    != verifiedDiagnosticCapabilities.DiagnosticsBuild
                || recoveryKey.MapRevision
                    != verifiedDiagnosticCapabilities.MapRevision)
            {
                throw new InvalidOperationException(
                    "The SetAxisPosition recovery key does not match the current axis, build, and map revision.");
            }

            connection.EnsureSessionGeneration(expectedSessionGeneration);
        }

        private void ValidateAxisSetPositionOutcomeCapabilities(
            LMCAdminCapabilities capabilities,
            long expectedSessionGeneration,
            ushort axisReference)
        {
            if (capabilities == null)
            {
                throw new ArgumentNullException("verifiedCapabilities");
            }

            if (!ReferenceEquals(capabilities.ConnectionOwner, connection))
            {
                throw new InvalidOperationException(
                    "The supplied admin capabilities belong to another connection.");
            }

            if (!capabilities.IsBoundTo(
                    this,
                    expectedSessionGeneration)
                || capabilities.ObservationSequence
                    != CurrentCapabilityObservationSequence)
            {
                throw new InvalidOperationException(
                    "The supplied admin capabilities are not the current observation for this admin owner and session.");
            }

            if (capabilities.ConnectionSessionGeneration
                    != expectedSessionGeneration
                || capabilities.Response == null
                || capabilities.Response.SchemaVersion
                    != ProtocolSchemaVersion)
            {
                throw new InvalidOperationException(
                    "The supplied admin capabilities belong to another schema or stale connection session.");
            }

            if (!capabilities.Supports(
                    LMCAdminFeature.AxisSetPositionOutcomeRead)
                || axisReference < 1
                || axisReference > capabilities.PhysicalAxisCount)
            {
                throw new NotSupportedException(
                    "The connected PLC does not advertise read-only SetAxisPosition outcome queries for this physical axis.");
            }

            connection.EnsureSessionGeneration(expectedSessionGeneration);
        }
    }
}

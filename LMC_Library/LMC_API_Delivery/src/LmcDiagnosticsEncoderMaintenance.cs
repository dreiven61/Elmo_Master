using System;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public sealed partial class LMCDiagnostics
    {
        public LMCPreparedEncoderMaintenance
            PrepareTw20EncoderErrorWarningReset(
                LMCTw20EncoderErrorWarningResetRequest request,
                LMCDiagnosticCapabilities verifiedCapabilities,
                LMCTw20EncoderErrorWarningResetExecuteToken executeToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (executeToken == null)
            {
                throw new ArgumentNullException("executeToken");
            }

            var sessionGeneration = connection.SessionGeneration;
            ValidateEncoderMaintenanceCapabilities(
                verifiedCapabilities,
                request.Kind,
                sessionGeneration,
                true);
            executeToken.ConsumeForPreparation();
            return PrepareEncoderMaintenanceCore(
                request,
                verifiedCapabilities,
                sessionGeneration);
        }

        public LMCPreparedEncoderMaintenance
            PrepareTw19MultiturnPositionReset(
                LMCTw19MultiturnPositionResetRequest request,
                LMCDiagnosticCapabilities verifiedCapabilities,
                LMCTw19MultiturnPositionResetExecuteToken executeToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (executeToken == null)
            {
                throw new ArgumentNullException("executeToken");
            }

            var sessionGeneration = connection.SessionGeneration;
            ValidateEncoderMaintenanceCapabilities(
                verifiedCapabilities,
                request.Kind,
                sessionGeneration,
                true);
            executeToken.ConsumeForPreparation();
            return PrepareEncoderMaintenanceCore(
                request,
                verifiedCapabilities,
                sessionGeneration);
        }

        public LMCEncoderMaintenanceStartAcknowledgement
            StartEncoderMaintenance(
                LMCPreparedEncoderMaintenance preparedCommand)
        {
            ValidatePreparedEncoderMaintenance(preparedCommand, true);
            return StartEncoderMaintenanceCore(preparedCommand);
        }

        public async Task<LMCEncoderMaintenanceStartAcknowledgement>
            StartEncoderMaintenanceAsync(
                LMCPreparedEncoderMaintenance preparedCommand,
                CancellationToken cancellationToken)
        {
            ValidatePreparedEncoderMaintenance(preparedCommand, true);
            return await RunStateMutatingAsync(
                () => StartEncoderMaintenanceCore(preparedCommand),
                cancellationToken).ConfigureAwait(false);
        }

        public LMCEncoderMaintenanceOutcomeResult
            ReadEncoderMaintenanceOutcome(
                LMCEncoderMaintenanceRecoveryKey recoveryKey)
        {
            return ReadEncoderMaintenanceOutcomeCore(recoveryKey);
        }

        public async Task<LMCEncoderMaintenanceOutcomeResult>
            ReadEncoderMaintenanceOutcomeAsync(
                LMCEncoderMaintenanceRecoveryKey recoveryKey,
                CancellationToken cancellationToken)
        {
            if (recoveryKey == null)
            {
                throw new ArgumentNullException("recoveryKey");
            }

            return await RunStateMutatingAsync(
                () => ReadEncoderMaintenanceOutcomeCore(recoveryKey),
                cancellationToken).ConfigureAwait(false);
        }

        public LMCEncoderMaintenanceOutcomeRetirementResult
            RetireEncoderMaintenanceOutcome(
                LMCEncoderMaintenanceOutcomeResult terminalOutcome)
        {
            return RetireEncoderMaintenanceOutcomeCore(terminalOutcome);
        }

        public async Task<LMCEncoderMaintenanceOutcomeRetirementResult>
            RetireEncoderMaintenanceOutcomeAsync(
                LMCEncoderMaintenanceOutcomeResult terminalOutcome,
                CancellationToken cancellationToken)
        {
            ValidateTerminalEncoderMaintenanceOutcome(terminalOutcome);
            return await RunStateMutatingAsync(
                () => RetireEncoderMaintenanceOutcomeCore(terminalOutcome),
                cancellationToken).ConfigureAwait(false);
        }

        private LMCPreparedEncoderMaintenance PrepareEncoderMaintenanceCore(
            LMCEncoderMaintenanceRequest request,
            LMCDiagnosticCapabilities verifiedCapabilities,
            long sessionGeneration)
        {
            var originalRequestId = NextRequestId();
            var recoveryKey = new LMCEncoderMaintenanceRecoveryKey(
                ProtocolSchemaVersion,
                originalRequestId,
                verifiedCapabilities.DiagnosticsBuild,
                verifiedCapabilities.DiagnosticsBootId,
                verifiedCapabilities.MapRevision,
                LMCEncoderMaintenanceClientIntentId.Create(),
                request.Kind,
                request.CompatibilityProfileId,
                request.DriveReference,
                request.FeedbackSocket,
                request.TimeoutMilliseconds,
                request.CompatibilityEvidenceId);

            return new LMCPreparedEncoderMaintenance(
                this,
                sessionGeneration,
                verifiedCapabilities,
                request,
                recoveryKey,
                LMCEncoderMaintenanceContract.ExecuteToken(request.Kind));
        }

        private LMCEncoderMaintenanceStartAcknowledgement
            StartEncoderMaintenanceCore(
                LMCPreparedEncoderMaintenance preparedCommand)
        {
            try
            {
                ValidatePreparedEncoderMaintenance(preparedCommand, true);
                var freshCapabilities = GetCapabilities();
                ValidateFreshEncoderMaintenanceIdentity(
                    preparedCommand.RecoveryKey,
                    freshCapabilities,
                    preparedCommand.ConnectionSessionGeneration);

                var request = LMC_DiagnosticsFrame.StartEncoderMaintenance(
                    preparedCommand.RecoveryKey,
                    preparedCommand.ExecuteToken);
                var raw = connection.Exchange(
                    request,
                    preparedCommand.ConnectionSessionGeneration,
                    preparedCommand.ConsumeAtWriteBoundary);
                return LMC_DiagnosticsParser.ParseStartEncoderMaintenance(
                    raw,
                    preparedCommand.RecoveryKey);
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                ThrowStartFailure(preparedCommand, exception, true);
                throw;
            }
            catch (LMCDiagnosticsDispatchRejectedException exception)
            {
                ThrowStartFailure(preparedCommand, exception, true);
                throw;
            }
            catch (LMCDiagnosticsNotSupportedException exception)
            {
                ThrowStartFailure(preparedCommand, exception, true);
                throw;
            }
            catch (Exception exception)
            {
                ThrowStartFailure(preparedCommand, exception, false);
                throw;
            }
        }

        private LMCEncoderMaintenanceOutcomeResult
            ReadEncoderMaintenanceOutcomeCore(
                LMCEncoderMaintenanceRecoveryKey recoveryKey)
        {
            if (recoveryKey == null)
            {
                throw new ArgumentNullException("recoveryKey");
            }

            var sessionGeneration = connection.SessionGeneration;
            var freshCapabilities = GetCapabilities();
            ValidateFreshEncoderMaintenanceIdentity(
                recoveryKey,
                freshCapabilities,
                sessionGeneration);
            var requestId = NextRequestId();
            var request = LMC_DiagnosticsFrame.ReadEncoderMaintenanceOutcome(
                requestId,
                recoveryKey);

            try
            {
                var raw = connection.Exchange(request, sessionGeneration);
                return LMC_DiagnosticsParser.ParseEncoderMaintenanceOutcome(
                    raw,
                    requestId,
                    recoveryKey);
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                throw new LMCEncoderMaintenanceOutcomeQueryException(
                    recoveryKey,
                    exception);
            }
            catch (LMCDiagnosticsDispatchRejectedException exception)
            {
                throw new LMCEncoderMaintenanceOutcomeQueryException(
                    recoveryKey,
                    exception);
            }
            catch (LMCDiagnosticsNotSupportedException exception)
            {
                throw new LMCEncoderMaintenanceOutcomeQueryException(
                    recoveryKey,
                    exception);
            }
        }

        private LMCEncoderMaintenanceOutcomeRetirementResult
            RetireEncoderMaintenanceOutcomeCore(
                LMCEncoderMaintenanceOutcomeResult terminalOutcome)
        {
            ValidateTerminalEncoderMaintenanceOutcome(terminalOutcome);
            var recoveryKey = terminalOutcome.RecoveryKey;
            var sessionGeneration = connection.SessionGeneration;
            var freshCapabilities = GetCapabilities();
            ValidateFreshEncoderMaintenanceIdentity(
                recoveryKey,
                freshCapabilities,
                sessionGeneration);
            var requestId = NextRequestId();
            var request = LMC_DiagnosticsFrame
                .RetireEncoderMaintenanceOutcome(
                    requestId,
                    recoveryKey,
                    terminalOutcome.RecordGeneration);

            try
            {
                var raw = connection.Exchange(request, sessionGeneration);
                return LMC_DiagnosticsParser
                    .ParseEncoderMaintenanceOutcomeRetirement(
                        raw,
                        requestId,
                        terminalOutcome);
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                throw new LMCEncoderMaintenanceOutcomeRetirementException(
                    terminalOutcome,
                    exception);
            }
            catch (LMCDiagnosticsDispatchRejectedException exception)
            {
                throw new LMCEncoderMaintenanceOutcomeRetirementException(
                    terminalOutcome,
                    exception);
            }
            catch (LMCDiagnosticsNotSupportedException exception)
            {
                throw new LMCEncoderMaintenanceOutcomeRetirementException(
                    terminalOutcome,
                    exception);
            }
        }

        private static void ThrowStartFailure(
            LMCPreparedEncoderMaintenance preparedCommand,
            Exception exception,
            bool explicitRejection)
        {
            if (preparedCommand == null || !preparedCommand.IsConsumed)
            {
                return;
            }

            if (explicitRejection)
            {
                throw new LMCEncoderMaintenanceCommandRejectedException(
                    preparedCommand,
                    exception);
            }

            throw new LMCEncoderMaintenanceOutcomeUncertainException(
                preparedCommand,
                exception);
        }

        private void ValidatePreparedEncoderMaintenance(
            LMCPreparedEncoderMaintenance preparedCommand,
            bool requireCurrentObservation)
        {
            if (preparedCommand == null)
            {
                throw new ArgumentNullException("preparedCommand");
            }

            if (!ReferenceEquals(preparedCommand.Owner, this)
                || preparedCommand.Request == null
                || preparedCommand.RecoveryKey == null
                || preparedCommand.VerifiedCapabilities == null
                || preparedCommand.ConnectionSessionGeneration <= 0
                || preparedCommand.ExecuteToken
                    != LMCEncoderMaintenanceContract.ExecuteToken(
                        preparedCommand.Request.Kind))
            {
                throw new InvalidOperationException(
                    "The prepared encoder maintenance command belongs to another diagnostics owner or has invalid identity.");
            }

            preparedCommand.ThrowIfConsumed();
            ValidateEncoderMaintenanceCapabilities(
                preparedCommand.VerifiedCapabilities,
                preparedCommand.Request.Kind,
                preparedCommand.ConnectionSessionGeneration,
                requireCurrentObservation);

            var key = preparedCommand.RecoveryKey;
            var request = preparedCommand.Request;
            if (key.SchemaVersion != ProtocolSchemaVersion
                || key.DiagnosticsBuild
                    != preparedCommand.VerifiedCapabilities.DiagnosticsBuild
                || key.DiagnosticsBootId
                    != preparedCommand.VerifiedCapabilities.DiagnosticsBootId
                || key.MapRevision
                    != preparedCommand.VerifiedCapabilities.MapRevision
                || key.Kind != request.Kind
                || key.CompatibilityProfileId
                    != request.CompatibilityProfileId
                || key.DriveReference != request.DriveReference
                || key.FeedbackSocket != request.FeedbackSocket
                || key.CommandValue != request.CommandValue
                || key.TimeoutMilliseconds
                    != request.TimeoutMilliseconds
                || !key.CompatibilityEvidenceId.Equals(
                    request.CompatibilityEvidenceId))
            {
                throw new InvalidOperationException(
                    "The prepared encoder maintenance recovery key does not match its request and verified capabilities.");
            }
        }

        private void ValidateFreshEncoderMaintenanceIdentity(
            LMCEncoderMaintenanceRecoveryKey recoveryKey,
            LMCDiagnosticCapabilities freshCapabilities,
            long expectedSessionGeneration)
        {
            ValidateEncoderMaintenanceCapabilities(
                freshCapabilities,
                recoveryKey.Kind,
                expectedSessionGeneration,
                true);
            if (freshCapabilities.DiagnosticsBuild
                    != recoveryKey.DiagnosticsBuild
                || freshCapabilities.DiagnosticsBootId
                    != recoveryKey.DiagnosticsBootId
                || freshCapabilities.MapRevision != recoveryKey.MapRevision)
            {
                throw new InvalidOperationException(
                    "Fresh diagnostics Build, BootId, or MapRevision does not match the encoder maintenance recovery key. No maintenance command was sent.");
            }
        }

        private void ValidateEncoderMaintenanceCapabilities(
            LMCDiagnosticCapabilities capabilities,
            LMCEncoderMaintenanceKind kind,
            long expectedSessionGeneration,
            bool requireCurrentObservation)
        {
            if (capabilities == null)
            {
                throw new ArgumentNullException("verifiedCapabilities");
            }

            if (!capabilities.IsBoundTo(this, expectedSessionGeneration)
                || capabilities.ConnectionSessionGeneration
                    != expectedSessionGeneration)
            {
                throw new InvalidOperationException(
                    "The supplied diagnostics capabilities belong to another owner or stale session.");
            }

            if (requireCurrentObservation
                && capabilities.ObservationSequence
                    != CurrentCapabilityObservationSequence)
            {
                throw new InvalidOperationException(
                    "The supplied diagnostics capabilities are not the current observation.");
            }

            var requiredCapability =
                LMCEncoderMaintenanceContract.RequiredCapability(kind);
            if (capabilities.Response == null
                || !capabilities.Response.IsSuccess
                || (capabilities.CapabilityBits & requiredCapability)
                    != requiredCapability
                || capabilities.DiagnosticsBuild == 0
                || capabilities.DiagnosticsBootId == 0
                || capabilities.MapRevision == 0
                || capabilities.MaxRequestPayloadBytes
                    < LMC_DiagnosticsFrame
                        .RetireEncoderMaintenanceOutcomeRequestPayloadLength
                || capabilities.MaxResponsePayloadBytes
                    < LMC_DiagnosticsParser
                        .EncoderMaintenanceOutcomeResponsePayloadLength)
            {
                throw new NotSupportedException(
                    "The PLC does not advertise the complete encoder maintenance start, outcome-query, and retirement contract with stable identity and sufficient payload limits.");
            }

            connection.EnsureSessionGeneration(expectedSessionGeneration);
        }

        private static void ValidateTerminalEncoderMaintenanceOutcome(
            LMCEncoderMaintenanceOutcomeResult terminalOutcome)
        {
            if (terminalOutcome == null)
            {
                throw new ArgumentNullException("terminalOutcome");
            }

            if (!terminalOutcome.IsTerminal
                || terminalOutcome.RecoveryKey == null
                || terminalOutcome.RecordGeneration == 0
                || terminalOutcome.OwnerGeneration == 0)
            {
                throw new ArgumentException(
                    "Only a parsed exact terminal encoder maintenance outcome can be retired.",
                    "terminalOutcome");
            }
        }
    }
}

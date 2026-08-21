using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public sealed partial class LMCAdmin
    {
        private const LMCAdminFeature SetOperationModeCapabilityTriad =
            LMCAdminFeature.AxisSetOperationModeStart
            | LMCAdminFeature.AxisSetOperationModeOutcomeRead
            | LMCAdminFeature.AxisSetOperationModeOutcomeRetire;

        public LMCPreparedAxisSetOperationMode PrepareAxisSetOperationMode(
            LMCSingleAxis axis,
            LMCDriveOperationMode requestedMode,
            uint timeoutMilliseconds,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            LMCAxisSetOperationModeExecuteToken executeToken)
        {
            if (executeToken == null)
            {
                throw new ArgumentNullException("executeToken");
            }

            LMC_AdminFrame.ValidateSetOperationModeRequest(
                requestedMode,
                timeoutMilliseconds,
                0);
            var sessionGeneration = ValidateAxisOwner(axis);
            ValidateAxisSetOperationModeCapabilities(
                verifiedCapabilities,
                sessionGeneration,
                axis.AxisReference,
                true);
            ValidateAxisSetPositionDiagnosticCapabilities(
                verifiedDiagnosticCapabilities,
                sessionGeneration,
                true);

            var clientIntentId =
                LMCAxisSetOperationModeClientIntentId.Create();
            executeToken.ConsumeForPreparation();
            var requestId = NextRequestId();
            var recoveryKey = new LMCAxisSetOperationModeRecoveryKey(
                ProtocolSchemaVersion,
                requestId,
                verifiedDiagnosticCapabilities.DiagnosticsBuild,
                verifiedDiagnosticCapabilities.DiagnosticsBootId,
                verifiedDiagnosticCapabilities.MapRevision,
                clientIntentId,
                axis.AxisReference,
                requestedMode,
                timeoutMilliseconds);
            return new LMCPreparedAxisSetOperationMode(
                connection,
                axis,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                sessionGeneration,
                recoveryKey);
        }

        public LMCAxisSetOperationModeStartAcknowledgement
            StartAxisSetOperationMode(
                LMCPreparedAxisSetOperationMode preparedCommand)
        {
            ValidatePreparedAxisSetOperationMode(preparedCommand);
            var request = LMC_AdminFrame.StartAxisSetOperationMode(
                preparedCommand.RecoveryKey);
            var mutationCoordinator =
                connection.GetAxisPowerOnWaitCoordinator(
                    preparedCommand.ConnectionSessionGeneration,
                    preparedCommand.AxisReference);

            LMCGroupResetObserverScope.ThrowIfMemberMutationReentrant(
                connection,
                preparedCommand.ConnectionSessionGeneration,
                preparedCommand.AxisReference);
            mutationCoordinator.MutationGate.Wait();
            var crossedWriteBoundary = false;
            var reservedMutationGeneration = 0L;
            try
            {
                ValidatePreparedAxisSetOperationMode(preparedCommand);
                try
                {
                    LMCAxisSetOperationModeStartAcknowledgement result = null;
                    LMCAxisSetOperationModeStartAcknowledgement published = null;
                    connection.Exchange(
                        request,
                        preparedCommand.ConnectionSessionGeneration,
                        () =>
                        {
                            ValidatePreparedAxisSetOperationMode(
                                preparedCommand);
                            preparedCommand.Axis
                                .EnsureAdminSetOperationModeMutationAdmission(
                                    request);
                            reservedMutationGeneration = mutationCoordinator
                                .MarkMutationMayHaveBeenSent();
                            try
                            {
                                preparedCommand.ConsumeAtWriteBoundary();
                            }
                            catch
                            {
                                mutationCoordinator.TryRollbackRejectedMutation(
                                    reservedMutationGeneration);
                                throw;
                            }
                            crossedWriteBoundary = true;
                        },
                        response =>
                        {
                            var parsed = LMC_AdminParser
                                .ParseStartAxisSetOperationMode(
                                    response,
                                    preparedCommand.RequestId,
                                    preparedCommand.RequestedMode);
                            result = new
                                LMCAxisSetOperationModeStartAcknowledgement(
                                    parsed.Response,
                                    preparedCommand,
                                    parsed.RequestedMode,
                                    parsed.NativeCommandState);
                            return result.IsAccepted;
                        },
                        () => published = result);
                    if (!result.IsAccepted)
                    {
                        throw new
                            LMCAxisSetOperationModeRejectedException(result);
                    }

                    return published;
                }
                catch (LMCAdminCommandException)
                {
                    if (crossedWriteBoundary)
                    {
                        mutationCoordinator.TryRollbackRejectedMutation(
                            reservedMutationGeneration);
                    }

                    throw;
                }
                catch (Exception ex)
                {
                    if (crossedWriteBoundary)
                    {
                        throw new
                            LMCAxisSetOperationModeOutcomeUncertainException(
                                preparedCommand,
                                InvalidateUncertainSetOperationModeSession(
                                    preparedCommand,
                                    ex));
                    }

                    throw;
                }
            }
            finally
            {
                mutationCoordinator.MutationGate.Release();
            }
        }

        public async Task<LMCAxisSetOperationModeStartAcknowledgement>
            StartAxisSetOperationModeAsync(
                LMCPreparedAxisSetOperationMode preparedCommand,
                CancellationToken cancellationToken)
        {
            ValidatePreparedAxisSetOperationMode(preparedCommand);
            cancellationToken.ThrowIfCancellationRequested();
            var request = LMC_AdminFrame.StartAxisSetOperationMode(
                preparedCommand.RecoveryKey);
            var mutationCoordinator =
                connection.GetAxisPowerOnWaitCoordinator(
                    preparedCommand.ConnectionSessionGeneration,
                    preparedCommand.AxisReference);

            LMCGroupResetObserverScope.ThrowIfMemberMutationReentrant(
                connection,
                preparedCommand.ConnectionSessionGeneration,
                preparedCommand.AxisReference);
            await mutationCoordinator.MutationGate.WaitAsync(
                cancellationToken).ConfigureAwait(false);
            var crossedWriteBoundary = false;
            var reservedMutationGeneration = 0L;
            try
            {
                ValidatePreparedAxisSetOperationMode(preparedCommand);
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    LMCAxisSetOperationModeStartAcknowledgement result = null;
                    LMCAxisSetOperationModeStartAcknowledgement published = null;
                    await connection.ExchangeAsync(
                        request,
                        preparedCommand.ConnectionSessionGeneration,
                        cancellationToken,
                        () =>
                        {
                            ValidatePreparedAxisSetOperationMode(
                                preparedCommand);
                            preparedCommand.Axis
                                .EnsureAdminSetOperationModeMutationAdmission(
                                    request);
                            reservedMutationGeneration = mutationCoordinator
                                .MarkMutationMayHaveBeenSent();
                            try
                            {
                                preparedCommand.ConsumeAtWriteBoundary();
                            }
                            catch
                            {
                                mutationCoordinator.TryRollbackRejectedMutation(
                                    reservedMutationGeneration);
                                throw;
                            }
                            crossedWriteBoundary = true;
                        },
                        response =>
                        {
                            var parsed = LMC_AdminParser
                                .ParseStartAxisSetOperationMode(
                                    response,
                                    preparedCommand.RequestId,
                                    preparedCommand.RequestedMode);
                            result = new
                                LMCAxisSetOperationModeStartAcknowledgement(
                                    parsed.Response,
                                    preparedCommand,
                                    parsed.RequestedMode,
                                    parsed.NativeCommandState);
                            return result.IsAccepted;
                        },
                        () => published = result).ConfigureAwait(false);
                    if (!result.IsAccepted)
                    {
                        throw new
                            LMCAxisSetOperationModeRejectedException(result);
                    }

                    return published;
                }
                catch (LMCAdminCommandException)
                {
                    if (crossedWriteBoundary)
                    {
                        mutationCoordinator.TryRollbackRejectedMutation(
                            reservedMutationGeneration);
                    }

                    throw;
                }
                catch (Exception ex)
                {
                    if (crossedWriteBoundary)
                    {
                        throw new
                            LMCAxisSetOperationModeOutcomeUncertainException(
                                preparedCommand,
                                InvalidateUncertainSetOperationModeSession(
                                    preparedCommand,
                                    ex));
                    }

                    throw;
                }
            }
            finally
            {
                mutationCoordinator.MutationGate.Release();
            }
        }

        public LMCAxisSetOperationModeOutcomeResult
            ReadAxisSetOperationModeOutcome(
                LMCSingleAxis axis,
                LMCAxisSetOperationModeRecoveryKey recoveryKey,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            var sessionGeneration = ValidateAxisOwner(axis);
            ValidateAxisSetOperationModeRecovery(
                axis,
                recoveryKey,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                sessionGeneration);
            var queryRequestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_AdminFrame.ReadAxisSetOperationModeOutcome(
                    queryRequestId,
                    recoveryKey),
                sessionGeneration);
            var parsed = ParseAxisSetOperationModeOutcomeAndFaultSession(
                raw,
                queryRequestId,
                recoveryKey,
                sessionGeneration);
            connection.EnsureSessionGeneration(sessionGeneration);
            return new LMCAxisSetOperationModeOutcomeResult(
                parsed.Response,
                recoveryKey,
                parsed);
        }

        public async Task<LMCAxisSetOperationModeOutcomeResult>
            ReadAxisSetOperationModeOutcomeAsync(
                LMCSingleAxis axis,
                LMCAxisSetOperationModeRecoveryKey recoveryKey,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
                CancellationToken cancellationToken)
        {
            var sessionGeneration = ValidateAxisOwner(axis);
            ValidateAxisSetOperationModeRecovery(
                axis,
                recoveryKey,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                sessionGeneration);
            cancellationToken.ThrowIfCancellationRequested();
            var queryRequestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_AdminFrame.ReadAxisSetOperationModeOutcome(
                    queryRequestId,
                    recoveryKey),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var parsed = ParseAxisSetOperationModeOutcomeAndFaultSession(
                raw,
                queryRequestId,
                recoveryKey,
                sessionGeneration);
            connection.EnsureSessionGeneration(sessionGeneration);
            return new LMCAxisSetOperationModeOutcomeResult(
                parsed.Response,
                recoveryKey,
                parsed);
        }

        public LMCAxisSetOperationModeOutcomeRetirementResult
            RetireAxisSetOperationModeOutcome(
                LMCSingleAxis axis,
                LMCAxisSetOperationModeRecoveryKey recoveryKey,
                uint recordGeneration,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            var sessionGeneration = ValidateAxisOwner(axis);
            ValidateAxisSetOperationModeRecovery(
                axis,
                recoveryKey,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                sessionGeneration);
            if (recordGeneration == 0)
            {
                throw new ArgumentOutOfRangeException("recordGeneration");
            }

            var retireRequestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_AdminFrame.RetireAxisSetOperationModeOutcome(
                    retireRequestId,
                    recoveryKey,
                    recordGeneration),
                sessionGeneration);
            var parsed =
                ParseAxisSetOperationModeRetirementAndFaultSession(
                    raw,
                    retireRequestId,
                    recoveryKey,
                    recordGeneration,
                    sessionGeneration);
            connection.EnsureSessionGeneration(sessionGeneration);
            return new LMCAxisSetOperationModeOutcomeRetirementResult(
                parsed.Response,
                recoveryKey,
                parsed);
        }

        public LMCAxisSetOperationModeOutcomeRetirementResult
            RetireAxisSetOperationModeOutcome(
                LMCSingleAxis axis,
                LMCAxisSetOperationModeOutcomeResult terminalOutcome,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            ValidateTerminalAxisSetOperationModeOutcome(terminalOutcome);
            return RetireAxisSetOperationModeOutcome(
                axis,
                terminalOutcome.RecoveryKey,
                terminalOutcome.RecordGeneration,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities);
        }

        public async Task<LMCAxisSetOperationModeOutcomeRetirementResult>
            RetireAxisSetOperationModeOutcomeAsync(
                LMCSingleAxis axis,
                LMCAxisSetOperationModeRecoveryKey recoveryKey,
                uint recordGeneration,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
                CancellationToken cancellationToken)
        {
            var sessionGeneration = ValidateAxisOwner(axis);
            ValidateAxisSetOperationModeRecovery(
                axis,
                recoveryKey,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                sessionGeneration);
            if (recordGeneration == 0)
            {
                throw new ArgumentOutOfRangeException("recordGeneration");
            }

            cancellationToken.ThrowIfCancellationRequested();
            var retireRequestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_AdminFrame.RetireAxisSetOperationModeOutcome(
                    retireRequestId,
                    recoveryKey,
                    recordGeneration),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var parsed =
                ParseAxisSetOperationModeRetirementAndFaultSession(
                    raw,
                    retireRequestId,
                    recoveryKey,
                    recordGeneration,
                    sessionGeneration);
            connection.EnsureSessionGeneration(sessionGeneration);
            return new LMCAxisSetOperationModeOutcomeRetirementResult(
                parsed.Response,
                recoveryKey,
                parsed);
        }

        public Task<LMCAxisSetOperationModeOutcomeRetirementResult>
            RetireAxisSetOperationModeOutcomeAsync(
                LMCSingleAxis axis,
                LMCAxisSetOperationModeOutcomeResult terminalOutcome,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
                CancellationToken cancellationToken)
        {
            ValidateTerminalAxisSetOperationModeOutcome(terminalOutcome);
            return RetireAxisSetOperationModeOutcomeAsync(
                axis,
                terminalOutcome.RecoveryKey,
                terminalOutcome.RecordGeneration,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                cancellationToken);
        }

        private void ValidatePreparedAxisSetOperationMode(
            LMCPreparedAxisSetOperationMode preparedCommand)
        {
            if (preparedCommand == null)
            {
                throw new ArgumentNullException("preparedCommand");
            }

            if (!ReferenceEquals(preparedCommand.ConnectionOwner, connection)
                || preparedCommand.RecoveryKey == null
                || preparedCommand.RequestId == 0
                || preparedCommand.Axis == null
                || preparedCommand.Axis.AxisReference
                    != preparedCommand.AxisReference
                || !ReferenceEquals(preparedCommand.Axis.Connection, connection)
                || preparedCommand.Axis.SessionGeneration
                    != preparedCommand.ConnectionSessionGeneration)
            {
                throw new InvalidOperationException(
                    "The prepared SetOperationMode command context is invalid.");
            }

            preparedCommand.ThrowIfConsumed();
            ValidateAxisSetOperationModeCapabilities(
                preparedCommand.VerifiedCapabilities,
                preparedCommand.ConnectionSessionGeneration,
                preparedCommand.AxisReference,
                false);
            ValidateAxisSetPositionDiagnosticCapabilities(
                preparedCommand.VerifiedDiagnosticCapabilities,
                preparedCommand.ConnectionSessionGeneration,
                false);
            if (preparedCommand.RecoveryKey.DiagnosticsBuild
                    != preparedCommand.VerifiedDiagnosticCapabilities
                        .DiagnosticsBuild
                || preparedCommand.RecoveryKey.DiagnosticsBootId
                    != preparedCommand.VerifiedDiagnosticCapabilities
                        .DiagnosticsBootId
                || preparedCommand.RecoveryKey.MapRevision
                    != preparedCommand.VerifiedDiagnosticCapabilities
                        .MapRevision)
            {
                throw new InvalidOperationException(
                    "The prepared SetOperationMode diagnostics identity is invalid.");
            }

            preparedCommand.Axis.EnsureCurrentSessionForUse();
        }

        private void ValidateAxisSetOperationModeCapabilities(
            LMCAdminCapabilities capabilities,
            long expectedSessionGeneration,
            ushort axisReference,
            bool requireCurrentObservation)
        {
            if (capabilities == null)
            {
                throw new ArgumentNullException("verifiedCapabilities");
            }

            if (!ReferenceEquals(capabilities.ConnectionOwner, connection)
                || !capabilities.IsBoundTo(this, expectedSessionGeneration))
            {
                throw new InvalidOperationException(
                    "The supplied admin capabilities belong to another owner or session.");
            }

            if (requireCurrentObservation
                && capabilities.ObservationSequence
                    != CurrentCapabilityObservationSequence)
            {
                throw new InvalidOperationException(
                    "The supplied admin capabilities are not the current observation.");
            }

            if (capabilities.Response == null
                || capabilities.Response.SchemaVersion != ProtocolSchemaVersion
                || capabilities.ConnectionSessionGeneration
                    != expectedSessionGeneration)
            {
                throw new InvalidOperationException(
                    "The supplied admin capabilities are stale or use another schema.");
            }

            if (!capabilities.Supports(SetOperationModeCapabilityTriad)
                || capabilities.ErrorCatalogVersion < 6
                || axisReference < 1
                || axisReference > capabilities.PhysicalAxisCount)
            {
                throw new NotSupportedException(
                    "The connected PLC does not advertise the complete SetOperationMode lifecycle for this physical axis.");
            }

            connection.EnsureSessionGeneration(expectedSessionGeneration);
        }

        private void ValidateAxisSetOperationModeRecovery(
            LMCSingleAxis axis,
            LMCAxisSetOperationModeRecoveryKey recoveryKey,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            long expectedSessionGeneration)
        {
            if (recoveryKey == null)
            {
                throw new ArgumentNullException("recoveryKey");
            }

            ValidateAxisSetOperationModeCapabilities(
                verifiedCapabilities,
                expectedSessionGeneration,
                axis.AxisReference,
                true);
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
                    "The SetOperationMode recovery key does not match the current axis, build, and map revision.");
            }

            connection.EnsureSessionGeneration(expectedSessionGeneration);
        }

        private static void ValidateTerminalAxisSetOperationModeOutcome(
            LMCAxisSetOperationModeOutcomeResult terminalOutcome)
        {
            if (terminalOutcome == null)
            {
                throw new ArgumentNullException("terminalOutcome");
            }

            if (terminalOutcome.Response == null
                || !terminalOutcome.Response.IsSuccess
                || terminalOutcome.RecoveryKey == null
                || !terminalOutcome.IsTerminal
                || terminalOutcome.RecordGeneration == 0)
            {
                throw new InvalidOperationException(
                    "Only an exact terminal SetOperationMode outcome with a nonzero generation may be retired.");
            }
        }

        private LMCParsedAxisSetOperationModeOutcome
            ParseAxisSetOperationModeOutcomeAndFaultSession(
                byte[] raw,
                uint queryRequestId,
                LMCAxisSetOperationModeRecoveryKey recoveryKey,
                long expectedSessionGeneration)
        {
            try
            {
                return LMC_AdminParser.ParseAxisSetOperationModeOutcome(
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

        private LMCParsedAxisSetOperationModeOutcome
            ParseAxisSetOperationModeRetirementAndFaultSession(
                byte[] raw,
                uint retireRequestId,
                LMCAxisSetOperationModeRecoveryKey recoveryKey,
                uint recordGeneration,
                long expectedSessionGeneration)
        {
            try
            {
                return LMC_AdminParser
                    .ParseAxisSetOperationModeOutcomeRetirement(
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

        private Exception InvalidateUncertainSetOperationModeSession(
            LMCPreparedAxisSetOperationMode preparedCommand,
            Exception cause)
        {
            try
            {
                connection.TryInvalidateSessionAfterUncertainMutation(
                    preparedCommand.ConnectionSessionGeneration,
                    cause);
                return cause;
            }
            catch (Exception invalidationFailure)
            {
                return new AggregateException(
                    "SetOperationMode outcome and session invalidation both failed.",
                    cause,
                    invalidationFailure);
            }
        }
    }
}

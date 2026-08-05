using System;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public sealed partial class LMCAdmin
    {
        public LMCPreparedAxisDs402Home PrepareAxisDs402Home(
            LMCSingleAxis axis,
            LMCAxisDs402HomeParameters parameters,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            LMCAxisDs402HomeExecuteToken executeToken)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            if (executeToken == null)
            {
                throw new ArgumentNullException("executeToken");
            }

            LMC_AdminFrame.ValidateAxisDs402HomeParameters(
                parameters.HomingMethod,
                parameters.Position,
                parameters.Velocity,
                parameters.Acceleration,
                parameters.DistanceLimit,
                parameters.TorqueLimit,
                parameters.BufferMode,
                parameters.TimeoutMilliseconds);
            var sessionGeneration = ValidateAxisOwner(axis);
            ValidateAxisDs402HomeCapabilities(
                verifiedCapabilities,
                sessionGeneration,
                axis.AxisReference,
                true);
            ValidateAxisDs402HomeDiagnosticCapabilities(
                verifiedDiagnosticCapabilities,
                sessionGeneration,
                true);
            executeToken.ConsumeForPreparation();
            var recoveryKey = new LMCAxisDs402HomeRecoveryKey(
                ProtocolSchemaVersion,
                NextRequestId(),
                verifiedDiagnosticCapabilities.DiagnosticsBuild,
                verifiedDiagnosticCapabilities.DiagnosticsBootId,
                verifiedDiagnosticCapabilities.MapRevision,
                LMCAxisDs402HomeClientIntentId.Create(),
                axis.AxisReference,
                parameters);

            return new LMCPreparedAxisDs402Home(
                connection,
                axis,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                sessionGeneration,
                recoveryKey);
        }

        public LMCAxisDs402HomeStartAcknowledgement StartAxisDs402Home(
            LMCPreparedAxisDs402Home preparedCommand)
        {
            ValidatePreparedAxisDs402Home(preparedCommand, true);
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
                ValidatePreparedAxisDs402Home(preparedCommand, true);
                var freshAdminCapabilities = GetCapabilities();
                var freshDiagnosticCapabilities =
                    connection.Diagnostics.GetCapabilities();
                ValidateFreshAxisDs402HomeIdentity(
                    preparedCommand,
                    freshAdminCapabilities,
                    freshDiagnosticCapabilities);
                var request = LMC_AdminFrame.StartAxisDs402Home(
                    preparedCommand.RecoveryKey);
                try
                {
                    LMCAxisDs402HomeStartAcknowledgement acknowledgement = null;
                    LMCAxisDs402HomeStartAcknowledgement published = null;
                    connection.Exchange(
                        request,
                        preparedCommand.ConnectionSessionGeneration,
                        () =>
                        {
                            ValidatePreparedAxisDs402Home(
                                preparedCommand,
                                false);
                            ValidateFreshAxisDs402HomeIdentity(
                                preparedCommand,
                                freshAdminCapabilities,
                                freshDiagnosticCapabilities);
                            preparedCommand.Axis
                                .EnsureAdminStartAxisDs402HomeMutationAdmission(
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
                            var parsed =
                                LMC_AdminParser.ParseStartAxisDs402Home(
                                    response,
                                    preparedCommand.RequestId,
                                    preparedCommand.Parameters.HomingMethod);
                            acknowledgement =
                                new LMCAxisDs402HomeStartAcknowledgement(
                                    parsed.Response,
                                    preparedCommand,
                                    parsed.HomingMethod,
                                    parsed.NativeCommandState);
                            return acknowledgement.IsAccepted;
                        },
                        () => published = acknowledgement);
                    if (!acknowledgement.IsAccepted)
                    {
                        throw new LMCAxisDs402HomeRejectedException(
                            acknowledgement);
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
                catch (Exception exception)
                {
                    if (crossedWriteBoundary)
                    {
                        throw new LMCAxisDs402HomeOutcomeUncertainException(
                            preparedCommand,
                            InvalidateUncertainAxisDs402HomeSession(
                                preparedCommand,
                                exception));
                    }

                    throw;
                }
            }
            finally
            {
                mutationCoordinator.MutationGate.Release();
            }
        }

        public async Task<LMCAxisDs402HomeStartAcknowledgement>
            StartAxisDs402HomeAsync(
                LMCPreparedAxisDs402Home preparedCommand,
                CancellationToken cancellationToken)
        {
            ValidatePreparedAxisDs402Home(preparedCommand, true);
            cancellationToken.ThrowIfCancellationRequested();
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
                ValidatePreparedAxisDs402Home(preparedCommand, true);
                cancellationToken.ThrowIfCancellationRequested();
                var freshAdminCapabilities = await GetCapabilitiesAsync(
                    cancellationToken).ConfigureAwait(false);
                var freshDiagnosticCapabilities =
                    await connection.Diagnostics.GetCapabilitiesAsync(
                        cancellationToken).ConfigureAwait(false);
                ValidateFreshAxisDs402HomeIdentity(
                    preparedCommand,
                    freshAdminCapabilities,
                    freshDiagnosticCapabilities);
                var request = LMC_AdminFrame.StartAxisDs402Home(
                    preparedCommand.RecoveryKey);
                try
                {
                    LMCAxisDs402HomeStartAcknowledgement acknowledgement = null;
                    LMCAxisDs402HomeStartAcknowledgement published = null;
                    await connection.ExchangeAsync(
                        request,
                        preparedCommand.ConnectionSessionGeneration,
                        cancellationToken,
                        () =>
                        {
                            ValidatePreparedAxisDs402Home(
                                preparedCommand,
                                false);
                            ValidateFreshAxisDs402HomeIdentity(
                                preparedCommand,
                                freshAdminCapabilities,
                                freshDiagnosticCapabilities);
                            preparedCommand.Axis
                                .EnsureAdminStartAxisDs402HomeMutationAdmission(
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
                            var parsed =
                                LMC_AdminParser.ParseStartAxisDs402Home(
                                    response,
                                    preparedCommand.RequestId,
                                    preparedCommand.Parameters.HomingMethod);
                            acknowledgement =
                                new LMCAxisDs402HomeStartAcknowledgement(
                                    parsed.Response,
                                    preparedCommand,
                                    parsed.HomingMethod,
                                    parsed.NativeCommandState);
                            return acknowledgement.IsAccepted;
                        },
                        () => published = acknowledgement)
                        .ConfigureAwait(false);
                    if (!acknowledgement.IsAccepted)
                    {
                        throw new LMCAxisDs402HomeRejectedException(
                            acknowledgement);
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
                catch (Exception exception)
                {
                    if (crossedWriteBoundary)
                    {
                        throw new LMCAxisDs402HomeOutcomeUncertainException(
                            preparedCommand,
                            InvalidateUncertainAxisDs402HomeSession(
                                preparedCommand,
                                exception));
                    }

                    throw;
                }
            }
            finally
            {
                mutationCoordinator.MutationGate.Release();
            }
        }

        private void ValidatePreparedAxisDs402Home(
            LMCPreparedAxisDs402Home preparedCommand,
            bool requireCurrentObservation)
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
                    "The prepared DS402 Home command context is invalid.");
            }

            preparedCommand.ThrowIfConsumed();
            ValidateAxisDs402HomeCapabilities(
                preparedCommand.VerifiedCapabilities,
                preparedCommand.ConnectionSessionGeneration,
                preparedCommand.AxisReference,
                requireCurrentObservation);
            ValidateAxisDs402HomeDiagnosticCapabilities(
                preparedCommand.VerifiedDiagnosticCapabilities,
                preparedCommand.ConnectionSessionGeneration,
                requireCurrentObservation);
            preparedCommand.Axis.EnsureCurrentSessionForUse();
        }

        private void ValidateFreshAxisDs402HomeIdentity(
            LMCPreparedAxisDs402Home preparedCommand,
            LMCAdminCapabilities freshAdminCapabilities,
            LMCDiagnosticCapabilities freshDiagnosticCapabilities)
        {
            ValidateAxisDs402HomeCapabilities(
                freshAdminCapabilities,
                preparedCommand.ConnectionSessionGeneration,
                preparedCommand.AxisReference,
                true);
            ValidateAxisDs402HomeDiagnosticCapabilities(
                freshDiagnosticCapabilities,
                preparedCommand.ConnectionSessionGeneration,
                true);
            if (freshDiagnosticCapabilities.DiagnosticsBuild
                    != preparedCommand.RecoveryKey.DiagnosticsBuild
                || freshDiagnosticCapabilities.DiagnosticsBootId
                    != preparedCommand.RecoveryKey.DiagnosticsBootId
                || freshDiagnosticCapabilities.MapRevision
                    != preparedCommand.RecoveryKey.MapRevision)
            {
                throw new InvalidOperationException(
                    "Fresh diagnostics identity does not match the prepared DS402 Home command. No Home command was sent.");
            }
        }

        private void ValidateAxisDs402HomeCapabilities(
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
                    "The supplied admin capabilities belong to another owner or stale session.");
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
                || !capabilities.Supports(LMCAdminFeature.AxisDs402Home)
                || capabilities.ErrorCatalogVersion < 4
                || axisReference < 1
                || axisReference > capabilities.PhysicalAxisCount)
            {
                throw new NotSupportedException(
                    "The connected PLC does not advertise DS402 Home for this physical axis.");
            }

            connection.EnsureSessionGeneration(expectedSessionGeneration);
        }

        private void ValidateAxisDs402HomeDiagnosticCapabilities(
            LMCDiagnosticCapabilities capabilities,
            long expectedSessionGeneration,
            bool requireCurrentObservation)
        {
            if (capabilities == null)
            {
                throw new ArgumentNullException(
                    "verifiedDiagnosticCapabilities");
            }

            if (!capabilities.IsBoundTo(
                    connection.Diagnostics,
                    expectedSessionGeneration))
            {
                throw new InvalidOperationException(
                    "The supplied diagnostics capabilities belong to another owner or stale session.");
            }

            if (requireCurrentObservation
                && capabilities.ObservationSequence
                    != connection.Diagnostics.CurrentCapabilityObservationSequence)
            {
                throw new InvalidOperationException(
                    "The supplied diagnostics capabilities are not the current observation.");
            }

            if (capabilities.Response == null
                || !capabilities.Response.IsSuccess
                || capabilities.DiagnosticsBuild == 0
                || capabilities.DiagnosticsBootId == 0
                || capabilities.MapRevision == 0)
            {
                throw new NotSupportedException(
                    "DS402 Home requires a successful diagnostics identity observation.");
            }

            connection.EnsureSessionGeneration(expectedSessionGeneration);
        }

        private Exception InvalidateUncertainAxisDs402HomeSession(
            LMCPreparedAxisDs402Home preparedCommand,
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
                    "DS402 Home outcome and session invalidation both failed.",
                    cause,
                    invalidationFailure);
            }
        }
    }
}

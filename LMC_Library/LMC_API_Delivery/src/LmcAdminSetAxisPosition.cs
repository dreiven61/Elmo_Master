using System;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public sealed partial class LMCAdmin
    {
        public LMCPreparedAxisSetPosition PrepareAxisSetPosition(
            LMCSingleAxis axis,
            int targetPosition,
            int expectedActualPosition,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            LMCAxisSetPositionExecuteToken executeToken)
        {
            if (executeToken == null)
            {
                throw new ArgumentNullException("executeToken");
            }

            var sessionGeneration = ValidateAxisOwner(axis);
            ValidateAxisSetPositionCapabilities(
                verifiedCapabilities,
                sessionGeneration,
                axis.AxisReference,
                true);
            ValidateAxisSetPositionDiagnosticCapabilities(
                verifiedDiagnosticCapabilities,
                sessionGeneration,
                true);
            var clientIntentId =
                LMCAxisSetPositionClientIntentId.Create();
            executeToken.ConsumeForPreparation();
            var requestId = NextRequestId();
            var recoveryKey = new LMCAxisSetPositionRecoveryKey(
                ProtocolSchemaVersion,
                requestId,
                verifiedDiagnosticCapabilities.DiagnosticsBuild,
                verifiedDiagnosticCapabilities.DiagnosticsBootId,
                verifiedDiagnosticCapabilities.MapRevision,
                clientIntentId,
                axis.AxisReference,
                targetPosition,
                expectedActualPosition,
                LMCAxisSetPositionSemanticMode
                    .ActualAndDestinationApplicationUnits);

            return new LMCPreparedAxisSetPosition(
                connection,
                axis,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                sessionGeneration,
                recoveryKey);
        }

        public LMCAxisSetPositionResult SetAxisPosition(
            LMCPreparedAxisSetPosition preparedCommand)
        {
            ValidatePreparedAxisSetPosition(preparedCommand);
            var request = LMC_AdminFrame.SetAxisPosition(
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
                ValidatePreparedAxisSetPosition(preparedCommand);
                try
                {
                    LMCAxisSetPositionResult result = null;
                    LMCAxisSetPositionResult publishedResult = null;
                    connection.Exchange(
                        request,
                        preparedCommand.ConnectionSessionGeneration,
                        () =>
                        {
                            ValidatePreparedAxisSetPosition(preparedCommand);
                            preparedCommand.Axis
                                .EnsureAdminSetPositionMutationAdmission(
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
                                LMC_AdminParser.ParseSetAxisPosition(
                                    response,
                                    preparedCommand.RequestId,
                                    preparedCommand.TargetPosition);
                            result = new LMCAxisSetPositionResult(
                                parsed.Response,
                                preparedCommand,
                                parsed.AppliedPosition,
                                parsed.SemanticMode,
                                parsed.NativeCommandState);
                            return result.IsSuccess;
                        },
                        () => publishedResult = result);
                    if (!result.IsSuccess)
                    {
                        throw new LMCAxisSetPositionRejectedException(result);
                    }

                    return publishedResult;
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
                        throw new LMCAxisSetPositionOutcomeUncertainException(
                            preparedCommand,
                            InvalidateUncertainSetPositionSession(
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

        public async Task<LMCAxisSetPositionResult> SetAxisPositionAsync(
            LMCPreparedAxisSetPosition preparedCommand,
            CancellationToken cancellationToken)
        {
            ValidatePreparedAxisSetPosition(preparedCommand);
            cancellationToken.ThrowIfCancellationRequested();
            var request = LMC_AdminFrame.SetAxisPosition(
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
                ValidatePreparedAxisSetPosition(preparedCommand);
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    LMCAxisSetPositionResult result = null;
                    LMCAxisSetPositionResult publishedResult = null;
                    await connection.ExchangeAsync(
                        request,
                        preparedCommand.ConnectionSessionGeneration,
                        cancellationToken,
                        () =>
                        {
                            ValidatePreparedAxisSetPosition(preparedCommand);
                            preparedCommand.Axis
                                .EnsureAdminSetPositionMutationAdmission(
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
                                LMC_AdminParser.ParseSetAxisPosition(
                                    response,
                                    preparedCommand.RequestId,
                                    preparedCommand.TargetPosition);
                            result = new LMCAxisSetPositionResult(
                                parsed.Response,
                                preparedCommand,
                                parsed.AppliedPosition,
                                parsed.SemanticMode,
                                parsed.NativeCommandState);
                            return result.IsSuccess;
                        },
                        () => publishedResult = result)
                        .ConfigureAwait(false);
                    if (!result.IsSuccess)
                    {
                        throw new LMCAxisSetPositionRejectedException(result);
                    }

                    return publishedResult;
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
                        throw new LMCAxisSetPositionOutcomeUncertainException(
                            preparedCommand,
                            InvalidateUncertainSetPositionSession(
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

        private void ValidatePreparedAxisSetPosition(
            LMCPreparedAxisSetPosition preparedCommand)
        {
            if (preparedCommand == null)
            {
                throw new ArgumentNullException("preparedCommand");
            }

            if (!ReferenceEquals(preparedCommand.ConnectionOwner, connection))
            {
                throw new InvalidOperationException(
                    "The prepared SetAxisPosition command belongs to another connection.");
            }

            if (preparedCommand.RecoveryKey == null
                || preparedCommand.SchemaVersion != ProtocolSchemaVersion
                || preparedCommand.RequestId == 0
                || preparedCommand.SemanticMode
                    != LMCAxisSetPositionSemanticMode
                        .ActualAndDestinationApplicationUnits
                || preparedCommand.Axis == null
                || preparedCommand.Axis.AxisReference
                    != preparedCommand.AxisReference
                || !ReferenceEquals(
                    preparedCommand.Axis.Connection,
                    connection)
                || preparedCommand.Axis.SessionGeneration
                    != preparedCommand.ConnectionSessionGeneration)
            {
                throw new InvalidOperationException(
                    "The prepared SetAxisPosition command context is invalid.");
            }

            preparedCommand.ThrowIfConsumed();
            ValidateAxisSetPositionCapabilities(
                preparedCommand.VerifiedCapabilities,
                preparedCommand.ConnectionSessionGeneration,
                preparedCommand.AxisReference,
                false);
            ValidateAxisSetPositionDiagnosticCapabilities(
                preparedCommand.VerifiedDiagnosticCapabilities,
                preparedCommand.ConnectionSessionGeneration,
                false);
            if (preparedCommand.DiagnosticsBuild
                    != preparedCommand.VerifiedDiagnosticCapabilities
                        .DiagnosticsBuild
                || preparedCommand.DiagnosticsBootId
                    != preparedCommand.VerifiedDiagnosticCapabilities
                        .DiagnosticsBootId
                || preparedCommand.MapRevision
                    != preparedCommand.VerifiedDiagnosticCapabilities
                        .MapRevision)
            {
                throw new InvalidOperationException(
                    "The prepared SetAxisPosition diagnostics identity is invalid.");
            }
            preparedCommand.Axis.EnsureCurrentSessionForUse();
        }

        private Exception InvalidateUncertainSetPositionSession(
            LMCPreparedAxisSetPosition preparedCommand,
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
                    "SetAxisPosition outcome and session invalidation both failed.",
                    cause,
                    invalidationFailure);
            }
        }

        private void ValidateAxisSetPositionCapabilities(
            LMCAdminCapabilities capabilities,
            long expectedSessionGeneration,
            ushort axisReference,
            bool requireCurrentObservation)
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
                    expectedSessionGeneration))
            {
                throw new InvalidOperationException(
                    "The supplied admin capabilities are not bound to this admin owner and connection session.");
            }

            if (requireCurrentObservation
                && capabilities.ObservationSequence
                    != CurrentCapabilityObservationSequence)
            {
                throw new InvalidOperationException(
                    "The supplied admin capabilities are not the current observation.");
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

            if (!capabilities.Supports(LMCAdminFeature.AxisSetPosition)
                || !capabilities.Supports(
                    LMCAdminFeature.AxisSetPositionOutcomeRead)
                || !capabilities.Supports(
                    LMCAdminFeature.AxisSetPositionOutcomeRetirement)
                || axisReference < 1
                || axisReference > capabilities.PhysicalAxisCount)
            {
                throw new NotSupportedException(
                    "The connected PLC does not advertise SetAxisPosition for this physical axis.");
            }

            connection.EnsureSessionGeneration(expectedSessionGeneration);
        }

        private void ValidateAxisSetPositionDiagnosticCapabilities(
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
                    "The supplied diagnostics capabilities belong to another owner or stale connection session.");
            }

            if (requireCurrentObservation
                && capabilities.ObservationSequence
                    != connection.Diagnostics
                        .CurrentCapabilityObservationSequence)
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
                    "SetAxisPosition requires a successful diagnostics capability observation with stable build, BootId, and map revision identity.");
            }

            connection.EnsureSessionGeneration(expectedSessionGeneration);
        }
    }
}

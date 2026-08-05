using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public sealed partial class LMCAdmin
    {
        public LMCPreparedHome PrepareLmcHome(
            LMCSingleAxis axis,
            int expectedActualPosition,
            int timeoutMilliseconds,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            LMCHomeExecuteToken executeToken)
        {
            if (executeToken == null)
            {
                throw new ArgumentNullException("executeToken");
            }

            LMC_AdminFrame.ValidateLmcHome(
                LMCHomeSemanticMode.CurrentPositionZero,
                timeoutMilliseconds);
            var sessionGeneration = ValidateAxisOwner(axis);
            ValidateLmcHomeAdminCapabilities(
                verifiedCapabilities,
                sessionGeneration,
                axis.AxisReference,
                true);
            ValidateLmcHomeDiagnosticCapabilities(
                verifiedDiagnosticCapabilities,
                sessionGeneration,
                true);

            var clientIntentId = LMCHomeClientIntentId.Create();
            executeToken.ConsumeForPreparation();
            var recoveryKey = new LMCHomeRecoveryKey(
                ProtocolSchemaVersion,
                NextRequestId(),
                verifiedDiagnosticCapabilities.DiagnosticsBuild,
                verifiedDiagnosticCapabilities.DiagnosticsBootId,
                verifiedDiagnosticCapabilities.MapRevision,
                clientIntentId,
                axis.AxisReference,
                expectedActualPosition,
                timeoutMilliseconds,
                LMCHomeSemanticMode.CurrentPositionZero);
            return new LMCPreparedHome(
                connection,
                axis,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                sessionGeneration,
                recoveryKey);
        }

        public LMCHomeStartAcknowledgement StartLmcHome(
            LMCPreparedHome preparedCommand)
        {
            ValidatePreparedLmcHome(preparedCommand);
            var request = LMC_AdminFrame.StartLmcHome(
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
                ValidatePreparedLmcHome(preparedCommand);
                try
                {
                    LMCHomeStartAcknowledgement acknowledgement = null;
                    LMCHomeStartAcknowledgement published = null;
                    connection.Exchange(
                        request,
                        preparedCommand.ConnectionSessionGeneration,
                        () =>
                        {
                            ValidatePreparedLmcHome(preparedCommand);
                            preparedCommand.Axis
                                .EnsureAdminStartLmcHomeMutationAdmission(
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
                            var parsed = LMC_AdminParser.ParseStartLmcHome(
                                response,
                                preparedCommand.RequestId,
                                preparedCommand.SemanticMode);
                            acknowledgement =
                                new LMCHomeStartAcknowledgement(
                                    parsed.Response,
                                    preparedCommand,
                                    parsed.SemanticMode,
                                    parsed.NativeCommandState);
                            return acknowledgement.IsAccepted;
                        },
                        () => published = acknowledgement);
                    if (!acknowledgement.IsAccepted)
                    {
                        throw new LMCHomeStartRejectedException(
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
                catch (Exception ex)
                {
                    if (crossedWriteBoundary)
                    {
                        throw new
                            LMCHomeStartOutcomeUncertainException(
                                preparedCommand,
                                InvalidateUncertainLmcHomeSession(
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

        public async Task<LMCHomeStartAcknowledgement>
            StartLmcHomeAsync(
                LMCPreparedHome preparedCommand,
                CancellationToken cancellationToken)
        {
            ValidatePreparedLmcHome(preparedCommand);
            cancellationToken.ThrowIfCancellationRequested();
            var request = LMC_AdminFrame.StartLmcHome(
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
                ValidatePreparedLmcHome(preparedCommand);
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    LMCHomeStartAcknowledgement acknowledgement = null;
                    LMCHomeStartAcknowledgement published = null;
                    await connection.ExchangeAsync(
                        request,
                        preparedCommand.ConnectionSessionGeneration,
                        cancellationToken,
                        () =>
                        {
                            ValidatePreparedLmcHome(preparedCommand);
                            preparedCommand.Axis
                                .EnsureAdminStartLmcHomeMutationAdmission(
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
                            var parsed = LMC_AdminParser.ParseStartLmcHome(
                                response,
                                preparedCommand.RequestId,
                                preparedCommand.SemanticMode);
                            acknowledgement =
                                new LMCHomeStartAcknowledgement(
                                    parsed.Response,
                                    preparedCommand,
                                    parsed.SemanticMode,
                                    parsed.NativeCommandState);
                            return acknowledgement.IsAccepted;
                        },
                        () => published = acknowledgement)
                        .ConfigureAwait(false);
                    if (!acknowledgement.IsAccepted)
                    {
                        throw new LMCHomeStartRejectedException(
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
                catch (Exception ex)
                {
                    if (crossedWriteBoundary)
                    {
                        throw new
                            LMCHomeStartOutcomeUncertainException(
                                preparedCommand,
                                InvalidateUncertainLmcHomeSession(
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

        public LMCHomeOutcomeResult ReadLmcHomeOutcome(
            LMCSingleAxis axis,
            LMCHomeRecoveryKey recoveryKey,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            var sessionGeneration = ValidateAxisOwner(axis);
            ValidateLmcHomeRecoveryRequest(
                axis,
                recoveryKey,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                sessionGeneration);
            var queryRequestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_AdminFrame.ReadLmcHomeOutcome(
                    queryRequestId,
                    verifiedDiagnosticCapabilities.DiagnosticsBootId,
                    recoveryKey),
                sessionGeneration);
            var parsed = ParseLmcHomeOutcomeAndFaultMalformedSession(
                raw,
                queryRequestId,
                recoveryKey,
                sessionGeneration);
            connection.EnsureSessionGeneration(sessionGeneration);
            return CreateLmcHomeOutcomeResult(parsed, recoveryKey);
        }

        public async Task<LMCHomeOutcomeResult>
            ReadLmcHomeOutcomeAsync(
                LMCSingleAxis axis,
                LMCHomeRecoveryKey recoveryKey,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
                CancellationToken cancellationToken)
        {
            var sessionGeneration = ValidateAxisOwner(axis);
            ValidateLmcHomeRecoveryRequest(
                axis,
                recoveryKey,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                sessionGeneration);
            cancellationToken.ThrowIfCancellationRequested();
            var queryRequestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_AdminFrame.ReadLmcHomeOutcome(
                    queryRequestId,
                    verifiedDiagnosticCapabilities.DiagnosticsBootId,
                    recoveryKey),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var parsed = ParseLmcHomeOutcomeAndFaultMalformedSession(
                raw,
                queryRequestId,
                recoveryKey,
                sessionGeneration);
            connection.EnsureSessionGeneration(sessionGeneration);
            return CreateLmcHomeOutcomeResult(parsed, recoveryKey);
        }

        public LMCHomeOutcomeRetirementResult
            RetireLmcHomeOutcome(
                LMCSingleAxis axis,
                LMCHomeRecoveryKey recoveryKey,
                uint recordGeneration,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            var sessionGeneration = ValidateAxisOwner(axis);
            ValidateLmcHomeRetirement(
                axis,
                recoveryKey,
                recordGeneration,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                sessionGeneration);
            var retireRequestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_AdminFrame.RetireLmcHomeOutcome(
                    retireRequestId,
                    verifiedDiagnosticCapabilities.DiagnosticsBootId,
                    recoveryKey,
                    recordGeneration),
                sessionGeneration);
            var parsed =
                ParseLmcHomeRetirementAndFaultMalformedSession(
                    raw,
                    retireRequestId,
                    recoveryKey,
                    recordGeneration,
                    sessionGeneration);
            connection.EnsureSessionGeneration(sessionGeneration);
            return new LMCHomeOutcomeRetirementResult(
                CreateLmcHomeOutcomeResult(parsed, recoveryKey));
        }

        public LMCHomeOutcomeRetirementResult
            RetireLmcHomeOutcome(
                LMCSingleAxis axis,
                LMCHomeOutcomeResult terminalOutcome,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            ValidateTerminalLmcHomeOutcome(terminalOutcome);
            return RetireLmcHomeOutcome(
                axis,
                terminalOutcome.RecoveryKey,
                terminalOutcome.RecordGeneration,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities);
        }

        public async Task<LMCHomeOutcomeRetirementResult>
            RetireLmcHomeOutcomeAsync(
                LMCSingleAxis axis,
                LMCHomeRecoveryKey recoveryKey,
                uint recordGeneration,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
                CancellationToken cancellationToken)
        {
            var sessionGeneration = ValidateAxisOwner(axis);
            ValidateLmcHomeRetirement(
                axis,
                recoveryKey,
                recordGeneration,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                sessionGeneration);
            cancellationToken.ThrowIfCancellationRequested();
            var retireRequestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_AdminFrame.RetireLmcHomeOutcome(
                    retireRequestId,
                    verifiedDiagnosticCapabilities.DiagnosticsBootId,
                    recoveryKey,
                    recordGeneration),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var parsed =
                ParseLmcHomeRetirementAndFaultMalformedSession(
                    raw,
                    retireRequestId,
                    recoveryKey,
                    recordGeneration,
                    sessionGeneration);
            connection.EnsureSessionGeneration(sessionGeneration);
            return new LMCHomeOutcomeRetirementResult(
                CreateLmcHomeOutcomeResult(parsed, recoveryKey));
        }

        public Task<LMCHomeOutcomeRetirementResult>
            RetireLmcHomeOutcomeAsync(
                LMCSingleAxis axis,
                LMCHomeOutcomeResult terminalOutcome,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
                CancellationToken cancellationToken)
        {
            ValidateTerminalLmcHomeOutcome(terminalOutcome);
            return RetireLmcHomeOutcomeAsync(
                axis,
                terminalOutcome.RecoveryKey,
                terminalOutcome.RecordGeneration,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                cancellationToken);
        }

        private static void ValidateTerminalLmcHomeOutcome(
            LMCHomeOutcomeResult terminalOutcome)
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
                    "Only a successfully queried terminal LMC_Home outcome may be retired.");
            }
        }

        private void ValidateLmcHomeRetirement(
            LMCSingleAxis axis,
            LMCHomeRecoveryKey recoveryKey,
            uint recordGeneration,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            long expectedSessionGeneration)
        {
            if (recordGeneration == 0)
            {
                throw new ArgumentOutOfRangeException("recordGeneration");
            }

            ValidateLmcHomeRecoveryRequest(
                axis,
                recoveryKey,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                expectedSessionGeneration);
        }

        private void ValidatePreparedLmcHome(
            LMCPreparedHome preparedCommand)
        {
            if (preparedCommand == null)
            {
                throw new ArgumentNullException("preparedCommand");
            }

            if (!ReferenceEquals(preparedCommand.ConnectionOwner, connection)
                || preparedCommand.RecoveryKey == null
                || preparedCommand.SchemaVersion != ProtocolSchemaVersion
                || preparedCommand.RequestId == 0
                || preparedCommand.SemanticMode
                    != LMCHomeSemanticMode.CurrentPositionZero
                || preparedCommand.TargetPosition != 0
                || preparedCommand.Axis == null
                || preparedCommand.Axis.AxisReference
                    != preparedCommand.AxisReference
                || !ReferenceEquals(preparedCommand.Axis.Connection, connection)
                || preparedCommand.Axis.SessionGeneration
                    != preparedCommand.ConnectionSessionGeneration)
            {
                throw new InvalidOperationException(
                    "The prepared LMC_Home command context is invalid.");
            }

            LMC_AdminFrame.ValidateLmcHome(
                preparedCommand.SemanticMode,
                preparedCommand.TimeoutMilliseconds);
            preparedCommand.ThrowIfConsumed();
            ValidateLmcHomeAdminCapabilities(
                preparedCommand.VerifiedCapabilities,
                preparedCommand.ConnectionSessionGeneration,
                preparedCommand.AxisReference,
                false);
            ValidateLmcHomeDiagnosticCapabilities(
                preparedCommand.VerifiedDiagnosticCapabilities,
                preparedCommand.ConnectionSessionGeneration,
                false);
            if (preparedCommand.DiagnosticsBuild
                    != preparedCommand.VerifiedDiagnosticCapabilities
                        .DiagnosticsBuild
                || preparedCommand.OriginalDiagnosticsBootId
                    != preparedCommand.VerifiedDiagnosticCapabilities
                        .DiagnosticsBootId
                || preparedCommand.MapRevision
                    != preparedCommand.VerifiedDiagnosticCapabilities
                        .MapRevision)
            {
                throw new InvalidOperationException(
                    "The prepared LMC_Home diagnostics identity is invalid.");
            }

            preparedCommand.Axis.EnsureCurrentSessionForUse();
        }

        private void ValidateLmcHomeRecoveryRequest(
            LMCSingleAxis axis,
            LMCHomeRecoveryKey recoveryKey,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            long expectedSessionGeneration)
        {
            if (recoveryKey == null)
            {
                throw new ArgumentNullException("recoveryKey");
            }

            ValidateLmcHomeAdminCapabilities(
                verifiedCapabilities,
                expectedSessionGeneration,
                axis.AxisReference,
                true);
            ValidateLmcHomeDiagnosticCapabilities(
                verifiedDiagnosticCapabilities,
                expectedSessionGeneration,
                true);
            LMC_AdminFrame.ValidateLmcHome(
                recoveryKey.SemanticMode,
                recoveryKey.TimeoutMilliseconds);
            if (recoveryKey.SchemaVersion != ProtocolSchemaVersion
                || recoveryKey.AxisReference != axis.AxisReference
                || recoveryKey.TargetPosition != 0
                || recoveryKey.DiagnosticsBuild
                    != verifiedDiagnosticCapabilities.DiagnosticsBuild
                || recoveryKey.MapRevision
                    != verifiedDiagnosticCapabilities.MapRevision)
            {
                throw new InvalidOperationException(
                    "The LMC_Home recovery key does not match the current axis, build, and map revision.");
            }

            connection.EnsureSessionGeneration(expectedSessionGeneration);
        }

        private void ValidateLmcHomeAdminCapabilities(
            LMCAdminCapabilities capabilities,
            long expectedSessionGeneration,
            ushort axisReference,
            bool requireCurrentObservation)
        {
            if (capabilities == null)
            {
                throw new ArgumentNullException("verifiedCapabilities");
            }

            if (!capabilities.IsBoundTo(this, expectedSessionGeneration)
                || capabilities.ConnectionSessionGeneration
                    != expectedSessionGeneration
                || capabilities.Response == null
                || capabilities.Response.SchemaVersion != ProtocolSchemaVersion)
            {
                throw new InvalidOperationException(
                    "The supplied admin capabilities belong to another owner, schema, or session.");
            }

            if (requireCurrentObservation
                && capabilities.ObservationSequence
                    != CurrentCapabilityObservationSequence)
            {
                throw new InvalidOperationException(
                    "The supplied admin capabilities are not the current observation.");
            }

            if (!capabilities.Supports(LMCAdminFeature.AxisHome)
                || axisReference < 1
                || axisReference > capabilities.PhysicalAxisCount)
            {
                throw new NotSupportedException(
                    "The connected PLC does not advertise LMC_Home for this physical axis.");
            }

            connection.EnsureSessionGeneration(expectedSessionGeneration);
        }

        private void ValidateLmcHomeDiagnosticCapabilities(
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
                    "The supplied diagnostics capabilities belong to another owner or session.");
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
                    "LMC_Home requires a successful diagnostics identity observation.");
            }

            connection.EnsureSessionGeneration(expectedSessionGeneration);
        }

        private Exception InvalidateUncertainLmcHomeSession(
            LMCPreparedHome preparedCommand,
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
                    "LMC_Home outcome and session invalidation both failed.",
                    cause,
                    invalidationFailure);
            }
        }

        private LMCParsedHomeOutcome
            ParseLmcHomeOutcomeAndFaultMalformedSession(
                byte[] raw,
                uint requestId,
                LMCHomeRecoveryKey recoveryKey,
                long expectedSessionGeneration)
        {
            try
            {
                return LMC_AdminParser.ParseLmcHomeOutcome(
                    raw,
                    requestId,
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

        private LMCParsedHomeOutcome
            ParseLmcHomeRetirementAndFaultMalformedSession(
                byte[] raw,
                uint requestId,
                LMCHomeRecoveryKey recoveryKey,
                uint recordGeneration,
                long expectedSessionGeneration)
        {
            try
            {
                return LMC_AdminParser.ParseLmcHomeOutcomeRetirement(
                    raw,
                    requestId,
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

        private static LMCHomeOutcomeResult
            CreateLmcHomeOutcomeResult(
                LMCParsedHomeOutcome parsed,
                LMCHomeRecoveryKey recoveryKey)
        {
            return new LMCHomeOutcomeResult(
                parsed.Response,
                recoveryKey,
                parsed.RecordState,
                parsed.OriginalCommandStatus,
                parsed.OriginalErrorId,
                parsed.OriginalDetailCode,
                parsed.AxisStatus,
                parsed.AxisError,
                parsed.RawDrivePositionBefore,
                parsed.RawDrivePositionAfter,
                parsed.ActualApplicationPositionAfter,
                parsed.SetApplicationPositionAfter,
                parsed.ActualInternalPositionAfter,
                parsed.SetInternalPositionAfter,
                parsed.DestinationInternalPositionAfter,
                parsed.MasterInternalPositionAfter,
                parsed.NativeCommandState,
                parsed.EvidenceFlags,
                parsed.StartMilliseconds,
                parsed.CompletionMilliseconds,
                parsed.StopState,
                parsed.RuntimePhase,
                parsed.RecordGeneration);
        }
    }
}

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public sealed partial class LMCAdmin
    {
        /// <summary>
        /// Internal bridge for an already approved engineering profile. The
        /// public engineering-unit Prepare surface remains closed until the
        /// axis scale, rounding, range and method profile is qualified.
        /// </summary>
        internal LMCPreparedAxisDs402HomeEx PrepareAxisDs402HomeExApprovedPlan(
            LMCSingleAxis axis,
            LMCAxisDs402HomeExExecutionPlan executionPlan,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            LMCAxisDs402HomeExExecuteToken executeToken)
        {
            if (executionPlan == null)
            {
                throw new ArgumentNullException("executionPlan");
            }

            if (executeToken == null)
            {
                throw new ArgumentNullException("executeToken");
            }

            var sessionGeneration = ValidateAxisOwner(axis);
            ValidateAxisDs402HomeExCapabilities(
                verifiedCapabilities,
                sessionGeneration,
                axis.AxisReference,
                true);
            ValidateAxisDs402HomeDiagnosticCapabilities(
                verifiedDiagnosticCapabilities,
                sessionGeneration,
                true);
            executeToken.ConsumeForPreparation();

            var recoveryKey = new LMCAxisDs402HomeExRecoveryKey(
                ProtocolSchemaVersion,
                NextRequestId(),
                verifiedDiagnosticCapabilities.DiagnosticsBuild,
                verifiedDiagnosticCapabilities.DiagnosticsBootId,
                verifiedDiagnosticCapabilities.MapRevision,
                LMCAxisDs402HomeExClientIntentId.Create(),
                axis.AxisReference,
                executionPlan);

            return new LMCPreparedAxisDs402HomeEx(
                connection,
                axis,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                sessionGeneration,
                recoveryKey);
        }

        public LMCAxisDs402HomeExStartAcknowledgement StartAxisDs402HomeEx(
            LMCPreparedAxisDs402HomeEx preparedCommand)
        {
            ValidatePreparedAxisDs402HomeEx(preparedCommand, true);
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
                ValidatePreparedAxisDs402HomeEx(preparedCommand, true);
                var freshAdminCapabilities = GetCapabilities();
                var freshDiagnosticCapabilities =
                    connection.Diagnostics.GetCapabilities();
                ValidateFreshAxisDs402HomeExIdentity(
                    preparedCommand,
                    freshAdminCapabilities,
                    freshDiagnosticCapabilities);
                var request = LMC_AdminFrame.StartAxisDs402HomeEx(
                    preparedCommand.RecoveryKey);

                try
                {
                    LMCAxisDs402HomeExStartAcknowledgement acknowledgement = null;
                    LMCAxisDs402HomeExStartAcknowledgement published = null;
                    connection.Exchange(
                        request,
                        preparedCommand.ConnectionSessionGeneration,
                        () =>
                        {
                            ValidatePreparedAxisDs402HomeEx(
                                preparedCommand,
                                false);
                            ValidateFreshAxisDs402HomeExIdentity(
                                preparedCommand,
                                freshAdminCapabilities,
                                freshDiagnosticCapabilities);
                            preparedCommand.Axis
                                .EnsureAdminStartAxisDs402HomeExMutationAdmission(
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
                                .ParseStartAxisDs402HomeEx(
                                    response,
                                    preparedCommand.RequestId,
                                    preparedCommand.ExecutionPlan.HomingMethod);
                            acknowledgement =
                                new LMCAxisDs402HomeExStartAcknowledgement(
                                    parsed.Response,
                                    preparedCommand,
                                    parsed.HomingMethod,
                                    parsed.NativeCommandState);
                            return acknowledgement.IsAccepted;
                        },
                        () => published = acknowledgement);

                    if (!acknowledgement.IsAccepted)
                    {
                        throw new LMCAxisDs402HomeExRejectedException(
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
                        throw new LMCAxisDs402HomeExOutcomeUncertainException(
                            preparedCommand,
                            InvalidateUncertainAxisDs402HomeExSession(
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

        public async Task<LMCAxisDs402HomeExStartAcknowledgement>
            StartAxisDs402HomeExAsync(
                LMCPreparedAxisDs402HomeEx preparedCommand,
                CancellationToken cancellationToken)
        {
            ValidatePreparedAxisDs402HomeEx(preparedCommand, true);
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
                ValidatePreparedAxisDs402HomeEx(preparedCommand, true);
                cancellationToken.ThrowIfCancellationRequested();
                var freshAdminCapabilities = await GetCapabilitiesAsync(
                    cancellationToken).ConfigureAwait(false);
                var freshDiagnosticCapabilities =
                    await connection.Diagnostics.GetCapabilitiesAsync(
                        cancellationToken).ConfigureAwait(false);
                ValidateFreshAxisDs402HomeExIdentity(
                    preparedCommand,
                    freshAdminCapabilities,
                    freshDiagnosticCapabilities);
                var request = LMC_AdminFrame.StartAxisDs402HomeEx(
                    preparedCommand.RecoveryKey);

                try
                {
                    LMCAxisDs402HomeExStartAcknowledgement acknowledgement = null;
                    LMCAxisDs402HomeExStartAcknowledgement published = null;
                    await connection.ExchangeAsync(
                        request,
                        preparedCommand.ConnectionSessionGeneration,
                        cancellationToken,
                        () =>
                        {
                            ValidatePreparedAxisDs402HomeEx(
                                preparedCommand,
                                false);
                            ValidateFreshAxisDs402HomeExIdentity(
                                preparedCommand,
                                freshAdminCapabilities,
                                freshDiagnosticCapabilities);
                            preparedCommand.Axis
                                .EnsureAdminStartAxisDs402HomeExMutationAdmission(
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
                                .ParseStartAxisDs402HomeEx(
                                    response,
                                    preparedCommand.RequestId,
                                    preparedCommand.ExecutionPlan.HomingMethod);
                            acknowledgement =
                                new LMCAxisDs402HomeExStartAcknowledgement(
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
                        throw new LMCAxisDs402HomeExRejectedException(
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
                        throw new LMCAxisDs402HomeExOutcomeUncertainException(
                            preparedCommand,
                            InvalidateUncertainAxisDs402HomeExSession(
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

        public LMCAxisDs402HomeExOutcomeResult ReadAxisDs402HomeExOutcome(
            LMCSingleAxis axis,
            LMCAxisDs402HomeExRecoveryKey recoveryKey,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            var sessionGeneration = ValidateAxisOwner(axis);
            ValidateAxisDs402HomeExRecoveryContext(
                axis,
                recoveryKey,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                sessionGeneration);

            var queryRequestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_AdminFrame.ReadAxisDs402HomeExOutcome(
                    queryRequestId,
                    recoveryKey),
                sessionGeneration);
            var parsed = ParseAxisDs402HomeExOutcomeAndFaultMalformedSession(
                raw,
                queryRequestId,
                recoveryKey,
                sessionGeneration);
            connection.EnsureSessionGeneration(sessionGeneration);
            return CreateAxisDs402HomeExOutcomeResult(parsed, recoveryKey);
        }

        public async Task<LMCAxisDs402HomeExOutcomeResult>
            ReadAxisDs402HomeExOutcomeAsync(
                LMCSingleAxis axis,
                LMCAxisDs402HomeExRecoveryKey recoveryKey,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
                CancellationToken cancellationToken)
        {
            var sessionGeneration = ValidateAxisOwner(axis);
            ValidateAxisDs402HomeExRecoveryContext(
                axis,
                recoveryKey,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                sessionGeneration);
            cancellationToken.ThrowIfCancellationRequested();

            var queryRequestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_AdminFrame.ReadAxisDs402HomeExOutcome(
                    queryRequestId,
                    recoveryKey),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var parsed = ParseAxisDs402HomeExOutcomeAndFaultMalformedSession(
                raw,
                queryRequestId,
                recoveryKey,
                sessionGeneration);
            connection.EnsureSessionGeneration(sessionGeneration);
            return CreateAxisDs402HomeExOutcomeResult(parsed, recoveryKey);
        }

        public LMCAxisDs402HomeExOutcomeRetirementResult
            RetireAxisDs402HomeExOutcome(
                LMCSingleAxis axis,
                LMCAxisDs402HomeExOutcomeResult terminalOutcome,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
        {
            var sessionGeneration = ValidateAxisOwner(axis);
            ValidateAxisDs402HomeExRetirement(
                axis,
                terminalOutcome,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                sessionGeneration);

            var retireRequestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_AdminFrame.RetireAxisDs402HomeExOutcome(
                    retireRequestId,
                    terminalOutcome.RecoveryKey,
                    terminalOutcome.RecordGeneration),
                sessionGeneration);
            var parsed = ParseAxisDs402HomeExRetirementAndFaultMalformedSession(
                raw,
                retireRequestId,
                terminalOutcome.RecoveryKey,
                terminalOutcome.RecordGeneration,
                sessionGeneration);
            connection.EnsureSessionGeneration(sessionGeneration);
            return new LMCAxisDs402HomeExOutcomeRetirementResult(
                CreateAxisDs402HomeExOutcomeResult(
                    parsed,
                    terminalOutcome.RecoveryKey));
        }

        public async Task<LMCAxisDs402HomeExOutcomeRetirementResult>
            RetireAxisDs402HomeExOutcomeAsync(
                LMCSingleAxis axis,
                LMCAxisDs402HomeExOutcomeResult terminalOutcome,
                LMCAdminCapabilities verifiedCapabilities,
                LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
                CancellationToken cancellationToken)
        {
            var sessionGeneration = ValidateAxisOwner(axis);
            ValidateAxisDs402HomeExRetirement(
                axis,
                terminalOutcome,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                sessionGeneration);
            cancellationToken.ThrowIfCancellationRequested();

            var retireRequestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_AdminFrame.RetireAxisDs402HomeExOutcome(
                    retireRequestId,
                    terminalOutcome.RecoveryKey,
                    terminalOutcome.RecordGeneration),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var parsed = ParseAxisDs402HomeExRetirementAndFaultMalformedSession(
                raw,
                retireRequestId,
                terminalOutcome.RecoveryKey,
                terminalOutcome.RecordGeneration,
                sessionGeneration);
            connection.EnsureSessionGeneration(sessionGeneration);
            return new LMCAxisDs402HomeExOutcomeRetirementResult(
                CreateAxisDs402HomeExOutcomeResult(
                    parsed,
                    terminalOutcome.RecoveryKey));
        }

        private static LMCAxisDs402HomeExOutcomeResult
            CreateAxisDs402HomeExOutcomeResult(
                LMCParsedAxisDs402HomeExOutcome parsed,
                LMCAxisDs402HomeExRecoveryKey recoveryKey)
        {
            return new LMCAxisDs402HomeExOutcomeResult(
                parsed.Response,
                recoveryKey,
                parsed.RecordState,
                parsed.OriginalCommandStatus,
                parsed.OriginalErrorId,
                parsed.OriginalDetailCode,
                parsed.Ds402StatusWord,
                parsed.ActualPosition,
                parsed.ExpectedFinalPosition,
                parsed.StartCycle,
                parsed.CompletionCycle,
                parsed.NativeCommandState,
                parsed.RecordGeneration,
                parsed.CleanupProofFlags,
                parsed.SdoExecutorToken);
        }

        private LMCParsedAxisDs402HomeExOutcome
            ParseAxisDs402HomeExOutcomeAndFaultMalformedSession(
                byte[] raw,
                uint queryRequestId,
                LMCAxisDs402HomeExRecoveryKey recoveryKey,
                long expectedSessionGeneration)
        {
            try
            {
                return LMC_AdminParser.ParseAxisDs402HomeExOutcome(
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

        private LMCParsedAxisDs402HomeExOutcome
            ParseAxisDs402HomeExRetirementAndFaultMalformedSession(
                byte[] raw,
                uint retireRequestId,
                LMCAxisDs402HomeExRecoveryKey recoveryKey,
                uint expectedRecordGeneration,
                long expectedSessionGeneration)
        {
            try
            {
                return LMC_AdminParser.ParseRetireAxisDs402HomeExOutcome(
                    raw,
                    retireRequestId,
                    recoveryKey,
                    expectedRecordGeneration);
            }
            catch (InvalidDataException ex)
            {
                connection.TryInvalidateSessionAfterUncertainMutation(
                    expectedSessionGeneration,
                    ex);
                throw;
            }
        }

        private void ValidatePreparedAxisDs402HomeEx(
            LMCPreparedAxisDs402HomeEx preparedCommand,
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
                    "The prepared HomeDS402Ex command context is invalid.");
            }

            preparedCommand.ThrowIfConsumed();
            ValidateAxisDs402HomeExCapabilities(
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

        private void ValidateFreshAxisDs402HomeExIdentity(
            LMCPreparedAxisDs402HomeEx preparedCommand,
            LMCAdminCapabilities freshAdminCapabilities,
            LMCDiagnosticCapabilities freshDiagnosticCapabilities)
        {
            ValidateAxisDs402HomeExCapabilities(
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
                    "Fresh diagnostics identity does not match the prepared HomeDS402Ex command. No Start was sent.");
            }
        }

        private void ValidateAxisDs402HomeExRecoveryContext(
            LMCSingleAxis axis,
            LMCAxisDs402HomeExRecoveryKey recoveryKey,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            long expectedSessionGeneration)
        {
            if (recoveryKey == null)
            {
                throw new ArgumentNullException("recoveryKey");
            }

            ValidateAxisDs402HomeExCapabilities(
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
                    "The HomeDS402Ex recovery key does not match the current axis and fresh diagnostics identity.");
            }

            connection.EnsureSessionGeneration(expectedSessionGeneration);
        }

        private void ValidateAxisDs402HomeExRetirement(
            LMCSingleAxis axis,
            LMCAxisDs402HomeExOutcomeResult terminalOutcome,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            long expectedSessionGeneration)
        {
            if (terminalOutcome == null)
            {
                throw new ArgumentNullException("terminalOutcome");
            }

            if (!terminalOutcome.IsTerminal
                || terminalOutcome.RecordGeneration == 0)
            {
                throw new InvalidOperationException(
                    "HomeDS402Ex retirement requires an exact terminal outcome with a nonzero record generation.");
            }

            ValidateAxisDs402HomeExRecoveryContext(
                axis,
                terminalOutcome.RecoveryKey,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                expectedSessionGeneration);
        }

        private void ValidateAxisDs402HomeExCapabilities(
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
                || !capabilities.Supports(LMCAdminFeature.AxisDs402HomeEx)
                || capabilities.ErrorCatalogVersion < 7
                || axisReference < 1
                || axisReference > capabilities.PhysicalAxisCount)
            {
                throw new NotSupportedException(
                    "The connected PLC does not advertise HomeDS402Ex for this physical axis.");
            }

            connection.EnsureSessionGeneration(expectedSessionGeneration);
        }

        private Exception InvalidateUncertainAxisDs402HomeExSession(
            LMCPreparedAxisDs402HomeEx preparedCommand,
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
                    "HomeDS402Ex outcome and session invalidation both failed.",
                    cause,
                    invalidationFailure);
            }
        }
    }
}

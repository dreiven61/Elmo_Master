using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public partial class LMCGroupAxis
    {
        public LMCGroupDisableWaitContinuation
            PendingGroupDisableWaitContinuation
        {
            get
            {
                lock (groupEnableWaitCoordinator.Sync)
                {
                    return groupEnableWaitCoordinator
                        .PendingDisableContinuation;
                }
            }
        }

        /// <summary>
        /// Retires an accepted GroupDisable continuation without wire traffic
        /// after a newer, same-session GroupPowerOff continuation published
        /// stable PowerOff proof. This records supersession; it does not claim
        /// that GroupDisable reached powered-on Disabled state.
        /// </summary>
        /// <param name="disableContinuation">
        /// Exact latest pending GroupDisable continuation.
        /// </param>
        /// <param name="powerOffContinuation">
        /// Newer completed stable GroupPowerOff continuation from the same
        /// connection, session, and group.
        /// </param>
        /// <returns>
        /// True only when supersession was published atomically. False means
        /// no continuation state or pending pointer was changed.
        /// </returns>
        public bool TryRetirePendingGroupDisableAfterStablePowerOff(
            LMCGroupDisableWaitContinuation disableContinuation,
            LMCGroupPowerStateWaitContinuation powerOffContinuation)
        {
            if (disableContinuation == null)
            {
                throw new ArgumentNullException("disableContinuation");
            }
            if (powerOffContinuation == null)
            {
                throw new ArgumentNullException("powerOffContinuation");
            }

            EnsureCurrentSessionForUse();
            lock (groupEnableWaitCoordinator.Sync)
            {
                if (groupEnableWaitCoordinator.DisableWaitInProgress
                    || groupEnableWaitCoordinator
                        .DisableAcceptanceObserverInProgress)
                {
                    return false;
                }

                if (!disableContinuation.IsPending
                    || !ReferenceEquals(
                        disableContinuation.Coordinator,
                        groupEnableWaitCoordinator)
                    || !disableContinuation.BelongsTo(
                        connection,
                        sessionGeneration,
                        GroupReference)
                    || !ReferenceEquals(
                        groupEnableWaitCoordinator
                            .PendingDisableContinuation,
                        disableContinuation))
                {
                    return false;
                }

                if (powerOffContinuation.ExpectedPowerOn
                    || !powerOffContinuation.IsCompleted
                    || !ReferenceEquals(
                        powerOffContinuation.Coordinator,
                        groupEnableWaitCoordinator)
                    || !powerOffContinuation.BelongsTo(
                        connection,
                        sessionGeneration,
                        GroupReference)
                    || !powerOffContinuation.HasStableProof)
                {
                    return false;
                }

                var powerEvidence = powerOffContinuation.CaptureEvidence(0);
                var currentMutationGeneration =
                    groupEnableWaitCoordinator.MutationGeneration;
                if (!powerEvidence.PowerCommandAccepted
                    || powerEvidence.InterveningMutationDetected
                    || powerEvidence.LastObservedStatus == null
                    || !powerEvidence.LastObservedStatus.IsSuccess
                    || powerEvidence.LastObservedStatus.IsPowerOn
                    || powerEvidence.PowerMutationGeneration
                        <= disableContinuation.DisableMutationGeneration
                    || powerEvidence.PowerMutationGeneration
                        != currentMutationGeneration
                    || powerEvidence.ObservedMutationGeneration
                        != currentMutationGeneration)
                {
                    return false;
                }

                disableContinuation.MarkSupersededByStablePowerOff(
                    powerOffContinuation);
                groupEnableWaitCoordinator.PendingDisableContinuation = null;
                return true;
            }
        }

        public Task<LMCGroupDisableWaitContinuation>
            BeginGroupDisableWaitForStableDisabledAsync(
                CancellationToken cancellationToken)
        {
            return BeginGroupDisableWaitForStableDisabledAsync(
                new LMCGroupDisableWaitOptions(),
                null,
                cancellationToken);
        }

        public Task<LMCGroupDisableWaitContinuation>
            BeginGroupDisableWaitForStableDisabledAsync(
                LMCGroupDisableWaitOptions options,
                CancellationToken cancellationToken)
        {
            return BeginGroupDisableWaitForStableDisabledAsync(
                options,
                null,
                cancellationToken);
        }

        public Task<LMCGroupDisableWaitContinuation>
            BeginGroupDisableWaitForStableDisabledAsync(
                LMCGroupDisableWaitOptions options,
                Action<LMCGroupDisableWaitContinuation>
                    acceptedContinuationObserver,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return BeginGroupDisableWaitForStableDisabledCoreAsync(
                options,
                acceptedContinuationObserver,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                false,
                null);
        }

        internal Task<LMCGroupDisableWaitContinuation>
            BeginGroupDisableWaitForStableDisabledAsync(
                LMCGroupDisableWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Action beforeAcceptedContinuationPublication)
        {
            if (elapsedMilliseconds == null)
            {
                throw new ArgumentNullException("elapsedMilliseconds");
            }
            return BeginGroupDisableWaitForStableDisabledCoreAsync(
                options,
                null,
                cancellationToken,
                elapsedMilliseconds,
                false,
                beforeAcceptedContinuationPublication);
        }

        public Task<LMCGroupDisableWaitResult>
            ResumeGroupDisableWaitForStableDisabledAsync(
                LMCGroupDisableWaitContinuation continuation,
                CancellationToken cancellationToken)
        {
            return ResumeGroupDisableWaitForStableDisabledAsync(
                continuation,
                new LMCGroupDisableWaitOptions
                {
                    StableSampleCount = continuation == null
                        ? LMCGroupDisableWaitOptions
                            .DefaultStableSampleCount
                        : continuation.RequiredStableSampleCount
                },
                cancellationToken);
        }

        public Task<LMCGroupDisableWaitResult>
            ResumeGroupDisableWaitForStableDisabledAsync(
                LMCGroupDisableWaitContinuation continuation,
                LMCGroupDisableWaitOptions options,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return ResumeGroupDisableWaitForStableDisabledCoreAsync(
                continuation,
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync,
                null,
                false);
        }

        internal Task<LMCGroupDisableWaitResult>
            ResumeGroupDisableWaitForStableDisabledAsync(
                LMCGroupDisableWaitContinuation continuation,
                LMCGroupDisableWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync)
        {
            if (elapsedMilliseconds == null)
            {
                throw new ArgumentNullException("elapsedMilliseconds");
            }
            if (delayAsync == null)
            {
                throw new ArgumentNullException("delayAsync");
            }
            return ResumeGroupDisableWaitForStableDisabledCoreAsync(
                continuation,
                options,
                cancellationToken,
                elapsedMilliseconds,
                delayAsync,
                null,
                false);
        }

        internal Task<LMCGroupDisableWaitResult>
            ResumeGroupDisableWaitForStableDisabledAsync(
                LMCGroupDisableWaitContinuation continuation,
                LMCGroupDisableWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action beforeStatusResultPublication)
        {
            if (elapsedMilliseconds == null)
            {
                throw new ArgumentNullException("elapsedMilliseconds");
            }
            if (delayAsync == null)
            {
                throw new ArgumentNullException("delayAsync");
            }
            return ResumeGroupDisableWaitForStableDisabledCoreAsync(
                continuation,
                options,
                cancellationToken,
                elapsedMilliseconds,
                delayAsync,
                beforeStatusResultPublication,
                false);
        }

        internal Task<LMCGroupDisableWaitResult>
            ResumeGroupDisableWaitForStableDisabledAsync(
                LMCGroupDisableWaitContinuation continuation,
                LMCGroupDisableWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action beforeStatusResultPublication,
                Action afterStatusResultPublication)
        {
            if (elapsedMilliseconds == null)
            {
                throw new ArgumentNullException("elapsedMilliseconds");
            }
            if (delayAsync == null)
            {
                throw new ArgumentNullException("delayAsync");
            }
            return ResumeGroupDisableWaitForStableDisabledCoreAsync(
                continuation,
                options,
                cancellationToken,
                elapsedMilliseconds,
                delayAsync,
                beforeStatusResultPublication,
                false,
                afterStatusResultPublication);
        }

        public Task<LMCGroupDisableWaitResult>
            GroupDisableAndWaitForStableDisabledAsync(
                CancellationToken cancellationToken)
        {
            return GroupDisableAndWaitForStableDisabledAsync(
                new LMCGroupDisableWaitOptions(),
                null,
                cancellationToken);
        }

        public Task<LMCGroupDisableWaitResult>
            GroupDisableAndWaitForStableDisabledAsync(
                LMCGroupDisableWaitOptions options,
                CancellationToken cancellationToken)
        {
            return GroupDisableAndWaitForStableDisabledAsync(
                options,
                null,
                cancellationToken);
        }

        public async Task<LMCGroupDisableWaitResult>
            GroupDisableAndWaitForStableDisabledAsync(
                LMCGroupDisableWaitOptions options,
                Action<LMCGroupDisableWaitContinuation>
                    acceptedContinuationObserver,
                CancellationToken cancellationToken)
        {
            var validatedOptions = ValidateGroupDisableWaitOptions(options);
            var stopwatch = Stopwatch.StartNew();
            var continuation = await
                BeginGroupDisableWaitForStableDisabledCoreAsync(
                    validatedOptions,
                    acceptedContinuationObserver,
                    cancellationToken,
                    () => stopwatch.ElapsedMilliseconds,
                    true,
                    null)
                .ConfigureAwait(false);
            return await ResumeGroupDisableWaitForStableDisabledCoreAsync(
                continuation,
                validatedOptions,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync,
                null,
                true).ConfigureAwait(false);
        }

        internal async Task<LMCGroupDisableWaitResult>
            GroupDisableAndWaitForStableDisabledAsync(
                LMCGroupDisableWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action afterAcceptedContinuationPublication)
        {
            if (elapsedMilliseconds == null)
            {
                throw new ArgumentNullException("elapsedMilliseconds");
            }
            if (delayAsync == null)
            {
                throw new ArgumentNullException("delayAsync");
            }
            var validatedOptions = ValidateGroupDisableWaitOptions(options);
            var continuation = await
                BeginGroupDisableWaitForStableDisabledCoreAsync(
                    validatedOptions,
                    null,
                    cancellationToken,
                    elapsedMilliseconds,
                    true,
                    null).ConfigureAwait(false);
            if (afterAcceptedContinuationPublication != null)
            {
                afterAcceptedContinuationPublication();
            }
            return await ResumeGroupDisableWaitForStableDisabledCoreAsync(
                continuation,
                validatedOptions,
                cancellationToken,
                elapsedMilliseconds,
                delayAsync,
                null,
                true).ConfigureAwait(false);
        }

        public Task<LMCGroupStableDisabledWaitResult>
            WaitForStableDisabledAsync(CancellationToken cancellationToken)
        {
            return WaitForStableDisabledAsync(
                new LMCGroupDisableWaitOptions(),
                cancellationToken);
        }

        public Task<LMCGroupStableDisabledWaitResult>
            WaitForStableDisabledAsync(
                LMCGroupDisableWaitOptions options,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return WaitForStableDisabledCoreAsync(
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        internal Task<LMCGroupStableDisabledWaitResult>
            WaitForStableDisabledAsync(
                LMCGroupDisableWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action beforeStatusResultDecision,
                Action afterStatusResultPublication = null)
        {
            if (elapsedMilliseconds == null)
            {
                throw new ArgumentNullException("elapsedMilliseconds");
            }
            if (delayAsync == null)
            {
                throw new ArgumentNullException("delayAsync");
            }
            return WaitForStableDisabledCoreAsync(
                options,
                cancellationToken,
                elapsedMilliseconds,
                delayAsync,
                beforeStatusResultDecision,
                afterStatusResultPublication);
        }

        private async Task<LMCGroupDisableWaitContinuation>
            BeginGroupDisableWaitForStableDisabledCoreAsync(
                LMCGroupDisableWaitOptions options,
                Action<LMCGroupDisableWaitContinuation>
                    acceptedContinuationObserver,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                bool reserveResumeOwnership,
                Action beforeAcceptedContinuationPublication)
        {
            var validatedOptions = ValidateGroupDisableWaitOptions(options);
            var tracker = new LMCGroupDisableWaitTracker(
                validatedOptions.StableSampleCount);
            LMCGroupDisableWaitContinuation continuation = null;
            var statusGateAcquired = false;
            var mutationGateAcquired = false;
            var observerLatchSet = false;
            var observerInvocationActive = false;
            var keepResumeReservation = false;

            try
            {
                EnsureCurrentSessionForUse();
                cancellationToken.ThrowIfCancellationRequested();
                var observerHandoffRemaining =
                    validatedOptions.TimeoutMilliseconds
                    - elapsedMilliseconds();
                if (observerHandoffRemaining <= 0
                    || !WaitForGroupResetObserverHandoffForSafetyMutation(
                        cancellationToken,
                        (int)observerHandoffRemaining))
                {
                    throw new LMCGroupDisableDeadlineException();
                }
                await AcquireGroupDisableGateAsync(
                    groupEnableWaitCoordinator.StatusObservationGate,
                    validatedOptions,
                    cancellationToken,
                    elapsedMilliseconds).ConfigureAwait(false);
                statusGateAcquired = true;
                await AcquireGroupDisableGateAsync(
                    groupEnableWaitCoordinator.MutationGate,
                    validatedOptions,
                    cancellationToken,
                    elapsedMilliseconds).ConfigureAwait(false);
                mutationGateAcquired = true;

                lock (groupEnableWaitCoordinator.Sync)
                {
                    var pending = groupEnableWaitCoordinator
                        .PendingDisableContinuation;
                    if ((pending != null && pending.IsPending)
                        || groupEnableWaitCoordinator
                            .DisableAcceptanceObserverInProgress
                        || groupEnableWaitCoordinator
                            .DisableWaitInProgress)
                    {
                        if (pending != null)
                        {
                            throw new LMCGroupDisableWaitPendingException(
                                pending);
                        }
                        throw new InvalidOperationException(
                            "A GroupDisable wait is finishing; a new 0x2048 request is not allowed yet.");
                    }
                }

                var publication = await SendGroupDisableForWaitAsync(
                    tracker,
                    validatedOptions,
                    acceptedContinuationObserver != null,
                    reserveResumeOwnership,
                    cancellationToken,
                    elapsedMilliseconds,
                    beforeAcceptedContinuationPublication)
                    .ConfigureAwait(false);
                var acknowledgement = publication.Acknowledgement;
                continuation = publication.Continuation;
                if (!acknowledgement.IsSuccess)
                {
                    throw new LMCGroupDisableRejectedException(
                        tracker.CaptureEvidence(
                            false,
                            elapsedMilliseconds()));
                }

                observerLatchSet = acceptedContinuationObserver != null;

                if (mutationGateAcquired)
                {
                    groupEnableWaitCoordinator.MutationGate.Release();
                    mutationGateAcquired = false;
                }
                if (statusGateAcquired)
                {
                    groupEnableWaitCoordinator.StatusObservationGate.Release();
                    statusGateAcquired = false;
                }

                if (acceptedContinuationObserver != null)
                {
                    observerInvocationActive = true;
                    acceptedContinuationObserver(continuation);
                    observerInvocationActive = false;
                }

                ThrowIfGroupDisableCallExpired(
                    validatedOptions,
                    cancellationToken,
                    elapsedMilliseconds);
                keepResumeReservation = reserveResumeOwnership;
                return continuation;
            }
            catch (LMCGroupDisableRejectedException)
            {
                throw;
            }
            catch (LMCGroupDisableWaitPendingException)
            {
                throw;
            }
            catch (LMCGroupDisableDeadlineException)
            {
                throw new LMCGroupDisableWaitTimeoutException(
                    continuation == null
                        ? tracker.CaptureEvidence(
                            false,
                            elapsedMilliseconds())
                        : continuation.CaptureEvidence(
                            elapsedMilliseconds()),
                    continuation);
            }
            catch (OperationCanceledException ex)
            {
                if (observerInvocationActive)
                {
                    throw;
                }

                throw new LMCGroupDisableWaitCanceledException(
                    continuation == null
                        ? tracker.CaptureEvidence(
                            false,
                            elapsedMilliseconds())
                        : continuation.CaptureEvidence(
                            elapsedMilliseconds()),
                    continuation,
                    ex,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                if (continuation != null || observerInvocationActive)
                {
                    throw;
                }

                throw new LMCGroupDisableSubmissionException(
                    tracker.CaptureEvidence(
                        false,
                        elapsedMilliseconds()),
                    ex);
            }
            finally
            {
                if (observerLatchSet)
                {
                    lock (groupEnableWaitCoordinator.Sync)
                    {
                        groupEnableWaitCoordinator
                            .DisableAcceptanceObserverInProgress = false;
                    }
                }
                if (reserveResumeOwnership
                    && continuation != null
                    && !keepResumeReservation)
                {
                    lock (groupEnableWaitCoordinator.Sync)
                    {
                        groupEnableWaitCoordinator.DisableWaitInProgress =
                            false;
                    }
                }
                if (mutationGateAcquired)
                {
                    groupEnableWaitCoordinator.MutationGate.Release();
                }
                if (statusGateAcquired)
                {
                    groupEnableWaitCoordinator.StatusObservationGate.Release();
                }
            }
        }

        private async Task<LMCGroupDisableSubmissionPublication>
            SendGroupDisableForWaitAsync(
            LMCGroupDisableWaitTracker tracker,
            LMCGroupDisableWaitOptions options,
            bool acceptanceObserverWillRun,
            bool reserveResumeOwnership,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            Action beforeAcceptedContinuationPublication)
        {
            var remaining = GetGroupDisableWaitRemaining(
                options,
                cancellationToken,
                elapsedMilliseconds);
            using (var deadlineCancellation =
                new CancellationTokenSource())
            using (var preWriteCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    deadlineCancellation.Token))
            {
                deadlineCancellation.CancelAfter((int)remaining);
                try
                {
                    var raw = await connection.ExchangeAsyncDrainAfterWrite(
                        LMC_Frame.LMCGroupDisable(GroupReference),
                        sessionGeneration,
                        preWriteCancellation.Token,
                        deadlineCancellation.Token,
                        () => ThrowIfGroupDisableCallExpired(
                            options,
                            cancellationToken,
                            elapsedMilliseconds),
                        () =>
                        {
                            tracker.MarkSubmissionOutcomeUncertain();
                            tracker.SetDisableMutationGeneration(
                                groupEnableWaitCoordinator
                                    .MarkMutationMayHaveBeenSent(true));
                            groupEnableWaitCoordinator
                                .ResetPendingMutationProof();
                        }).ConfigureAwait(false);
                var acknowledgement =
                    LMCConnection.ParseCommandAcknowledgement(
                        raw,
                        "GroupDisable");
                if (acknowledgement.IsSuccess)
                {
                    groupEnableWaitCoordinator
                        .FinalizeSafetyMutationAcknowledgement(
                            tracker.DisableMutationGeneration);
                }
                else
                {
                    groupEnableWaitCoordinator
                        .TryRestoreGroupResetAfterRejectedSafetyMutation(
                            tracker.DisableMutationGeneration);
                    tracker.SetAcknowledgement(acknowledgement);
                }
                    if (acknowledgement.IsSuccess
                        && beforeAcceptedContinuationPublication != null)
                    {
                        beforeAcceptedContinuationPublication();
                    }
                    LMCGroupDisableWaitContinuation continuation = null;
                    connection.PublishSessionBoundSendPriorityResult(
                        sessionGeneration,
                        LMC_CommandId.GroupProfileUnlock,
                        () =>
                        {
                            lock (groupEnableWaitCoordinator.Sync)
                            {
                                if (!acknowledgement.IsSuccess)
                                {
                                    return;
                                }

                                tracker.SetAcknowledgement(acknowledgement);

                                continuation =
                                    new LMCGroupDisableWaitContinuation(
                                        groupEnableWaitCoordinator,
                                        connection,
                                        GroupName,
                                        GroupReference,
                                        sessionGeneration,
                                        tracker);
                                groupEnableWaitCoordinator
                                    .PendingDisableContinuation =
                                    continuation;
                                groupEnableWaitCoordinator
                                    .DisableAcceptanceObserverInProgress =
                                    acceptanceObserverWillRun;
                                groupEnableWaitCoordinator
                                    .DisableWaitInProgress =
                                    reserveResumeOwnership;
                                var pendingEnable =
                                    groupEnableWaitCoordinator
                                        .PendingContinuation;
                                if (pendingEnable != null
                                    && pendingEnable.IsPending)
                                {
                                    CompletePendingGroupEnableContinuation(
                                        pendingEnable);
                                }
                            }
                        });
                    return new LMCGroupDisableSubmissionPublication(
                        acknowledgement,
                        continuation);
                }
                catch (LMCPostWriteDeadlineException)
                {
                    tracker.MarkTransportInvalidatedAtDeadline();
                    throw new LMCGroupDisableDeadlineException();
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCGroupDisableDeadlineException();
                }
            }
        }

        private sealed class LMCGroupDisableSubmissionPublication
        {
            internal LMCGroupDisableSubmissionPublication(
                LMC_Response acknowledgement,
                LMCGroupDisableWaitContinuation continuation)
            {
                Acknowledgement = acknowledgement
                    ?? throw new ArgumentNullException("acknowledgement");
                Continuation = continuation;
            }

            internal LMC_Response Acknowledgement { get; private set; }
            internal LMCGroupDisableWaitContinuation Continuation
            {
                get;
                private set;
            }
        }

        private async Task<LMCGroupDisableWaitResult>
            ResumeGroupDisableWaitForStableDisabledCoreAsync(
                LMCGroupDisableWaitContinuation continuation,
                LMCGroupDisableWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action beforeStatusResultPublication = null,
                bool ownsReservedWait = false,
                Action afterStatusResultPublication = null)
        {
            var validatedOptions = ValidateGroupDisableWaitOptions(options);
            EnsureGroupDisableContinuationOwner(continuation);
            if (validatedOptions.StableSampleCount
                != continuation.RequiredStableSampleCount)
            {
                throw new ArgumentException(
                    "StableSampleCount must match the accepted GroupDisable continuation.",
                    "options");
            }

            lock (groupEnableWaitCoordinator.Sync)
            {
                EnsureGroupDisableContinuationOwnerCore(continuation);
                if (groupEnableWaitCoordinator
                    .DisableAcceptanceObserverInProgress)
                {
                    throw new LMCGroupDisableWaitPendingException(
                        continuation);
                }
                if (groupEnableWaitCoordinator.DisableWaitInProgress)
                {
                    if (!ownsReservedWait)
                    {
                        throw new InvalidOperationException(
                            "Another GroupDisable status-only wait is already running.");
                    }
                }
                else if (ownsReservedWait)
                {
                    throw new InvalidOperationException(
                        "The compound GroupDisable resume reservation was lost.");
                }
                else
                {
                    groupEnableWaitCoordinator.DisableWaitInProgress = true;
                }
                continuation.ResetProofCounters();
            }

            var completed = false;
            try
            {
                while (true)
                {
                    ThrowIfGroupDisableMutationIntervened(
                        continuation,
                        elapsedMilliseconds);
                    LMCGroupReadStatusResult status;
                    try
                    {
                        status = await ReadGroupDisableStatusAsync(
                            continuation,
                            validatedOptions,
                            cancellationToken,
                            elapsedMilliseconds,
                            beforeStatusResultPublication)
                            .ConfigureAwait(false);
                        if (afterStatusResultPublication != null)
                        {
                            afterStatusResultPublication();
                        }
                    }
                    catch (LMCGroupDisableInterferenceException)
                    {
                        throw;
                    }
                    catch (LMCGroupDisableDeadlineException)
                    {
                        throw new LMCGroupDisableWaitTimeoutException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation);
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new LMCGroupDisableWaitCanceledException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation,
                            ex,
                            cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        throw new LMCGroupDisableStatusException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation,
                            null,
                            ex);
                    }

                    if (continuation.IsCompleted)
                    {
                        completed = true;
                        return new LMCGroupDisableWaitResult(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation);
                    }
                    if (!status.IsSuccess)
                    {
                        throw new LMCGroupDisableStatusException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation,
                            status,
                            null);
                    }
                    if (!status.IsPowerOn)
                    {
                        throw new LMCGroupDisableInterferenceException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation);
                    }

                    await DelayGroupDisableWaitAsync(
                        validatedOptions,
                        cancellationToken,
                        elapsedMilliseconds,
                        delayAsync).ConfigureAwait(false);
                }
            }
            catch (LMCGroupDisableWaitTimeoutException)
            {
                throw;
            }
            catch (LMCGroupDisableWaitCanceledException)
            {
                throw;
            }
            catch (LMCGroupDisableStatusException)
            {
                throw;
            }
            catch (LMCGroupDisableInterferenceException)
            {
                throw;
            }
            catch (LMCGroupDisableDeadlineException)
            {
                throw new LMCGroupDisableWaitTimeoutException(
                    continuation.CaptureEvidence(elapsedMilliseconds()),
                    continuation);
            }
            catch (OperationCanceledException ex)
            {
                throw new LMCGroupDisableWaitCanceledException(
                    continuation.CaptureEvidence(elapsedMilliseconds()),
                    continuation,
                    ex,
                    cancellationToken);
            }
            finally
            {
                lock (groupEnableWaitCoordinator.Sync)
                {
                    if (!completed && continuation.IsPending)
                    {
                        continuation.ResetProofCounters();
                    }
                    groupEnableWaitCoordinator.DisableWaitInProgress = false;
                }
            }
        }

        private async Task<LMCGroupReadStatusResult>
            ReadGroupDisableStatusAsync(
                LMCGroupDisableWaitContinuation continuation,
                LMCGroupDisableWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Action beforeStatusResultPublication)
        {
            var statusGateAcquired = false;
            var mutationGateAcquired = false;
            try
            {
                await AcquireGroupDisableGateAsync(
                    groupEnableWaitCoordinator.StatusObservationGate,
                    options,
                    cancellationToken,
                    elapsedMilliseconds).ConfigureAwait(false);
                statusGateAcquired = true;
                await AcquireGroupDisableGateAsync(
                    groupEnableWaitCoordinator.MutationGate,
                    options,
                    cancellationToken,
                    elapsedMilliseconds).ConfigureAwait(false);
                mutationGateAcquired = true;
                ThrowIfGroupDisableMutationIntervened(
                    continuation,
                    elapsedMilliseconds);

                var remaining = GetGroupDisableWaitRemaining(
                    options,
                    cancellationToken,
                    elapsedMilliseconds);
                using (var deadlineCancellation =
                    new CancellationTokenSource())
                using (var preWriteCancellation =
                    CancellationTokenSource.CreateLinkedTokenSource(
                        cancellationToken,
                        deadlineCancellation.Token))
                {
                    deadlineCancellation.CancelAfter((int)remaining);
                    byte[] raw;
                    try
                    {
                        raw = await connection.ExchangeAsyncDrainAfterWrite(
                            LMC_Frame.LMCGroupReadStatus(GroupReference),
                            sessionGeneration,
                            preWriteCancellation.Token,
                            deadlineCancellation.Token,
                            () => ThrowIfGroupDisableCallExpired(
                                options,
                                cancellationToken,
                                elapsedMilliseconds),
                            null).ConfigureAwait(false);
                    }
                    catch (LMCPostWriteDeadlineException)
                    {
                        continuation.MarkTransportInvalidatedAtDeadline();
                        throw new LMCGroupDisableDeadlineException();
                    }
                    catch (OperationCanceledException)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        throw new LMCGroupDisableDeadlineException();
                    }

                    var status =
                        LMCConnection.ParseGroupReadStatusResult(raw);
                    if (beforeStatusResultPublication != null)
                    {
                        beforeStatusResultPublication();
                    }
                    var completed = false;
                    connection.PublishSessionBoundSendPriorityResult(
                        sessionGeneration,
                        LMC_CommandId.GroupStatus,
                        () =>
                        {
                            long actualGeneration;
                            if (!groupEnableWaitCoordinator
                                .TryPublishForMutationGeneration(
                                    continuation.DisableMutationGeneration,
                                    () =>
                                    {
                                        EnsureGroupDisableContinuationOwnerCore(
                                            continuation);
                                        continuation.Observe(status);
                                        if (continuation
                                                .HasStableDisabledProof
                                            && !cancellationToken
                                                .IsCancellationRequested
                                            && !deadlineCancellation
                                                .IsCancellationRequested
                                            && elapsedMilliseconds()
                                                < options
                                                    .TimeoutMilliseconds)
                                        {
                                            continuation.MarkCompleted();
                                            groupEnableWaitCoordinator
                                                .PendingDisableContinuation =
                                                null;
                                            completed = true;
                                        }
                                    },
                                    out actualGeneration))
                            {
                                continuation.ObserveMutationGeneration(
                                    actualGeneration);
                            }
                        });
                    if (!completed)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (deadlineCancellation.IsCancellationRequested
                            || elapsedMilliseconds()
                                >= options.TimeoutMilliseconds)
                        {
                            throw new LMCGroupDisableDeadlineException();
                        }
                        ThrowIfGroupDisableMutationIntervened(
                            continuation,
                            elapsedMilliseconds);
                    }
                    return status;
                }
            }
            finally
            {
                if (mutationGateAcquired)
                {
                    groupEnableWaitCoordinator.MutationGate.Release();
                }
                if (statusGateAcquired)
                {
                    groupEnableWaitCoordinator.StatusObservationGate.Release();
                }
            }
        }

        private async Task<LMCGroupStableDisabledWaitResult>
            WaitForStableDisabledCoreAsync(
                LMCGroupDisableWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action beforeStatusResultDecision = null,
                Action afterStatusResultPublication = null)
        {
            var validatedOptions = ValidateGroupDisableWaitOptions(options);
            var tracker = new LMCGroupStableDisabledWaitTracker(
                validatedOptions.StableSampleCount);
            try
            {
                while (true)
                {
                    var lastStatus = await ReadStableDisabledStatusOnlyAsync(
                        tracker,
                        validatedOptions,
                        cancellationToken,
                        elapsedMilliseconds,
                        beforeStatusResultDecision,
                        afterStatusResultPublication)
                        .ConfigureAwait(false);
                    if (!lastStatus.IsSuccess || !lastStatus.IsPowerOn)
                    {
                        throw new LMCGroupStableDisabledStatusException(
                            tracker.CaptureEvidence(elapsedMilliseconds()),
                            lastStatus,
                            null);
                    }
                    if (tracker.CompletionPublished)
                    {
                        return new LMCGroupStableDisabledWaitResult(
                            tracker.CaptureEvidence(elapsedMilliseconds()));
                    }

                    await DelayGroupDisableWaitAsync(
                        validatedOptions,
                        cancellationToken,
                        elapsedMilliseconds,
                        delayAsync).ConfigureAwait(false);
                }
            }
            catch (LMCGroupStableDisabledStatusException)
            {
                throw;
            }
            catch (LMCGroupDisableDeadlineException)
            {
                throw new LMCGroupStableDisabledWaitTimeoutException(
                    tracker.CaptureEvidence(elapsedMilliseconds()));
            }
            catch (OperationCanceledException ex)
            {
                throw new LMCGroupStableDisabledWaitCanceledException(
                    tracker.CaptureEvidence(elapsedMilliseconds()),
                    ex,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                var evidence = tracker.CaptureEvidence(
                    elapsedMilliseconds());
                throw new LMCGroupStableDisabledStatusException(
                    evidence,
                    evidence.LastObservedStatus,
                    ex);
            }
        }

        private async Task<LMCGroupReadStatusResult>
            ReadStableDisabledStatusOnlyAsync(
                LMCGroupStableDisabledWaitTracker tracker,
                LMCGroupDisableWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Action beforeStatusResultPublication,
                Action afterStatusResultPublication)
        {
            var statusGateAcquired = false;
            var mutationGateAcquired = false;
            try
            {
                await AcquireGroupDisableGateAsync(
                    groupEnableWaitCoordinator.StatusObservationGate,
                    options,
                    cancellationToken,
                    elapsedMilliseconds).ConfigureAwait(false);
                statusGateAcquired = true;
                await AcquireGroupDisableGateAsync(
                    groupEnableWaitCoordinator.MutationGate,
                    options,
                    cancellationToken,
                    elapsedMilliseconds).ConfigureAwait(false);
                mutationGateAcquired = true;
                var remaining = GetGroupDisableWaitRemaining(
                    options,
                    cancellationToken,
                    elapsedMilliseconds);
                using (var deadlineCancellation =
                    new CancellationTokenSource())
                using (var preWriteCancellation =
                    CancellationTokenSource.CreateLinkedTokenSource(
                        cancellationToken,
                        deadlineCancellation.Token))
                {
                    deadlineCancellation.CancelAfter((int)remaining);
                    try
                    {
                        var raw = await connection.ExchangeAsyncDrainAfterWrite(
                            LMC_Frame.LMCGroupReadStatus(GroupReference),
                            sessionGeneration,
                            preWriteCancellation.Token,
                            deadlineCancellation.Token,
                            () => ThrowIfGroupDisableCallExpired(
                                options,
                                cancellationToken,
                                elapsedMilliseconds),
                            null)
                            .ConfigureAwait(false);
                        var status =
                            LMCConnection.ParseGroupReadStatusResult(raw);
                        if (beforeStatusResultPublication != null)
                        {
                            beforeStatusResultPublication();
                        }
                        LMCGroupReadStatusResult publishedStatus = null;
                        connection.PublishSessionBoundSendPriorityResult(
                            sessionGeneration,
                            LMC_CommandId.GroupStatus,
                            () =>
                            {
                                publishedStatus = status;
                                tracker.Observe(status);
                                if (tracker.HasStableProof
                                    && !cancellationToken
                                        .IsCancellationRequested
                                    && !deadlineCancellation
                                        .IsCancellationRequested
                                    && elapsedMilliseconds()
                                        < options.TimeoutMilliseconds)
                                {
                                    tracker.MarkCompletionPublished();
                                }
                            });
                        if (afterStatusResultPublication != null)
                        {
                            afterStatusResultPublication();
                        }
                        if (!tracker.CompletionPublished)
                        {
                            ThrowIfGroupDisableCallExpired(
                                options,
                                cancellationToken,
                                elapsedMilliseconds);
                            if (deadlineCancellation
                                .IsCancellationRequested)
                            {
                                throw new LMCGroupDisableDeadlineException();
                            }
                        }
                        return publishedStatus;
                    }
                    catch (LMCPostWriteDeadlineException)
                    {
                        tracker.MarkTransportInvalidatedAtDeadline();
                        throw new LMCGroupDisableDeadlineException();
                    }
                    catch (OperationCanceledException)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        throw new LMCGroupDisableDeadlineException();
                    }
                }
            }
            finally
            {
                if (mutationGateAcquired)
                {
                    groupEnableWaitCoordinator.MutationGate.Release();
                }
                if (statusGateAcquired)
                {
                    groupEnableWaitCoordinator.StatusObservationGate.Release();
                }
            }
        }

        private void EnsureGroupDisableContinuationOwner(
            LMCGroupDisableWaitContinuation continuation)
        {
            EnsureCurrentSessionForUse();
            lock (groupEnableWaitCoordinator.Sync)
            {
                EnsureGroupDisableContinuationOwnerCore(continuation);
            }
        }

        private void EnsureGroupDisableContinuationOwnerCore(
            LMCGroupDisableWaitContinuation continuation)
        {
            if (continuation != null && !continuation.IsPending)
            {
                throw new LMCGroupDisableWaitResolvedException(continuation);
            }
            if (continuation == null
                || !continuation.IsPending
                || !ReferenceEquals(
                    continuation.Coordinator,
                    groupEnableWaitCoordinator)
                || !continuation.BelongsTo(
                    connection,
                    sessionGeneration,
                    GroupReference)
                || !ReferenceEquals(
                    groupEnableWaitCoordinator.PendingDisableContinuation,
                    continuation))
            {
                throw new InvalidOperationException(
                    "The GroupDisable continuation does not belong to this active connection, session, group, or pending operation.");
            }
        }

        private void ThrowIfGroupDisableMutationIntervened(
            LMCGroupDisableWaitContinuation continuation,
            Func<long> elapsedMilliseconds)
        {
            var actualGeneration =
                groupEnableWaitCoordinator.MutationGeneration;
            continuation.ObserveMutationGeneration(actualGeneration);
            if (continuation.DisableMutationGeneration <= 0
                || actualGeneration
                    != continuation.DisableMutationGeneration)
            {
                throw new LMCGroupDisableInterferenceException(
                    continuation.CaptureEvidence(elapsedMilliseconds()),
                    continuation);
            }
        }

        private void ThrowIfRawGroupDisableCommandIsUnsafe()
        {
            lock (groupEnableWaitCoordinator.Sync)
            {
                var pending = groupEnableWaitCoordinator
                    .PendingDisableContinuation;
                if (pending != null && pending.IsPending)
                {
                    throw new LMCGroupDisableWaitPendingException(pending);
                }
                if (groupEnableWaitCoordinator.DisableWaitInProgress
                    || groupEnableWaitCoordinator
                        .DisableAcceptanceObserverInProgress)
                {
                    throw new InvalidOperationException(
                        "A GroupDisable wait is active or finishing; raw 0x2048 is not allowed.");
                }
            }
        }

        private static LMCGroupDisableWaitOptions
            ValidateGroupDisableWaitOptions(
                LMCGroupDisableWaitOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            return options.SnapshotAndValidate();
        }

        private static async Task AcquireGroupDisableGateAsync(
            SemaphoreSlim gate,
            LMCGroupDisableWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds)
        {
            var remaining = GetGroupDisableWaitRemaining(
                options,
                cancellationToken,
                elapsedMilliseconds);
            using (var deadlineCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken))
            {
                deadlineCancellation.CancelAfter((int)remaining);
                try
                {
                    await gate.WaitAsync(deadlineCancellation.Token)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCGroupDisableDeadlineException();
                }
            }
        }

        private static async Task DelayGroupDisableWaitAsync(
            LMCGroupDisableWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            Func<int, CancellationToken, Task> delayAsync)
        {
            var remaining = GetGroupDisableWaitRemaining(
                options,
                cancellationToken,
                elapsedMilliseconds);
            using (var deadlineCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken))
            {
                deadlineCancellation.CancelAfter((int)remaining);
                try
                {
                    await delayAsync(
                        Math.Min(
                            options.PollIntervalMilliseconds,
                            (int)remaining),
                        deadlineCancellation.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCGroupDisableDeadlineException();
                }
            }
        }

        private static long GetGroupDisableWaitRemaining(
            LMCGroupDisableWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var remaining = options.TimeoutMilliseconds
                - elapsedMilliseconds();
            if (remaining <= 0)
            {
                throw new LMCGroupDisableDeadlineException();
            }
            return remaining;
        }

        private static void ThrowIfGroupDisableCallExpired(
            LMCGroupDisableWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (elapsedMilliseconds() >= options.TimeoutMilliseconds)
            {
                throw new LMCGroupDisableDeadlineException();
            }
        }

        private sealed class LMCGroupDisableDeadlineException
            : TimeoutException
        {
        }
    }
}

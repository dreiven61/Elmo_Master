using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public partial class LMCSingleAxis
    {
        /// <summary>
        /// The latest accepted Axis Reset whose stable error clearance state has not
        /// yet been resolved. Resuming this continuation sends status reads
        /// only.
        /// </summary>
        public LMCAxisResetWaitContinuation PendingResetWaitContinuation
        {
            get
            {
                lock (powerOnWaitCoordinator.Sync)
                {
                    return powerOnWaitCoordinator.PendingResetContinuation;
                }
            }
        }

        /// <summary>
        /// Sends exactly one 0x2024 Axis Reset request and returns after its
        /// successful acknowledgement is preserved. No 0x2028 status read is
        /// sent by this method.
        /// </summary>
        public Task<LMCAxisResetWaitContinuation>
            BeginResetWaitForStableErrorClearanceAsync(
                CancellationToken cancellationToken)
        {
            return BeginResetWaitForStableErrorClearanceAsync(
                new LMCAxisResetWaitOptions(),
                cancellationToken);
        }

        public Task<LMCAxisResetWaitContinuation>
            BeginResetWaitForStableErrorClearanceAsync(
                LMCAxisResetWaitOptions options,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return BeginResetWaitForStableErrorClearanceAsync(
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        public Task<LMCAxisResetWaitContinuation>
            BeginResetWaitForStableErrorClearanceAsync(
                Action<LMCAxisResetWaitContinuation>
                    acceptedContinuationObserver,
                CancellationToken cancellationToken)
        {
            return BeginResetWaitForStableErrorClearanceAsync(
                new LMCAxisResetWaitOptions(),
                acceptedContinuationObserver,
                cancellationToken);
        }

        public Task<LMCAxisResetWaitContinuation>
            BeginResetWaitForStableErrorClearanceAsync(
                LMCAxisResetWaitOptions options,
                Action<LMCAxisResetWaitContinuation>
                    acceptedContinuationObserver,
                CancellationToken cancellationToken)
        {
            if (acceptedContinuationObserver == null)
            {
                throw new ArgumentNullException(
                    "acceptedContinuationObserver");
            }

            var stopwatch = Stopwatch.StartNew();
            return BeginResetWaitForStableErrorClearanceAsync(
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync,
                null,
                null,
                acceptedContinuationObserver);
        }

        internal async Task<LMCAxisResetWaitContinuation>
            BeginResetWaitForStableErrorClearanceAsync(
                LMCAxisResetWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action beforeAcceptedContinuationPublication = null,
                Action beforeResetWriteCommit = null,
                Action<LMCAxisResetWaitContinuation>
                    acceptedContinuationObserver = null)
        {
            var validated = ValidateAxisResetWaitOptions(
                options,
                elapsedMilliseconds,
                delayAsync);
            var request = LMC_Frame.LMCAxisReset(AxisReference);
            var tracker = new LMCAxisResetWaitTracker(
                validated.StableSampleCount);
            LMCAxisResetWaitContinuation continuation = null;
            var mutationGateAcquired = false;
            var observerStatusGateAcquired = false;
            var observerLatchSet = false;
            var observerInvocationActive = false;

            try
            {
                EnsureCurrentSessionForUse();
                cancellationToken.ThrowIfCancellationRequested();
                if (acceptedContinuationObserver != null)
                {
                    await AcquireAxisResetStatusGateAsync(
                        validated,
                        cancellationToken,
                        elapsedMilliseconds).ConfigureAwait(false);
                    observerStatusGateAcquired = true;
                    EnsureCurrentSessionForUse();
                }
                await AcquireAxisResetMutationGateAsync(
                    validated,
                    cancellationToken,
                    elapsedMilliseconds).ConfigureAwait(false);
                mutationGateAcquired = true;
                EnsureCurrentSessionForUse();
                EnsureResetSubmissionAdmission();

                var acknowledgement = await SendAxisResetForWaitAsync(
                    request,
                    tracker,
                    validated,
                    cancellationToken,
                    elapsedMilliseconds,
                    beforeResetWriteCommit).ConfigureAwait(false);
                if (!acknowledgement.IsSuccess)
                {
                    connection.PublishSessionBoundSendPriorityResult(
                        sessionGeneration,
                        LMC_CommandId.Reset,
                        () => tracker.SetAcknowledgement(
                            acknowledgement));
                    throw new LMCAxisResetRejectedException(
                        tracker.CaptureEvidence(elapsedMilliseconds()));
                }

                if (beforeAcceptedContinuationPublication != null)
                {
                    beforeAcceptedContinuationPublication();
                }

                connection.PublishSessionBoundSendPriorityResult(
                    sessionGeneration,
                    LMC_CommandId.Reset,
                    () =>
                    {
                        lock (powerOnWaitCoordinator.Sync)
                        {
                            EnsureResetSubmissionAdmissionCore();
                            tracker.SetAcknowledgement(acknowledgement);
                            var acceptedContinuation =
                                new LMCAxisResetWaitContinuation(
                                    powerOnWaitCoordinator,
                                    connection,
                                    AxisName,
                                    AxisReference,
                                    sessionGeneration,
                                    tracker);
                            var previous = powerOnWaitCoordinator
                                .PendingResetContinuation;
                            if (previous != null && previous.IsPending)
                            {
                                previous.MarkSuperseded();
                            }

                            powerOnWaitCoordinator
                                .PendingResetContinuation =
                                acceptedContinuation;
                            powerOnWaitCoordinator
                                .ResetAcceptanceObserverInProgress =
                                acceptedContinuationObserver != null;
                            continuation = acceptedContinuation;
                        }
                    });

                observerLatchSet = continuation != null
                    && acceptedContinuationObserver != null;
                if (acceptedContinuationObserver != null)
                {
                    powerOnWaitCoordinator.MutationGate.Release();
                    mutationGateAcquired = false;
                    powerOnWaitCoordinator.StatusObservationGate.Release();
                    observerStatusGateAcquired = false;
                    observerInvocationActive = true;
                    acceptedContinuationObserver(continuation);
                    observerInvocationActive = false;
                }

                ThrowIfAxisResetWaitExpiredAfterPublication(
                    cancellationToken,
                    elapsedMilliseconds,
                    validated.TimeoutMilliseconds);
                return continuation;
            }
            catch (LMCAxisResetRejectedException)
            {
                throw;
            }
            catch (LMCAxisStopWaitPendingException)
            {
                throw;
            }
            catch (LMCAxisAcceptedObserverInProgressException)
            {
                throw;
            }
            catch (LMCAxisResetWaitDeadlineException)
            {
                throw new LMCAxisResetWaitTimeoutException(
                    continuation == null
                        ? tracker.CaptureEvidence(elapsedMilliseconds())
                        : continuation.CaptureEvidence(elapsedMilliseconds()),
                    continuation);
            }
            catch (OperationCanceledException ex)
            {
                if (observerInvocationActive)
                {
                    throw;
                }

                throw new LMCAxisResetWaitCanceledException(
                    continuation == null
                        ? tracker.CaptureEvidence(elapsedMilliseconds())
                        : continuation.CaptureEvidence(elapsedMilliseconds()),
                    continuation,
                    ex,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                if (continuation != null)
                {
                    throw;
                }

                throw new LMCAxisResetSubmissionException(
                    tracker.CaptureEvidence(elapsedMilliseconds()),
                    ex);
            }
            finally
            {
                if (observerLatchSet)
                {
                    lock (powerOnWaitCoordinator.Sync)
                    {
                        powerOnWaitCoordinator
                            .ResetAcceptanceObserverInProgress = false;
                    }
                }

                if (mutationGateAcquired)
                {
                    powerOnWaitCoordinator.MutationGate.Release();
                }
                if (observerStatusGateAcquired)
                {
                    powerOnWaitCoordinator.StatusObservationGate.Release();
                }
            }
        }

        /// <summary>
        /// Resumes status-only 0x2028 polling for an accepted Axis Reset. This
        /// method never sends another 0x2024 request.
        /// </summary>
        public Task<LMCAxisResetWaitResult>
            ResumeResetWaitForStableErrorClearanceAsync(
                LMCAxisResetWaitContinuation continuation,
                CancellationToken cancellationToken)
        {
            var options = new LMCAxisResetWaitOptions();
            if (continuation != null)
            {
                options.StableSampleCount =
                    continuation.RequiredStableSampleCount;
            }

            return ResumeResetWaitForStableErrorClearanceAsync(
                continuation,
                options,
                cancellationToken);
        }

        /// <summary>
        /// Resumes status-only 0x2028 polling for an accepted Axis Reset using
        /// the supplied deadline, poll interval, and stable-sample options.
        /// This method never sends another 0x2024 request.
        /// </summary>
        public Task<LMCAxisResetWaitResult>
            ResumeResetWaitForStableErrorClearanceAsync(
                LMCAxisResetWaitContinuation continuation,
                LMCAxisResetWaitOptions options,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return ResumeResetWaitForStableErrorClearanceAsync(
                continuation,
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        internal async Task<LMCAxisResetWaitResult>
            ResumeResetWaitForStableErrorClearanceAsync(
                LMCAxisResetWaitContinuation continuation,
                LMCAxisResetWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action beforeStatusResultPublication = null,
                Action afterStatusResultPublication = null,
                Action beforeStatusCoordinatorLock = null)
        {
            var validated = ValidateAxisResetWaitOptions(
                options,
                elapsedMilliseconds,
                delayAsync);
            EnsureResetContinuationOwner(continuation);
            if (validated.StableSampleCount
                != continuation.RequiredStableSampleCount)
            {
                throw new ArgumentException(
                    "StableSampleCount must match the accepted Axis Reset continuation.",
                    "options");
            }

            var statusGateAcquired = false;
            var waitRegistered = false;
            var waitCompleted = false;
            LMCAxisPowerOnWaitContinuation powerOnObservationTarget = null;

            lock (powerOnWaitCoordinator.Sync)
            {
                EnsureResetContinuationOwnerCore(continuation);
                if (powerOnWaitCoordinator.ResetWaitInProgress)
                {
                    throw new InvalidOperationException(
                        "Another Axis Reset status-only wait is already running.");
                }

                powerOnWaitCoordinator.ResetWaitInProgress = true;
                waitRegistered = true;
            }

            try
            {
                try
                {
                    await AcquireAxisResetStatusGateAsync(
                        validated,
                        cancellationToken,
                        elapsedMilliseconds).ConfigureAwait(false);
                    statusGateAcquired = true;
                }
                catch (LMCAxisResetWaitDeadlineException)
                {
                    throw new LMCAxisResetWaitTimeoutException(
                        continuation.CaptureEvidence(elapsedMilliseconds()),
                        continuation);
                }
                catch (OperationCanceledException ex)
                {
                    throw new LMCAxisResetWaitCanceledException(
                        continuation.CaptureEvidence(elapsedMilliseconds()),
                        continuation,
                        ex,
                        cancellationToken);
                }

                lock (powerOnWaitCoordinator.Sync)
                {
                    EnsureResetContinuationOwnerCore(continuation);
                    continuation.ResetProofCounters();
                    powerOnObservationTarget = powerOnWaitCoordinator
                        .PendingContinuation;
                    ResetPendingPowerOnPowerOffProofCore(
                        powerOnObservationTarget);
                }

                while (true)
                {
                    LMCReadStatusResult status;
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        ThrowIfAxisResetWaitMutationIntervened(
                            continuation,
                            elapsedMilliseconds);
                        status = await ReadAxisStatusForResetWaitAsync(
                            continuation,
                            powerOnObservationTarget,
                            validated,
                            cancellationToken,
                            elapsedMilliseconds,
                            beforeStatusResultPublication,
                            afterStatusResultPublication,
                            beforeStatusCoordinatorLock)
                            .ConfigureAwait(false);
                    }
                    catch (LMCAxisResetInterferenceException)
                    {
                        throw;
                    }
                    catch (LMCAxisResetWaitDeadlineException)
                    {
                        throw new LMCAxisResetWaitTimeoutException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation);
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new LMCAxisResetWaitCanceledException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation,
                            ex,
                            cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        throw new LMCAxisResetStatusException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation,
                            null,
                            ex);
                    }

                    if (!status.IsReadSuccessful)
                    {
                        throw new LMCAxisResetStatusException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation,
                            status,
                            null);
                    }

                    if (continuation.IsCompleted)
                    {
                        var evidence = continuation.CaptureEvidence(
                            elapsedMilliseconds());
                        waitCompleted = true;
                        return new LMCAxisResetWaitResult(
                            evidence,
                            continuation);
                    }

                    try
                    {
                        await DelayAxisResetWaitAsync(
                            validated,
                            cancellationToken,
                            elapsedMilliseconds,
                            delayAsync).ConfigureAwait(false);
                    }
                    catch (LMCAxisResetWaitDeadlineException)
                    {
                        throw new LMCAxisResetWaitTimeoutException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation);
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new LMCAxisResetWaitCanceledException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation,
                            ex,
                            cancellationToken);
                    }
                }
            }
            finally
            {
                if (waitRegistered)
                {
                    lock (powerOnWaitCoordinator.Sync)
                    {
                        if (!waitCompleted)
                        {
                            continuation.ResetProofCounters();
                            ResetPendingPowerOnPowerOffProofCore(
                                powerOnObservationTarget);
                        }

                        powerOnWaitCoordinator.ResetWaitInProgress = false;
                    }
                }

                if (statusGateAcquired)
                {
                    powerOnWaitCoordinator.StatusObservationGate.Release();
                }
            }
        }

        /// <summary>
        /// Polls only 0x2028 until stable native axis error clearance is
        /// observed. This API is suitable after reconnect or process restart
        /// and does not attribute the state to any Reset request.
        /// </summary>
        public Task<LMCAxisStableErrorClearanceWaitResult>
            WaitForStableErrorClearanceAsync(
                CancellationToken cancellationToken)
        {
            return WaitForStableErrorClearanceAsync(
                new LMCAxisResetWaitOptions(),
                cancellationToken);
        }

        public Task<LMCAxisStableErrorClearanceWaitResult>
            WaitForStableErrorClearanceAsync(
                LMCAxisResetWaitOptions options,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return WaitForStableErrorClearanceAsync(
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        internal Task<LMCAxisStableErrorClearanceWaitResult>
            WaitForStableErrorClearanceAsync(
                LMCAxisResetWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action beforeStatusResultPublication = null,
                Action afterStatusResultPublication = null,
                Action afterBaselineMutationGenerationCaptured = null)
        {
            return WaitForStableErrorClearanceCoreAsync(
                options,
                cancellationToken,
                elapsedMilliseconds,
                delayAsync,
                beforeStatusResultPublication,
                afterStatusResultPublication,
                afterBaselineMutationGenerationCaptured);
        }

        private async Task<LMCAxisStableErrorClearanceWaitResult>
            WaitForStableErrorClearanceCoreAsync(
                LMCAxisResetWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action beforeStatusResultPublication,
                Action afterStatusResultPublication,
                Action afterBaselineMutationGenerationCaptured)
        {
            var validated = ValidateAxisResetWaitOptions(
                options,
                elapsedMilliseconds,
                delayAsync);
            var tracker = new LMCAxisStableErrorClearanceWaitTracker(
                validated.StableSampleCount);
            EnsureCurrentSessionForUse();
            EnsureNoAxisAcceptedMutationObserverInProgress();
            var statusGateAcquired = false;
            try
            {
                try
                {
                    await AcquireAxisResetStatusGateAsync(
                        validated,
                        cancellationToken,
                        elapsedMilliseconds).ConfigureAwait(false);
                    statusGateAcquired = true;
                    EnsureNoAxisAcceptedMutationObserverInProgress();
                    lock (powerOnWaitCoordinator.Sync)
                    {
                        tracker.SetBaselineMutationGeneration(
                            powerOnWaitCoordinator.MutationGeneration);
                    }
                    if (afterBaselineMutationGenerationCaptured != null)
                    {
                        afterBaselineMutationGenerationCaptured();
                    }
                }
                catch (LMCAxisResetWaitDeadlineException)
                {
                    throw new
                        LMCAxisStableErrorClearanceWaitTimeoutException(
                            tracker.CaptureEvidence(
                                elapsedMilliseconds()));
                }
                catch (OperationCanceledException ex)
                {
                    throw new
                        LMCAxisStableErrorClearanceWaitCanceledException(
                            tracker.CaptureEvidence(
                                elapsedMilliseconds()),
                            ex,
                            cancellationToken);
                }

                while (true)
                {
                    LMCReadStatusResult status;
                    try
                    {
                        status = await
                            ReadStableErrorClearanceStatusOnlyAsync(
                                tracker,
                                validated,
                                cancellationToken,
                                elapsedMilliseconds,
                                beforeStatusResultPublication,
                                afterStatusResultPublication)
                            .ConfigureAwait(false);
                    }
                    catch (LMCAxisResetWaitDeadlineException)
                    {
                        throw new
                            LMCAxisStableErrorClearanceWaitTimeoutException(
                                tracker.CaptureEvidence(
                                    elapsedMilliseconds()));
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new
                            LMCAxisStableErrorClearanceWaitCanceledException(
                                tracker.CaptureEvidence(
                                    elapsedMilliseconds()),
                                ex,
                                cancellationToken);
                    }
                    catch (LMCAxisStableErrorClearanceStatusException)
                    {
                        throw;
                    }
                    catch (LMCAxisAcceptedObserverInProgressException)
                    {
                        throw;
                    }
                    catch (LMCAxisStableErrorClearanceInterferenceException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        var evidence = tracker.CaptureEvidence(
                            elapsedMilliseconds());
                        throw new
                            LMCAxisStableErrorClearanceStatusException(
                                evidence,
                                evidence.LastObservedStatus,
                                ex);
                    }

                    if (!status.IsReadSuccessful)
                    {
                        throw new
                            LMCAxisStableErrorClearanceStatusException(
                                tracker.CaptureEvidence(
                                    elapsedMilliseconds()),
                                status,
                                null);
                    }
                    if (tracker.CompletionPublished)
                    {
                        return new
                            LMCAxisStableErrorClearanceWaitResult(
                                tracker.CaptureEvidence(
                                    elapsedMilliseconds()));
                    }

                    try
                    {
                        await DelayAxisResetWaitAsync(
                            validated,
                            cancellationToken,
                            elapsedMilliseconds,
                            delayAsync).ConfigureAwait(false);
                    }
                    catch (LMCAxisResetWaitDeadlineException)
                    {
                        throw new
                            LMCAxisStableErrorClearanceWaitTimeoutException(
                                tracker.CaptureEvidence(
                                    elapsedMilliseconds()));
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new
                            LMCAxisStableErrorClearanceWaitCanceledException(
                                tracker.CaptureEvidence(
                                    elapsedMilliseconds()),
                                ex,
                                cancellationToken);
                    }
                }
            }
            finally
            {
                if (statusGateAcquired)
                {
                    powerOnWaitCoordinator.StatusObservationGate.Release();
                }
            }
        }

        private async Task<LMCReadStatusResult>
            ReadStableErrorClearanceStatusOnlyAsync(
                LMCAxisStableErrorClearanceWaitTracker tracker,
                LMCAxisResetWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Action beforeStatusResultPublication,
                Action afterStatusResultPublication)
        {
            EnsureCurrentSessionForUse();
            ThrowIfStableErrorClearanceProcessLocalMutationIntervened(
                tracker,
                elapsedMilliseconds);
            var remaining = GetAxisResetWaitRemaining(
                options,
                cancellationToken,
                elapsedMilliseconds);
            var powerOnObservationTarget =
                CapturePendingPowerOnStatusObservation();
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
                        LMC_Frame.LMCAxisReadStatus(AxisReference),
                        sessionGeneration,
                        preWriteCancellation.Token,
                        deadlineCancellation.Token,
                        () =>
                        {
                            ThrowIfAxisResetWaitCannotStartWire(
                                cancellationToken,
                                deadlineCancellation,
                                elapsedMilliseconds,
                                options.TimeoutMilliseconds);
                            EnsureNoAxisAcceptedMutationObserverInProgress();
                            ThrowIfStableErrorClearanceProcessLocalMutationIntervened(
                                tracker,
                                elapsedMilliseconds);
                        },
                        null).ConfigureAwait(false);
                }
                catch (LMCPostWriteDeadlineException)
                {
                    tracker.MarkTransportInvalidatedAtDeadline();
                    throw new LMCAxisResetWaitDeadlineException();
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCAxisResetWaitDeadlineException();
                }

                var status = LMCConnection.ParseReadStatusResult(raw);
                if (beforeStatusResultPublication != null)
                {
                    beforeStatusResultPublication();
                }

                LMCReadStatusResult publishedStatus = null;
                connection.PublishSessionBoundSendPriorityResult(
                    sessionGeneration,
                    LMC_CommandId.ReadStatus,
                    () =>
                    {
                        lock (powerOnWaitCoordinator.Sync)
                        {
                            var actualMutationGeneration =
                                powerOnWaitCoordinator.MutationGeneration;
                            tracker.ObserveMutationGeneration(
                                actualMutationGeneration);
                            if (actualMutationGeneration
                                != tracker.BaselineMutationGeneration)
                            {
                                throw new
                                    LMCAxisStableErrorClearanceInterferenceException(
                                        tracker.CaptureEvidence(
                                            elapsedMilliseconds()));
                            }
                            publishedStatus = status;
                            ObservePendingPowerOnStatus(
                                powerOnObservationTarget,
                                status);
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
                        }
                    });
                if (afterStatusResultPublication != null)
                {
                    afterStatusResultPublication();
                }
                if (!tracker.CompletionPublished)
                {
                    ThrowIfAxisResetWaitExpiredAfterWire(
                        cancellationToken,
                        deadlineCancellation,
                        elapsedMilliseconds,
                        options.TimeoutMilliseconds);
                }
                return publishedStatus;
            }
        }

        private void
            ThrowIfStableErrorClearanceProcessLocalMutationIntervened(
            LMCAxisStableErrorClearanceWaitTracker tracker,
            Func<long> elapsedMilliseconds)
        {
            lock (powerOnWaitCoordinator.Sync)
            {
                var actualMutationGeneration = powerOnWaitCoordinator
                    .MutationGeneration;
                tracker.ObserveMutationGeneration(actualMutationGeneration);
                if (actualMutationGeneration
                    != tracker.BaselineMutationGeneration)
                {
                    throw new
                        LMCAxisStableErrorClearanceInterferenceException(
                            tracker.CaptureEvidence(
                                elapsedMilliseconds()));
                }
            }
        }

        public Task<LMCAxisResetWaitResult>
            ResetAndWaitForStableErrorClearanceAsync(
                CancellationToken cancellationToken)
        {
            return ResetAndWaitForStableErrorClearanceAsync(
                new LMCAxisResetWaitOptions(),
                cancellationToken);
        }

        public Task<LMCAxisResetWaitResult>
            ResetAndWaitForStableErrorClearanceAsync(
                LMCAxisResetWaitOptions options,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return ResetAndWaitForStableErrorClearanceAsync(
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        public Task<LMCAxisResetWaitResult>
            ResetAndWaitForStableErrorClearanceAsync(
                LMCAxisResetWaitOptions options,
                Action<LMCAxisResetWaitContinuation>
                    acceptedContinuationObserver,
                CancellationToken cancellationToken)
        {
            if (acceptedContinuationObserver == null)
            {
                throw new ArgumentNullException(
                    "acceptedContinuationObserver");
            }

            var stopwatch = Stopwatch.StartNew();
            return ResetAndWaitForStableErrorClearanceAsync(
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync,
                null,
                acceptedContinuationObserver);
        }

        internal async Task<LMCAxisResetWaitResult>
            ResetAndWaitForStableErrorClearanceAsync(
                LMCAxisResetWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action beforeResetWriteCommit = null,
                Action<LMCAxisResetWaitContinuation>
                    acceptedContinuationObserver = null)
        {
            var continuation = await
                BeginResetWaitForStableErrorClearanceAsync(
                    options,
                    cancellationToken,
                    elapsedMilliseconds,
                    delayAsync,
                    null,
                    beforeResetWriteCommit,
                    acceptedContinuationObserver).ConfigureAwait(false);
            return await ResumeResetWaitForStableErrorClearanceAsync(
                continuation,
                options,
                cancellationToken,
                elapsedMilliseconds,
                delayAsync).ConfigureAwait(false);
        }

        private void EnsureResetContinuationOwner(
            LMCAxisResetWaitContinuation continuation)
        {
            EnsureCurrentSessionForUse();
            lock (powerOnWaitCoordinator.Sync)
            {
                EnsureResetContinuationOwnerCore(continuation);
            }
        }

        private void EnsureResetContinuationOwnerCore(
            LMCAxisResetWaitContinuation continuation)
        {
            EnsureNoAxisAcceptedMutationObserverInProgressCore();
            if (continuation == null
                || !continuation.IsPending
                || !ReferenceEquals(
                    continuation.Coordinator,
                    powerOnWaitCoordinator)
                || !continuation.BelongsTo(
                    connection,
                    sessionGeneration,
                    AxisReference)
                || !ReferenceEquals(
                    powerOnWaitCoordinator.PendingResetContinuation,
                    continuation))
            {
                throw new InvalidOperationException(
                    "The Axis Reset continuation does not belong to this active connection, session, axis, or latest pending operation.");
            }
        }

        private void EnsureResetSubmissionAdmission()
        {
            lock (powerOnWaitCoordinator.Sync)
            {
                EnsureResetSubmissionAdmissionCore();
            }
        }

        private void EnsureResetSubmissionAdmissionCore()
        {
            EnsureNoAxisAcceptedMutationObserverInProgressCore();
            var pendingStop = powerOnWaitCoordinator
                .PendingStopContinuation;
            if (pendingStop != null && pendingStop.IsPending)
            {
                throw new LMCAxisStopWaitPendingException(pendingStop);
            }
        }

        private void ResolveResetContinuation(
            LMCAxisResetWaitContinuation continuation,
            long elapsedMilliseconds)
        {
            lock (powerOnWaitCoordinator.Sync)
            {
                EnsureResetContinuationOwnerCore(continuation);
                var actualMutationGeneration = powerOnWaitCoordinator
                    .MutationGeneration;
                continuation.ObserveMutationGeneration(
                    actualMutationGeneration);
                if (continuation.ResetMutationGeneration <= 0
                    || actualMutationGeneration
                        != continuation.ResetMutationGeneration)
                {
                    throw new LMCAxisResetInterferenceException(
                        continuation.CaptureEvidence(elapsedMilliseconds),
                        continuation);
                }

                if (!continuation.HasStableErrorClearProof)
                {
                    throw new InvalidOperationException(
                        "The Axis Reset continuation cannot complete without stable error clearance proof.");
                }

                continuation.MarkCompleted();
                powerOnWaitCoordinator.PendingResetContinuation = null;
            }
        }

        private async Task AcquireAxisResetStatusGateAsync(
            LMCAxisResetWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds)
        {
            var remaining = GetAxisResetWaitRemaining(
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
                    await powerOnWaitCoordinator.StatusObservationGate
                        .WaitAsync(deadlineCancellation.Token)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCAxisResetWaitDeadlineException();
                }
            }
        }

        private async Task AcquireAxisResetMutationGateAsync(
            LMCAxisResetWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds)
        {
            LMCGroupResetObserverScope.ThrowIfMemberMutationReentrant(
                connection,
                sessionGeneration,
                AxisReference);
            var remaining = GetAxisResetWaitRemaining(
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
                    await powerOnWaitCoordinator.MutationGate
                        .WaitAsync(deadlineCancellation.Token)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCAxisResetWaitDeadlineException();
                }
            }
        }

        private async Task<LMC_Response> SendAxisResetForWaitAsync(
            byte[] request,
            LMCAxisResetWaitTracker tracker,
            LMCAxisResetWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            Action beforeResetWriteCommit)
        {
            var remaining = GetAxisResetWaitRemaining(
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
                        request,
                        sessionGeneration,
                        preWriteCancellation.Token,
                        deadlineCancellation.Token,
                        () =>
                        {
                            ThrowIfAxisResetWaitCannotStartWire(
                                cancellationToken,
                                deadlineCancellation,
                                elapsedMilliseconds,
                                options.TimeoutMilliseconds);
                            if (beforeResetWriteCommit != null)
                            {
                                beforeResetWriteCommit();
                            }
                        },
                        () =>
                        {
                            tracker.MarkSubmissionOutcomeUncertain();
                            tracker.SetResetMutationGeneration(
                                powerOnWaitCoordinator
                                    .MarkMutationMayHaveBeenSent());
                        })
                        .ConfigureAwait(false);
                }
                catch (LMCPostWriteDeadlineException)
                {
                    tracker.MarkTransportInvalidatedAtDeadline();
                    throw new LMCAxisResetWaitDeadlineException();
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCAxisResetWaitDeadlineException();
                }

                var acknowledgement =
                    LMCConnection.ParseCommandAcknowledgement(
                        raw,
                        "Axis Reset");
                if (!acknowledgement.IsSuccess)
                {
                    powerOnWaitCoordinator.TryRollbackRejectedMutation(
                        tracker.ResetMutationGeneration);
                }

                return acknowledgement;
            }
        }

        private async Task<LMCReadStatusResult>
            ReadAxisStatusForResetWaitAsync(
                LMCAxisResetWaitContinuation continuation,
                LMCAxisPowerOnWaitContinuation powerOnObservationTarget,
                LMCAxisResetWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Action beforeStatusResultPublication,
                Action afterStatusResultPublication,
                Action beforeStatusCoordinatorLock)
        {
            EnsureResetContinuationOwner(continuation);
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfAxisResetWaitMutationIntervened(
                continuation,
                elapsedMilliseconds);
            var remaining = GetAxisResetWaitRemaining(
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
                        LMC_Frame.LMCAxisReadStatus(AxisReference),
                        sessionGeneration,
                        preWriteCancellation.Token,
                        deadlineCancellation.Token,
                        () =>
                        {
                            ThrowIfAxisResetWaitCannotStartWire(
                                cancellationToken,
                                deadlineCancellation,
                                elapsedMilliseconds,
                                options.TimeoutMilliseconds);
                            ThrowIfAxisResetWaitMutationIntervened(
                                continuation,
                                elapsedMilliseconds);
                        },
                        null).ConfigureAwait(false);
                }
                catch (LMCPostWriteDeadlineException)
                {
                    continuation.MarkTransportInvalidatedAtDeadline();
                    throw new LMCAxisResetWaitDeadlineException();
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCAxisResetWaitDeadlineException();
                }

                var result = LMCConnection.ParseReadStatusResult(raw);
                if (beforeStatusResultPublication != null)
                {
                    beforeStatusResultPublication();
                }

                connection.PublishSessionBoundSendPriorityResult(
                    sessionGeneration,
                    LMC_CommandId.ReadStatus,
                    () =>
                    {
                        if (beforeStatusCoordinatorLock != null)
                        {
                            beforeStatusCoordinatorLock();
                        }

                        lock (powerOnWaitCoordinator.Sync)
                        {
                            long actualMutationGeneration;
                            var published = powerOnWaitCoordinator
                                .TryPublishForMutationGeneration(
                                    continuation.ResetMutationGeneration,
                                    () =>
                                    {
                                        EnsureResetContinuationOwnerCore(
                                            continuation);
                                        ObservePendingPowerOnStatus(
                                            powerOnObservationTarget,
                                            result);
                                        continuation.Observe(result);
                                        if (!cancellationToken
                                                .IsCancellationRequested
                                            && !deadlineCancellation
                                                .IsCancellationRequested
                                            && elapsedMilliseconds()
                                                < options
                                                    .TimeoutMilliseconds
                                            && continuation
                                                .HasStableErrorClearProof)
                                        {
                                            ResolveResetContinuation(
                                                continuation,
                                                elapsedMilliseconds());
                                        }
                                    },
                                    out actualMutationGeneration);
                            continuation.ObserveMutationGeneration(
                                actualMutationGeneration);
                            if (!published)
                            {
                                throw new LMCAxisResetInterferenceException(
                                    continuation.CaptureEvidence(
                                        elapsedMilliseconds()),
                                    continuation);
                            }
                        }
                    });

                if (afterStatusResultPublication != null)
                {
                    afterStatusResultPublication();
                }

                if (!continuation.IsCompleted)
                {
                    ThrowIfAxisResetWaitExpiredAfterWire(
                        cancellationToken,
                        deadlineCancellation,
                        elapsedMilliseconds,
                        options.TimeoutMilliseconds);
                    ThrowIfAxisResetWaitMutationIntervened(
                        continuation,
                        elapsedMilliseconds);
                }
                return result;
            }
        }

        private void ThrowIfAxisResetWaitMutationIntervened(
            LMCAxisResetWaitContinuation continuation,
            Func<long> elapsedMilliseconds)
        {
            lock (powerOnWaitCoordinator.Sync)
            {
                EnsureResetContinuationOwnerCore(continuation);
                var actualGeneration = powerOnWaitCoordinator
                    .MutationGeneration;
                continuation.ObserveMutationGeneration(actualGeneration);
                if (continuation.ResetMutationGeneration <= 0
                    || actualGeneration != continuation.ResetMutationGeneration)
                {
                    throw new LMCAxisResetInterferenceException(
                        continuation.CaptureEvidence(elapsedMilliseconds()),
                        continuation);
                }
            }
        }

        private static async Task DelayAxisResetWaitAsync(
            LMCAxisResetWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            Func<int, CancellationToken, Task> delayAsync)
        {
            var remaining = GetAxisResetWaitRemaining(
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
                    throw new LMCAxisResetWaitDeadlineException();
                }

                cancellationToken.ThrowIfCancellationRequested();
                if (elapsedMilliseconds() >= options.TimeoutMilliseconds
                    || (deadlineCancellation.IsCancellationRequested
                        && !cancellationToken.IsCancellationRequested))
                {
                    throw new LMCAxisResetWaitDeadlineException();
                }
            }
        }

        private static long GetAxisResetWaitRemaining(
            LMCAxisResetWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var remaining = options.TimeoutMilliseconds
                - elapsedMilliseconds();
            if (remaining <= 0)
            {
                throw new LMCAxisResetWaitDeadlineException();
            }

            return remaining;
        }

        private static LMCAxisResetWaitOptions
            ValidateAxisResetWaitOptions(
                LMCAxisResetWaitOptions options,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            if (elapsedMilliseconds == null)
            {
                throw new ArgumentNullException("elapsedMilliseconds");
            }

            if (delayAsync == null)
            {
                throw new ArgumentNullException("delayAsync");
            }

            return options.SnapshotAndValidate();
        }

        private static void ThrowIfAxisResetWaitCannotStartWire(
            CancellationToken cancellationToken,
            CancellationTokenSource deadlineCancellation,
            Func<long> elapsedMilliseconds,
            int timeoutMilliseconds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (elapsedMilliseconds() >= timeoutMilliseconds
                || (deadlineCancellation.IsCancellationRequested
                    && !cancellationToken.IsCancellationRequested))
            {
                throw new LMCAxisResetWaitDeadlineException();
            }
        }

        private static void ThrowIfAxisResetWaitExpiredAfterWire(
            CancellationToken cancellationToken,
            CancellationTokenSource deadlineCancellation,
            Func<long> elapsedMilliseconds,
            int timeoutMilliseconds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (elapsedMilliseconds() >= timeoutMilliseconds
                || (deadlineCancellation.IsCancellationRequested
                    && !cancellationToken.IsCancellationRequested))
            {
                throw new LMCAxisResetWaitDeadlineException();
            }
        }

        private static void ThrowIfAxisResetWaitExpiredAfterPublication(
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            int timeoutMilliseconds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (elapsedMilliseconds() >= timeoutMilliseconds)
            {
                throw new LMCAxisResetWaitDeadlineException();
            }
        }

        private sealed class LMCAxisResetWaitDeadlineException
            : TimeoutException
        {
        }
    }
}

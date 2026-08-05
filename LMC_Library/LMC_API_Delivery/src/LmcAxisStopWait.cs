using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public partial class LMCSingleAxis
    {
        /// <summary>
        /// The latest accepted Axis Stop whose stable standstill state has not
        /// yet been resolved. Resuming this continuation sends status reads
        /// only.
        /// </summary>
        public LMCAxisStopWaitContinuation PendingStopWaitContinuation
        {
            get
            {
                lock (powerOnWaitCoordinator.Sync)
                {
                    return powerOnWaitCoordinator.PendingStopContinuation;
                }
            }
        }

        /// <summary>
        /// Retires the exact latest pending Stop only when the supplied
        /// Power Off continuation is a newer, same-session, completed stable
        /// PowerOff-and-Standstill proof. No RPC request is sent.
        /// </summary>
        public bool TryRetirePendingStopAfterStablePowerOff(
            LMCAxisStopWaitContinuation stopContinuation,
            LMCAxisPowerOffWaitContinuation powerOffContinuation)
        {
            try
            {
                EnsureCurrentSessionForUse();
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            if (stopContinuation == null || powerOffContinuation == null)
            {
                return false;
            }

            lock (powerOnWaitCoordinator.Sync)
            {
                if (powerOnWaitCoordinator
                        .StopAcceptanceObserverInProgress
                    || !stopContinuation.IsPending
                    || !ReferenceEquals(
                        stopContinuation.Coordinator,
                        powerOnWaitCoordinator)
                    || !stopContinuation.BelongsTo(
                        connection,
                        sessionGeneration,
                        AxisReference)
                    || !ReferenceEquals(
                        powerOnWaitCoordinator.PendingStopContinuation,
                        stopContinuation)
                    || !ReferenceEquals(
                        powerOffContinuation.Coordinator,
                        powerOnWaitCoordinator)
                    || !powerOffContinuation.BelongsTo(
                        connection,
                        sessionGeneration,
                        AxisReference)
                    || !powerOffContinuation.IsCompleted
                    || !powerOffContinuation
                        .HasStablePowerOffStandstillProof
                    || powerOffContinuation.PowerOffMutationGeneration
                        <= stopContinuation.StopMutationGeneration
                    || powerOffContinuation.ObservedMutationGeneration
                        != powerOffContinuation.PowerOffMutationGeneration
                    || powerOnWaitCoordinator.MutationGeneration
                        != powerOffContinuation.PowerOffMutationGeneration)
                {
                    return false;
                }

                stopContinuation.MarkSupersededByStablePowerOff(
                    powerOffContinuation);
                powerOnWaitCoordinator.PendingStopContinuation = null;
                return true;
            }
        }

        /// <summary>
        /// Sends exactly one 0x2022 Axis Stop request and returns after its
        /// successful acknowledgement is preserved. No 0x2028 status read is
        /// sent by this method.
        /// </summary>
        public Task<LMCAxisStopWaitContinuation>
            BeginStopWaitForStableStandstillAsync(
                int deceleration,
                int jerk,
                CancellationToken cancellationToken)
        {
            return BeginStopWaitForStableStandstillAsync(
                deceleration,
                jerk,
                new LMCAxisStopWaitOptions(),
                cancellationToken);
        }

        public Task<LMCAxisStopWaitContinuation>
            BeginStopWaitForStableStandstillAsync(
                int deceleration,
                int jerk,
                LMCAxisStopWaitOptions options,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return BeginStopWaitForStableStandstillAsync(
                deceleration,
                jerk,
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        public Task<LMCAxisStopWaitContinuation>
            BeginStopWaitForStableStandstillAsync(
                int deceleration,
                int jerk,
                Action<LMCAxisStopWaitContinuation>
                    acceptedContinuationObserver,
                CancellationToken cancellationToken)
        {
            return BeginStopWaitForStableStandstillAsync(
                deceleration,
                jerk,
                new LMCAxisStopWaitOptions(),
                acceptedContinuationObserver,
                cancellationToken);
        }

        public Task<LMCAxisStopWaitContinuation>
            BeginStopWaitForStableStandstillAsync(
                int deceleration,
                int jerk,
                LMCAxisStopWaitOptions options,
                Action<LMCAxisStopWaitContinuation>
                    acceptedContinuationObserver,
                CancellationToken cancellationToken)
        {
            if (acceptedContinuationObserver == null)
            {
                throw new ArgumentNullException(
                    "acceptedContinuationObserver");
            }

            var stopwatch = Stopwatch.StartNew();
            return BeginStopWaitForStableStandstillAsync(
                deceleration,
                jerk,
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync,
                null,
                null,
                acceptedContinuationObserver);
        }

        public Task<LMCAxisStopWaitContinuation>
            BeginStopWaitForStableStandstillWithResetTakeoverAsync(
                LMCAxisResetWaitContinuation resetContinuation,
                int deceleration,
                int jerk,
                LMCAxisStopWaitOptions options,
                CancellationToken cancellationToken)
        {
            return BeginStopWaitForStableStandstillWithResetTakeoverAsync(
                resetContinuation,
                deceleration,
                jerk,
                options,
                null,
                cancellationToken);
        }

        public Task<LMCAxisStopWaitContinuation>
            BeginStopWaitForStableStandstillWithResetTakeoverAsync(
                LMCAxisResetWaitContinuation resetContinuation,
                int deceleration,
                int jerk,
                LMCAxisStopWaitOptions options,
                Action<LMCAxisStopWaitContinuation>
                    acceptedContinuationObserver,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return BeginStopWaitForStableStandstillAsync(
                deceleration,
                jerk,
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync,
                null,
                null,
                acceptedContinuationObserver,
                resetContinuation);
        }

        internal async Task<LMCAxisStopWaitContinuation>
            BeginStopWaitForStableStandstillAsync(
                int deceleration,
                int jerk,
                LMCAxisStopWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action beforeAcceptedContinuationPublication = null,
                Action beforeStopWriteCommit = null,
                Action<LMCAxisStopWaitContinuation>
                    acceptedContinuationObserver = null,
                LMCAxisResetWaitContinuation resetTakeoverContinuation = null)
        {
            var validated = ValidateAxisStopWaitOptions(
                options,
                elapsedMilliseconds,
                delayAsync);
            var request = LMC_Frame.LMCAxisStop(
                AxisReference,
                deceleration,
                jerk);
            var tracker = new LMCAxisStopWaitTracker(
                deceleration,
                jerk,
                validated.StableSampleCount);
            LMCAxisStopWaitContinuation continuation = null;
            var mutationGateAcquired = false;
            var takeoverStatusGateAcquired = false;
            var observerLatchSet = false;
            var observerInvocationActive = false;

            try
            {
                EnsureCurrentSessionForUse();
                cancellationToken.ThrowIfCancellationRequested();
                if (resetTakeoverContinuation != null
                    || acceptedContinuationObserver != null)
                {
                    await AcquireAxisStopStatusGateAsync(
                        validated,
                        cancellationToken,
                        elapsedMilliseconds).ConfigureAwait(false);
                    takeoverStatusGateAcquired = true;
                    EnsureCurrentSessionForUse();
                }
                await AcquireAxisStopMutationGateAsync(
                    validated,
                    cancellationToken,
                    elapsedMilliseconds).ConfigureAwait(false);
                mutationGateAcquired = true;
                EnsureCurrentSessionForUse();
                EnsureStopSubmissionAdmission(resetTakeoverContinuation);

                var publication = await SendAxisStopForWaitAsync(
                    request,
                    tracker,
                    validated,
                    cancellationToken,
                    elapsedMilliseconds,
                    beforeStopWriteCommit,
                    beforeAcceptedContinuationPublication,
                    acceptedContinuationObserver != null,
                    resetTakeoverContinuation).ConfigureAwait(false);
                continuation = publication.Continuation;
                observerLatchSet = continuation != null
                    && acceptedContinuationObserver != null;
                if (!publication.Acknowledgement.IsSuccess)
                {
                    throw new LMCAxisStopRejectedException(
                        tracker.CaptureEvidence(elapsedMilliseconds()));
                }

                if (acceptedContinuationObserver != null)
                {
                    powerOnWaitCoordinator.MutationGate.Release();
                    mutationGateAcquired = false;
                    if (takeoverStatusGateAcquired)
                    {
                        powerOnWaitCoordinator.StatusObservationGate
                            .Release();
                        takeoverStatusGateAcquired = false;
                    }
                    observerInvocationActive = true;
                    acceptedContinuationObserver(continuation);
                    observerInvocationActive = false;
                }

                ThrowIfAxisStopWaitExpiredAfterPublication(
                    cancellationToken,
                    elapsedMilliseconds,
                    validated.TimeoutMilliseconds);
                return continuation;
            }
            catch (LMCAxisStopRejectedException)
            {
                throw;
            }
            catch (LMCAxisResetWaitPendingException)
            {
                throw;
            }
            catch (LMCAxisAcceptedObserverInProgressException)
            {
                throw;
            }
            catch (LMCAxisStopWaitDeadlineException)
            {
                throw new LMCAxisStopWaitTimeoutException(
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

                throw new LMCAxisStopWaitCanceledException(
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

                throw new LMCAxisStopSubmissionException(
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
                            .StopAcceptanceObserverInProgress = false;
                    }
                }

                if (mutationGateAcquired)
                {
                    powerOnWaitCoordinator.MutationGate.Release();
                }
                if (takeoverStatusGateAcquired)
                {
                    powerOnWaitCoordinator.StatusObservationGate.Release();
                }
            }
        }

        /// <summary>
        /// Resumes status-only 0x2028 polling for an accepted Axis Stop. This
        /// method never sends another 0x2022 request.
        /// </summary>
        public Task<LMCAxisStopWaitResult>
            ResumeStopWaitForStableStandstillAsync(
                LMCAxisStopWaitContinuation continuation,
                CancellationToken cancellationToken)
        {
            var options = new LMCAxisStopWaitOptions();
            if (continuation != null)
            {
                options.StableSampleCount =
                    continuation.RequiredStableSampleCount;
            }

            return ResumeStopWaitForStableStandstillAsync(
                continuation,
                options,
                cancellationToken);
        }

        /// <summary>
        /// Resumes status-only 0x2028 polling for an accepted Axis Stop using
        /// the supplied deadline, poll interval, and stable-sample options.
        /// This method never sends another 0x2022 request.
        /// </summary>
        public Task<LMCAxisStopWaitResult>
            ResumeStopWaitForStableStandstillAsync(
                LMCAxisStopWaitContinuation continuation,
                LMCAxisStopWaitOptions options,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return ResumeStopWaitForStableStandstillAsync(
                continuation,
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        internal async Task<LMCAxisStopWaitResult>
            ResumeStopWaitForStableStandstillAsync(
                LMCAxisStopWaitContinuation continuation,
                LMCAxisStopWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action beforeStatusResultPublication = null)
        {
            var validated = ValidateAxisStopWaitOptions(
                options,
                elapsedMilliseconds,
                delayAsync);
            EnsureStopContinuationOwner(continuation);
            if (validated.StableSampleCount
                != continuation.RequiredStableSampleCount)
            {
                throw new ArgumentException(
                    "StableSampleCount must match the accepted Axis Stop continuation.",
                    "options");
            }

            var statusGateAcquired = false;
            var waitRegistered = false;
            var waitCompleted = false;
            LMCAxisPowerOnWaitContinuation powerOnObservationTarget = null;
            try
            {
                try
                {
                    await AcquireAxisStopStatusGateAsync(
                        validated,
                        cancellationToken,
                        elapsedMilliseconds).ConfigureAwait(false);
                    statusGateAcquired = true;
                }
                catch (LMCAxisStopWaitDeadlineException)
                {
                    throw new LMCAxisStopWaitTimeoutException(
                        continuation.CaptureEvidence(elapsedMilliseconds()),
                        continuation);
                }
                catch (OperationCanceledException ex)
                {
                    throw new LMCAxisStopWaitCanceledException(
                        continuation.CaptureEvidence(elapsedMilliseconds()),
                        continuation,
                        ex,
                        cancellationToken);
                }

                lock (powerOnWaitCoordinator.Sync)
                {
                    EnsureStopContinuationOwnerCore(continuation);
                    if (powerOnWaitCoordinator.StopWaitInProgress)
                    {
                        throw new InvalidOperationException(
                            "Another Axis Stop status-only wait is already running.");
                    }

                    powerOnWaitCoordinator.StopWaitInProgress = true;
                    waitRegistered = true;
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
                        ThrowIfAxisStopWaitMutationIntervened(
                            continuation,
                            elapsedMilliseconds);
                        status = await ReadAxisStatusForStopWaitAsync(
                            continuation,
                            powerOnObservationTarget,
                            validated,
                            cancellationToken,
                            elapsedMilliseconds,
                            beforeStatusResultPublication)
                            .ConfigureAwait(false);
                    }
                    catch (LMCAxisStopInterferenceException)
                    {
                        throw;
                    }
                    catch (LMCAxisStopWaitDeadlineException)
                    {
                        throw new LMCAxisStopWaitTimeoutException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation);
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new LMCAxisStopWaitCanceledException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation,
                            ex,
                            cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        throw new LMCAxisStopStatusException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation,
                            null,
                            ex);
                    }

                    if (!status.IsReadSuccessful)
                    {
                        throw new LMCAxisStopStatusException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation,
                            status,
                            null);
                    }

                    if (continuation.HasStableStandstillProof)
                    {
                        var evidence = continuation.CaptureEvidence(
                            elapsedMilliseconds());
                        ResolveStopContinuation(
                            continuation,
                            elapsedMilliseconds());
                        waitCompleted = true;
                        return new LMCAxisStopWaitResult(
                            evidence,
                            continuation);
                    }

                    try
                    {
                        await DelayAxisStopWaitAsync(
                            validated,
                            cancellationToken,
                            elapsedMilliseconds,
                            delayAsync).ConfigureAwait(false);
                    }
                    catch (LMCAxisStopWaitDeadlineException)
                    {
                        throw new LMCAxisStopWaitTimeoutException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation);
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new LMCAxisStopWaitCanceledException(
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

                        powerOnWaitCoordinator.StopWaitInProgress = false;
                    }
                }

                if (statusGateAcquired)
                {
                    powerOnWaitCoordinator.StatusObservationGate.Release();
                }
            }
        }

        /// <summary>
        /// Polls only 0x2028 until stable standstill is observed. This API is
        /// suitable after reconnect or process restart and does not claim
        /// that the state was caused by any particular Stop request.
        /// </summary>
        public Task<LMCAxisStableStandstillWaitResult>
            WaitForStableStandstillAsync(
                CancellationToken cancellationToken)
        {
            return WaitForStableStandstillAsync(
                new LMCAxisStopWaitOptions(),
                cancellationToken);
        }

        public Task<LMCAxisStableStandstillWaitResult>
            WaitForStableStandstillAsync(
                LMCAxisStopWaitOptions options,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return WaitForStableStandstillAsync(
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        internal Task<LMCAxisStableStandstillWaitResult>
            WaitForStableStandstillAsync(
                LMCAxisStopWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action beforeStatusResultPublication = null,
                Action afterStatusResultPublication = null,
                Action afterBaselineMutationGenerationCaptured = null)
        {
            return WaitForStableStandstillCoreAsync(
                options,
                cancellationToken,
                elapsedMilliseconds,
                delayAsync,
                beforeStatusResultPublication,
                afterStatusResultPublication,
                afterBaselineMutationGenerationCaptured);
        }

        private async Task<LMCAxisStableStandstillWaitResult>
            WaitForStableStandstillCoreAsync(
                LMCAxisStopWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action beforeStatusResultPublication,
                Action afterStatusResultPublication,
                Action afterBaselineMutationGenerationCaptured)
        {
            var validated = ValidateAxisStopWaitOptions(
                options,
                elapsedMilliseconds,
                delayAsync);
            var tracker = new LMCAxisStableStandstillWaitTracker(
                validated.StableSampleCount);
            EnsureCurrentSessionForUse();
            EnsureNoAxisAcceptedMutationObserverInProgress();
            var statusGateAcquired = false;
            try
            {
                try
                {
                    await AcquireAxisStopStatusGateAsync(
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
                catch (LMCAxisStopWaitDeadlineException)
                {
                    throw new
                        LMCAxisStableStandstillWaitTimeoutException(
                            tracker.CaptureEvidence(
                                elapsedMilliseconds()));
                }
                catch (OperationCanceledException ex)
                {
                    throw new
                        LMCAxisStableStandstillWaitCanceledException(
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
                            ReadStableStandstillStatusOnlyAsync(
                                tracker,
                                validated,
                                cancellationToken,
                                elapsedMilliseconds,
                                beforeStatusResultPublication,
                                afterStatusResultPublication)
                            .ConfigureAwait(false);
                    }
                    catch (LMCAxisStopWaitDeadlineException)
                    {
                        throw new
                            LMCAxisStableStandstillWaitTimeoutException(
                                tracker.CaptureEvidence(
                                    elapsedMilliseconds()));
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new
                            LMCAxisStableStandstillWaitCanceledException(
                                tracker.CaptureEvidence(
                                    elapsedMilliseconds()),
                                ex,
                                cancellationToken);
                    }
                    catch (LMCAxisStableStandstillStatusException)
                    {
                        throw;
                    }
                    catch (LMCAxisAcceptedObserverInProgressException)
                    {
                        throw;
                    }
                    catch (LMCAxisStableStandstillInterferenceException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        var evidence = tracker.CaptureEvidence(
                            elapsedMilliseconds());
                        throw new LMCAxisStableStandstillStatusException(
                            evidence,
                            evidence.LastObservedStatus,
                            ex);
                    }

                    if (!status.IsReadSuccessful)
                    {
                        throw new LMCAxisStableStandstillStatusException(
                            tracker.CaptureEvidence(
                                elapsedMilliseconds()),
                            status,
                            null);
                    }
                    if (tracker.CompletionPublished)
                    {
                        return new LMCAxisStableStandstillWaitResult(
                            tracker.CaptureEvidence(
                                elapsedMilliseconds()));
                    }

                    try
                    {
                        await DelayAxisStopWaitAsync(
                            validated,
                            cancellationToken,
                            elapsedMilliseconds,
                            delayAsync).ConfigureAwait(false);
                    }
                    catch (LMCAxisStopWaitDeadlineException)
                    {
                        throw new
                            LMCAxisStableStandstillWaitTimeoutException(
                                tracker.CaptureEvidence(
                                    elapsedMilliseconds()));
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new
                            LMCAxisStableStandstillWaitCanceledException(
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
            ReadStableStandstillStatusOnlyAsync(
                LMCAxisStableStandstillWaitTracker tracker,
                LMCAxisStopWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Action beforeStatusResultPublication,
                Action afterStatusResultPublication)
        {
            EnsureCurrentSessionForUse();
            ThrowIfStableStandstillProcessLocalMutationIntervened(
                tracker,
                elapsedMilliseconds);
            var remaining = GetAxisStopWaitRemaining(
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
                            ThrowIfAxisStopWaitCannotStartWire(
                                cancellationToken,
                                deadlineCancellation,
                                elapsedMilliseconds,
                                options.TimeoutMilliseconds);
                            EnsureNoAxisAcceptedMutationObserverInProgress();
                            ThrowIfStableStandstillProcessLocalMutationIntervened(
                                tracker,
                                elapsedMilliseconds);
                        },
                        null).ConfigureAwait(false);
                }
                catch (LMCPostWriteDeadlineException)
                {
                    tracker.MarkTransportInvalidatedAtDeadline();
                    throw new LMCAxisStopWaitDeadlineException();
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCAxisStopWaitDeadlineException();
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
                                    LMCAxisStableStandstillInterferenceException(
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
                    ThrowIfAxisStopWaitExpiredAfterWire(
                        cancellationToken,
                        deadlineCancellation,
                        elapsedMilliseconds,
                        options.TimeoutMilliseconds);
                }
                return publishedStatus;
            }
        }

        private void
            ThrowIfStableStandstillProcessLocalMutationIntervened(
            LMCAxisStableStandstillWaitTracker tracker,
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
                        LMCAxisStableStandstillInterferenceException(
                            tracker.CaptureEvidence(
                                elapsedMilliseconds()));
                }
            }
        }

        public Task<LMCAxisStopWaitResult>
            StopAndWaitForStableStandstillAsync(
                int deceleration,
                int jerk,
                CancellationToken cancellationToken)
        {
            return StopAndWaitForStableStandstillAsync(
                deceleration,
                jerk,
                new LMCAxisStopWaitOptions(),
                cancellationToken);
        }

        public Task<LMCAxisStopWaitResult>
            StopAndWaitForStableStandstillAsync(
                int deceleration,
                int jerk,
                LMCAxisStopWaitOptions options,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return StopAndWaitForStableStandstillAsync(
                deceleration,
                jerk,
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        public Task<LMCAxisStopWaitResult>
            StopAndWaitForStableStandstillAsync(
                int deceleration,
                int jerk,
                LMCAxisStopWaitOptions options,
                Action<LMCAxisStopWaitContinuation>
                    acceptedContinuationObserver,
                CancellationToken cancellationToken)
        {
            if (acceptedContinuationObserver == null)
            {
                throw new ArgumentNullException(
                    "acceptedContinuationObserver");
            }

            var stopwatch = Stopwatch.StartNew();
            return StopAndWaitForStableStandstillAsync(
                deceleration,
                jerk,
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync,
                null,
                acceptedContinuationObserver);
        }

        public Task<LMCAxisStopWaitResult>
            StopAndWaitForStableStandstillWithResetTakeoverAsync(
                LMCAxisResetWaitContinuation resetContinuation,
                int deceleration,
                int jerk,
                LMCAxisStopWaitOptions options,
                Action<LMCAxisStopWaitContinuation>
                    acceptedContinuationObserver,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return StopAndWaitForStableStandstillAsync(
                deceleration,
                jerk,
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync,
                null,
                acceptedContinuationObserver,
                resetContinuation);
        }

        internal async Task<LMCAxisStopWaitResult>
            StopAndWaitForStableStandstillAsync(
                int deceleration,
                int jerk,
                LMCAxisStopWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action beforeStopWriteCommit = null,
                Action<LMCAxisStopWaitContinuation>
                    acceptedContinuationObserver = null,
                LMCAxisResetWaitContinuation resetTakeoverContinuation = null)
        {
            var continuation = await
                BeginStopWaitForStableStandstillAsync(
                    deceleration,
                    jerk,
                    options,
                    cancellationToken,
                    elapsedMilliseconds,
                    delayAsync,
                    null,
                    beforeStopWriteCommit,
                    acceptedContinuationObserver,
                    resetTakeoverContinuation).ConfigureAwait(false);
            return await ResumeStopWaitForStableStandstillAsync(
                continuation,
                options,
                cancellationToken,
                elapsedMilliseconds,
                delayAsync).ConfigureAwait(false);
        }

        private void EnsureStopContinuationOwner(
            LMCAxisStopWaitContinuation continuation)
        {
            EnsureCurrentSessionForUse();
            lock (powerOnWaitCoordinator.Sync)
            {
                EnsureStopContinuationOwnerCore(continuation);
            }
        }

        private void EnsureStopContinuationOwnerCore(
            LMCAxisStopWaitContinuation continuation)
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
                    powerOnWaitCoordinator.PendingStopContinuation,
                    continuation))
            {
                throw new InvalidOperationException(
                    "The Axis Stop continuation does not belong to this active connection, session, axis, or latest pending operation.");
            }
        }

        private void EnsureStopSubmissionAdmission(
            LMCAxisResetWaitContinuation resetTakeoverContinuation)
        {
            lock (powerOnWaitCoordinator.Sync)
            {
                EnsureStopSubmissionAdmissionCore(
                    resetTakeoverContinuation);
            }
        }

        private void EnsureStopSubmissionAdmissionCore(
            LMCAxisResetWaitContinuation resetTakeoverContinuation)
        {
            EnsureNoAxisAcceptedMutationObserverInProgressCore();
            var pendingReset = powerOnWaitCoordinator
                .PendingResetContinuation;
            if (resetTakeoverContinuation == null)
            {
                if (pendingReset != null && pendingReset.IsPending)
                {
                    throw new LMCAxisResetWaitPendingException(pendingReset);
                }
                return;
            }

            if (!resetTakeoverContinuation.IsPending
                || !ReferenceEquals(
                    resetTakeoverContinuation.Coordinator,
                    powerOnWaitCoordinator)
                || !resetTakeoverContinuation.BelongsTo(
                    connection,
                    sessionGeneration,
                    AxisReference)
                || !ReferenceEquals(
                    pendingReset,
                    resetTakeoverContinuation))
            {
                throw new InvalidOperationException(
                    "The Axis Reset continuation is not the latest pending operation for this active connection, session, and axis.");
            }
        }

        private void ResolveStopContinuation(
            LMCAxisStopWaitContinuation continuation,
            long elapsedMilliseconds)
        {
            lock (powerOnWaitCoordinator.Sync)
            {
                EnsureStopContinuationOwnerCore(continuation);
                var actualMutationGeneration = powerOnWaitCoordinator
                    .MutationGeneration;
                continuation.ObserveMutationGeneration(
                    actualMutationGeneration);
                if (continuation.StopMutationGeneration <= 0
                    || actualMutationGeneration
                        != continuation.StopMutationGeneration)
                {
                    throw new LMCAxisStopInterferenceException(
                        continuation.CaptureEvidence(elapsedMilliseconds),
                        continuation);
                }

                if (!continuation.HasStableStandstillProof)
                {
                    throw new InvalidOperationException(
                        "The Axis Stop continuation cannot complete without stable standstill proof.");
                }

                continuation.MarkCompleted();
                powerOnWaitCoordinator.PendingStopContinuation = null;
            }
        }

        private async Task AcquireAxisStopStatusGateAsync(
            LMCAxisStopWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds)
        {
            var remaining = GetAxisStopWaitRemaining(
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
                    throw new LMCAxisStopWaitDeadlineException();
                }
            }
        }

        private async Task AcquireAxisStopMutationGateAsync(
            LMCAxisStopWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds)
        {
            LMCGroupResetObserverScope.ThrowIfMemberMutationReentrant(
                connection,
                sessionGeneration,
                AxisReference);
            var remaining = GetAxisStopWaitRemaining(
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
                    throw new LMCAxisStopWaitDeadlineException();
                }
            }
        }

        private async Task<LMCAxisStopSubmissionPublication>
            SendAxisStopForWaitAsync(
            byte[] request,
            LMCAxisStopWaitTracker tracker,
            LMCAxisStopWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            Action beforeStopWriteCommit,
            Action beforeAcceptedContinuationPublication,
            bool acceptedObserverRequested,
            LMCAxisResetWaitContinuation resetTakeoverContinuation)
        {
            var remaining = GetAxisStopWaitRemaining(
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
                            ThrowIfAxisStopWaitCannotStartWire(
                                cancellationToken,
                                deadlineCancellation,
                                elapsedMilliseconds,
                                options.TimeoutMilliseconds);
                            EnsureStopSubmissionAdmission(
                                resetTakeoverContinuation);
                            if (beforeStopWriteCommit != null)
                            {
                                beforeStopWriteCommit();
                            }
                        },
                        () =>
                        {
                            tracker.MarkSubmissionOutcomeUncertain();
                            tracker.SetStopMutationGeneration(
                                powerOnWaitCoordinator
                                    .MarkMutationMayHaveBeenSent());
                        })
                        .ConfigureAwait(false);
                }
                catch (LMCPostWriteDeadlineException)
                {
                    tracker.MarkTransportInvalidatedAtDeadline();
                    throw new LMCAxisStopWaitDeadlineException();
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCAxisStopWaitDeadlineException();
                }

                var acknowledgement =
                    LMCConnection.ParseCommandAcknowledgement(
                        raw,
                        "Axis Stop");
                if (!acknowledgement.IsSuccess)
                {
                    powerOnWaitCoordinator.TryRollbackRejectedMutation(
                        tracker.StopMutationGeneration);
                }
                if (acknowledgement.IsSuccess
                    && beforeAcceptedContinuationPublication != null)
                {
                    beforeAcceptedContinuationPublication();
                }

                LMCAxisStopWaitContinuation continuation = null;
                connection.PublishSessionBoundSendPriorityResult(
                    sessionGeneration,
                    LMC_CommandId.Stop,
                    () =>
                    {
                        lock (powerOnWaitCoordinator.Sync)
                        {
                            tracker.SetAcknowledgement(acknowledgement);
                            if (!acknowledgement.IsSuccess)
                            {
                                return;
                            }

                            EnsureStopSubmissionAdmissionCore(
                                resetTakeoverContinuation);
                            var acceptedContinuation =
                                new LMCAxisStopWaitContinuation(
                                    powerOnWaitCoordinator,
                                    connection,
                                    AxisName,
                                    AxisReference,
                                    sessionGeneration,
                                    tracker);
                            var previous = powerOnWaitCoordinator
                                .PendingStopContinuation;
                            if (previous != null && previous.IsPending)
                            {
                                previous.MarkSuperseded();
                            }

                            if (resetTakeoverContinuation != null)
                            {
                                resetTakeoverContinuation
                                    .MarkSupersededBySafetyStop(
                                        acceptedContinuation);
                                powerOnWaitCoordinator
                                    .PendingResetContinuation = null;
                            }

                            powerOnWaitCoordinator
                                .PendingStopContinuation =
                                acceptedContinuation;
                            powerOnWaitCoordinator
                                .StopAcceptanceObserverInProgress =
                                acceptedObserverRequested;
                            continuation = acceptedContinuation;
                        }
                    });
                return new LMCAxisStopSubmissionPublication(
                    acknowledgement,
                    continuation);
            }
        }

        private async Task<LMCReadStatusResult>
            ReadAxisStatusForStopWaitAsync(
                LMCAxisStopWaitContinuation continuation,
                LMCAxisPowerOnWaitContinuation powerOnObservationTarget,
                LMCAxisStopWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Action beforeStatusResultPublication)
        {
            EnsureStopContinuationOwner(continuation);
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfAxisStopWaitMutationIntervened(
                continuation,
                elapsedMilliseconds);
            var remaining = GetAxisStopWaitRemaining(
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
                            ThrowIfAxisStopWaitCannotStartWire(
                                cancellationToken,
                                deadlineCancellation,
                                elapsedMilliseconds,
                                options.TimeoutMilliseconds);
                            ThrowIfAxisStopWaitMutationIntervened(
                                continuation,
                                elapsedMilliseconds);
                        },
                        null).ConfigureAwait(false);
                }
                catch (LMCPostWriteDeadlineException)
                {
                    continuation.MarkTransportInvalidatedAtDeadline();
                    throw new LMCAxisStopWaitDeadlineException();
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCAxisStopWaitDeadlineException();
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
                        lock (powerOnWaitCoordinator.Sync)
                        {
                            long actualMutationGeneration;
                            var published = powerOnWaitCoordinator
                                .TryPublishForMutationGeneration(
                                    continuation.StopMutationGeneration,
                                    () =>
                                    {
                                        EnsureStopContinuationOwnerCore(
                                            continuation);
                                        ObservePendingPowerOnStatus(
                                            powerOnObservationTarget,
                                            result);
                                        continuation.Observe(result);
                                    },
                                    out actualMutationGeneration);
                            continuation.ObserveMutationGeneration(
                                actualMutationGeneration);
                            if (!published)
                            {
                                throw new LMCAxisStopInterferenceException(
                                    continuation.CaptureEvidence(
                                        elapsedMilliseconds()),
                                    continuation);
                            }
                        }
                    });

                ThrowIfAxisStopWaitExpiredAfterWire(
                    cancellationToken,
                    deadlineCancellation,
                    elapsedMilliseconds,
                    options.TimeoutMilliseconds);
                return result;
            }
        }

        private void ThrowIfAxisStopWaitMutationIntervened(
            LMCAxisStopWaitContinuation continuation,
            Func<long> elapsedMilliseconds)
        {
            lock (powerOnWaitCoordinator.Sync)
            {
                EnsureStopContinuationOwnerCore(continuation);
                var actualGeneration = powerOnWaitCoordinator
                    .MutationGeneration;
                continuation.ObserveMutationGeneration(actualGeneration);
                if (continuation.StopMutationGeneration <= 0
                    || actualGeneration != continuation.StopMutationGeneration)
                {
                    throw new LMCAxisStopInterferenceException(
                        continuation.CaptureEvidence(elapsedMilliseconds()),
                        continuation);
                }
            }
        }

        private static async Task DelayAxisStopWaitAsync(
            LMCAxisStopWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            Func<int, CancellationToken, Task> delayAsync)
        {
            var remaining = GetAxisStopWaitRemaining(
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
                    throw new LMCAxisStopWaitDeadlineException();
                }

                cancellationToken.ThrowIfCancellationRequested();
                if (elapsedMilliseconds() >= options.TimeoutMilliseconds
                    || (deadlineCancellation.IsCancellationRequested
                        && !cancellationToken.IsCancellationRequested))
                {
                    throw new LMCAxisStopWaitDeadlineException();
                }
            }
        }

        private static long GetAxisStopWaitRemaining(
            LMCAxisStopWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var remaining = options.TimeoutMilliseconds
                - elapsedMilliseconds();
            if (remaining <= 0)
            {
                throw new LMCAxisStopWaitDeadlineException();
            }

            return remaining;
        }

        private static LMCAxisStopWaitOptions
            ValidateAxisStopWaitOptions(
                LMCAxisStopWaitOptions options,
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

        private static void ThrowIfAxisStopWaitCannotStartWire(
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
                throw new LMCAxisStopWaitDeadlineException();
            }
        }

        private static void ThrowIfAxisStopWaitExpiredAfterWire(
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
                throw new LMCAxisStopWaitDeadlineException();
            }
        }

        private static void ThrowIfAxisStopWaitExpiredAfterPublication(
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            int timeoutMilliseconds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (elapsedMilliseconds() >= timeoutMilliseconds)
            {
                throw new LMCAxisStopWaitDeadlineException();
            }
        }

        private sealed class LMCAxisStopWaitDeadlineException
            : TimeoutException
        {
        }

        private sealed class LMCAxisStopSubmissionPublication
        {
            internal LMCAxisStopSubmissionPublication(
                LMC_Response acknowledgement,
                LMCAxisStopWaitContinuation continuation)
            {
                Acknowledgement = acknowledgement;
                Continuation = continuation;
            }

            internal LMC_Response Acknowledgement { get; private set; }
            internal LMCAxisStopWaitContinuation Continuation
            {
                get;
                private set;
            }
        }
    }
}

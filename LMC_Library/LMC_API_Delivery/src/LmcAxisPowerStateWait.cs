using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public partial class LMCSingleAxis
    {
        /// <summary>
        /// Evidence for one accepted Axis Power On whose stable PowerOn state is
        /// not yet resolved. A pending continuation blocks another Power On.
        /// </summary>
        public LMCAxisPowerOnWaitContinuation PendingPowerOnWaitContinuation
        {
            get
            {
                lock (powerOnWaitCoordinator.Sync)
                {
                    return powerOnWaitCoordinator.PendingContinuation;
                }
            }
        }

        /// <summary>
        /// The latest accepted Axis Power Off whose stable PowerOff and
        /// Standstill state has not yet been resolved. Resuming this
        /// continuation sends status reads only.
        /// </summary>
        public LMCAxisPowerOffWaitContinuation
            PendingPowerOffWaitContinuation
        {
            get
            {
                lock (powerOnWaitCoordinator.Sync)
                {
                    return powerOnWaitCoordinator
                        .PendingPowerOffContinuation;
                }
            }
        }

        /// <summary>
        /// Sends exactly one Axis Power On request, preserves the accepted
        /// acknowledgement, then polls 0x2028 until PowerOn is stable.
        /// </summary>
        public Task<LMCAxisPowerStateWaitResult>
            PowerOnAndWaitForStableStateAsync(
                CancellationToken cancellationToken)
        {
            return PowerOnAndWaitForStableStateAsync(
                new LMCAxisPowerStateWaitOptions(),
                cancellationToken);
        }

        public Task<LMCAxisPowerStateWaitResult>
            PowerOnAndWaitForStableStateAsync(
                LMCAxisPowerStateWaitOptions options,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return PowerOnAndWaitForStableStateAsync(
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        /// <summary>
        /// Sends exactly one Axis Power On request and synchronously invokes
        /// <paramref name="acceptedContinuationObserver"/> after the accepted
        /// continuation is installed but before cancellation, deadline, or
        /// status polling is observed. If the observer throws, polling does not
        /// start and the accepted continuation remains pending.
        /// </summary>
        public Task<LMCAxisPowerStateWaitResult>
            PowerOnAndWaitForStableStateAsync(
                LMCAxisPowerStateWaitOptions options,
                Action<LMCAxisPowerOnWaitContinuation>
                    acceptedContinuationObserver,
                CancellationToken cancellationToken)
        {
            if (acceptedContinuationObserver == null)
            {
                throw new ArgumentNullException(
                    "acceptedContinuationObserver");
            }

            var stopwatch = Stopwatch.StartNew();
            return PowerOnAndWaitForStableStateAsync(
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync,
                acceptedContinuationObserver);
        }

        /// <summary>
        /// Resumes only 0x2028 polling for an already accepted Axis Power On.
        /// No 0x2023 request is sent by this method.
        /// </summary>
        public Task<LMCAxisPowerStateWaitResult>
            ResumePowerOnWaitForStableStateAsync(
                LMCAxisPowerOnWaitContinuation continuation,
                CancellationToken cancellationToken)
        {
            return ResumePowerOnWaitForStableStateAsync(
                continuation,
                new LMCAxisPowerStateWaitOptions(),
                cancellationToken);
        }

        public Task<LMCAxisPowerStateWaitResult>
            ResumePowerOnWaitForStableStateAsync(
                LMCAxisPowerOnWaitContinuation continuation,
                LMCAxisPowerStateWaitOptions options,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return ResumePowerOnWaitForStableStateAsync(
                continuation,
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        /// <summary>
        /// Resolves the exact pending Power On continuation only after this
        /// axis handle has observed the required consecutive PowerOff and
        /// Standstill status samples. This method sends no wire command.
        /// </summary>
        public void ResolvePowerOnWaitAfterStablePowerOff(
            LMCAxisPowerOnWaitContinuation continuation)
        {
            if (!powerOnWaitCoordinator.StatusObservationGate.Wait(0))
            {
                throw new InvalidOperationException(
                    "The pending Axis Power On cannot be resolved while a status-only Power On verification is active.");
            }

            try
            {
                EnsurePowerOnContinuationOwner(continuation);
                lock (powerOnWaitCoordinator.Sync)
                {
                    EnsurePowerOnContinuationOwnerCore(continuation);
                    if (powerOnWaitCoordinator
                        .AcceptanceObserverInProgress)
                    {
                        throw new InvalidOperationException(
                            "The pending Axis Power On cannot be resolved while its accepted-continuation observer is running.");
                    }
                    if (powerOnWaitCoordinator.WaitInProgress)
                    {
                        throw new InvalidOperationException(
                            "The pending Axis Power On cannot be resolved while a status-only Power On verification is active.");
                    }

                    if (!continuation.HasStablePowerOffStandstillProof)
                    {
                        throw new InvalidOperationException(
                            "The pending Axis Power On cannot be resolved until three consecutive successful PowerOff and Standstill status samples are observed.");
                    }

                    continuation.MarkResolved();
                    powerOnWaitCoordinator.PendingContinuation = null;
                }
            }
            finally
            {
                powerOnWaitCoordinator.StatusObservationGate.Release();
            }
        }

        /// <summary>
        /// Performs read-only 0x2028 polling. This helper never sends an Axis
        /// Power On or Power Off command and is suitable for restart recovery.
        /// </summary>
        public Task<LMCAxisPowerStateWaitResult> WaitForPowerStateAsync(
            bool expectedPowerOn,
            CancellationToken cancellationToken)
        {
            return WaitForPowerStateAsync(
                expectedPowerOn,
                new LMCAxisPowerStateWaitOptions(),
                cancellationToken);
        }

        public Task<LMCAxisPowerStateWaitResult> WaitForPowerStateAsync(
            bool expectedPowerOn,
            LMCAxisPowerStateWaitOptions options,
            CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return WaitForPowerStateAsync(
                expectedPowerOn,
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        /// <summary>
        /// Sends exactly one 0x2023 Axis Power Off request and returns after a
        /// valid successful acknowledgement has been preserved as a
        /// session-bound continuation. This method performs no 0x2028 status
        /// reads and never acquires the status-observation gate.
        /// </summary>
        public Task<LMCAxisPowerOffWaitContinuation>
            BeginPowerOffWaitForStableStateAsync(
            CancellationToken cancellationToken)
        {
            return BeginPowerOffWaitForStableStateAsync(
                new LMCAxisPowerStateWaitOptions(),
                cancellationToken);
        }

        public Task<LMCAxisPowerOffWaitContinuation>
            BeginPowerOffWaitForStableStateAsync(
            LMCAxisPowerStateWaitOptions options,
            CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return BeginPowerOffWaitForStableStateAsync(
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        /// <summary>
        /// Sends exactly one Axis Power Off request and synchronously invokes
        /// <paramref name="acceptedContinuationObserver"/> after the accepted
        /// continuation and its observer latch are published, but before
        /// cancellation or deadline is observed. If the observer throws, its
        /// original exception is propagated and the continuation stays
        /// pending for status-only recovery.
        /// </summary>
        public Task<LMCAxisPowerOffWaitContinuation>
            BeginPowerOffWaitForStableStateAsync(
            LMCAxisPowerStateWaitOptions options,
            Action<LMCAxisPowerOffWaitContinuation>
                acceptedContinuationObserver,
            CancellationToken cancellationToken)
        {
            if (acceptedContinuationObserver == null)
            {
                throw new ArgumentNullException(
                    "acceptedContinuationObserver");
            }

            var stopwatch = Stopwatch.StartNew();
            return BeginPowerOffWaitForStableStateAsync(
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync,
                null,
                null,
                acceptedContinuationObserver);
        }

        internal async Task<LMCAxisPowerOffWaitContinuation>
            BeginPowerOffWaitForStableStateAsync(
            LMCAxisPowerStateWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            Func<int, CancellationToken, Task> delayAsync,
            Action beforeAcceptedContinuationPublication = null,
            Action beforePowerOffWriteCommit = null,
            Action<LMCAxisPowerOffWaitContinuation>
                acceptedContinuationObserver = null)
        {
            var validated = ValidateAxisPowerStateWaitOptions(
                options,
                elapsedMilliseconds,
                delayAsync);
            var tracker = new LMCAxisPowerOffWaitTracker(
                validated.StableSampleCount);
            LMCAxisPowerOffWaitContinuation continuation = null;
            var mutationGateAcquired = false;
            var observerLatchSet = false;
            var observerInvocationActive = false;

            try
            {
                EnsureCurrentSessionForUse();
                cancellationToken.ThrowIfCancellationRequested();
                await AcquireAxisPowerOffMutationGateAsync(
                    validated,
                    cancellationToken,
                    elapsedMilliseconds).ConfigureAwait(false);
                mutationGateAcquired = true;
                EnsureCurrentSessionForUse();
                EnsureNoAxisAcceptedMutationObserverInProgress();
                EnsureNoPowerOffAcceptanceObserverInProgress();
                var publication = await SendAxisPowerOffForWaitAsync(
                    LMC_Frame.LMCAxisPower(AxisReference, false),
                    tracker,
                    validated,
                    cancellationToken,
                    elapsedMilliseconds,
                    beforeAcceptedContinuationPublication,
                    beforePowerOffWriteCommit,
                    acceptedContinuationObserver != null)
                    .ConfigureAwait(false);
                continuation = publication.Continuation;
                observerLatchSet = continuation != null
                    && acceptedContinuationObserver != null;
                if (!publication.Acknowledgement.IsSuccess)
                {
                    throw new LMCAxisPowerOffRejectedException(
                        tracker.CaptureEvidence(elapsedMilliseconds()));
                }

                if (acceptedContinuationObserver != null)
                {
                    powerOnWaitCoordinator.MutationGate.Release();
                    mutationGateAcquired = false;
                    observerInvocationActive = true;
                    acceptedContinuationObserver(continuation);
                    observerInvocationActive = false;
                }

                ThrowIfAxisPowerOffWaitExpiredAfterPublication(
                    cancellationToken,
                    elapsedMilliseconds,
                    validated.TimeoutMilliseconds);
                return continuation;
            }
            catch (LMCAxisPowerOffRejectedException)
            {
                throw;
            }
            catch (LMCAxisPowerOffWaitDeadlineException)
            {
                throw new LMCAxisPowerOffWaitTimeoutException(
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

                throw new LMCAxisPowerOffWaitCanceledException(
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

                throw new LMCAxisPowerOffSubmissionException(
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
                            .PowerOffAcceptanceObserverInProgress = false;
                    }
                }

                if (mutationGateAcquired)
                {
                    powerOnWaitCoordinator.MutationGate.Release();
                }
            }
        }

        /// <summary>
        /// Resumes only 0x2028 status polling for an accepted Axis Power Off.
        /// This method never sends a 0x2023 request. Same-axis mutation
        /// attribution is process-local to LMCSingleAxis writes in this
        /// connection session and does not cover PLC, another RPC client,
        /// direct SDO, or group mutations.
        /// </summary>
        public Task<LMCAxisPowerOffWaitResult>
            ResumePowerOffWaitForStableStateAsync(
            LMCAxisPowerOffWaitContinuation continuation,
            CancellationToken cancellationToken)
        {
            return ResumePowerOffWaitForStableStateAsync(
                continuation,
                new LMCAxisPowerStateWaitOptions(),
                cancellationToken);
        }

        public Task<LMCAxisPowerOffWaitResult>
            ResumePowerOffWaitForStableStateAsync(
            LMCAxisPowerOffWaitContinuation continuation,
            LMCAxisPowerStateWaitOptions options,
            CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return ResumePowerOffWaitForStableStateAsync(
                continuation,
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        internal async Task<LMCAxisPowerOffWaitResult>
            ResumePowerOffWaitForStableStateAsync(
            LMCAxisPowerOffWaitContinuation continuation,
            LMCAxisPowerStateWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            Func<int, CancellationToken, Task> delayAsync,
            Action beforeStatusResultPublication = null,
            Action afterStatusResultPublication = null)
        {
            var validated = ValidateAxisPowerStateWaitOptions(
                options,
                elapsedMilliseconds,
                delayAsync);
            EnsurePowerOffContinuationOwner(continuation);
            if (validated.StableSampleCount
                != continuation.RequiredStableSampleCount)
            {
                throw new ArgumentException(
                    "StableSampleCount must match the accepted Axis Power Off continuation.",
                    "options");
            }
            EnsureNoPowerOffAcceptanceObserverInProgress();

            var statusGateAcquired = false;
            var waitRegistered = false;
            var waitCompleted = false;
            LMCAxisPowerOnWaitContinuation powerOnObservationTarget = null;
            try
            {
                try
                {
                    await AcquireAxisPowerOffStatusGateAsync(
                        validated,
                        cancellationToken,
                        elapsedMilliseconds).ConfigureAwait(false);
                    statusGateAcquired = true;
                }
                catch (LMCAxisPowerOffWaitDeadlineException)
                {
                    throw new LMCAxisPowerOffWaitTimeoutException(
                        continuation.CaptureEvidence(elapsedMilliseconds()),
                        continuation);
                }
                catch (OperationCanceledException ex)
                {
                    throw new LMCAxisPowerOffWaitCanceledException(
                        continuation.CaptureEvidence(elapsedMilliseconds()),
                        continuation,
                        ex,
                        cancellationToken);
                }

                lock (powerOnWaitCoordinator.Sync)
                {
                    EnsurePowerOffContinuationOwnerCore(continuation);
                    if (powerOnWaitCoordinator.PowerOffWaitInProgress)
                    {
                        throw new InvalidOperationException(
                            "Another Axis Power Off status-only wait is already running.");
                    }

                    powerOnWaitCoordinator.PowerOffWaitInProgress = true;
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
                        ThrowIfAxisPowerOffWaitMutationIntervened(
                            continuation,
                            elapsedMilliseconds);
                        status = await ReadAxisStatusForPowerOffWaitAsync(
                            continuation,
                            powerOnObservationTarget,
                            validated,
                            cancellationToken,
                            elapsedMilliseconds,
                            beforeStatusResultPublication,
                            afterStatusResultPublication)
                            .ConfigureAwait(false);
                    }
                    catch (LMCAxisPowerOffInterferenceException)
                    {
                        throw;
                    }
                    catch (LMCAxisPowerOffWaitDeadlineException)
                    {
                        var evidence = continuation.CaptureEvidence(
                            elapsedMilliseconds());
                        throw new LMCAxisPowerOffWaitTimeoutException(
                            evidence,
                            continuation);
                    }
                    catch (OperationCanceledException ex)
                    {
                        var evidence = continuation.CaptureEvidence(
                            elapsedMilliseconds());
                        throw new LMCAxisPowerOffWaitCanceledException(
                            evidence,
                            continuation,
                            ex,
                            cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        var evidence = continuation.CaptureEvidence(
                            elapsedMilliseconds());
                        throw new LMCAxisPowerOffStatusException(
                            evidence,
                            continuation,
                            null,
                            ex);
                    }

                    if (!status.IsReadSuccessful)
                    {
                        var evidence = continuation.CaptureEvidence(
                            elapsedMilliseconds());
                        throw new LMCAxisPowerOffStatusException(
                            evidence,
                            continuation,
                            status,
                            null);
                    }

                    if (continuation.IsCompleted)
                    {
                        var evidence = continuation.CaptureEvidence(
                            elapsedMilliseconds());
                        waitCompleted = true;
                        return new LMCAxisPowerOffWaitResult(
                            evidence,
                            continuation);
                    }

                    try
                    {
                        await DelayAxisPowerOffWaitAsync(
                            validated,
                            cancellationToken,
                            elapsedMilliseconds,
                            delayAsync).ConfigureAwait(false);
                    }
                    catch (LMCAxisPowerOffWaitDeadlineException)
                    {
                        var evidence = continuation.CaptureEvidence(
                            elapsedMilliseconds());
                        throw new LMCAxisPowerOffWaitTimeoutException(
                            evidence,
                            continuation);
                    }
                    catch (OperationCanceledException ex)
                    {
                        var evidence = continuation.CaptureEvidence(
                            elapsedMilliseconds());
                        throw new LMCAxisPowerOffWaitCanceledException(
                            evidence,
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

                        powerOnWaitCoordinator.PowerOffWaitInProgress = false;
                    }
                }

                if (statusGateAcquired)
                {
                    powerOnWaitCoordinator.StatusObservationGate.Release();
                }
            }
        }

        /// <summary>
        /// Sends one Axis Power Off and then composes the accepted continuation
        /// with status-only stable-state verification. Pending Power On
        /// evidence is observed but never resolved automatically.
        /// </summary>
        public Task<LMCAxisPowerOffWaitResult>
            PowerOffAndWaitForStableStateAsync(
            CancellationToken cancellationToken)
        {
            return PowerOffAndWaitForStableStateAsync(
                new LMCAxisPowerStateWaitOptions(),
                cancellationToken);
        }

        public Task<LMCAxisPowerOffWaitResult>
            PowerOffAndWaitForStableStateAsync(
            LMCAxisPowerStateWaitOptions options,
            CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return PowerOffAndWaitForStableStateAsync(
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        /// <summary>
        /// Sends one Axis Power Off, invokes the accepted-continuation
        /// observer before any status read, and then performs status-only
        /// stable-state verification.
        /// </summary>
        public Task<LMCAxisPowerOffWaitResult>
            PowerOffAndWaitForStableStateAsync(
            LMCAxisPowerStateWaitOptions options,
            Action<LMCAxisPowerOffWaitContinuation>
                acceptedContinuationObserver,
            CancellationToken cancellationToken)
        {
            if (acceptedContinuationObserver == null)
            {
                throw new ArgumentNullException(
                    "acceptedContinuationObserver");
            }

            var stopwatch = Stopwatch.StartNew();
            return PowerOffAndWaitForStableStateAsync(
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync,
                null,
                acceptedContinuationObserver);
        }

        internal async Task<LMCAxisPowerOffWaitResult>
            PowerOffAndWaitForStableStateAsync(
            LMCAxisPowerStateWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            Func<int, CancellationToken, Task> delayAsync,
            Action beforePowerOffWriteCommit = null,
            Action<LMCAxisPowerOffWaitContinuation>
                acceptedContinuationObserver = null)
        {
            var validated = ValidateAxisPowerStateWaitOptions(
                options,
                elapsedMilliseconds,
                delayAsync);
            var continuation = await BeginPowerOffWaitForStableStateAsync(
                validated,
                cancellationToken,
                elapsedMilliseconds,
                delayAsync,
                null,
                beforePowerOffWriteCommit,
                acceptedContinuationObserver).ConfigureAwait(false);
            return await ResumePowerOffWaitForStableStateAsync(
                continuation,
                validated,
                cancellationToken,
                elapsedMilliseconds,
                delayAsync).ConfigureAwait(false);
        }

        private void EnsurePowerOffContinuationOwner(
            LMCAxisPowerOffWaitContinuation continuation)
        {
            EnsureCurrentSessionForUse();
            lock (powerOnWaitCoordinator.Sync)
            {
                EnsurePowerOffContinuationOwnerCore(continuation);
            }
        }

        private void EnsurePowerOffContinuationOwnerCore(
            LMCAxisPowerOffWaitContinuation continuation)
        {
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
                    powerOnWaitCoordinator.PendingPowerOffContinuation,
                    continuation))
            {
                throw new InvalidOperationException(
                    "The Axis Power Off continuation does not belong to this active connection, session, axis, or latest pending operation.");
            }
        }

        private async Task AcquireAxisPowerOffStatusGateAsync(
            LMCAxisPowerStateWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds)
        {
            var remaining = GetAxisPowerOffWaitRemaining(
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
                    throw new LMCAxisPowerOffWaitDeadlineException();
                }
            }
        }

        private async Task AcquireAxisPowerOffMutationGateAsync(
            LMCAxisPowerStateWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds)
        {
            LMCGroupResetObserverScope.ThrowIfMemberMutationReentrant(
                connection,
                sessionGeneration,
                AxisReference);
            var remaining = GetAxisPowerOffWaitRemaining(
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
                    throw new LMCAxisPowerOffWaitDeadlineException();
                }
            }
        }

        private async Task<LMCAxisPowerOffSubmissionPublication>
            SendAxisPowerOffForWaitAsync(
            byte[] request,
            LMCAxisPowerOffWaitTracker tracker,
            LMCAxisPowerStateWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            Action beforeAcceptedContinuationPublication,
            Action beforePowerOffWriteCommit,
            bool acceptanceObserverWillRun)
        {
            var remaining = GetAxisPowerOffWaitRemaining(
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
                            ThrowIfAxisPowerOffWaitCannotStartWire(
                                cancellationToken,
                                deadlineCancellation,
                                elapsedMilliseconds,
                                options.TimeoutMilliseconds);
                            if (beforePowerOffWriteCommit != null)
                            {
                                beforePowerOffWriteCommit();
                            }
                        },
                        () =>
                        {
                            tracker.MarkSubmissionOutcomeUncertain();
                            tracker.SetPowerOffMutationGeneration(
                                powerOnWaitCoordinator
                                    .MarkMutationMayHaveBeenSent());
                        })
                        .ConfigureAwait(false);
                }
                catch (LMCPostWriteDeadlineException)
                {
                    tracker.MarkTransportInvalidatedAtDeadline();
                    throw new LMCAxisPowerOffWaitDeadlineException();
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCAxisPowerOffWaitDeadlineException();
                }

                var acknowledgement =
                    LMCConnection.ParseCommandAcknowledgement(
                        raw,
                        "Axis Power Off");
                if (!acknowledgement.IsSuccess)
                {
                    powerOnWaitCoordinator.TryRollbackRejectedMutation(
                        tracker.PowerOffMutationGeneration);
                }
                LMCAxisPowerOffWaitContinuation continuation = null;
                connection.PublishSessionBoundSendPriorityResult(
                    sessionGeneration,
                    LMC_CommandId.Power,
                    () =>
                    {
                        lock (powerOnWaitCoordinator.Sync)
                        {
                            tracker.SetAcknowledgement(acknowledgement);
                            if (!acknowledgement.IsSuccess)
                            {
                                return;
                            }

                            if (beforeAcceptedContinuationPublication
                                != null)
                            {
                                beforeAcceptedContinuationPublication();
                            }

                            continuation =
                                new LMCAxisPowerOffWaitContinuation(
                                    powerOnWaitCoordinator,
                                    connection,
                                    AxisName,
                                    AxisReference,
                                    sessionGeneration,
                                    tracker);
                            var previous = powerOnWaitCoordinator
                                .PendingPowerOffContinuation;
                            if (previous != null && previous.IsPending)
                            {
                                previous.MarkSuperseded();
                            }

                            powerOnWaitCoordinator
                                .PendingPowerOffContinuation = continuation;
                            powerOnWaitCoordinator
                                .PowerOffAcceptanceObserverInProgress =
                                acceptanceObserverWillRun;
                        }
                    });

                return new LMCAxisPowerOffSubmissionPublication(
                    acknowledgement,
                    continuation);
            }
        }

        private async Task<LMCReadStatusResult>
            ReadAxisStatusForPowerOffWaitAsync(
            LMCAxisPowerOffWaitContinuation continuation,
            LMCAxisPowerOnWaitContinuation powerOnObservationTarget,
            LMCAxisPowerStateWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            Action beforeStatusResultPublication,
            Action afterStatusResultPublication)
        {
            EnsurePowerOffContinuationOwner(continuation);
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfAxisPowerOffWaitMutationIntervened(
                continuation,
                elapsedMilliseconds);
            var remaining = GetAxisPowerOffWaitRemaining(
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
                            ThrowIfAxisPowerOffWaitCannotStartWire(
                                cancellationToken,
                                deadlineCancellation,
                                elapsedMilliseconds,
                                options.TimeoutMilliseconds);
                            ThrowIfAxisPowerOffWaitMutationIntervened(
                                continuation,
                                elapsedMilliseconds);
                        },
                        null)
                        .ConfigureAwait(false);
                }
                catch (LMCPostWriteDeadlineException)
                {
                    continuation.MarkTransportInvalidatedAtDeadline();
                    throw new LMCAxisPowerOffWaitDeadlineException();
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCAxisPowerOffWaitDeadlineException();
                }

                var result = LMCConnection.ParseReadStatusResult(raw);
                if (beforeStatusResultPublication != null)
                {
                    beforeStatusResultPublication();
                }

                var completed = false;
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
                                    continuation
                                        .PowerOffMutationGeneration,
                                    () =>
                                    {
                                        EnsurePowerOffContinuationOwnerCore(
                                            continuation);
                                        ObservePendingPowerOnStatus(
                                            powerOnObservationTarget,
                                            result);
                                        continuation.Observe(result);
                                        if (continuation
                                                .HasStablePowerOffStandstillProof
                                            && CanResolveAxisPowerOffAtPublication(
                                                cancellationToken,
                                                deadlineCancellation,
                                                elapsedMilliseconds,
                                                options
                                                    .TimeoutMilliseconds))
                                        {
                                            continuation.MarkCompleted();
                                            powerOnWaitCoordinator
                                                .PendingPowerOffContinuation =
                                                null;
                                            completed = true;
                                        }
                                    },
                                    out actualMutationGeneration);
                            continuation.ObserveMutationGeneration(
                                actualMutationGeneration);
                            if (!published)
                            {
                                throw new
                                    LMCAxisPowerOffInterferenceException(
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

                if (completed)
                {
                    return result;
                }

                ThrowIfAxisPowerOffWaitExpiredAfterWire(
                    cancellationToken,
                    deadlineCancellation,
                    elapsedMilliseconds,
                    options.TimeoutMilliseconds);
                ThrowIfAxisPowerOffWaitMutationIntervened(
                    continuation,
                    elapsedMilliseconds);
                return result;
            }
        }

        private void ThrowIfAxisPowerOffWaitMutationIntervened(
            LMCAxisPowerOffWaitContinuation continuation,
            Func<long> elapsedMilliseconds)
        {
            lock (powerOnWaitCoordinator.Sync)
            {
                EnsurePowerOffContinuationOwnerCore(continuation);
                var actualGeneration = powerOnWaitCoordinator
                    .MutationGeneration;
                continuation.ObserveMutationGeneration(actualGeneration);
                if (continuation.PowerOffMutationGeneration <= 0
                    || actualGeneration
                        != continuation.PowerOffMutationGeneration)
                {
                    throw new LMCAxisPowerOffInterferenceException(
                        continuation.CaptureEvidence(elapsedMilliseconds()),
                        continuation);
                }
            }
        }

        private static bool CanResolveAxisPowerOffAtPublication(
            CancellationToken cancellationToken,
            CancellationTokenSource deadlineCancellation,
            Func<long> elapsedMilliseconds,
            int timeoutMilliseconds)
        {
            return !cancellationToken.IsCancellationRequested
                && !deadlineCancellation.IsCancellationRequested
                && elapsedMilliseconds() < timeoutMilliseconds;
        }

        private static async Task DelayAxisPowerOffWaitAsync(
            LMCAxisPowerStateWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            Func<int, CancellationToken, Task> delayAsync)
        {
            var remaining = GetAxisPowerOffWaitRemaining(
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
                    throw new LMCAxisPowerOffWaitDeadlineException();
                }

                cancellationToken.ThrowIfCancellationRequested();
                if (elapsedMilliseconds() >= options.TimeoutMilliseconds
                    || (deadlineCancellation.IsCancellationRequested
                        && !cancellationToken.IsCancellationRequested))
                {
                    throw new LMCAxisPowerOffWaitDeadlineException();
                }
            }
        }

        private static long GetAxisPowerOffWaitRemaining(
            LMCAxisPowerStateWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var remaining = options.TimeoutMilliseconds
                - elapsedMilliseconds();
            if (remaining <= 0)
            {
                throw new LMCAxisPowerOffWaitDeadlineException();
            }

            return remaining;
        }

        private static void ThrowIfAxisPowerOffWaitCannotStartWire(
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
                throw new LMCAxisPowerOffWaitDeadlineException();
            }
        }

        private static void ThrowIfAxisPowerOffWaitExpiredAfterWire(
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
                throw new LMCAxisPowerOffWaitDeadlineException();
            }
        }

        private static void ThrowIfAxisPowerOffWaitExpiredAfterPublication(
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            int timeoutMilliseconds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (elapsedMilliseconds() >= timeoutMilliseconds)
            {
                throw new LMCAxisPowerOffWaitDeadlineException();
            }
        }

        private sealed class LMCAxisPowerOffSubmissionPublication
        {
            internal LMCAxisPowerOffSubmissionPublication(
                LMC_Response acknowledgement,
                LMCAxisPowerOffWaitContinuation continuation)
            {
                Acknowledgement = acknowledgement
                    ?? throw new ArgumentNullException("acknowledgement");
                Continuation = continuation;
            }

            internal LMC_Response Acknowledgement { get; private set; }
            internal LMCAxisPowerOffWaitContinuation Continuation
            {
                get;
                private set;
            }
        }

        private sealed class LMCAxisPowerOffWaitDeadlineException
            : TimeoutException
        {
        }

        internal Task<LMCAxisPowerStateWaitResult>
            PowerOnAndWaitForStableStateAsync(
                LMCAxisPowerStateWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync)
        {
            return PowerOnAndWaitForStableStateAsync(
                options,
                cancellationToken,
                elapsedMilliseconds,
                delayAsync,
                null,
                null);
        }

        internal async Task<LMCAxisPowerStateWaitResult>
            PowerOnAndWaitForStableStateAsync(
                LMCAxisPowerStateWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action<LMCAxisPowerOnWaitContinuation>
                    acceptedContinuationObserver,
                Action beforePowerOnWriteCommit = null,
                Action beforeAcceptedContinuationPublication = null,
                Action beforeStatusResultPublication = null,
                Action afterStatusResultPublication = null,
                Action beforeStatusCoordinatorLock = null,
                Action afterPowerOnAcknowledgementParsed = null)
        {
            var validated = ValidateAxisPowerStateWaitOptions(
                options,
                elapsedMilliseconds,
                delayAsync);
            var tracker = new LMCAxisPowerOnWaitTracker(
                validated.StableSampleCount);
            LMCAxisPowerOnWaitContinuation continuation = null;
            var mutationGateAcquired = false;
            try
            {
                EnsureCurrentSessionForUse();
                try
                {
                    await AcquireAxisPowerOnMutationGateAsync(
                        validated,
                        cancellationToken,
                        elapsedMilliseconds).ConfigureAwait(false);
                    mutationGateAcquired = true;
                }
                catch (LMCAxisPowerOnWaitDeadlineException)
                {
                    throw new LMCAxisPowerStateWaitTimeoutException(
                        true,
                        tracker.CaptureEvidence(elapsedMilliseconds()),
                        null);
                }
                catch (OperationCanceledException ex)
                {
                    throw new LMCAxisPowerStateWaitCanceledException(
                        true,
                        tracker.CaptureEvidence(elapsedMilliseconds()),
                        null,
                        ex,
                        cancellationToken);
                }

                EnsureCurrentSessionForUse();
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
                catch (OperationCanceledException ex)
                {
                    throw new LMCAxisPowerStateWaitCanceledException(
                        true,
                        tracker.CaptureEvidence(elapsedMilliseconds()),
                        null,
                        ex,
                        cancellationToken);
                }
                lock (powerOnWaitCoordinator.Sync)
                {
                    EnsureNoAxisAcceptedMutationObserverInProgressCore();
                    EnsureNoPowerOffAcceptanceObserverInProgressCore();
                    var pending = powerOnWaitCoordinator.PendingContinuation;
                    if (pending != null && pending.IsPending)
                    {
                        throw new LMCAxisPowerOnPendingException(pending);
                    }

                    if (elapsedMilliseconds()
                        >= validated.TimeoutMilliseconds)
                    {
                        throw new LMCAxisPowerStateWaitTimeoutException(
                            true,
                            tracker.CaptureEvidence(elapsedMilliseconds()),
                            null);
                    }
                }

                LMCAxisPowerOnSubmissionPublication publication;
                try
                {
                    publication = await SendPowerOnForWaitAsync(
                        tracker,
                        validated,
                        cancellationToken,
                        elapsedMilliseconds,
                        beforePowerOnWriteCommit,
                        beforeAcceptedContinuationPublication,
                        acceptedContinuationObserver != null,
                        afterPowerOnAcknowledgementParsed)
                        .ConfigureAwait(false);
                    continuation = publication.Continuation;
                }
                catch (LMCAxisPowerOnWaitDeadlineException)
                {
                    throw new LMCAxisPowerStateWaitTimeoutException(
                        true,
                        tracker.CaptureEvidence(elapsedMilliseconds()),
                        null);
                }
                catch (OperationCanceledException ex)
                {
                    throw new LMCAxisPowerStateWaitCanceledException(
                        true,
                        tracker.CaptureEvidence(elapsedMilliseconds()),
                        null,
                        ex,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    throw new LMCAxisPowerOnSubmissionException(
                        tracker.CaptureEvidence(elapsedMilliseconds()),
                        ex);
                }

                if (!publication.Acknowledgement.IsSuccess)
                {
                    throw new LMCAxisPowerOnRejectedException(
                        tracker.CaptureEvidence(elapsedMilliseconds()));
                }
            }
            finally
            {
                if (mutationGateAcquired)
                {
                    powerOnWaitCoordinator.MutationGate.Release();
                }
            }

            if (acceptedContinuationObserver != null)
            {
                try
                {
                    acceptedContinuationObserver(continuation);
                }
                finally
                {
                    lock (powerOnWaitCoordinator.Sync)
                    {
                        powerOnWaitCoordinator.AcceptanceObserverInProgress =
                            false;
                    }
                }
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (elapsedMilliseconds()
                    >= validated.TimeoutMilliseconds)
                {
                    throw new LMCAxisPowerOnWaitDeadlineException();
                }
            }
            catch (LMCAxisPowerOnWaitDeadlineException)
            {
                throw new LMCAxisPowerStateWaitTimeoutException(
                    true,
                    continuation.CaptureEvidence(elapsedMilliseconds()),
                    continuation);
            }
            catch (OperationCanceledException ex)
            {
                throw new LMCAxisPowerStateWaitCanceledException(
                    true,
                    continuation.CaptureEvidence(elapsedMilliseconds()),
                    continuation,
                    ex,
                    cancellationToken);
            }

            return await PollAcceptedPowerOnAsync(
                continuation,
                validated,
                false,
                cancellationToken,
                elapsedMilliseconds,
                delayAsync,
                beforeStatusResultPublication,
                afterStatusResultPublication,
                beforeStatusCoordinatorLock).ConfigureAwait(false);
        }

        private async Task AcquireAxisPowerOnMutationGateAsync(
            LMCAxisPowerStateWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds)
        {
            LMCGroupResetObserverScope.ThrowIfMemberMutationReentrant(
                connection,
                sessionGeneration,
                AxisReference);
            var remaining = GetAxisPowerOnWaitRemaining(
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
                    await powerOnWaitCoordinator.MutationGate.WaitAsync(
                        deadlineCancellation.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCAxisPowerOnWaitDeadlineException();
                }
            }
        }

        private async Task<LMCAxisPowerOnSubmissionPublication>
            SendPowerOnForWaitAsync(
            LMCAxisPowerOnWaitTracker tracker,
            LMCAxisPowerStateWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            Action beforePowerOnWriteCommit,
            Action beforeAcceptedContinuationPublication,
            bool acceptanceObserverWillRun,
            Action afterPowerOnAcknowledgementParsed)
        {
            EnsureCurrentSessionForUse();
            var remaining = GetAxisPowerOnWaitRemaining(
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
                        LMC_Frame.LMCAxisPower(AxisReference, true),
                        sessionGeneration,
                        preWriteCancellation.Token,
                        deadlineCancellation.Token,
                        () =>
                        {
                            ThrowIfAxisPowerOnWaitCannotStartWire(
                                cancellationToken,
                                deadlineCancellation,
                                elapsedMilliseconds,
                                options.TimeoutMilliseconds);
                            if (beforePowerOnWriteCommit != null)
                            {
                                beforePowerOnWriteCommit();
                            }
                        },
                        () =>
                        {
                            tracker.MarkSubmissionOutcomeUncertain();
                            tracker.SetPowerOnMutationGeneration(
                                powerOnWaitCoordinator
                                    .MarkMutationMayHaveBeenSent());
                        })
                        .ConfigureAwait(false);
                }
                catch (LMCPostWriteDeadlineException)
                {
                    tracker.MarkTransportInvalidatedAtDeadline();
                    throw new LMCAxisPowerOnWaitDeadlineException();
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCAxisPowerOnWaitDeadlineException();
                }

                var acknowledgement =
                    LMCConnection.ParseCommandAcknowledgement(
                        raw,
                        "Axis Power On");
                if (!acknowledgement.IsSuccess)
                {
                    powerOnWaitCoordinator.TryRollbackRejectedMutation(
                        tracker.PowerOnMutationGeneration);
                }
                if (afterPowerOnAcknowledgementParsed != null)
                {
                    afterPowerOnAcknowledgementParsed();
                }

                LMCAxisPowerOnWaitContinuation continuation = null;
                connection.PublishSessionBoundSendPriorityResult(
                    sessionGeneration,
                    LMC_CommandId.Power,
                    () =>
                    {
                        lock (powerOnWaitCoordinator.Sync)
                        {
                            tracker.SetAcknowledgement(acknowledgement);
                            if (!acknowledgement.IsSuccess)
                            {
                                return;
                            }

                            if (beforeAcceptedContinuationPublication
                                != null)
                            {
                                beforeAcceptedContinuationPublication();
                            }

                            continuation =
                                new LMCAxisPowerOnWaitContinuation(
                                    powerOnWaitCoordinator,
                                    connection,
                                    AxisName,
                                    AxisReference,
                                    sessionGeneration,
                                    tracker);
                            powerOnWaitCoordinator.PendingContinuation =
                                continuation;
                            powerOnWaitCoordinator
                                .AcceptanceObserverInProgress =
                                acceptanceObserverWillRun;
                        }
                    });
                return new LMCAxisPowerOnSubmissionPublication(
                    acknowledgement,
                    continuation);
            }
        }

        internal Task<LMCAxisPowerStateWaitResult>
            ResumePowerOnWaitForStableStateAsync(
                LMCAxisPowerOnWaitContinuation continuation,
                LMCAxisPowerStateWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action beforeStatusResultPublication = null,
                Action afterStatusResultPublication = null,
                Action beforeStatusCoordinatorLock = null)
        {
            var validated = ValidateAxisPowerStateWaitOptions(
                options,
                elapsedMilliseconds,
                delayAsync);
            EnsurePowerOnContinuationOwner(continuation);
            lock (powerOnWaitCoordinator.Sync)
            {
                if (powerOnWaitCoordinator.AcceptanceObserverInProgress)
                {
                    throw new InvalidOperationException(
                        "The accepted Axis Power On continuation cannot be resumed while its acceptance observer is running.");
                }
            }
            return PollAcceptedPowerOnAsync(
                continuation,
                validated,
                true,
                cancellationToken,
                elapsedMilliseconds,
                delayAsync,
                beforeStatusResultPublication,
                afterStatusResultPublication,
                beforeStatusCoordinatorLock);
        }

        internal async Task<LMCAxisPowerStateWaitResult>
            WaitForPowerStateAsync(
                bool expectedPowerOn,
                LMCAxisPowerStateWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync)
        {
            var validated = ValidateAxisPowerStateWaitOptions(
                options,
                elapsedMilliseconds,
                delayAsync);
            var tracker = new LMCAxisPowerOnWaitTracker(
                validated.StableSampleCount);
            EnsureCurrentSessionForUse();
            var statusGateAcquired = false;
            try
            {
                try
                {
                    await AcquireAxisPowerOnStatusGateAsync(
                        validated,
                        cancellationToken,
                        elapsedMilliseconds).ConfigureAwait(false);
                    statusGateAcquired = true;
                }
                catch (LMCAxisPowerOnWaitDeadlineException)
                {
                    throw new LMCAxisPowerStateWaitTimeoutException(
                        expectedPowerOn,
                        tracker.CaptureEvidence(elapsedMilliseconds()),
                        null);
                }
                catch (OperationCanceledException ex)
                {
                    throw new LMCAxisPowerStateWaitCanceledException(
                        expectedPowerOn,
                        tracker.CaptureEvidence(elapsedMilliseconds()),
                        null,
                        ex,
                        cancellationToken);
                }

                while (true)
                {
                    LMCReadStatusResult status;
                    try
                    {
                        status = await ReadAxisStatusForReadOnlyPowerWaitAsync(
                            tracker,
                            validated,
                            cancellationToken,
                            elapsedMilliseconds).ConfigureAwait(false);
                    }
                    catch (LMCAxisPowerOnWaitDeadlineException)
                    {
                        throw new LMCAxisPowerStateWaitTimeoutException(
                            expectedPowerOn,
                            tracker.CaptureEvidence(elapsedMilliseconds()),
                            null);
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new LMCAxisPowerStateWaitCanceledException(
                            expectedPowerOn,
                            tracker.CaptureEvidence(elapsedMilliseconds()),
                            null,
                            ex,
                            cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        throw new LMCAxisPowerStateStatusException(
                            expectedPowerOn,
                            tracker.CaptureEvidence(elapsedMilliseconds()),
                            null,
                            null,
                            ex);
                    }

                    if (!status.IsReadSuccessful)
                    {
                        throw new LMCAxisPowerStateStatusException(
                            expectedPowerOn,
                            tracker.CaptureEvidence(elapsedMilliseconds()),
                            null,
                            status,
                            null);
                    }

                    var stable = expectedPowerOn
                        ? tracker.HasStablePowerOnProof
                        : tracker.HasStablePowerOffStandstillProof;
                    if (stable)
                    {
                        return new LMCAxisPowerStateWaitResult(
                            expectedPowerOn,
                            tracker.CaptureEvidence(elapsedMilliseconds()),
                            null,
                            false);
                    }

                    try
                    {
                        await DelayAcceptedPowerOnWaitAsync(
                            validated,
                            cancellationToken,
                            elapsedMilliseconds,
                            delayAsync).ConfigureAwait(false);
                    }
                    catch (LMCAxisPowerOnWaitDeadlineException)
                    {
                        throw new LMCAxisPowerStateWaitTimeoutException(
                            expectedPowerOn,
                            tracker.CaptureEvidence(elapsedMilliseconds()),
                            null);
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new LMCAxisPowerStateWaitCanceledException(
                            expectedPowerOn,
                            tracker.CaptureEvidence(elapsedMilliseconds()),
                            null,
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

        private async Task AcquireAxisPowerOnStatusGateAsync(
            LMCAxisPowerStateWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds)
        {
            var remaining = GetAxisPowerOnWaitRemaining(
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
                    throw new LMCAxisPowerOnWaitDeadlineException();
                }
            }
        }

        private async Task<LMCReadStatusResult>
            ReadAxisStatusForReadOnlyPowerWaitAsync(
            LMCAxisPowerOnWaitTracker tracker,
            LMCAxisPowerStateWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds)
        {
            EnsureCurrentSessionForUse();
            var remaining = GetAxisPowerOnWaitRemaining(
                options,
                cancellationToken,
                elapsedMilliseconds);
            var observationTarget =
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
                        () => ThrowIfAxisPowerOnWaitCannotStartWire(
                            cancellationToken,
                            deadlineCancellation,
                            elapsedMilliseconds,
                            options.TimeoutMilliseconds),
                        null).ConfigureAwait(false);
                }
                catch (LMCPostWriteDeadlineException)
                {
                    tracker.MarkTransportInvalidatedAtDeadline();
                    throw new LMCAxisPowerOnWaitDeadlineException();
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCAxisPowerOnWaitDeadlineException();
                }

                var result = LMCConnection.ParseReadStatusResult(raw);
                connection.PublishSessionBoundSendPriorityResult(
                    sessionGeneration,
                    LMC_CommandId.ReadStatus,
                    () =>
                    {
                        ObservePendingPowerOnStatus(
                            observationTarget,
                            result);
                        tracker.Observe(result);
                    });

                ThrowIfAxisPowerOnWaitExpiredAfterWire(
                    cancellationToken,
                    deadlineCancellation,
                    elapsedMilliseconds,
                    options.TimeoutMilliseconds);
                return result;
            }
        }

        private async Task<LMCReadStatusResult>
            ReadAxisStatusForPowerOnWaitAsync(
            LMCAxisPowerOnWaitContinuation continuation,
            LMCAxisPowerStateWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            Action beforeStatusResultPublication,
            Action afterStatusResultPublication,
            Action beforeStatusCoordinatorLock)
        {
            EnsurePowerOnContinuationOwner(continuation);
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfAxisPowerOnWaitMutationIntervened(
                continuation,
                elapsedMilliseconds);
            var remaining = GetAxisPowerOnWaitRemaining(
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
                            ThrowIfAxisPowerOnWaitCannotStartWire(
                                cancellationToken,
                                deadlineCancellation,
                                elapsedMilliseconds,
                                options.TimeoutMilliseconds);
                            ThrowIfAxisPowerOnWaitMutationIntervened(
                                continuation,
                                elapsedMilliseconds);
                        },
                        null).ConfigureAwait(false);
                }
                catch (LMCPostWriteDeadlineException)
                {
                    continuation.MarkTransportInvalidatedAtDeadline();
                    throw new LMCAxisPowerOnWaitDeadlineException();
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCAxisPowerOnWaitDeadlineException();
                }

                var result = LMCConnection.ParseReadStatusResult(raw);
                if (beforeStatusResultPublication != null)
                {
                    beforeStatusResultPublication();
                }

                var completed = false;
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
                                    continuation.PowerOnMutationGeneration,
                                    () =>
                                    {
                                        EnsurePowerOnContinuationOwnerCore(
                                            continuation);
                                        continuation.Observe(result);
                                        if (continuation
                                                .HasStablePowerOnProof
                                            && CanResolveAxisPowerOnAtPublication(
                                                cancellationToken,
                                                deadlineCancellation,
                                                elapsedMilliseconds,
                                                options
                                                    .TimeoutMilliseconds))
                                        {
                                            continuation.MarkResolved();
                                            powerOnWaitCoordinator
                                                .PendingContinuation = null;
                                            completed = true;
                                        }
                                    },
                                    out actualMutationGeneration);
                            continuation.ObserveMutationGeneration(
                                actualMutationGeneration);
                            if (!published)
                            {
                                throw new
                                    LMCAxisPowerOnInterferenceException(
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

                if (completed)
                {
                    return result;
                }

                ThrowIfAxisPowerOnWaitExpiredAfterWire(
                    cancellationToken,
                    deadlineCancellation,
                    elapsedMilliseconds,
                    options.TimeoutMilliseconds);
                ThrowIfAxisPowerOnWaitMutationIntervened(
                    continuation,
                    elapsedMilliseconds);
                return result;
            }
        }

        private void ThrowIfAxisPowerOnWaitMutationIntervened(
            LMCAxisPowerOnWaitContinuation continuation,
            Func<long> elapsedMilliseconds)
        {
            lock (powerOnWaitCoordinator.Sync)
            {
                EnsurePowerOnContinuationOwnerCore(continuation);
                var actualGeneration = powerOnWaitCoordinator
                    .MutationGeneration;
                continuation.ObserveMutationGeneration(actualGeneration);
                if (continuation.PowerOnMutationGeneration <= 0
                    || actualGeneration
                        != continuation.PowerOnMutationGeneration)
                {
                    throw new LMCAxisPowerOnInterferenceException(
                        continuation.CaptureEvidence(elapsedMilliseconds()),
                        continuation);
                }
            }
        }

        private static bool CanResolveAxisPowerOnAtPublication(
            CancellationToken cancellationToken,
            CancellationTokenSource deadlineCancellation,
            Func<long> elapsedMilliseconds,
            int timeoutMilliseconds)
        {
            return !cancellationToken.IsCancellationRequested
                && !deadlineCancellation.IsCancellationRequested
                && elapsedMilliseconds() < timeoutMilliseconds;
        }

        private static async Task DelayAcceptedPowerOnWaitAsync(
            LMCAxisPowerStateWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            Func<int, CancellationToken, Task> delayAsync)
        {
            var remaining = GetAxisPowerOnWaitRemaining(
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
                    throw new LMCAxisPowerOnWaitDeadlineException();
                }

                cancellationToken.ThrowIfCancellationRequested();
                if (elapsedMilliseconds() >= options.TimeoutMilliseconds
                    || (deadlineCancellation.IsCancellationRequested
                        && !cancellationToken.IsCancellationRequested))
                {
                    throw new LMCAxisPowerOnWaitDeadlineException();
                }
            }
        }

        private static long GetAxisPowerOnWaitRemaining(
            LMCAxisPowerStateWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var remaining = options.TimeoutMilliseconds
                - elapsedMilliseconds();
            if (remaining <= 0)
            {
                throw new LMCAxisPowerOnWaitDeadlineException();
            }

            return remaining;
        }

        private static void ThrowIfAxisPowerOnWaitCannotStartWire(
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
                throw new LMCAxisPowerOnWaitDeadlineException();
            }
        }

        private static void ThrowIfAxisPowerOnWaitExpiredAfterWire(
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
                throw new LMCAxisPowerOnWaitDeadlineException();
            }
        }

        private sealed class LMCAxisPowerOnSubmissionPublication
        {
            internal LMCAxisPowerOnSubmissionPublication(
                LMC_Response acknowledgement,
                LMCAxisPowerOnWaitContinuation continuation)
            {
                Acknowledgement = acknowledgement
                    ?? throw new ArgumentNullException("acknowledgement");
                Continuation = continuation;
            }

            internal LMC_Response Acknowledgement { get; private set; }
            internal LMCAxisPowerOnWaitContinuation Continuation
            {
                get;
                private set;
            }
        }

        private sealed class LMCAxisPowerOnWaitDeadlineException
            : TimeoutException
        {
        }

        private async Task<LMCAxisPowerStateWaitResult>
            PollAcceptedPowerOnAsync(
                LMCAxisPowerOnWaitContinuation continuation,
                LMCAxisPowerStateWaitOptions options,
                bool reusedAcceptedAcknowledgement,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action beforeStatusResultPublication = null,
                Action afterStatusResultPublication = null,
                Action beforeStatusCoordinatorLock = null)
        {
            EnsurePowerOnContinuationOwner(continuation);
            var statusGateAcquired = false;
            var waitRegistered = false;
            var waitCompleted = false;
            try
            {
                try
                {
                    await AcquireAxisPowerOnStatusGateAsync(
                        options,
                        cancellationToken,
                        elapsedMilliseconds).ConfigureAwait(false);
                    statusGateAcquired = true;
                }
                catch (LMCAxisPowerOnWaitDeadlineException)
                {
                    throw new LMCAxisPowerStateWaitTimeoutException(
                        true,
                        continuation.CaptureEvidence(elapsedMilliseconds()),
                        continuation);
                }
                catch (OperationCanceledException ex)
                {
                    throw new LMCAxisPowerStateWaitCanceledException(
                        true,
                        continuation.CaptureEvidence(elapsedMilliseconds()),
                        continuation,
                        ex,
                        cancellationToken);
                }

                lock (powerOnWaitCoordinator.Sync)
                {
                    EnsurePowerOnContinuationOwnerCore(continuation);
                    if (powerOnWaitCoordinator.WaitInProgress)
                    {
                        throw new InvalidOperationException(
                            "Another Axis Power On status-only wait is already running.");
                    }

                    powerOnWaitCoordinator.WaitInProgress = true;
                    waitRegistered = true;
                }

                while (true)
                {
                    LMCReadStatusResult status;
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        ThrowIfAxisPowerOnWaitMutationIntervened(
                            continuation,
                            elapsedMilliseconds);
                        status = await ReadAxisStatusForPowerOnWaitAsync(
                            continuation,
                            options,
                            cancellationToken,
                            elapsedMilliseconds,
                            beforeStatusResultPublication,
                            afterStatusResultPublication,
                            beforeStatusCoordinatorLock)
                            .ConfigureAwait(false);
                    }
                    catch (LMCAxisPowerOnInterferenceException)
                    {
                        throw;
                    }
                    catch (LMCAxisPowerOnWaitDeadlineException)
                    {
                        throw new LMCAxisPowerStateWaitTimeoutException(
                            true,
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation);
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new LMCAxisPowerStateWaitCanceledException(
                            true,
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation,
                            ex,
                            cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        throw new LMCAxisPowerStateStatusException(
                            true,
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation,
                            null,
                            ex);
                    }

                    if (!status.IsReadSuccessful)
                    {
                        throw new LMCAxisPowerStateStatusException(
                            true,
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation,
                            status,
                            null);
                    }

                    if (!continuation.IsPending)
                    {
                        var evidence = continuation.CaptureEvidence(
                            elapsedMilliseconds());
                        waitCompleted = true;
                        return new LMCAxisPowerStateWaitResult(
                            true,
                            evidence,
                            continuation,
                            reusedAcceptedAcknowledgement);
                    }

                    try
                    {
                        await DelayAcceptedPowerOnWaitAsync(
                            options,
                            cancellationToken,
                            elapsedMilliseconds,
                            delayAsync).ConfigureAwait(false);
                    }
                    catch (LMCAxisPowerOnWaitDeadlineException)
                    {
                        throw new LMCAxisPowerStateWaitTimeoutException(
                            true,
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation);
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new LMCAxisPowerStateWaitCanceledException(
                            true,
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
                        }

                        powerOnWaitCoordinator.WaitInProgress = false;
                    }
                }

                if (statusGateAcquired)
                {
                    powerOnWaitCoordinator.StatusObservationGate.Release();
                }
            }
        }

        private LMC_Response SendPowerOnWithPendingGuard()
        {
            EnsureCurrentSessionForUse();
            LMCGroupResetObserverScope.ThrowIfMemberMutationReentrant(
                connection,
                sessionGeneration,
                AxisReference);
            powerOnWaitCoordinator.MutationGate.Wait();
            try
            {
                EnsureNoPendingPowerOnContinuation();
                return SendPower(true);
            }
            finally
            {
                powerOnWaitCoordinator.MutationGate.Release();
            }
        }

        private async Task<LMC_Response> SendPowerOnWithPendingGuardAsync(
            CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            LMCGroupResetObserverScope.ThrowIfMemberMutationReentrant(
                connection,
                sessionGeneration,
                AxisReference);
            await powerOnWaitCoordinator.MutationGate.WaitAsync(
                cancellationToken).ConfigureAwait(false);
            try
            {
                EnsureNoPendingPowerOnContinuation();
                return await SendPowerOnUncheckedAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            finally
            {
                powerOnWaitCoordinator.MutationGate.Release();
            }
        }

        private LMC_Response
            SendPowerOffWithAcceptanceObserverGuard()
        {
            EnsureCurrentSessionForUse();
            LMCGroupResetObserverScope.ThrowIfMemberMutationReentrant(
                connection,
                sessionGeneration,
                AxisReference);
            powerOnWaitCoordinator.MutationGate.Wait();
            try
            {
                EnsureNoPowerOffAcceptanceObserverInProgress();
                return SendPower(false);
            }
            finally
            {
                powerOnWaitCoordinator.MutationGate.Release();
            }
        }

        private async Task<LMC_Response>
            SendPowerOffWithAcceptanceObserverGuardAsync(
            CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            LMCGroupResetObserverScope.ThrowIfMemberMutationReentrant(
                connection,
                sessionGeneration,
                AxisReference);
            await powerOnWaitCoordinator.MutationGate.WaitAsync(
                cancellationToken).ConfigureAwait(false);
            try
            {
                EnsureNoPowerOffAcceptanceObserverInProgress();
                return await SendAsyncWhileAxisMutationGateHeld(
                    LMC_Frame.LMCAxisPower(AxisReference, false),
                    cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                powerOnWaitCoordinator.MutationGate.Release();
            }
        }

        private Task<LMC_Response> SendPowerOnUncheckedAsync(
            CancellationToken cancellationToken)
        {
            return SendAsyncWhileAxisMutationGateHeld(
                LMC_Frame.LMCAxisPower(AxisReference, true),
                cancellationToken);
        }

        private void EnsureNoPendingPowerOnContinuation()
        {
            lock (powerOnWaitCoordinator.Sync)
            {
                EnsureNoPowerOffAcceptanceObserverInProgressCore();
                var pending = powerOnWaitCoordinator.PendingContinuation;
                if (pending != null && pending.IsPending)
                {
                    throw new LMCAxisPowerOnPendingException(pending);
                }
            }
        }

        private void EnsureNoPowerOffAcceptanceObserverInProgress()
        {
            lock (powerOnWaitCoordinator.Sync)
            {
                EnsureNoPowerOffAcceptanceObserverInProgressCore();
            }
        }

        private void EnsureNoPowerOffAcceptanceObserverInProgressCore()
        {
            if (powerOnWaitCoordinator
                .PowerOffAcceptanceObserverInProgress)
            {
                throw new InvalidOperationException(
                    "The accepted Axis Power Off observer is still running.");
            }
        }

        private void EnsurePowerOnContinuationOwner(
            LMCAxisPowerOnWaitContinuation continuation)
        {
            EnsureCurrentSessionForUse();
            lock (powerOnWaitCoordinator.Sync)
            {
                EnsurePowerOnContinuationOwnerCore(continuation);
            }
        }

        private void EnsurePowerOnContinuationOwnerCore(
            LMCAxisPowerOnWaitContinuation continuation)
        {
            if (continuation == null
                || !continuation.IsPending
                || !continuation.BelongsTo(
                    connection,
                    sessionGeneration,
                    AxisReference)
                || !ReferenceEquals(
                    powerOnWaitCoordinator.PendingContinuation,
                    continuation))
            {
                throw new InvalidOperationException(
                    "The Axis Power On continuation does not belong to this active connection, session, axis, or pending operation.");
            }
        }

        private void ObservePendingPowerOnStatus(
            LMCAxisPowerOnWaitContinuation observationTarget,
            LMCReadStatusResult status)
        {
            lock (powerOnWaitCoordinator.Sync)
            {
                if (observationTarget != null
                    && observationTarget.IsPending
                    && ReferenceEquals(
                        powerOnWaitCoordinator.PendingContinuation,
                        observationTarget))
                {
                    observationTarget.Observe(status);
                }
            }
        }

        private LMCAxisPowerOnWaitContinuation
            CapturePendingPowerOnStatusObservation()
        {
            lock (powerOnWaitCoordinator.Sync)
            {
                return powerOnWaitCoordinator.PendingContinuation;
            }
        }

        private void ResetPendingPowerOnPowerOffProofCore(
            LMCAxisPowerOnWaitContinuation observationTarget)
        {
            if (observationTarget != null
                && observationTarget.IsPending
                && ReferenceEquals(
                    powerOnWaitCoordinator.PendingContinuation,
                    observationTarget))
            {
                observationTarget.ResetPowerOffStandstillProof();
            }
        }

        private static LMCAxisPowerStateWaitOptions
            ValidateAxisPowerStateWaitOptions(
                LMCAxisPowerStateWaitOptions options,
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

        private static Task DelayAsync(
            int delayMilliseconds,
            CancellationToken cancellationToken)
        {
            return Task.Delay(delayMilliseconds, cancellationToken);
        }
    }
}

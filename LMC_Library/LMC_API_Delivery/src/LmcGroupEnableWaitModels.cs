using System;
using System.Diagnostics;
using System.Threading;

namespace LasalMotionControlLib
{
    internal sealed class LMCGroupEnableWaitCoordinator
    {
        private long mutationGeneration;
        private long provisionalResetSafetyMutationGeneration;
        private LMCGroupResetWaitContinuation
            provisionalResetSafetyContinuation;
        private bool provisionalResetSafetyPermanentlySuperseded;

        internal LMCGroupEnableWaitCoordinator()
        {
            Sync = new object();
            StatusObservationGate = new SemaphoreSlim(1, 1);
            MutationGate = new SemaphoreSlim(1, 1);
        }

        internal object Sync { get; private set; }
        internal SemaphoreSlim StatusObservationGate { get; private set; }
        internal SemaphoreSlim MutationGate { get; private set; }
        internal LMCGroupEnableWaitContinuation PendingContinuation { get; set; }
        internal LMCGroupStopWaitContinuation PendingStopContinuation
        {
            get;
            set;
        }
        internal LMCGroupPowerStateWaitContinuation
            PendingPowerStateContinuation
        {
            get;
            set;
        }
        internal LMCGroupDisableWaitContinuation PendingDisableContinuation
        {
            get;
            set;
        }
        internal LMCGroupResetWaitContinuation PendingResetContinuation
        {
            get;
            set;
        }
        internal bool WaitInProgress { get; set; }
        internal bool StopWaitInProgress { get; set; }
        internal bool PowerStateWaitInProgress { get; set; }
        internal bool DisableWaitInProgress { get; set; }
        internal bool PowerAcceptanceObserverInProgress { get; set; }
        internal bool EnableAcceptanceObserverInProgress { get; set; }
        internal bool DisableAcceptanceObserverInProgress { get; set; }
        internal bool ResetAcceptanceObserverInProgress { get; set; }
        internal bool ResetWaitInProgress { get; set; }
        internal bool DirectEnableInProgress { get; set; }

        internal long MutationGeneration
        {
            get
            {
                lock (Sync)
                {
                    return mutationGeneration;
                }
            }
        }

        internal long MarkMutationMayHaveBeenSent()
        {
            return MarkMutationMayHaveBeenSent(false);
        }

        internal long MarkMutationMayHaveBeenSent(
            bool groupResetSafetyMutation)
        {
            lock (Sync)
            {
                provisionalResetSafetyMutationGeneration = 0;
                provisionalResetSafetyContinuation = null;
                provisionalResetSafetyPermanentlySuperseded = false;
                if (ResetAcceptanceObserverInProgress)
                {
                    throw new InvalidOperationException(
                        "A Group Reset accepted-continuation observer is still running.");
                }

                if (PendingResetContinuation != null
                    && PendingResetContinuation.IsPending)
                {
                    if (!groupResetSafetyMutation)
                    {
                        throw new LMCGroupResetWaitPendingException(
                            PendingResetContinuation);
                    }

                    PendingResetContinuation
                        .MarkSupersededBySafetyMutation();
                    provisionalResetSafetyContinuation =
                        PendingResetContinuation;
                    PendingResetContinuation = null;
                }

                mutationGeneration++;
                if (provisionalResetSafetyContinuation != null)
                {
                    provisionalResetSafetyMutationGeneration =
                        mutationGeneration;
                }
                return mutationGeneration;
            }
        }

        internal void FinalizeSafetyMutationAcknowledgement(
            long reservedGeneration)
        {
            lock (Sync)
            {
                if (provisionalResetSafetyMutationGeneration
                    == reservedGeneration)
                {
                    provisionalResetSafetyMutationGeneration = 0;
                    provisionalResetSafetyContinuation = null;
                    provisionalResetSafetyPermanentlySuperseded = false;
                }
            }
        }

        internal bool TryPermanentlySupersedeProvisionalGroupReset(
            LMCGroupResetWaitContinuation continuation)
        {
            lock (Sync)
            {
                if (continuation == null
                    || provisionalResetSafetyMutationGeneration <= 0
                    || !ReferenceEquals(
                        provisionalResetSafetyContinuation,
                        continuation)
                    || continuation.State
                        != LMCGroupResetWaitContinuationState
                            .SupersededBySafetyMutation)
                {
                    return false;
                }

                provisionalResetSafetyPermanentlySuperseded = true;
                return true;
            }
        }

        internal void ThrowIfGroupResetBlocksUnsafeMutation()
        {
            lock (Sync)
            {
                if (ResetAcceptanceObserverInProgress)
                {
                    throw new InvalidOperationException(
                        "A Group Reset accepted-continuation observer is still running.");
                }

                var pending = PendingResetContinuation;
                if (pending != null && pending.IsPending)
                {
                    throw new LMCGroupResetWaitPendingException(pending);
                }
            }
        }

        internal void ThrowIfGroupResetObserverReentrantSafetyMutation()
        {
            WaitForGroupResetObserverHandoffForSafetyMutation(
                CancellationToken.None);
        }

        internal void WaitForGroupResetObserverHandoffForSafetyMutation(
            CancellationToken cancellationToken)
        {
            WaitForGroupResetObserverHandoffForSafetyMutation(
                cancellationToken,
                Timeout.Infinite);
        }

        internal bool WaitForGroupResetObserverHandoffForSafetyMutation(
            CancellationToken cancellationToken,
            int timeoutMilliseconds)
        {
            if (timeoutMilliseconds < Timeout.Infinite)
            {
                throw new ArgumentOutOfRangeException(
                    "timeoutMilliseconds");
            }

            var stopwatch = timeoutMilliseconds == Timeout.Infinite
                ? null
                : Stopwatch.StartNew();
            lock (Sync)
            {
                while (ResetAcceptanceObserverInProgress)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var waitMilliseconds = 25;
                    if (stopwatch != null)
                    {
                        var remaining = timeoutMilliseconds
                            - stopwatch.ElapsedMilliseconds;
                        if (remaining <= 0)
                        {
                            return false;
                        }

                        waitMilliseconds = (int)Math.Min(
                            waitMilliseconds,
                            remaining);
                    }

                    Monitor.Wait(Sync, waitMilliseconds);
                }

                return true;
            }
        }

        /// <summary>
        /// Removes only the exact most-recent process-local mutation
        /// reservation after a structurally valid negative acknowledgement.
        /// The caller must still hold MutationGate.
        /// </summary>
        internal bool TryRollbackRejectedMutation(long reservedGeneration)
        {
            lock (Sync)
            {
                if (reservedGeneration <= 0
                    || mutationGeneration != reservedGeneration)
                {
                    return false;
                }

                if (provisionalResetSafetyMutationGeneration
                    == reservedGeneration)
                {
                    if (PendingResetContinuation != null
                        || provisionalResetSafetyContinuation == null
                        || !provisionalResetSafetyContinuation
                            .TryRestoreAfterRejectedSafetyMutation())
                    {
                        return false;
                    }

                    PendingResetContinuation =
                        provisionalResetSafetyContinuation;
                    provisionalResetSafetyMutationGeneration = 0;
                    provisionalResetSafetyContinuation = null;
                    provisionalResetSafetyPermanentlySuperseded = false;
                }

                mutationGeneration--;
                return true;
            }
        }

        /// <summary>
        /// Restores only a pending Group Reset provisionally superseded by a
        /// safety request whose structurally valid acknowledgement rejected
        /// the request. Non-Reset mutation-generation behavior is unchanged.
        /// </summary>
        internal bool TryRestoreGroupResetAfterRejectedSafetyMutation(
            long reservedGeneration)
        {
            lock (Sync)
            {
                if (reservedGeneration <= 0
                    || mutationGeneration != reservedGeneration
                    || provisionalResetSafetyMutationGeneration
                        != reservedGeneration
                    || PendingResetContinuation != null
                    || provisionalResetSafetyContinuation == null)
                {
                    return false;
                }

                if (provisionalResetSafetyPermanentlySuperseded)
                {
                    provisionalResetSafetyMutationGeneration = 0;
                    provisionalResetSafetyContinuation = null;
                    provisionalResetSafetyPermanentlySuperseded = false;
                    mutationGeneration--;
                    return true;
                }
                if (!provisionalResetSafetyContinuation
                    .TryRestoreAfterRejectedSafetyMutation())
                {
                    return false;
                }

                PendingResetContinuation =
                    provisionalResetSafetyContinuation;
                provisionalResetSafetyMutationGeneration = 0;
                provisionalResetSafetyContinuation = null;
                provisionalResetSafetyPermanentlySuperseded = false;
                mutationGeneration--;
                return true;
            }
        }

        internal void ResetPendingMutationProof()
        {
            lock (Sync)
            {
                ResetPendingMutationProofCore();
            }
        }

        private void ResetPendingMutationProofCore()
        {
            if (PendingContinuation != null
                && PendingContinuation.IsPending)
            {
                PendingContinuation.ResetProofCounters();
            }

            if (PendingPowerStateContinuation != null
                && PendingPowerStateContinuation.IsPending)
            {
                PendingPowerStateContinuation.ResetProofCounters();
            }

            if (PendingDisableContinuation != null
                && PendingDisableContinuation.IsPending)
            {
                PendingDisableContinuation.ResetProofCounters();
            }
        }

        internal bool TryPublishForMutationGeneration(
            long expectedGeneration,
            Action publish,
            out long actualGeneration)
        {
            if (publish == null)
            {
                throw new ArgumentNullException("publish");
            }

            lock (Sync)
            {
                actualGeneration = mutationGeneration;
                if (expectedGeneration <= 0
                    || actualGeneration != expectedGeneration)
                {
                    return false;
                }

                publish();
                return true;
            }
        }
    }

    /// <summary>
    /// Controls the total deadline for the current wait or resume call. A new wait
    /// includes the initial GroupEnable gate/write and all subsequent status polling.
    /// </summary>
    public sealed class LMCGroupEnableWaitOptions
    {
        public const int DefaultTimeoutMilliseconds = 5000;
        public const int DefaultPollIntervalMilliseconds = 50;
        public const int DefaultStableSampleCount = 3;

        public LMCGroupEnableWaitOptions()
        {
            TimeoutMilliseconds = DefaultTimeoutMilliseconds;
            PollIntervalMilliseconds = DefaultPollIntervalMilliseconds;
            StableSampleCount = DefaultStableSampleCount;
        }

        /// <summary>
        /// Total deadline measured from the current wait or resume call. A new wait
        /// includes the initial GroupEnable gate/write and all status polling.
        /// </summary>
        public int TimeoutMilliseconds { get; set; }
        public int PollIntervalMilliseconds { get; set; }
        public int StableSampleCount { get; set; }

        internal LMCGroupEnableWaitOptions SnapshotAndValidate()
        {
            if (TimeoutMilliseconds < 1 || TimeoutMilliseconds > 600000)
            {
                throw new ArgumentOutOfRangeException(
                    "TimeoutMilliseconds",
                    "TimeoutMilliseconds must be between 1 and 600000.");
            }

            if (PollIntervalMilliseconds < 1
                || PollIntervalMilliseconds > TimeoutMilliseconds)
            {
                throw new ArgumentOutOfRangeException(
                    "PollIntervalMilliseconds",
                    "PollIntervalMilliseconds must be positive and no greater than TimeoutMilliseconds.");
            }

            if (StableSampleCount < 1 || StableSampleCount > 100)
            {
                throw new ArgumentOutOfRangeException(
                    "StableSampleCount",
                    "StableSampleCount must be between 1 and 100.");
            }

            return new LMCGroupEnableWaitOptions
            {
                TimeoutMilliseconds = TimeoutMilliseconds,
                PollIntervalMilliseconds = PollIntervalMilliseconds,
                StableSampleCount = StableSampleCount
            };
        }
    }

    public enum LMCGroupEnableSubmissionOutcome
    {
        NotAttempted = 0,
        Rejected = 1,
        OutcomeUncertain = 2,
        Accepted = 3
    }

    internal sealed class LMCGroupEnableSubmissionTracker
    {
        private LMCGroupEnableSubmissionOutcome submissionOutcome;
        private LMC_Response acknowledgement;
        private bool transportInvalidatedAtDeadline;

        internal void MarkSubmissionOutcomeUncertain()
        {
            if (submissionOutcome
                == LMCGroupEnableSubmissionOutcome.NotAttempted)
            {
                submissionOutcome =
                    LMCGroupEnableSubmissionOutcome.OutcomeUncertain;
            }
        }

        internal void SetAcknowledgement(LMC_Response value)
        {
            acknowledgement = value;
            submissionOutcome = value != null
                    && value.IsFrameValid
                    && value.IsSuccess
                ? LMCGroupEnableSubmissionOutcome.Accepted
                : LMCGroupEnableSubmissionOutcome.Rejected;
        }

        internal void MarkTransportInvalidatedAtDeadline()
        {
            transportInvalidatedAtDeadline = true;
        }

        internal LMCGroupEnableWaitEvidence CaptureEvidence(
            long elapsedMilliseconds)
        {
            return new LMCGroupEnableWaitEvidence(
                submissionOutcome,
                acknowledgement,
                null,
                0,
                0,
                0,
                0,
                false,
                transportInvalidatedAtDeadline,
                elapsedMilliseconds);
        }

        internal LMCGroupEnableSubmissionOutcome SubmissionOutcome
        {
            get { return submissionOutcome; }
        }

        internal LMC_Response Acknowledgement
        {
            get { return acknowledgement; }
        }

        internal bool TransportInvalidatedAtDeadline
        {
            get { return transportInvalidatedAtDeadline; }
        }
    }

    /// <summary>
    /// Session-bound evidence that one GroupEnable command was accepted and must not be sent again.
    /// </summary>
    public sealed class LMCGroupEnableWaitContinuation
    {
        internal const int RequiredReleaseProofSampleCount = 3;

        private readonly object stateSync;
        private LMCGroupReadStatusResult lastObservedStatus;
        private int pollCount;
        private int stableSampleCount;
        private int disabledUnlockedSampleCount;
        private int poweredOffSampleCount;
        private int isPending;
        private readonly LMCGroupEnableSubmissionTracker submissionTracker;

        internal LMCGroupEnableWaitContinuation(
            LMCGroupEnableWaitCoordinator coordinator,
            string groupName,
            ushort groupReference,
            long sessionGeneration,
            LMC_Response acknowledgement,
            int requiredStableSampleCount)
            : this(
                coordinator,
                groupName,
                groupReference,
                sessionGeneration,
                CreateAcceptedSubmissionTracker(acknowledgement),
                requiredStableSampleCount)
        {
        }

        internal LMCGroupEnableWaitContinuation(
            LMCGroupEnableWaitCoordinator coordinator,
            string groupName,
            ushort groupReference,
            long sessionGeneration,
            LMCGroupEnableSubmissionTracker submissionTracker,
            int requiredStableSampleCount)
        {
            Coordinator = coordinator;
            stateSync = coordinator.Sync;
            this.submissionTracker = submissionTracker
                ?? throw new ArgumentNullException("submissionTracker");
            GroupName = groupName;
            GroupReference = groupReference;
            SessionGeneration = sessionGeneration;
            RequiredStableSampleCount = requiredStableSampleCount;
            isPending = 1;
        }

        private static LMCGroupEnableSubmissionTracker
            CreateAcceptedSubmissionTracker(LMC_Response acknowledgement)
        {
            var tracker = new LMCGroupEnableSubmissionTracker();
            tracker.SetAcknowledgement(acknowledgement);
            return tracker;
        }

        internal LMCGroupEnableWaitCoordinator Coordinator { get; private set; }

        public string GroupName { get; private set; }
        public ushort GroupReference { get; private set; }
        public long SessionGeneration { get; private set; }
        public LMC_Response Acknowledgement
        {
            get { return submissionTracker.Acknowledgement; }
        }
        public int RequiredStableSampleCount { get; private set; }

        public LMCGroupReadStatusResult LastObservedStatus
        {
            get
            {
                lock (stateSync)
                {
                    return lastObservedStatus;
                }
            }
        }

        /// <summary>
        /// Total successfully parsed 0x2045 responses observed while this continuation
        /// is pending. This includes helper-owned polls and direct GroupReadStatusResult calls.
        /// </summary>
        public int PollCount
        {
            get
            {
                lock (stateSync)
                {
                    return pollCount;
                }
            }
        }

        public int StableSampleCount
        {
            get
            {
                lock (stateSync)
                {
                    return stableSampleCount;
                }
            }
        }

        public int DisabledUnlockedSampleCount
        {
            get
            {
                lock (stateSync)
                {
                    return disabledUnlockedSampleCount;
                }
            }
        }

        public int PoweredOffSampleCount
        {
            get
            {
                lock (stateSync)
                {
                    return poweredOffSampleCount;
                }
            }
        }

        public bool IsPending
        {
            get
            {
                lock (stateSync)
                {
                    return isPending != 0;
                }
            }
        }

        internal bool HasLockedStandbyProof
        {
            get
            {
                lock (stateSync)
                {
                    return stableSampleCount >= RequiredStableSampleCount;
                }
            }
        }

        internal bool HasRetryReleaseProof
        {
            get
            {
                lock (stateSync)
                {
                    return disabledUnlockedSampleCount
                            >= RequiredReleaseProofSampleCount
                        || poweredOffSampleCount
                            >= RequiredReleaseProofSampleCount;
                }
            }
        }

        internal void Observe(LMCGroupReadStatusResult status)
        {
            lock (stateSync)
            {
                lastObservedStatus = status;
                pollCount++;

                if (status == null || !status.IsSuccess)
                {
                    ResetProofCountersCore();
                    return;
                }

                if (status.IsPowerOn && status.IsStandby)
                {
                    stableSampleCount++;
                }
                else
                {
                    stableSampleCount = 0;
                }

                if (status.IsPowerOn && status.IsDisabled && !status.IsStandby)
                {
                    disabledUnlockedSampleCount++;
                }
                else
                {
                    disabledUnlockedSampleCount = 0;
                }

                if (!status.IsPowerOn)
                {
                    poweredOffSampleCount++;
                }
                else
                {
                    poweredOffSampleCount = 0;
                }
            }
        }

        internal void ResetProofCounters()
        {
            lock (stateSync)
            {
                ResetProofCountersCore();
            }
        }

        internal void MarkCompleted()
        {
            lock (stateSync)
            {
                isPending = 0;
            }
        }

        internal void MarkTransportInvalidatedAtDeadline()
        {
            lock (stateSync)
            {
                submissionTracker.MarkTransportInvalidatedAtDeadline();
            }
        }

        internal LMCGroupEnableWaitEvidence CaptureEvidence(
            long elapsedMilliseconds = 0)
        {
            lock (stateSync)
            {
                return new LMCGroupEnableWaitEvidence(
                    submissionTracker.SubmissionOutcome,
                    submissionTracker.Acknowledgement,
                    lastObservedStatus,
                    pollCount,
                    stableSampleCount,
                    disabledUnlockedSampleCount,
                    poweredOffSampleCount,
                    isPending != 0,
                    submissionTracker.TransportInvalidatedAtDeadline,
                    elapsedMilliseconds);
            }
        }

        private void ResetProofCountersCore()
        {
            stableSampleCount = 0;
            disabledUnlockedSampleCount = 0;
            poweredOffSampleCount = 0;
        }
    }

    /// <summary>
    /// Immutable dispatch and status evidence for one GroupEnable-and-wait call.
    /// A successful acknowledgement proves acceptance only; locked standby still
    /// requires the configured consecutive successful 0x2045 observations.
    /// </summary>
    public sealed class LMCGroupEnableWaitEvidence
    {
        internal LMCGroupEnableWaitEvidence(
            LMCGroupEnableSubmissionOutcome submissionOutcome,
            LMC_Response acknowledgement,
            LMCGroupReadStatusResult lastObservedStatus,
            int pollCount,
            int stableSampleCount,
            int disabledUnlockedSampleCount,
            int poweredOffSampleCount,
            bool isPending,
            bool transportInvalidatedAtDeadline,
            long elapsedMilliseconds)
        {
            SubmissionOutcome = submissionOutcome;
            Acknowledgement = acknowledgement;
            LastObservedStatus = lastObservedStatus;
            PollCount = pollCount;
            StableSampleCount = stableSampleCount;
            DisabledUnlockedSampleCount = disabledUnlockedSampleCount;
            PoweredOffSampleCount = poweredOffSampleCount;
            IsPending = isPending;
            TransportInvalidatedAtDeadline =
                transportInvalidatedAtDeadline;
            ElapsedMilliseconds = Math.Max(0, elapsedMilliseconds);
        }

        public LMCGroupEnableSubmissionOutcome SubmissionOutcome
        {
            get;
            private set;
        }
        public bool CommandMayHaveBeenSent
        {
            get
            {
                return SubmissionOutcome
                    != LMCGroupEnableSubmissionOutcome.NotAttempted;
            }
        }
        public bool GroupEnableAccepted
        {
            get
            {
                return SubmissionOutcome
                    == LMCGroupEnableSubmissionOutcome.Accepted;
            }
        }
        public LMC_Response Acknowledgement { get; private set; }
        public LMCGroupReadStatusResult LastObservedStatus { get; private set; }
        public int PollCount { get; private set; }
        public int StableSampleCount { get; private set; }
        public int DisabledUnlockedSampleCount { get; private set; }
        public int PoweredOffSampleCount { get; private set; }
        public bool IsPending { get; private set; }
        public bool TransportInvalidatedAtDeadline { get; private set; }
        public long ElapsedMilliseconds { get; private set; }
    }

    public sealed class LMCGroupEnableWaitResult
    {
        internal LMCGroupEnableWaitResult(
            LMCGroupEnableWaitContinuation continuation,
            LMCGroupReadStatusResult finalStatus,
            bool reusedAcceptedAcknowledgement,
            long elapsedMilliseconds = 0)
        {
            var evidence = continuation.CaptureEvidence(
                elapsedMilliseconds);
            Continuation = continuation;
            Evidence = evidence;
            Acknowledgement = continuation.Acknowledgement;
            FinalStatus = finalStatus;
            PollCount = evidence.PollCount;
            StableSampleCount = evidence.StableSampleCount;
            ReusedAcceptedAcknowledgement = reusedAcceptedAcknowledgement;
        }

        public LMC_Response Acknowledgement { get; private set; }
        public LMCGroupEnableWaitEvidence Evidence { get; private set; }
        public LMCGroupEnableSubmissionOutcome SubmissionOutcome
        {
            get { return Evidence.SubmissionOutcome; }
        }
        public bool CommandMayHaveBeenSent
        {
            get { return Evidence.CommandMayHaveBeenSent; }
        }
        public bool GroupEnableAccepted
        {
            get { return Evidence.GroupEnableAccepted; }
        }
        public LMCGroupReadStatusResult FinalStatus { get; private set; }
        public int PollCount { get; private set; }
        public int StableSampleCount { get; private set; }
        public bool ReusedAcceptedAcknowledgement { get; private set; }
        public LMCGroupEnableWaitContinuation Continuation { get; private set; }
    }

    public sealed class LMCGroupEnableRejectedException : InvalidOperationException
    {
        internal LMCGroupEnableRejectedException(
            LMCGroupEnableWaitEvidence evidence)
            : base(
                "GroupEnable was rejected. Status="
                + evidence.Acknowledgement.Status
                + ", ErrorId="
                + evidence.Acknowledgement.ErrorId
                + ".")
        {
            Evidence = evidence
                ?? throw new ArgumentNullException("evidence");
        }

        public LMCGroupEnableWaitEvidence Evidence { get; private set; }
        public LMC_Response Acknowledgement
        {
            get { return Evidence.Acknowledgement; }
        }
        public LMCGroupEnableSubmissionOutcome SubmissionOutcome
        {
            get { return Evidence.SubmissionOutcome; }
        }
    }

    public sealed class LMCGroupEnableSubmissionException
        : InvalidOperationException
    {
        internal LMCGroupEnableSubmissionException(
            LMCGroupEnableWaitEvidence evidence,
            Exception innerException)
            : base(
                "GroupEnable dispatch did not produce a valid acknowledgement. Inspect CommandMayHaveBeenSent before deciding whether to retry.",
                innerException)
        {
            Evidence = evidence
                ?? throw new ArgumentNullException("evidence");
        }

        public LMCGroupEnableWaitEvidence Evidence { get; private set; }
        public LMCGroupEnableSubmissionOutcome SubmissionOutcome
        {
            get { return Evidence.SubmissionOutcome; }
        }
        public bool CommandMayHaveBeenSent
        {
            get { return Evidence.CommandMayHaveBeenSent; }
        }
        public bool TransportInvalidatedAtDeadline
        {
            get { return Evidence.TransportInvalidatedAtDeadline; }
        }
    }

    public sealed class LMCGroupEnablePendingException : InvalidOperationException
    {
        internal LMCGroupEnablePendingException(
            LMCGroupEnableWaitContinuation continuation)
            : base(
                "A previously accepted GroupEnable is still pending verification. "
                + "Resume that continuation or explicitly release it after stable safe-state proof.")
        {
            Continuation = continuation;
        }

        public LMCGroupEnableWaitContinuation Continuation { get; private set; }
    }

    public sealed class LMCGroupEnableWaitResolvedException
        : InvalidOperationException
    {
        internal LMCGroupEnableWaitResolvedException(
            LMCGroupEnableWaitContinuation continuation)
            : this(continuation, 0)
        {
        }

        internal LMCGroupEnableWaitResolvedException(
            LMCGroupEnableWaitContinuation continuation,
            long elapsedMilliseconds)
            : base(
                "The accepted GroupEnable continuation was resolved by another operation before this wait completed.")
        {
            var evidence = continuation.CaptureEvidence(
                elapsedMilliseconds);
            Continuation = continuation;
            Evidence = evidence;
            Acknowledgement = continuation.Acknowledgement;
            LastObservedStatus = evidence.LastObservedStatus;
            PollCount = evidence.PollCount;
        }

        public LMCGroupEnableWaitContinuation Continuation { get; private set; }
        public LMCGroupEnableWaitEvidence Evidence { get; private set; }
        public LMC_Response Acknowledgement { get; private set; }
        public LMCGroupReadStatusResult LastObservedStatus { get; private set; }
        public int PollCount { get; private set; }
    }

    public sealed class LMCGroupEnableWaitTimeoutException : TimeoutException
    {
        internal LMCGroupEnableWaitTimeoutException(
            LMCGroupEnableWaitContinuation continuation)
            : this(continuation.CaptureEvidence(), continuation)
        {
        }

        internal LMCGroupEnableWaitTimeoutException(
            LMCGroupEnableWaitEvidence evidence,
            LMCGroupEnableWaitContinuation continuation)
            : base(
                evidence.TransportInvalidatedAtDeadline
                    ? "The GroupEnable wait exceeded the total deadline after an RPC write; the transport was invalidated and must be reconnected."
                    : evidence.GroupEnableAccepted
                        ? "GroupEnable was accepted, but locked standby was not observed before the total deadline."
                        : "GroupEnable was not sent before the total wait deadline.")
        {
            Evidence = evidence
                ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }

        public LMCGroupEnableWaitEvidence Evidence { get; private set; }
        public LMCGroupEnableWaitContinuation Continuation { get; private set; }
        public LMC_Response Acknowledgement
        {
            get { return Evidence.Acknowledgement; }
        }
        public LMCGroupReadStatusResult LastObservedStatus
        {
            get { return Evidence.LastObservedStatus; }
        }
        public int PollCount { get { return Evidence.PollCount; } }
        public LMCGroupEnableSubmissionOutcome SubmissionOutcome
        {
            get { return Evidence.SubmissionOutcome; }
        }
        public bool CommandMayHaveBeenSent
        {
            get { return Evidence.CommandMayHaveBeenSent; }
        }
        public bool TransportInvalidatedAtDeadline
        {
            get { return Evidence.TransportInvalidatedAtDeadline; }
        }
    }

    public sealed class LMCGroupEnableWaitCanceledException : OperationCanceledException
    {
        internal LMCGroupEnableWaitCanceledException(
            LMCGroupEnableWaitContinuation continuation,
            OperationCanceledException innerException,
            CancellationToken cancellationToken)
            : this(
                continuation.CaptureEvidence(),
                continuation,
                innerException,
                cancellationToken)
        {
        }

        internal LMCGroupEnableWaitCanceledException(
            LMCGroupEnableWaitEvidence evidence,
            LMCGroupEnableWaitContinuation continuation,
            OperationCanceledException innerException,
            CancellationToken cancellationToken)
            : base(
                evidence.GroupEnableAccepted
                    ? "GroupEnable was accepted, but locked-standby verification was canceled."
                    : "GroupEnable was canceled before a command acknowledgement was accepted.",
                innerException,
                cancellationToken)
        {
            Evidence = evidence
                ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }

        public LMCGroupEnableWaitEvidence Evidence { get; private set; }
        public LMCGroupEnableWaitContinuation Continuation { get; private set; }
        public LMC_Response Acknowledgement
        {
            get { return Evidence.Acknowledgement; }
        }
        public LMCGroupReadStatusResult LastObservedStatus
        {
            get { return Evidence.LastObservedStatus; }
        }
        public int PollCount { get { return Evidence.PollCount; } }
        public LMCGroupEnableSubmissionOutcome SubmissionOutcome
        {
            get { return Evidence.SubmissionOutcome; }
        }
        public bool CommandMayHaveBeenSent
        {
            get { return Evidence.CommandMayHaveBeenSent; }
        }
        public bool TransportInvalidatedAtDeadline
        {
            get { return Evidence.TransportInvalidatedAtDeadline; }
        }
    }

    public sealed class LMCGroupEnableStatusException : InvalidOperationException
    {
        internal LMCGroupEnableStatusException(
            LMCGroupEnableWaitContinuation continuation,
            LMCGroupReadStatusResult failedStatus,
            Exception innerException,
            long elapsedMilliseconds = 0)
            : base(
                "GroupEnable locked-standby verification could not read a successful group status.",
                innerException)
        {
            var evidence = continuation.CaptureEvidence(
                elapsedMilliseconds);
            Continuation = continuation;
            Evidence = evidence;
            Acknowledgement = continuation.Acknowledgement;
            FailedStatus = failedStatus;
            LastObservedStatus = evidence.LastObservedStatus;
            PollCount = evidence.PollCount;
        }

        public LMCGroupEnableWaitContinuation Continuation { get; private set; }
        public LMCGroupEnableWaitEvidence Evidence { get; private set; }
        public LMC_Response Acknowledgement { get; private set; }
        public LMCGroupReadStatusResult FailedStatus { get; private set; }
        public LMCGroupReadStatusResult LastObservedStatus { get; private set; }
        public int PollCount { get; private set; }
        public LMCGroupEnableSubmissionOutcome SubmissionOutcome
        {
            get { return Evidence.SubmissionOutcome; }
        }
        public bool TransportInvalidatedAtDeadline
        {
            get { return Evidence.TransportInvalidatedAtDeadline; }
        }
    }
}

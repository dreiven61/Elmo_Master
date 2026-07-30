using System;
using System.Threading;

namespace LasalMotionControlLib
{
    public enum LMCGroupDisableSubmissionOutcome
    {
        NotAttempted = 0,
        Rejected = 1,
        OutcomeUncertain = 2,
        Accepted = 3
    }

    /// <summary>
    /// Terminal ownership state for one accepted GroupDisable continuation.
    /// Stable PowerOff supersession is distinct from GroupDisable completion.
    /// </summary>
    public enum LMCGroupDisableWaitContinuationState
    {
        /// <summary>Status-only verification may still be resumed.</summary>
        Pending = 1,

        /// <summary>Powered-on Disabled proof was published.</summary>
        Completed = 2,

        /// <summary>
        /// A newer accepted GroupPowerOff published stable PowerOff proof.
        /// </summary>
        SupersededByStablePowerOff = 3
    }

    public sealed class LMCGroupDisableWaitOptions
    {
        public const int DefaultTimeoutMilliseconds = 5000;
        public const int DefaultPollIntervalMilliseconds = 50;
        public const int DefaultStableSampleCount = 3;

        public LMCGroupDisableWaitOptions()
        {
            TimeoutMilliseconds = DefaultTimeoutMilliseconds;
            PollIntervalMilliseconds = DefaultPollIntervalMilliseconds;
            StableSampleCount = DefaultStableSampleCount;
        }

        public int TimeoutMilliseconds { get; set; }
        public int PollIntervalMilliseconds { get; set; }
        public int StableSampleCount { get; set; }

        internal LMCGroupDisableWaitOptions SnapshotAndValidate()
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

            return new LMCGroupDisableWaitOptions
            {
                TimeoutMilliseconds = TimeoutMilliseconds,
                PollIntervalMilliseconds = PollIntervalMilliseconds,
                StableSampleCount = StableSampleCount
            };
        }
    }

    internal sealed class LMCGroupDisableWaitTracker
        : LMCGroupStatusWaitTracker
    {
        private readonly int requiredStableSampleCount;
        private LMCGroupDisableSubmissionOutcome submissionOutcome;
        private LMC_Response acknowledgement;
        private LMCGroupReadStatusResult lastObservedStatus;
        private int statusPollCount;
        private int stableSampleCount;
        private long disableMutationGeneration;
        private long observedMutationGeneration;
        private bool transportInvalidatedAtDeadline;

        internal LMCGroupDisableWaitTracker(int requiredStableSampleCount)
        {
            this.requiredStableSampleCount = requiredStableSampleCount;
        }

        internal bool HasStableProof
        {
            get { return stableSampleCount >= requiredStableSampleCount; }
        }

        internal long DisableMutationGeneration
        {
            get { return disableMutationGeneration; }
        }

        internal void SetDisableMutationGeneration(long value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException("value");
            }

            disableMutationGeneration = value;
            observedMutationGeneration = value;
        }

        internal void ObserveMutationGeneration(long value)
        {
            observedMutationGeneration = value;
        }

        internal void MarkSubmissionOutcomeUncertain()
        {
            if (submissionOutcome
                == LMCGroupDisableSubmissionOutcome.NotAttempted)
            {
                submissionOutcome =
                    LMCGroupDisableSubmissionOutcome.OutcomeUncertain;
            }
        }

        internal void SetAcknowledgement(LMC_Response value)
        {
            acknowledgement = value;
            submissionOutcome = value != null
                    && value.IsFrameValid
                    && value.IsSuccess
                ? LMCGroupDisableSubmissionOutcome.Accepted
                : LMCGroupDisableSubmissionOutcome.Rejected;
        }

        public void Observe(LMCGroupReadStatusResult status)
        {
            lastObservedStatus = status;
            statusPollCount++;
            if (IsStableDisabledStatus(status))
            {
                stableSampleCount++;
            }
            else
            {
                stableSampleCount = 0;
            }
        }

        internal static bool IsStableDisabledStatus(
            LMCGroupReadStatusResult status)
        {
            return status != null
                && status.IsSuccess
                && status.IsPowerOn
                && status.IsDisabled
                && !status.IsStandby;
        }

        public void MarkTransportInvalidatedAtDeadline()
        {
            transportInvalidatedAtDeadline = true;
        }

        internal void ResetProofCounters()
        {
            stableSampleCount = 0;
        }

        internal LMCGroupDisableWaitEvidence CaptureEvidence(
            bool isPending,
            long elapsedMilliseconds)
        {
            return new LMCGroupDisableWaitEvidence(
                submissionOutcome,
                acknowledgement,
                lastObservedStatus,
                statusPollCount,
                stableSampleCount,
                requiredStableSampleCount,
                disableMutationGeneration,
                observedMutationGeneration,
                isPending,
                transportInvalidatedAtDeadline,
                elapsedMilliseconds);
        }
    }

    internal sealed class LMCGroupStableDisabledWaitTracker
        : LMCGroupStatusWaitTracker
    {
        private readonly int requiredStableSampleCount;
        private LMCGroupReadStatusResult lastObservedStatus;
        private int statusPollCount;
        private int stableSampleCount;
        private bool completionPublished;
        private bool transportInvalidatedAtDeadline;

        internal LMCGroupStableDisabledWaitTracker(
            int requiredStableSampleCount)
        {
            this.requiredStableSampleCount = requiredStableSampleCount;
        }

        internal bool HasStableProof
        {
            get { return stableSampleCount >= requiredStableSampleCount; }
        }

        internal bool CompletionPublished
        {
            get { return completionPublished; }
        }

        public void Observe(LMCGroupReadStatusResult status)
        {
            lastObservedStatus = status;
            statusPollCount++;
            if (LMCGroupDisableWaitTracker.IsStableDisabledStatus(status))
            {
                stableSampleCount++;
            }
            else
            {
                stableSampleCount = 0;
            }
        }

        internal void MarkCompletionPublished()
        {
            completionPublished = true;
        }

        public void MarkTransportInvalidatedAtDeadline()
        {
            transportInvalidatedAtDeadline = true;
        }

        internal LMCGroupStableDisabledWaitEvidence CaptureEvidence(
            long elapsedMilliseconds)
        {
            return new LMCGroupStableDisabledWaitEvidence(
                lastObservedStatus,
                statusPollCount,
                stableSampleCount,
                requiredStableSampleCount,
                transportInvalidatedAtDeadline,
                elapsedMilliseconds);
        }
    }

    /// <summary>
    /// Session-bound evidence that one 0x2048 GroupDisable was accepted. Resume
    /// sends only 0x2045 status reads and never replays GroupDisable.
    /// </summary>
    public sealed class LMCGroupDisableWaitContinuation
    {
        private readonly object stateSync;
        private readonly LMCConnection ownerConnection;
        private readonly LMCGroupDisableWaitTracker tracker;
        private LMCGroupDisableWaitContinuationState state;
        private LMCGroupPowerStateWaitContinuation
            supersedingPowerOffContinuation;

        internal LMCGroupDisableWaitContinuation(
            LMCGroupEnableWaitCoordinator coordinator,
            LMCConnection ownerConnection,
            string groupName,
            ushort groupReference,
            long sessionGeneration,
            LMCGroupDisableWaitTracker tracker)
        {
            Coordinator = coordinator
                ?? throw new ArgumentNullException("coordinator");
            this.ownerConnection = ownerConnection
                ?? throw new ArgumentNullException("ownerConnection");
            this.tracker = tracker
                ?? throw new ArgumentNullException("tracker");
            stateSync = coordinator.Sync;
            GroupName = groupName ?? throw new ArgumentNullException("groupName");
            GroupReference = groupReference;
            SessionGeneration = sessionGeneration;
            state = LMCGroupDisableWaitContinuationState.Pending;
        }

        internal LMCGroupEnableWaitCoordinator Coordinator { get; private set; }
        internal LMCGroupDisableWaitTracker Tracker { get { return tracker; } }
        public string GroupName { get; private set; }
        public ushort GroupReference { get; private set; }
        public long SessionGeneration { get; private set; }
        /// <summary>
        /// Current pending, completed, or explicitly superseded state.
        /// </summary>
        public LMCGroupDisableWaitContinuationState State
        {
            get { lock (stateSync) { return state; } }
        }
        /// <summary>
        /// Exact same-session stable PowerOff proof used to supersede this
        /// continuation, or null when it remains pending or completed normally.
        /// </summary>
        public LMCGroupPowerStateWaitContinuation
            SupersedingPowerOffContinuation
        {
            get
            {
                lock (stateSync)
                {
                    return supersedingPowerOffContinuation;
                }
            }
        }
        public LMC_Response Acknowledgement
        {
            get { lock (stateSync) { return tracker.CaptureEvidence(state == LMCGroupDisableWaitContinuationState.Pending, 0).Acknowledgement; } }
        }
        public LMCGroupReadStatusResult LastObservedStatus
        {
            get { lock (stateSync) { return tracker.CaptureEvidence(state == LMCGroupDisableWaitContinuationState.Pending, 0).LastObservedStatus; } }
        }
        public int StatusPollCount
        {
            get { lock (stateSync) { return tracker.CaptureEvidence(state == LMCGroupDisableWaitContinuationState.Pending, 0).StatusPollCount; } }
        }
        public int StableDisabledSampleCount
        {
            get { lock (stateSync) { return tracker.CaptureEvidence(state == LMCGroupDisableWaitContinuationState.Pending, 0).StableDisabledSampleCount; } }
        }
        public int RequiredStableSampleCount
        {
            get { lock (stateSync) { return tracker.CaptureEvidence(state == LMCGroupDisableWaitContinuationState.Pending, 0).RequiredStableSampleCount; } }
        }
        public long DisableMutationGeneration
        {
            get { lock (stateSync) { return tracker.CaptureEvidence(state == LMCGroupDisableWaitContinuationState.Pending, 0).DisableMutationGeneration; } }
        }
        public long ObservedMutationGeneration
        {
            get
            {
                lock (stateSync)
                {
                        return tracker.CaptureEvidence(
                            state == LMCGroupDisableWaitContinuationState.Pending,
                            0)
                        .ObservedMutationGeneration;
                }
            }
        }
        public bool InterveningMutationDetected
        {
            get
            {
                lock (stateSync)
                {
                        return tracker.CaptureEvidence(
                            state == LMCGroupDisableWaitContinuationState.Pending,
                            0)
                        .InterveningMutationDetected;
                }
            }
        }
        public bool IsPending
        {
            get
            {
                lock (stateSync)
                {
                    return state
                        == LMCGroupDisableWaitContinuationState.Pending;
                }
            }
        }
        public bool IsCompleted
        {
            get
            {
                lock (stateSync)
                {
                    return state
                        == LMCGroupDisableWaitContinuationState.Completed;
                }
            }
        }
        /// <summary>
        /// True only when a newer stable PowerOff proof explicitly retired the
        /// pending Disable verification without claiming Disable completion.
        /// </summary>
        public bool IsSuperseded
        {
            get
            {
                lock (stateSync)
                {
                    return state == LMCGroupDisableWaitContinuationState
                        .SupersededByStablePowerOff;
                }
            }
        }

        internal bool HasStableDisabledProof
        {
            get { lock (stateSync) { return tracker.HasStableProof; } }
        }

        internal bool BelongsTo(
            LMCConnection connection,
            long expectedSessionGeneration,
            ushort groupReference)
        {
            return ReferenceEquals(ownerConnection, connection)
                && SessionGeneration == expectedSessionGeneration
                && GroupReference == groupReference;
        }

        internal void Observe(LMCGroupReadStatusResult status)
        {
            lock (stateSync) { tracker.Observe(status); }
        }
        internal void ObserveMutationGeneration(long value)
        {
            lock (stateSync) { tracker.ObserveMutationGeneration(value); }
        }
        internal void ResetProofCounters()
        {
            lock (stateSync) { tracker.ResetProofCounters(); }
        }
        internal void MarkCompleted()
        {
            lock (stateSync)
            {
                state = LMCGroupDisableWaitContinuationState.Completed;
            }
        }
        internal void MarkSupersededByStablePowerOff(
            LMCGroupPowerStateWaitContinuation powerOffContinuation)
        {
            if (powerOffContinuation == null)
            {
                throw new ArgumentNullException("powerOffContinuation");
            }

            lock (stateSync)
            {
                if (state != LMCGroupDisableWaitContinuationState.Pending)
                {
                    throw new InvalidOperationException(
                        "Only a pending GroupDisable continuation can be superseded.");
                }
                supersedingPowerOffContinuation = powerOffContinuation;
                state = LMCGroupDisableWaitContinuationState
                    .SupersededByStablePowerOff;
            }
        }
        internal void MarkTransportInvalidatedAtDeadline()
        {
            lock (stateSync) { tracker.MarkTransportInvalidatedAtDeadline(); }
        }
        internal LMCGroupDisableWaitEvidence CaptureEvidence(
            long elapsedMilliseconds)
        {
            lock (stateSync)
            {
                return tracker.CaptureEvidence(
                    state == LMCGroupDisableWaitContinuationState.Pending,
                    elapsedMilliseconds);
            }
        }
    }

    public sealed class LMCGroupDisableWaitEvidence
    {
        internal LMCGroupDisableWaitEvidence(
            LMCGroupDisableSubmissionOutcome submissionOutcome,
            LMC_Response acknowledgement,
            LMCGroupReadStatusResult lastObservedStatus,
            int statusPollCount,
            int stableSampleCount,
            int requiredStableSampleCount,
            long disableMutationGeneration,
            long observedMutationGeneration,
            bool isPending,
            bool transportInvalidatedAtDeadline,
            long elapsedMilliseconds)
        {
            SubmissionOutcome = submissionOutcome;
            Acknowledgement = acknowledgement;
            LastObservedStatus = lastObservedStatus;
            StatusPollCount = statusPollCount;
            StableDisabledSampleCount = stableSampleCount;
            RequiredStableSampleCount = requiredStableSampleCount;
            DisableMutationGeneration = disableMutationGeneration;
            ObservedMutationGeneration = observedMutationGeneration;
            IsPending = isPending;
            TransportInvalidatedAtDeadline = transportInvalidatedAtDeadline;
            ElapsedMilliseconds = Math.Max(0, elapsedMilliseconds);
        }

        public LMCGroupDisableSubmissionOutcome SubmissionOutcome { get; private set; }
        public bool CommandMayHaveBeenSent
        {
            get { return SubmissionOutcome != LMCGroupDisableSubmissionOutcome.NotAttempted; }
        }
        public bool GroupDisableAccepted
        {
            get { return SubmissionOutcome == LMCGroupDisableSubmissionOutcome.Accepted; }
        }
        public LMC_Response Acknowledgement { get; private set; }
        public LMCGroupReadStatusResult LastObservedStatus { get; private set; }
        public int StatusPollCount { get; private set; }
        public int StableDisabledSampleCount { get; private set; }
        public int StableSampleCount { get { return StableDisabledSampleCount; } }
        public int RequiredStableSampleCount { get; private set; }
        public long DisableMutationGeneration { get; private set; }
        public long ObservedMutationGeneration { get; private set; }
        public bool InterveningMutationDetected
        {
            get
            {
                return DisableMutationGeneration > 0
                    && ObservedMutationGeneration > 0
                    && DisableMutationGeneration != ObservedMutationGeneration;
            }
        }
        public bool IsPending { get; private set; }
        public bool TransportInvalidatedAtDeadline { get; private set; }
        public long ElapsedMilliseconds { get; private set; }
    }

    public sealed class LMCGroupDisableWaitResult
    {
        internal LMCGroupDisableWaitResult(
            LMCGroupDisableWaitEvidence evidence,
            LMCGroupDisableWaitContinuation continuation)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }
        public LMCGroupDisableWaitEvidence Evidence { get; private set; }
        public LMCGroupDisableWaitContinuation Continuation { get; private set; }
        public LMCGroupDisableSubmissionOutcome SubmissionOutcome { get { return Evidence.SubmissionOutcome; } }
        public LMC_Response Acknowledgement { get { return Evidence.Acknowledgement; } }
        public bool GroupDisableAccepted { get { return Evidence.GroupDisableAccepted; } }
        public LMCGroupReadStatusResult FinalStatus { get { return Evidence.LastObservedStatus; } }
        public int StatusPollCount { get { return Evidence.StatusPollCount; } }
        public int StableDisabledSampleCount { get { return Evidence.StableDisabledSampleCount; } }
        public int RequiredStableSampleCount { get { return Evidence.RequiredStableSampleCount; } }
        public bool TransportInvalidatedAtDeadline
        {
            get { return Evidence.TransportInvalidatedAtDeadline; }
        }
        public long ElapsedMilliseconds
        {
            get { return Evidence.ElapsedMilliseconds; }
        }
    }

    public sealed class LMCGroupDisableRejectedException : InvalidOperationException
    {
        internal LMCGroupDisableRejectedException(LMCGroupDisableWaitEvidence evidence)
            : base("GroupDisable was rejected; no disabled completion is claimed.")
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }
        public LMCGroupDisableWaitEvidence Evidence { get; private set; }
    }

    public sealed class LMCGroupDisableSubmissionException : InvalidOperationException
    {
        internal LMCGroupDisableSubmissionException(
            LMCGroupDisableWaitEvidence evidence,
            Exception innerException)
            : base(
                "GroupDisable did not produce a valid acknowledgement. Inspect CommandMayHaveBeenSent before retrying.",
                innerException)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }
        public LMCGroupDisableWaitEvidence Evidence { get; private set; }
    }

    public sealed class LMCGroupDisableWaitPendingException : InvalidOperationException
    {
        internal LMCGroupDisableWaitPendingException(
            LMCGroupDisableWaitContinuation continuation)
            : base("An accepted GroupDisable is still pending; resume status-only verification instead of replaying 0x2048.")
        {
            Continuation = continuation
                ?? throw new ArgumentNullException("continuation");
        }
        public LMCGroupDisableWaitContinuation Continuation { get; private set; }
    }

    public sealed class LMCGroupDisableWaitResolvedException : InvalidOperationException
    {
        internal LMCGroupDisableWaitResolvedException(
            LMCGroupDisableWaitContinuation continuation)
            : base("The GroupDisable continuation is already resolved.")
        {
            Continuation = continuation
                ?? throw new ArgumentNullException("continuation");
        }
        public LMCGroupDisableWaitContinuation Continuation { get; private set; }
    }

    public sealed class LMCGroupDisableWaitTimeoutException : TimeoutException
    {
        internal LMCGroupDisableWaitTimeoutException(
            LMCGroupDisableWaitEvidence evidence,
            LMCGroupDisableWaitContinuation continuation)
            : base("GroupDisable stable-disabled verification did not finish before the total deadline.")
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }
        public LMCGroupDisableWaitEvidence Evidence { get; private set; }
        public LMCGroupDisableWaitContinuation Continuation { get; private set; }
        public bool TransportInvalidatedAtDeadline
        {
            get { return Evidence.TransportInvalidatedAtDeadline; }
        }
        public long ElapsedMilliseconds
        {
            get { return Evidence.ElapsedMilliseconds; }
        }
    }

    public sealed class LMCGroupDisableWaitCanceledException : OperationCanceledException
    {
        internal LMCGroupDisableWaitCanceledException(
            LMCGroupDisableWaitEvidence evidence,
            LMCGroupDisableWaitContinuation continuation,
            OperationCanceledException innerException,
            CancellationToken cancellationToken)
            : base(
                "GroupDisable stable-disabled verification was canceled. Resume the accepted continuation instead of replaying 0x2048.",
                innerException,
                cancellationToken)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }
        public LMCGroupDisableWaitEvidence Evidence { get; private set; }
        public LMCGroupDisableWaitContinuation Continuation { get; private set; }
        public bool TransportInvalidatedAtDeadline
        {
            get { return Evidence.TransportInvalidatedAtDeadline; }
        }
        public long ElapsedMilliseconds
        {
            get { return Evidence.ElapsedMilliseconds; }
        }
    }

    public sealed class LMCGroupDisableStatusException : InvalidOperationException
    {
        internal LMCGroupDisableStatusException(
            LMCGroupDisableWaitEvidence evidence,
            LMCGroupDisableWaitContinuation continuation,
            LMCGroupReadStatusResult failedStatus,
            Exception innerException)
            : base("GroupDisable was accepted, but stable-disabled verification could not read a successful group status.", innerException)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
            FailedStatus = failedStatus;
        }
        public LMCGroupDisableWaitEvidence Evidence { get; private set; }
        public LMCGroupDisableWaitContinuation Continuation { get; private set; }
        public LMCGroupReadStatusResult FailedStatus { get; private set; }
        public bool TransportInvalidatedAtDeadline
        {
            get { return Evidence.TransportInvalidatedAtDeadline; }
        }
        public long ElapsedMilliseconds
        {
            get { return Evidence.ElapsedMilliseconds; }
        }
    }

    public sealed class LMCGroupDisableInterferenceException : InvalidOperationException
    {
        internal LMCGroupDisableInterferenceException(
            LMCGroupDisableWaitEvidence evidence,
            LMCGroupDisableWaitContinuation continuation)
            : base("Another mutation or a powered-off state prevents attributing stable Disabled proof to the accepted GroupDisable.")
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }
        public LMCGroupDisableWaitEvidence Evidence { get; private set; }
        public LMCGroupDisableWaitContinuation Continuation { get; private set; }
        public bool TransportInvalidatedAtDeadline
        {
            get { return Evidence.TransportInvalidatedAtDeadline; }
        }
        public long ElapsedMilliseconds
        {
            get { return Evidence.ElapsedMilliseconds; }
        }
    }

    public sealed class LMCGroupStableDisabledWaitEvidence
    {
        internal LMCGroupStableDisabledWaitEvidence(
            LMCGroupReadStatusResult lastObservedStatus,
            int statusPollCount,
            int stableSampleCount,
            int requiredStableSampleCount,
            bool transportInvalidatedAtDeadline,
            long elapsedMilliseconds)
        {
            LastObservedStatus = lastObservedStatus;
            StatusPollCount = statusPollCount;
            StableDisabledSampleCount = stableSampleCount;
            RequiredStableSampleCount = requiredStableSampleCount;
            TransportInvalidatedAtDeadline =
                transportInvalidatedAtDeadline;
            ElapsedMilliseconds = Math.Max(0, elapsedMilliseconds);
        }
        public LMCGroupReadStatusResult LastObservedStatus { get; private set; }
        public int StatusPollCount { get; private set; }
        public int StableDisabledSampleCount { get; private set; }
        public int RequiredStableSampleCount { get; private set; }
        public bool TransportInvalidatedAtDeadline { get; private set; }
        public long ElapsedMilliseconds { get; private set; }
    }

    public sealed class LMCGroupStableDisabledWaitResult
    {
        internal LMCGroupStableDisabledWaitResult(
            LMCGroupStableDisabledWaitEvidence evidence)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }
        public LMCGroupStableDisabledWaitEvidence Evidence { get; private set; }
        public LMCGroupReadStatusResult FinalStatus { get { return Evidence.LastObservedStatus; } }
        public int StatusPollCount { get { return Evidence.StatusPollCount; } }
        public int StableDisabledSampleCount { get { return Evidence.StableDisabledSampleCount; } }
        public bool TransportInvalidatedAtDeadline
        {
            get { return Evidence.TransportInvalidatedAtDeadline; }
        }
        public long ElapsedMilliseconds
        {
            get { return Evidence.ElapsedMilliseconds; }
        }
    }

    public sealed class LMCGroupStableDisabledWaitTimeoutException : TimeoutException
    {
        internal LMCGroupStableDisabledWaitTimeoutException(
            LMCGroupStableDisabledWaitEvidence evidence)
            : base("Stable Disabled status was not observed before the total deadline.")
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }
        public LMCGroupStableDisabledWaitEvidence Evidence { get; private set; }
    }

    public sealed class LMCGroupStableDisabledWaitCanceledException : OperationCanceledException
    {
        internal LMCGroupStableDisabledWaitCanceledException(
            LMCGroupStableDisabledWaitEvidence evidence,
            OperationCanceledException innerException,
            CancellationToken cancellationToken)
            : base("Stable Disabled status verification was canceled.", innerException, cancellationToken)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }
        public LMCGroupStableDisabledWaitEvidence Evidence { get; private set; }
    }

    public sealed class LMCGroupStableDisabledStatusException : InvalidOperationException
    {
        internal LMCGroupStableDisabledStatusException(
            LMCGroupStableDisabledWaitEvidence evidence,
            LMCGroupReadStatusResult failedStatus,
            Exception innerException)
            : base("Stable Disabled verification failed; PowerOff is not GroupDisable completion proof.", innerException)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            FailedStatus = failedStatus;
        }
        public LMCGroupStableDisabledWaitEvidence Evidence { get; private set; }
        public LMCGroupReadStatusResult FailedStatus { get; private set; }
    }
}

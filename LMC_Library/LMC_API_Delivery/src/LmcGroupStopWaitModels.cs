using System;
using System.Threading;

namespace LasalMotionControlLib
{
    public enum LMCGroupStopSubmissionOutcome
    {
        NotAttempted = 0,
        Rejected = 1,
        OutcomeUncertain = 2,
        Accepted = 3
    }

    /// <summary>
    /// Session-bound evidence that one GroupStop acknowledgement was accepted.
    /// Resuming this continuation performs status-only 0x2045 polling and
    /// never sends another 0x2085 request.
    /// </summary>
    public sealed class LMCGroupStopWaitContinuation
    {
        private readonly object stateSync;
        private readonly LMCConnection ownerConnection;
        private readonly LMCGroupStopWaitTracker tracker;
        private int state;

        internal LMCGroupStopWaitContinuation(
            LMCGroupEnableWaitCoordinator coordinator,
            LMCConnection ownerConnection,
            string groupName,
            ushort groupReference,
            long sessionGeneration,
            LMCGroupStopWaitTracker tracker)
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
            state = 1;
        }

        internal LMCGroupEnableWaitCoordinator Coordinator
        {
            get;
            private set;
        }

        internal LMCGroupStopWaitTracker Tracker
        {
            get { return tracker; }
        }

        public string GroupName { get; private set; }
        public ushort GroupReference { get; private set; }
        public long SessionGeneration { get; private set; }

        public LMC_Response Acknowledgement
        {
            get
            {
                lock (stateSync)
                {
                    return tracker.CaptureEvidence(0)
                        .StopAcknowledgement;
                }
            }
        }

        public LMCGroupReadStatusResult LastObservedStatus
        {
            get
            {
                lock (stateSync)
                {
                    return tracker.CaptureEvidence(0)
                        .LastObservedStatus;
                }
            }
        }

        public int StatusPollCount
        {
            get
            {
                lock (stateSync)
                {
                    return tracker.CaptureEvidence(0).StatusPollCount;
                }
            }
        }

        public int StableStandbySampleCount
        {
            get
            {
                lock (stateSync)
                {
                    return tracker.CaptureEvidence(0)
                        .StableStandbySampleCount;
                }
            }
        }

        public int RequiredStableSampleCount
        {
            get
            {
                lock (stateSync)
                {
                    return tracker.CaptureEvidence(0)
                        .RequiredStableSampleCount;
                }
            }
        }

        public long StopMutationGeneration
        {
            get
            {
                lock (stateSync)
                {
                    return tracker.CaptureEvidence(0)
                        .StopMutationGeneration;
                }
            }
        }

        public bool IsPending
        {
            get
            {
                lock (stateSync)
                {
                    return state == 1;
                }
            }
        }

        public bool IsCompleted
        {
            get
            {
                lock (stateSync)
                {
                    return state == 2;
                }
            }
        }

        public bool IsSuperseded
        {
            get
            {
                lock (stateSync)
                {
                    return state == 3;
                }
            }
        }

        internal bool HasStableStandbyProof
        {
            get
            {
                lock (stateSync)
                {
                    return tracker.HasStableProof;
                }
            }
        }

        internal bool BelongsTo(
            LMCConnection connection,
            long sessionGeneration,
            ushort groupReference)
        {
            return ReferenceEquals(ownerConnection, connection)
                && SessionGeneration == sessionGeneration
                && GroupReference == groupReference;
        }

        internal void Observe(LMCGroupReadStatusResult status)
        {
            lock (stateSync)
            {
                tracker.Observe(status);
            }
        }

        internal void ObserveMutationGeneration(long value)
        {
            lock (stateSync)
            {
                tracker.ObserveMutationGeneration(value);
            }
        }

        internal void MarkTransportInvalidatedAtDeadline()
        {
            lock (stateSync)
            {
                tracker.MarkTransportInvalidatedAtDeadline();
            }
        }

        internal LMCGroupStopWaitEvidence CaptureEvidence(
            long elapsedMilliseconds)
        {
            lock (stateSync)
            {
                return tracker.CaptureEvidence(elapsedMilliseconds);
            }
        }

        internal void ResetProofCounters()
        {
            lock (stateSync)
            {
                tracker.ResetProofCounters();
            }
        }

        internal void MarkCompleted()
        {
            lock (stateSync)
            {
                state = 2;
            }
        }

        internal void MarkSuperseded()
        {
            lock (stateSync)
            {
                if (state == 1)
                {
                    state = 3;
                }
            }
        }
    }

    /// <summary>
    /// Controls one GroupStop dispatch and the following status-only wait for
    /// stable LASAL group standby. The timeout is a total deadline that includes
    /// gate admission, the Stop exchange, status exchanges, and poll delays.
    /// </summary>
    public sealed class LMCGroupStopWaitOptions
    {
        public const int DefaultTimeoutMilliseconds = 5000;
        public const int DefaultPollIntervalMilliseconds = 50;
        public const int DefaultStableSampleCount = 3;

        public LMCGroupStopWaitOptions()
        {
            TimeoutMilliseconds = DefaultTimeoutMilliseconds;
            PollIntervalMilliseconds = DefaultPollIntervalMilliseconds;
            StableSampleCount = DefaultStableSampleCount;
        }

        public int TimeoutMilliseconds { get; set; }
        public int PollIntervalMilliseconds { get; set; }
        public int StableSampleCount { get; set; }

        internal LMCGroupStopWaitOptions SnapshotAndValidate()
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

            return new LMCGroupStopWaitOptions
            {
                TimeoutMilliseconds = TimeoutMilliseconds,
                PollIntervalMilliseconds = PollIntervalMilliseconds,
                StableSampleCount = StableSampleCount
            };
        }
    }

    /// <summary>
    /// Immutable evidence for a single Stop-and-wait call. A successful ACK
    /// proves dispatch acceptance only. Stable standby is proved separately by
    /// consecutive successful 0x2045 status responses.
    /// </summary>
    public sealed class LMCGroupStopWaitEvidence
    {
        internal LMCGroupStopWaitEvidence(
            LMCGroupStopSubmissionOutcome submissionOutcome,
            LMC_Response acknowledgement,
            LMCGroupReadStatusResult lastObservedStatus,
            int statusPollCount,
            int stableSampleCount,
            int requiredStableSampleCount,
            long stopMutationGeneration,
            long observedMutationGeneration,
            bool transportInvalidatedAtDeadline,
            long elapsedMilliseconds)
        {
            SubmissionOutcome = submissionOutcome;
            StopAcknowledgement = acknowledgement;
            LastObservedStatus = lastObservedStatus;
            StatusPollCount = statusPollCount;
            StableSampleCount = stableSampleCount;
            RequiredStableSampleCount = requiredStableSampleCount;
            StopMutationGeneration = stopMutationGeneration;
            ObservedMutationGeneration = observedMutationGeneration;
            TransportInvalidatedAtDeadline =
                transportInvalidatedAtDeadline;
            ElapsedMilliseconds = Math.Max(0, elapsedMilliseconds);
        }

        /// <summary>
        /// True after the RPC write commit boundary may have been reached. This
        /// remains conservative when cancellation races that boundary.
        /// </summary>
        public LMCGroupStopSubmissionOutcome SubmissionOutcome
        {
            get;
            private set;
        }
        public bool CommandMayHaveBeenSent
        {
            get
            {
                return SubmissionOutcome
                    != LMCGroupStopSubmissionOutcome.NotAttempted;
            }
        }
        public LMC_Response StopAcknowledgement { get; private set; }
        public bool StopAccepted
        {
            get
            {
                return SubmissionOutcome
                    == LMCGroupStopSubmissionOutcome.Accepted;
            }
        }
        public LMCGroupReadStatusResult LastObservedStatus { get; private set; }
        public int StatusPollCount { get; private set; }
        public int StableSampleCount { get; private set; }
        public int StableStandbySampleCount
        {
            get { return StableSampleCount; }
        }
        public int RequiredStableSampleCount { get; private set; }
        public long StopMutationGeneration { get; private set; }
        public long ObservedMutationGeneration { get; private set; }
        public bool InterveningMutationDetected
        {
            get
            {
                return StopMutationGeneration > 0
                    && ObservedMutationGeneration > 0
                    && StopMutationGeneration != ObservedMutationGeneration;
            }
        }
        public bool TransportInvalidatedAtDeadline { get; private set; }
        public long ElapsedMilliseconds { get; private set; }
    }

    internal interface LMCGroupStatusWaitTracker
    {
        void Observe(LMCGroupReadStatusResult status);
        void MarkTransportInvalidatedAtDeadline();
    }

    internal sealed class LMCGroupStopWaitTracker
        : LMCGroupStatusWaitTracker
    {
        private readonly int requiredStableSampleCount;
        private LMCGroupStopSubmissionOutcome submissionOutcome;
        private LMC_Response acknowledgement;
        private LMCGroupReadStatusResult lastObservedStatus;
        private int statusPollCount;
        private int stableSampleCount;
        private long stopMutationGeneration;
        private long observedMutationGeneration;
        private bool transportInvalidatedAtDeadline;

        internal LMCGroupStopWaitTracker(int requiredStableSampleCount)
        {
            this.requiredStableSampleCount = requiredStableSampleCount;
        }

        internal bool HasStableProof
        {
            get { return stableSampleCount >= requiredStableSampleCount; }
        }

        internal long StopMutationGeneration
        {
            get { return stopMutationGeneration; }
        }

        internal void SetStopMutationGeneration(long value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException("value");
            }

            stopMutationGeneration = value;
            observedMutationGeneration = value;
        }

        internal void ObserveMutationGeneration(long value)
        {
            observedMutationGeneration = value;
        }

        internal void MarkSubmissionOutcomeUncertain()
        {
            if (submissionOutcome
                == LMCGroupStopSubmissionOutcome.NotAttempted)
            {
                submissionOutcome =
                    LMCGroupStopSubmissionOutcome.OutcomeUncertain;
            }
        }

        internal void SetAcknowledgement(LMC_Response value)
        {
            acknowledgement = value;
            submissionOutcome = value != null
                    && value.IsFrameValid
                    && value.IsSuccess
                ? LMCGroupStopSubmissionOutcome.Accepted
                : LMCGroupStopSubmissionOutcome.Rejected;
        }

        public void Observe(LMCGroupReadStatusResult status)
        {
            lastObservedStatus = status;
            statusPollCount++;

            if (status != null && status.IsSuccess && status.IsStandby)
            {
                stableSampleCount++;
            }
            else
            {
                stableSampleCount = 0;
            }
        }

        public void MarkTransportInvalidatedAtDeadline()
        {
            transportInvalidatedAtDeadline = true;
        }

        internal void ResetProofCounters()
        {
            stableSampleCount = 0;
        }

        internal LMCGroupStopWaitEvidence CaptureEvidence(
            long elapsedMilliseconds)
        {
            return new LMCGroupStopWaitEvidence(
                submissionOutcome,
                acknowledgement,
                lastObservedStatus,
                statusPollCount,
                stableSampleCount,
                requiredStableSampleCount,
                stopMutationGeneration,
                observedMutationGeneration,
                transportInvalidatedAtDeadline,
                elapsedMilliseconds);
        }
    }

    public sealed class LMCGroupStopWaitResult
    {
        internal LMCGroupStopWaitResult(LMCGroupStopWaitEvidence evidence)
            : this(evidence, null)
        {
        }

        internal LMCGroupStopWaitResult(
            LMCGroupStopWaitEvidence evidence,
            LMCGroupStopWaitContinuation continuation)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }

        public LMCGroupStopWaitEvidence Evidence { get; private set; }
        public LMCGroupStopWaitContinuation Continuation
        {
            get;
            private set;
        }
        public LMCGroupStopSubmissionOutcome SubmissionOutcome
        {
            get { return Evidence.SubmissionOutcome; }
        }
        public LMC_Response Acknowledgement
        {
            get { return Evidence.StopAcknowledgement; }
        }
        public bool StopAccepted { get { return Evidence.StopAccepted; } }
        public LMCGroupReadStatusResult FinalStatus
        {
            get { return Evidence.LastObservedStatus; }
        }
        public int StatusPollCount { get { return Evidence.StatusPollCount; } }
        public int StableSampleCount { get { return Evidence.StableSampleCount; } }
        public int StableStandbySampleCount
        {
            get { return Evidence.StableStandbySampleCount; }
        }
        public int RequiredStableSampleCount
        {
            get { return Evidence.RequiredStableSampleCount; }
        }
        public long ElapsedMilliseconds
        {
            get { return Evidence.ElapsedMilliseconds; }
        }
        public bool TransportInvalidatedAtDeadline
        {
            get { return Evidence.TransportInvalidatedAtDeadline; }
        }
    }

    public sealed class LMCGroupStopRejectedException
        : InvalidOperationException
    {
        internal LMCGroupStopRejectedException(
            LMCGroupStopWaitEvidence evidence)
            : base("GroupStop was rejected; no standby completion is claimed.")
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }

        public LMCGroupStopWaitEvidence Evidence { get; private set; }
        public LMC_Response Acknowledgement
        {
            get { return Evidence.StopAcknowledgement; }
        }
    }

    public sealed class LMCGroupStopSubmissionException
        : InvalidOperationException
    {
        internal LMCGroupStopSubmissionException(
            LMCGroupStopWaitEvidence evidence,
            Exception innerException)
            : base(
                "GroupStop dispatch did not produce a valid acknowledgement. Inspect CommandMayHaveBeenSent before deciding whether to retry.",
                innerException)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }

        public LMCGroupStopWaitEvidence Evidence { get; private set; }
    }

    public sealed class LMCGroupStopWaitTimeoutException
        : TimeoutException
    {
        internal LMCGroupStopWaitTimeoutException(
            LMCGroupStopWaitEvidence evidence)
            : this(evidence, null)
        {
        }

        internal LMCGroupStopWaitTimeoutException(
            LMCGroupStopWaitEvidence evidence,
            LMCGroupStopWaitContinuation continuation)
            : base(
                evidence != null
                        && evidence.TransportInvalidatedAtDeadline
                    ? "GroupStop and stable-standby verification exceeded the total deadline after an RPC write; the transport was invalidated and must be reconnected."
                    : "GroupStop and stable-standby verification did not finish before the total deadline.")
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }

        public LMCGroupStopWaitEvidence Evidence { get; private set; }
        public LMCGroupStopWaitContinuation Continuation
        {
            get;
            private set;
        }
        public bool TransportInvalidatedAtDeadline
        {
            get { return Evidence.TransportInvalidatedAtDeadline; }
        }
    }

    public sealed class LMCGroupStopWaitCanceledException
        : OperationCanceledException
    {
        internal LMCGroupStopWaitCanceledException(
            LMCGroupStopWaitEvidence evidence,
            OperationCanceledException innerException,
            CancellationToken cancellationToken)
            : this(
                evidence,
                null,
                innerException,
                cancellationToken)
        {
        }

        internal LMCGroupStopWaitCanceledException(
            LMCGroupStopWaitEvidence evidence,
            LMCGroupStopWaitContinuation continuation,
            OperationCanceledException innerException,
            CancellationToken cancellationToken)
            : base(
                "GroupStop and stable-standby verification was canceled. Inspect the evidence before retrying.",
                innerException,
                cancellationToken)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }

        public LMCGroupStopWaitEvidence Evidence { get; private set; }
        public LMCGroupStopWaitContinuation Continuation
        {
            get;
            private set;
        }
    }

    public sealed class LMCGroupStopStatusException
        : InvalidOperationException
    {
        internal LMCGroupStopStatusException(
            LMCGroupStopWaitEvidence evidence,
            LMCGroupReadStatusResult failedStatus,
            Exception innerException)
            : this(evidence, null, failedStatus, innerException)
        {
        }

        internal LMCGroupStopStatusException(
            LMCGroupStopWaitEvidence evidence,
            LMCGroupStopWaitContinuation continuation,
            LMCGroupReadStatusResult failedStatus,
            Exception innerException)
            : base(
                "GroupStop was accepted, but stable-standby verification could not read a successful group status.",
                innerException)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
            FailedStatus = failedStatus;
        }

        public LMCGroupStopWaitEvidence Evidence { get; private set; }
        public LMCGroupStopWaitContinuation Continuation
        {
            get;
            private set;
        }
        public LMCGroupReadStatusResult FailedStatus { get; private set; }
    }

    public sealed class LMCGroupStopInterferenceException
        : InvalidOperationException
    {
        internal LMCGroupStopInterferenceException(
            LMCGroupStopWaitEvidence evidence)
            : this(evidence, null)
        {
        }

        internal LMCGroupStopInterferenceException(
            LMCGroupStopWaitEvidence evidence,
            LMCGroupStopWaitContinuation continuation)
            : base(
                "Another group mutation may have been sent after GroupStop; stable-standby proof was not attributed to the original Stop.")
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }

        public LMCGroupStopWaitEvidence Evidence { get; private set; }
        public LMCGroupStopWaitContinuation Continuation
        {
            get;
            private set;
        }
    }
}

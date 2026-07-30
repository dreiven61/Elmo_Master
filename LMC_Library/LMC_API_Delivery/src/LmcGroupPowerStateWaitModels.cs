using System;
using System.Threading;

namespace LasalMotionControlLib
{
    public enum LMCGroupPowerSubmissionOutcome
    {
        NotAttempted = 0,
        Rejected = 1,
        OutcomeUncertain = 2,
        Accepted = 3
    }

    /// <summary>
    /// Controls the total deadline and stable-sample requirement for a read-only
    /// group power-state wait. The deadline includes status-gate admission, wire
    /// exchange, and delays between polls.
    /// </summary>
    public sealed class LMCGroupPowerStateWaitOptions
    {
        public const int DefaultTimeoutMilliseconds = 5000;
        public const int DefaultPollIntervalMilliseconds = 50;
        public const int DefaultStableSampleCount = 3;

        public LMCGroupPowerStateWaitOptions()
        {
            TimeoutMilliseconds = DefaultTimeoutMilliseconds;
            PollIntervalMilliseconds = DefaultPollIntervalMilliseconds;
            StableSampleCount = DefaultStableSampleCount;
        }

        public int TimeoutMilliseconds { get; set; }
        public int PollIntervalMilliseconds { get; set; }
        public int StableSampleCount { get; set; }

        internal LMCGroupPowerStateWaitOptions SnapshotAndValidate()
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

            return new LMCGroupPowerStateWaitOptions
            {
                TimeoutMilliseconds = TimeoutMilliseconds,
                PollIntervalMilliseconds = PollIntervalMilliseconds,
                StableSampleCount = StableSampleCount
            };
        }
    }

    /// <summary>
    /// Immutable evidence captured when a power-state wait completes or stops.
    /// StatusPollCount counts successfully parsed 0x2045 responses, including
    /// parsed command-error responses.
    /// </summary>
    public sealed class LMCGroupPowerStateWaitEvidence
    {
        internal LMCGroupPowerStateWaitEvidence(
            bool expectedPowerOn,
            LMCGroupReadStatusResult lastObservedStatus,
            int statusPollCount,
            int stableSampleCount,
            int requiredStableSampleCount,
            bool transportInvalidatedAtDeadline,
            long elapsedMilliseconds)
            : this(
                expectedPowerOn,
                LMCGroupPowerSubmissionOutcome.NotAttempted,
                null,
                lastObservedStatus,
                statusPollCount,
                stableSampleCount,
                requiredStableSampleCount,
                0,
                0,
                transportInvalidatedAtDeadline,
                elapsedMilliseconds)
        {
        }

        internal LMCGroupPowerStateWaitEvidence(
            bool expectedPowerOn,
            LMCGroupPowerSubmissionOutcome submissionOutcome,
            LMC_Response acknowledgement,
            LMCGroupReadStatusResult lastObservedStatus,
            int statusPollCount,
            int stableSampleCount,
            int requiredStableSampleCount,
            long powerMutationGeneration,
            long observedMutationGeneration,
            bool transportInvalidatedAtDeadline,
            long elapsedMilliseconds)
        {
            ExpectedPowerOn = expectedPowerOn;
            SubmissionOutcome = submissionOutcome;
            PowerAcknowledgement = acknowledgement;
            LastObservedStatus = lastObservedStatus;
            StatusPollCount = statusPollCount;
            StableSampleCount = stableSampleCount;
            RequiredStableSampleCount = requiredStableSampleCount;
            PowerMutationGeneration = powerMutationGeneration;
            ObservedMutationGeneration = observedMutationGeneration;
            TransportInvalidatedAtDeadline =
                transportInvalidatedAtDeadline;
            ElapsedMilliseconds = Math.Max(0, elapsedMilliseconds);
        }

        public bool ExpectedPowerOn { get; private set; }
        public LMCGroupPowerSubmissionOutcome SubmissionOutcome
        {
            get;
            private set;
        }
        public bool CommandMayHaveBeenSent
        {
            get
            {
                return SubmissionOutcome
                    != LMCGroupPowerSubmissionOutcome.NotAttempted;
            }
        }
        public LMC_Response PowerAcknowledgement { get; private set; }
        public bool PowerCommandAccepted
        {
            get
            {
                return SubmissionOutcome
                    == LMCGroupPowerSubmissionOutcome.Accepted;
            }
        }
        public LMCGroupReadStatusResult LastObservedStatus { get; private set; }
        public int StatusPollCount { get; private set; }
        public int StableSampleCount { get; private set; }
        public int RequiredStableSampleCount { get; private set; }
        public long PowerMutationGeneration { get; private set; }
        public long ObservedMutationGeneration { get; private set; }
        public bool InterveningMutationDetected
        {
            get
            {
                return PowerMutationGeneration > 0
                    && ObservedMutationGeneration > 0
                    && PowerMutationGeneration
                        != ObservedMutationGeneration;
            }
        }
        public bool TransportInvalidatedAtDeadline { get; private set; }
        public long ElapsedMilliseconds { get; private set; }
    }

    internal sealed class LMCGroupPowerStateWaitTracker
        : LMCGroupStatusWaitTracker
    {
        private readonly bool expectedPowerOn;
        private readonly int requiredStableSampleCount;
        private LMCGroupPowerSubmissionOutcome submissionOutcome;
        private LMC_Response acknowledgement;
        private LMCGroupReadStatusResult lastObservedStatus;
        private int statusPollCount;
        private int stableSampleCount;
        private long powerMutationGeneration;
        private long observedMutationGeneration;
        private bool transportInvalidatedAtDeadline;

        internal LMCGroupPowerStateWaitTracker(
            bool expectedPowerOn,
            int requiredStableSampleCount)
        {
            this.expectedPowerOn = expectedPowerOn;
            this.requiredStableSampleCount = requiredStableSampleCount;
        }

        internal bool HasStableProof
        {
            get { return stableSampleCount >= requiredStableSampleCount; }
        }

        internal bool ExpectedPowerOn
        {
            get { return expectedPowerOn; }
        }

        internal long PowerMutationGeneration
        {
            get { return powerMutationGeneration; }
        }

        internal void SetPowerMutationGeneration(long value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException("value");
            }

            powerMutationGeneration = value;
            observedMutationGeneration = value;
        }

        internal void ObserveMutationGeneration(long value)
        {
            observedMutationGeneration = value;
        }

        internal void MarkSubmissionOutcomeUncertain()
        {
            if (submissionOutcome
                == LMCGroupPowerSubmissionOutcome.NotAttempted)
            {
                submissionOutcome =
                    LMCGroupPowerSubmissionOutcome.OutcomeUncertain;
            }
        }

        internal void SetAcknowledgement(LMC_Response value)
        {
            acknowledgement = value;
            submissionOutcome = value != null
                    && value.IsFrameValid
                    && value.IsSuccess
                ? LMCGroupPowerSubmissionOutcome.Accepted
                : LMCGroupPowerSubmissionOutcome.Rejected;
        }

        public void Observe(LMCGroupReadStatusResult status)
        {
            lastObservedStatus = status;
            statusPollCount++;

            if (status != null
                && status.IsSuccess
                && status.IsPowerOn == expectedPowerOn)
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

        internal LMCGroupPowerStateWaitEvidence CaptureEvidence(
            long elapsedMilliseconds)
        {
            return new LMCGroupPowerStateWaitEvidence(
                expectedPowerOn,
                submissionOutcome,
                acknowledgement,
                lastObservedStatus,
                statusPollCount,
                stableSampleCount,
                requiredStableSampleCount,
                powerMutationGeneration,
                observedMutationGeneration,
                transportInvalidatedAtDeadline,
                elapsedMilliseconds);
        }
    }

    /// <summary>
    /// Session-bound evidence for one accepted Group Power On or Power Off.
    /// Resume operations use only 0x2045 status reads and never replay the
    /// accepted 0x204A or 0x204B command.
    /// </summary>
    public sealed class LMCGroupPowerStateWaitContinuation
    {
        private readonly object stateSync;
        private readonly LMCConnection ownerConnection;
        private readonly LMCGroupPowerStateWaitTracker tracker;
        private int state;

        internal LMCGroupPowerStateWaitContinuation(
            LMCGroupEnableWaitCoordinator coordinator,
            LMCConnection ownerConnection,
            string groupName,
            ushort groupReference,
            long sessionGeneration,
            LMCGroupPowerStateWaitTracker tracker)
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

        internal LMCGroupPowerStateWaitTracker Tracker
        {
            get { return tracker; }
        }

        public string GroupName { get; private set; }
        public ushort GroupReference { get; private set; }
        public long SessionGeneration { get; private set; }
        public bool ExpectedPowerOn { get { return tracker.ExpectedPowerOn; } }
        public LMC_Response Acknowledgement
        {
            get { return CaptureEvidence(0).PowerAcknowledgement; }
        }
        public int StatusPollCount
        {
            get { return CaptureEvidence(0).StatusPollCount; }
        }
        public int StableSampleCount
        {
            get { return CaptureEvidence(0).StableSampleCount; }
        }
        public int RequiredStableSampleCount
        {
            get { return CaptureEvidence(0).RequiredStableSampleCount; }
        }
        public long PowerMutationGeneration
        {
            get { return CaptureEvidence(0).PowerMutationGeneration; }
        }
        public long ObservedMutationGeneration
        {
            get { return CaptureEvidence(0).ObservedMutationGeneration; }
        }
        public bool InterveningMutationDetected
        {
            get { return CaptureEvidence(0).InterveningMutationDetected; }
        }
        public bool IsPending { get { lock (stateSync) { return state == 1; } } }
        public bool IsCompleted { get { lock (stateSync) { return state == 2; } } }
        public bool IsSuperseded { get { lock (stateSync) { return state == 3; } } }

        internal bool HasStableProof
        {
            get { lock (stateSync) { return tracker.HasStableProof; } }
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
            lock (stateSync) { tracker.Observe(status); }
        }

        internal void ObserveMutationGeneration(long value)
        {
            lock (stateSync) { tracker.ObserveMutationGeneration(value); }
        }

        internal void MarkTransportInvalidatedAtDeadline()
        {
            lock (stateSync) { tracker.MarkTransportInvalidatedAtDeadline(); }
        }

        internal void ResetProofCounters()
        {
            lock (stateSync) { tracker.ResetProofCounters(); }
        }

        internal LMCGroupPowerStateWaitEvidence CaptureEvidence(
            long elapsedMilliseconds)
        {
            lock (stateSync)
            {
                return tracker.CaptureEvidence(elapsedMilliseconds);
            }
        }

        internal void MarkCompleted()
        {
            lock (stateSync) { state = 2; }
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

    public sealed class LMCGroupPowerStateWaitResult
    {
        internal LMCGroupPowerStateWaitResult(
            LMCGroupPowerStateWaitEvidence evidence)
            : this(evidence, null)
        {
        }

        internal LMCGroupPowerStateWaitResult(
            LMCGroupPowerStateWaitEvidence evidence,
            LMCGroupPowerStateWaitContinuation continuation)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }

        public LMCGroupPowerStateWaitEvidence Evidence { get; private set; }
        public LMCGroupPowerStateWaitContinuation Continuation
        {
            get;
            private set;
        }
        public LMCGroupPowerSubmissionOutcome SubmissionOutcome
        {
            get { return Evidence.SubmissionOutcome; }
        }
        public LMC_Response Acknowledgement
        {
            get { return Evidence.PowerAcknowledgement; }
        }
        public bool PowerCommandAccepted
        {
            get { return Evidence.PowerCommandAccepted; }
        }
        public bool ExpectedPowerOn { get { return Evidence.ExpectedPowerOn; } }
        public LMCGroupReadStatusResult FinalStatus
        {
            get { return Evidence.LastObservedStatus; }
        }
        public int StatusPollCount { get { return Evidence.StatusPollCount; } }
        public int StableSampleCount { get { return Evidence.StableSampleCount; } }
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

    public sealed class LMCGroupPowerStateWaitTimeoutException
        : TimeoutException
    {
        internal LMCGroupPowerStateWaitTimeoutException(
            LMCGroupPowerStateWaitEvidence evidence)
            : this(evidence, null)
        {
        }

        internal LMCGroupPowerStateWaitTimeoutException(
            LMCGroupPowerStateWaitEvidence evidence,
            LMCGroupPowerStateWaitContinuation continuation)
            : base(
                evidence != null
                        && evidence.TransportInvalidatedAtDeadline
                    ? "The group power-state wait exceeded the total deadline after a status write; the transport was invalidated and must be reconnected."
                    : "The expected group power state was not stable before the total wait deadline.")
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }

        public LMCGroupPowerStateWaitEvidence Evidence { get; private set; }
        public LMCGroupPowerStateWaitContinuation Continuation
        {
            get;
            private set;
        }
        public bool ExpectedPowerOn { get { return Evidence.ExpectedPowerOn; } }
        public LMCGroupReadStatusResult LastObservedStatus
        {
            get { return Evidence.LastObservedStatus; }
        }
        public int StatusPollCount { get { return Evidence.StatusPollCount; } }
        public int StableSampleCount { get { return Evidence.StableSampleCount; } }
        public bool TransportInvalidatedAtDeadline
        {
            get { return Evidence.TransportInvalidatedAtDeadline; }
        }
        public long ElapsedMilliseconds
        {
            get { return Evidence.ElapsedMilliseconds; }
        }
    }

    public sealed class LMCGroupPowerStateWaitCanceledException
        : OperationCanceledException
    {
        internal LMCGroupPowerStateWaitCanceledException(
            LMCGroupPowerStateWaitEvidence evidence,
            OperationCanceledException innerException,
            CancellationToken cancellationToken)
            : this(
                evidence,
                null,
                innerException,
                cancellationToken)
        {
        }

        internal LMCGroupPowerStateWaitCanceledException(
            LMCGroupPowerStateWaitEvidence evidence,
            LMCGroupPowerStateWaitContinuation continuation,
            OperationCanceledException innerException,
            CancellationToken cancellationToken)
            : base(
                continuation == null
                    ? "The read-only group power-state wait was canceled."
                    : "The accepted group power-state verification was canceled; resume the exact continuation without replaying the command.",
                innerException,
                cancellationToken)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }

        public LMCGroupPowerStateWaitEvidence Evidence { get; private set; }
        public LMCGroupPowerStateWaitContinuation Continuation
        {
            get;
            private set;
        }
        public bool ExpectedPowerOn { get { return Evidence.ExpectedPowerOn; } }
        public LMCGroupReadStatusResult LastObservedStatus
        {
            get { return Evidence.LastObservedStatus; }
        }
        public int StatusPollCount { get { return Evidence.StatusPollCount; } }
        public int StableSampleCount { get { return Evidence.StableSampleCount; } }
        public long ElapsedMilliseconds
        {
            get { return Evidence.ElapsedMilliseconds; }
        }
    }

    public sealed class LMCGroupPowerStateStatusException
        : InvalidOperationException
    {
        internal LMCGroupPowerStateStatusException(
            LMCGroupPowerStateWaitEvidence evidence,
            LMCGroupReadStatusResult failedStatus,
            Exception innerException)
            : this(evidence, null, failedStatus, innerException)
        {
        }

        internal LMCGroupPowerStateStatusException(
            LMCGroupPowerStateWaitEvidence evidence,
            LMCGroupPowerStateWaitContinuation continuation,
            LMCGroupReadStatusResult failedStatus,
            Exception innerException)
            : base(
                continuation == null
                    ? "The read-only group power-state wait could not read a successful group status."
                    : "The group power command was accepted, but verification could not read a successful group status.",
                innerException)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
            FailedStatus = failedStatus;
        }

        public LMCGroupPowerStateWaitEvidence Evidence { get; private set; }
        public LMCGroupPowerStateWaitContinuation Continuation
        {
            get;
            private set;
        }
        public LMCGroupReadStatusResult FailedStatus { get; private set; }
        public LMCGroupReadStatusResult LastObservedStatus
        {
            get { return Evidence.LastObservedStatus; }
        }
        public int StatusPollCount { get { return Evidence.StatusPollCount; } }
        public int StableSampleCount { get { return Evidence.StableSampleCount; } }
        public long ElapsedMilliseconds
        {
            get { return Evidence.ElapsedMilliseconds; }
        }
    }

    public sealed class LMCGroupPowerRejectedException
        : InvalidOperationException
    {
        internal LMCGroupPowerRejectedException(
            LMCGroupPowerStateWaitEvidence evidence)
            : base(
                "The Group Power command was rejected; no stable power-state completion is claimed.")
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }

        public LMCGroupPowerStateWaitEvidence Evidence { get; private set; }
        public LMC_Response Acknowledgement
        {
            get { return Evidence.PowerAcknowledgement; }
        }
    }

    public sealed class LMCGroupPowerSubmissionException
        : InvalidOperationException
    {
        internal LMCGroupPowerSubmissionException(
            LMCGroupPowerStateWaitEvidence evidence,
            Exception innerException)
            : base(
                "The Group Power dispatch did not produce a valid acknowledgement. Inspect CommandMayHaveBeenSent before deciding whether to retry.",
                innerException)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }

        public LMCGroupPowerStateWaitEvidence Evidence { get; private set; }
    }

    public sealed class LMCGroupPowerStateWaitPendingException
        : InvalidOperationException
    {
        internal LMCGroupPowerStateWaitPendingException(
            LMCGroupPowerStateWaitContinuation continuation)
            : base(
                "An accepted Group Power transition is still pending verification; do not replay the command.")
        {
            Continuation = continuation
                ?? throw new ArgumentNullException("continuation");
        }

        public LMCGroupPowerStateWaitContinuation Continuation
        {
            get;
            private set;
        }
    }

    public sealed class LMCGroupPowerStateWaitResolvedException
        : InvalidOperationException
    {
        internal LMCGroupPowerStateWaitResolvedException(
            LMCGroupPowerStateWaitContinuation continuation)
            : base(
                "The Group Power continuation is no longer the pending operation for this group and session.")
        {
            Continuation = continuation;
        }

        public LMCGroupPowerStateWaitContinuation Continuation
        {
            get;
            private set;
        }
    }

    public sealed class LMCGroupPowerInterferenceException
        : InvalidOperationException
    {
        internal LMCGroupPowerInterferenceException(
            LMCGroupPowerStateWaitEvidence evidence,
            LMCGroupPowerStateWaitContinuation continuation)
            : base(
                "Another group mutation may have been sent after the accepted Group Power command; stable-state proof was not attributed to the original command.")
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }

        public LMCGroupPowerStateWaitEvidence Evidence { get; private set; }
        public LMCGroupPowerStateWaitContinuation Continuation
        {
            get;
            private set;
        }
    }
}

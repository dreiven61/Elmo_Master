using System;
using System.Threading;

namespace LasalMotionControlLib
{
    public enum LMCAxisStopSubmissionOutcome
    {
        NotAttempted = 0,
        Rejected = 1,
        OutcomeUncertain = 2,
        Accepted = 3
    }

    public enum LMCAxisStopWaitContinuationState
    {
        Pending = 1,
        Completed = 2,
        SupersededByNewerStop = 3,
        SupersededByStablePowerOff = 4
    }

    /// <summary>
    /// Session-bound evidence that one Axis Stop acknowledgement was accepted.
    /// Resuming this continuation performs status-only 0x2028 polling and
    /// never sends another 0x2022 request.
    /// </summary>
    public sealed class LMCAxisStopWaitContinuation
    {
        private readonly object stateSync;
        private readonly LMCConnection ownerConnection;
        private readonly LMCAxisStopWaitTracker tracker;
        private LMCAxisStopWaitContinuationState state;
        private LMCAxisPowerOffWaitContinuation
            supersedingPowerOffContinuation;

        internal LMCAxisStopWaitContinuation(
            LMCAxisPowerOnWaitCoordinator coordinator,
            LMCConnection ownerConnection,
            string axisName,
            ushort axisReference,
            long sessionGeneration,
            LMCAxisStopWaitTracker tracker)
        {
            Coordinator = coordinator
                ?? throw new ArgumentNullException("coordinator");
            this.ownerConnection = ownerConnection
                ?? throw new ArgumentNullException("ownerConnection");
            this.tracker = tracker
                ?? throw new ArgumentNullException("tracker");
            stateSync = coordinator.Sync;
            AxisName = axisName ?? throw new ArgumentNullException("axisName");
            AxisReference = axisReference;
            SessionGeneration = sessionGeneration;
            state = LMCAxisStopWaitContinuationState.Pending;
        }

        internal LMCAxisPowerOnWaitCoordinator Coordinator
        {
            get;
            private set;
        }

        public string AxisName { get; private set; }
        public ushort AxisReference { get; private set; }
        public long SessionGeneration { get; private set; }

        public LMCAxisStopWaitContinuationState State
        {
            get
            {
                lock (stateSync)
                {
                    return state;
                }
            }
        }

        public LMCAxisPowerOffWaitContinuation
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

        public int Deceleration
        {
            get
            {
                lock (stateSync)
                {
                    return tracker.CaptureEvidence(0).Deceleration;
                }
            }
        }

        public int Jerk
        {
            get
            {
                lock (stateSync)
                {
                    return tracker.CaptureEvidence(0).Jerk;
                }
            }
        }

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

        public LMCReadStatusResult LastObservedStatus
        {
            get
            {
                lock (stateSync)
                {
                    return tracker.CaptureEvidence(0).LastObservedStatus;
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

        public int StableStandstillSampleCount
        {
            get
            {
                lock (stateSync)
                {
                    return tracker.CaptureEvidence(0)
                        .StableStandstillSampleCount;
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
                    return state
                        == LMCAxisStopWaitContinuationState.Pending;
                }
            }
        }

        public bool IsSuperseded
        {
            get
            {
                lock (stateSync)
                {
                    return state
                        == LMCAxisStopWaitContinuationState
                            .SupersededByNewerStop
                        || state
                        == LMCAxisStopWaitContinuationState
                            .SupersededByStablePowerOff;
                }
            }
        }

        internal bool HasStableStandstillProof
        {
            get
            {
                lock (stateSync)
                {
                    return tracker.HasStableStandstillProof;
                }
            }
        }

        internal bool BelongsTo(
            LMCConnection connection,
            long sessionGeneration,
            ushort axisReference)
        {
            return ReferenceEquals(ownerConnection, connection)
                && SessionGeneration == sessionGeneration
                && AxisReference == axisReference;
        }

        internal void Observe(LMCReadStatusResult status)
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

        internal LMCAxisStopWaitEvidence CaptureEvidence(
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
                state = LMCAxisStopWaitContinuationState.Completed;
            }
        }

        internal void MarkSuperseded()
        {
            lock (stateSync)
            {
                if (state == LMCAxisStopWaitContinuationState.Pending)
                {
                    state = LMCAxisStopWaitContinuationState
                        .SupersededByNewerStop;
                }
            }
        }

        internal void MarkSupersededByStablePowerOff(
            LMCAxisPowerOffWaitContinuation powerOffContinuation)
        {
            if (powerOffContinuation == null)
            {
                throw new ArgumentNullException("powerOffContinuation");
            }

            lock (stateSync)
            {
                if (state != LMCAxisStopWaitContinuationState.Pending)
                {
                    throw new InvalidOperationException(
                        "Only a pending Axis Stop can be retired by stable Power Off proof.");
                }

                supersedingPowerOffContinuation = powerOffContinuation;
                state = LMCAxisStopWaitContinuationState
                    .SupersededByStablePowerOff;
            }
        }
    }

    /// <summary>
    /// Controls one Axis Stop dispatch and its status-only stable-standstill
    /// verification. TimeoutMilliseconds is a total deadline that includes
    /// gate admission, Stop exchange, status exchanges, and poll delays.
    /// </summary>
    public sealed class LMCAxisStopWaitOptions
    {
        public const int DefaultTimeoutMilliseconds = 5000;
        public const int DefaultPollIntervalMilliseconds = 50;
        public const int DefaultStableSampleCount = 3;

        public LMCAxisStopWaitOptions()
        {
            TimeoutMilliseconds = DefaultTimeoutMilliseconds;
            PollIntervalMilliseconds = DefaultPollIntervalMilliseconds;
            StableSampleCount = DefaultStableSampleCount;
        }

        public int TimeoutMilliseconds { get; set; }
        public int PollIntervalMilliseconds { get; set; }
        public int StableSampleCount { get; set; }

        internal LMCAxisStopWaitOptions SnapshotAndValidate()
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

            return new LMCAxisStopWaitOptions
            {
                TimeoutMilliseconds = TimeoutMilliseconds,
                PollIntervalMilliseconds = PollIntervalMilliseconds,
                StableSampleCount = StableSampleCount
            };
        }
    }

    /// <summary>
    /// Immutable evidence for one Stop-and-wait call. A successful Stop ACK
    /// proves acceptance only. Completion requires consecutive successful
    /// 0x2028 reads that report LASAL standstill with no native axis error.
    /// </summary>
    public sealed class LMCAxisStopWaitEvidence
    {
        internal LMCAxisStopWaitEvidence(
            int deceleration,
            int jerk,
            LMCAxisStopSubmissionOutcome submissionOutcome,
            LMC_Response acknowledgement,
            LMCReadStatusResult lastObservedStatus,
            int statusPollCount,
            int stableStandstillSampleCount,
            int requiredStableSampleCount,
            long stopMutationGeneration,
            long observedMutationGeneration,
            bool transportInvalidatedAtDeadline,
            long elapsedMilliseconds)
        {
            Deceleration = deceleration;
            Jerk = jerk;
            SubmissionOutcome = submissionOutcome;
            StopAcknowledgement = acknowledgement;
            LastObservedStatus = lastObservedStatus;
            StatusPollCount = statusPollCount;
            StableStandstillSampleCount = stableStandstillSampleCount;
            RequiredStableSampleCount = requiredStableSampleCount;
            StopMutationGeneration = stopMutationGeneration;
            ObservedMutationGeneration = observedMutationGeneration;
            TransportInvalidatedAtDeadline =
                transportInvalidatedAtDeadline;
            ElapsedMilliseconds = Math.Max(0, elapsedMilliseconds);
        }

        public int Deceleration { get; private set; }
        public int Jerk { get; private set; }

        public LMCAxisStopSubmissionOutcome SubmissionOutcome
        {
            get;
            private set;
        }

        public bool CommandMayHaveBeenSent
        {
            get
            {
                return SubmissionOutcome
                    != LMCAxisStopSubmissionOutcome.NotAttempted;
            }
        }

        public LMC_Response StopAcknowledgement { get; private set; }

        public bool StopAccepted
        {
            get
            {
                return SubmissionOutcome
                    == LMCAxisStopSubmissionOutcome.Accepted;
            }
        }

        public LMCReadStatusResult LastObservedStatus { get; private set; }
        public int StatusPollCount { get; private set; }
        public int StableStandstillSampleCount { get; private set; }
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

    internal sealed class LMCAxisStopWaitTracker
    {
        private readonly int deceleration;
        private readonly int jerk;
        private readonly int requiredStableSampleCount;
        private LMCAxisStopSubmissionOutcome submissionOutcome;
        private LMC_Response acknowledgement;
        private LMCReadStatusResult lastObservedStatus;
        private int statusPollCount;
        private int stableStandstillSampleCount;
        private long stopMutationGeneration;
        private long observedMutationGeneration;
        private bool transportInvalidatedAtDeadline;

        internal LMCAxisStopWaitTracker(
            int deceleration,
            int jerk,
            int requiredStableSampleCount)
        {
            this.deceleration = deceleration;
            this.jerk = jerk;
            this.requiredStableSampleCount = requiredStableSampleCount;
        }

        internal bool HasStableStandstillProof
        {
            get
            {
                return stableStandstillSampleCount
                    >= requiredStableSampleCount;
            }
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
                == LMCAxisStopSubmissionOutcome.NotAttempted)
            {
                submissionOutcome =
                    LMCAxisStopSubmissionOutcome.OutcomeUncertain;
            }
        }

        internal void SetAcknowledgement(LMC_Response value)
        {
            acknowledgement = value;
            submissionOutcome = value != null
                    && value.IsFrameValid
                    && value.IsSuccess
                ? LMCAxisStopSubmissionOutcome.Accepted
                : LMCAxisStopSubmissionOutcome.Rejected;
        }

        internal void Observe(LMCReadStatusResult status)
        {
            lastObservedStatus = status;
            statusPollCount++;

            if (status != null
                && status.IsSuccess
                && status.IsStandstill)
            {
                stableStandstillSampleCount++;
            }
            else
            {
                stableStandstillSampleCount = 0;
            }
        }

        internal void MarkTransportInvalidatedAtDeadline()
        {
            transportInvalidatedAtDeadline = true;
        }

        internal void ResetProofCounters()
        {
            stableStandstillSampleCount = 0;
        }

        internal LMCAxisStopWaitEvidence CaptureEvidence(
            long elapsedMilliseconds)
        {
            return new LMCAxisStopWaitEvidence(
                deceleration,
                jerk,
                submissionOutcome,
                acknowledgement,
                lastObservedStatus,
                statusPollCount,
                stableStandstillSampleCount,
                requiredStableSampleCount,
                stopMutationGeneration,
                observedMutationGeneration,
                transportInvalidatedAtDeadline,
                elapsedMilliseconds);
        }
    }

    public sealed class LMCAxisStopWaitResult
    {
        internal LMCAxisStopWaitResult(LMCAxisStopWaitEvidence evidence)
            : this(evidence, null)
        {
        }

        internal LMCAxisStopWaitResult(
            LMCAxisStopWaitEvidence evidence,
            LMCAxisStopWaitContinuation continuation)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }

        public LMCAxisStopWaitEvidence Evidence { get; private set; }
        public LMCAxisStopWaitContinuation Continuation
        {
            get;
            private set;
        }
        public LMCAxisStopSubmissionOutcome SubmissionOutcome
        {
            get { return Evidence.SubmissionOutcome; }
        }
        public LMC_Response Acknowledgement
        {
            get { return Evidence.StopAcknowledgement; }
        }
        public bool StopAccepted { get { return Evidence.StopAccepted; } }
        public LMCReadStatusResult FinalStatus
        {
            get { return Evidence.LastObservedStatus; }
        }
        public int StatusPollCount { get { return Evidence.StatusPollCount; } }
        public int StableStandstillSampleCount
        {
            get { return Evidence.StableStandstillSampleCount; }
        }
        public int RequiredStableSampleCount
        {
            get { return Evidence.RequiredStableSampleCount; }
        }
        public bool TransportInvalidatedAtDeadline
        {
            get { return Evidence.TransportInvalidatedAtDeadline; }
        }
        public long ElapsedMilliseconds
        {
            get { return Evidence.ElapsedMilliseconds; }
        }
    }

    public sealed class LMCAxisStopRejectedException
        : InvalidOperationException
    {
        internal LMCAxisStopRejectedException(
            LMCAxisStopWaitEvidence evidence)
            : base(
                "Axis Stop was rejected; no stable-standstill completion is claimed.")
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }

        public LMCAxisStopWaitEvidence Evidence { get; private set; }
        public LMC_Response Acknowledgement
        {
            get { return Evidence.StopAcknowledgement; }
        }
    }

    public sealed class LMCAxisStopSubmissionException
        : InvalidOperationException
    {
        internal LMCAxisStopSubmissionException(
            LMCAxisStopWaitEvidence evidence,
            Exception innerException)
            : base(
                "Axis Stop dispatch did not produce a valid acknowledgement. Inspect CommandMayHaveBeenSent before deciding whether to retry.",
                innerException)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }

        public LMCAxisStopWaitEvidence Evidence { get; private set; }
    }

    public sealed class LMCAxisStopWaitTimeoutException
        : TimeoutException
    {
        internal LMCAxisStopWaitTimeoutException(
            LMCAxisStopWaitEvidence evidence)
            : this(evidence, null)
        {
        }

        internal LMCAxisStopWaitTimeoutException(
            LMCAxisStopWaitEvidence evidence,
            LMCAxisStopWaitContinuation continuation)
            : base(
                evidence != null
                        && evidence.TransportInvalidatedAtDeadline
                    ? "Axis Stop and stable-standstill verification exceeded the total deadline after an RPC write; the transport was invalidated and must be reconnected."
                    : "Axis Stop and stable-standstill verification did not finish before the total deadline.")
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }

        public LMCAxisStopWaitEvidence Evidence { get; private set; }
        public LMCAxisStopWaitContinuation Continuation
        {
            get;
            private set;
        }
        public LMC_Response Acknowledgement
        {
            get { return Evidence.StopAcknowledgement; }
        }
        public LMCReadStatusResult LastObservedStatus
        {
            get { return Evidence.LastObservedStatus; }
        }
        public int StatusPollCount { get { return Evidence.StatusPollCount; } }
        public bool TransportInvalidatedAtDeadline
        {
            get { return Evidence.TransportInvalidatedAtDeadline; }
        }
    }

    public sealed class LMCAxisStopWaitCanceledException
        : OperationCanceledException
    {
        internal LMCAxisStopWaitCanceledException(
            LMCAxisStopWaitEvidence evidence,
            OperationCanceledException innerException,
            CancellationToken cancellationToken)
            : this(
                evidence,
                null,
                innerException,
                cancellationToken)
        {
        }

        internal LMCAxisStopWaitCanceledException(
            LMCAxisStopWaitEvidence evidence,
            LMCAxisStopWaitContinuation continuation,
            OperationCanceledException innerException,
            CancellationToken cancellationToken)
            : base(
                "Axis Stop and stable-standstill verification was canceled. Inspect the evidence before retrying.",
                innerException,
                cancellationToken)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }

        public LMCAxisStopWaitEvidence Evidence { get; private set; }
        public LMCAxisStopWaitContinuation Continuation
        {
            get;
            private set;
        }
        public LMC_Response Acknowledgement
        {
            get { return Evidence.StopAcknowledgement; }
        }
        public LMCReadStatusResult LastObservedStatus
        {
            get { return Evidence.LastObservedStatus; }
        }
        public int StatusPollCount { get { return Evidence.StatusPollCount; } }
    }

    public sealed class LMCAxisStopStatusException
        : InvalidOperationException
    {
        internal LMCAxisStopStatusException(
            LMCAxisStopWaitEvidence evidence,
            LMCReadStatusResult failedStatus,
            Exception innerException)
            : this(evidence, null, failedStatus, innerException)
        {
        }

        internal LMCAxisStopStatusException(
            LMCAxisStopWaitEvidence evidence,
            LMCAxisStopWaitContinuation continuation,
            LMCReadStatusResult failedStatus,
            Exception innerException)
            : base(
                "Axis Stop was accepted, but stable-standstill verification could not obtain a successful axis status.",
                innerException)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
            FailedStatus = failedStatus;
        }

        public LMCAxisStopWaitEvidence Evidence { get; private set; }
        public LMCAxisStopWaitContinuation Continuation
        {
            get;
            private set;
        }
        public LMCReadStatusResult FailedStatus { get; private set; }
    }

    public sealed class LMCAxisStopInterferenceException
        : InvalidOperationException
    {
        internal LMCAxisStopInterferenceException(
            LMCAxisStopWaitEvidence evidence)
            : this(evidence, null)
        {
        }

        internal LMCAxisStopInterferenceException(
            LMCAxisStopWaitEvidence evidence,
            LMCAxisStopWaitContinuation continuation)
            : base(
                "Another mutation on this axis may have been sent after Axis Stop; stable-standstill proof was not attributed to the original Stop.")
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }

        public LMCAxisStopWaitEvidence Evidence { get; private set; }
        public LMCAxisStopWaitContinuation Continuation
        {
            get;
            private set;
        }
        public long ExpectedMutationGeneration
        {
            get { return Evidence.StopMutationGeneration; }
        }
        public long ObservedMutationGeneration
        {
            get { return Evidence.ObservedMutationGeneration; }
        }
    }

    public sealed class LMCAxisStopWaitPendingException
        : InvalidOperationException
    {
        internal LMCAxisStopWaitPendingException(
            LMCAxisStopWaitContinuation continuation)
            : base(
                "A pending accepted Axis Stop must be resolved before a legacy raw Stop or Reset is sent.")
        {
            Continuation = continuation
                ?? throw new ArgumentNullException("continuation");
        }

        public LMCAxisStopWaitContinuation Continuation
        {
            get;
            private set;
        }
    }

    internal sealed class LMCAxisStableStandstillWaitTracker
    {
        private readonly int requiredStableSampleCount;
        private LMCReadStatusResult lastObservedStatus;
        private int statusPollCount;
        private int stableStandstillSampleCount;
        private bool completionPublished;
        private bool transportInvalidatedAtDeadline;
        private long baselineMutationGeneration;
        private long observedMutationGeneration;

        internal LMCAxisStableStandstillWaitTracker(
            int requiredStableSampleCount)
        {
            this.requiredStableSampleCount = requiredStableSampleCount;
        }

        internal bool HasStableProof
        {
            get
            {
                return stableStandstillSampleCount
                    >= requiredStableSampleCount;
            }
        }

        internal bool CompletionPublished
        {
            get { return completionPublished; }
        }

        internal long BaselineMutationGeneration
        {
            get { return baselineMutationGeneration; }
        }

        internal void SetBaselineMutationGeneration(long value)
        {
            baselineMutationGeneration = value;
            observedMutationGeneration = value;
        }

        internal void ObserveMutationGeneration(long value)
        {
            observedMutationGeneration = value;
        }

        internal void Observe(LMCReadStatusResult status)
        {
            lastObservedStatus = status;
            statusPollCount++;
            if (status != null
                && status.IsSuccess
                && status.IsStandstill)
            {
                stableStandstillSampleCount++;
            }
            else
            {
                stableStandstillSampleCount = 0;
            }
        }

        internal void MarkCompletionPublished()
        {
            completionPublished = true;
        }

        internal void MarkTransportInvalidatedAtDeadline()
        {
            transportInvalidatedAtDeadline = true;
        }

        internal LMCAxisStableStandstillWaitEvidence CaptureEvidence(
            long elapsedMilliseconds)
        {
            return new LMCAxisStableStandstillWaitEvidence(
                lastObservedStatus,
                statusPollCount,
                stableStandstillSampleCount,
                requiredStableSampleCount,
                baselineMutationGeneration,
                observedMutationGeneration,
                transportInvalidatedAtDeadline,
                elapsedMilliseconds);
        }
    }

    /// <summary>
    /// Status-only evidence. It does not attribute standstill to any Stop
    /// request or acknowledgement. Baseline/observed mutation generations
    /// only invalidate proof for LMCSingleAxis writes in this process,
    /// connection session, and AxisReference. PLC logic, another process,
    /// direct SDO, and group operations are outside that detection boundary.
    /// </summary>
    public sealed class LMCAxisStableStandstillWaitEvidence
    {
        internal LMCAxisStableStandstillWaitEvidence(
            LMCReadStatusResult lastObservedStatus,
            int statusPollCount,
            int stableStandstillSampleCount,
            int requiredStableSampleCount,
            long baselineMutationGeneration,
            long observedMutationGeneration,
            bool transportInvalidatedAtDeadline,
            long elapsedMilliseconds)
        {
            LastObservedStatus = lastObservedStatus;
            StatusPollCount = statusPollCount;
            StableStandstillSampleCount = stableStandstillSampleCount;
            RequiredStableSampleCount = requiredStableSampleCount;
            BaselineMutationGeneration = baselineMutationGeneration;
            ObservedMutationGeneration = observedMutationGeneration;
            TransportInvalidatedAtDeadline =
                transportInvalidatedAtDeadline;
            ElapsedMilliseconds = Math.Max(0, elapsedMilliseconds);
        }

        public LMCReadStatusResult LastObservedStatus { get; private set; }
        public int StatusPollCount { get; private set; }
        public int StableStandstillSampleCount { get; private set; }
        public int RequiredStableSampleCount { get; private set; }
        public long BaselineMutationGeneration { get; private set; }
        public long ObservedMutationGeneration { get; private set; }
        public bool InterveningProcessLocalMutationDetected
        {
            get
            {
                return BaselineMutationGeneration
                    != ObservedMutationGeneration;
            }
        }
        public bool TransportInvalidatedAtDeadline { get; private set; }
        public long ElapsedMilliseconds { get; private set; }
    }

    public sealed class LMCAxisStableStandstillWaitResult
    {
        internal LMCAxisStableStandstillWaitResult(
            LMCAxisStableStandstillWaitEvidence evidence)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }

        public LMCAxisStableStandstillWaitEvidence Evidence
        {
            get;
            private set;
        }
        public LMCReadStatusResult FinalStatus
        {
            get { return Evidence.LastObservedStatus; }
        }
        public int StatusPollCount { get { return Evidence.StatusPollCount; } }
        public int StableStandstillSampleCount
        {
            get { return Evidence.StableStandstillSampleCount; }
        }
        public int RequiredStableSampleCount
        {
            get { return Evidence.RequiredStableSampleCount; }
        }
        public long BaselineMutationGeneration
        {
            get { return Evidence.BaselineMutationGeneration; }
        }
        public long ObservedMutationGeneration
        {
            get { return Evidence.ObservedMutationGeneration; }
        }
        public bool InterveningProcessLocalMutationDetected
        {
            get
            {
                return Evidence.InterveningProcessLocalMutationDetected;
            }
        }
        public bool TransportInvalidatedAtDeadline
        {
            get { return Evidence.TransportInvalidatedAtDeadline; }
        }
        public long ElapsedMilliseconds
        {
            get { return Evidence.ElapsedMilliseconds; }
        }
    }

    public sealed class LMCAxisStableStandstillWaitTimeoutException
        : TimeoutException
    {
        internal LMCAxisStableStandstillWaitTimeoutException(
            LMCAxisStableStandstillWaitEvidence evidence)
            : base(
                "Stable standstill status was not observed before the total deadline.")
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }

        public LMCAxisStableStandstillWaitEvidence Evidence
        {
            get;
            private set;
        }
    }

    public sealed class LMCAxisStableStandstillWaitCanceledException
        : OperationCanceledException
    {
        internal LMCAxisStableStandstillWaitCanceledException(
            LMCAxisStableStandstillWaitEvidence evidence,
            OperationCanceledException innerException,
            CancellationToken cancellationToken)
            : base(
                "Stable standstill status verification was canceled.",
                innerException,
                cancellationToken)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }

        public LMCAxisStableStandstillWaitEvidence Evidence
        {
            get;
            private set;
        }
    }

    public sealed class LMCAxisStableStandstillStatusException
        : InvalidOperationException
    {
        internal LMCAxisStableStandstillStatusException(
            LMCAxisStableStandstillWaitEvidence evidence,
            LMCReadStatusResult failedStatus,
            Exception innerException)
            : base(
                "Stable standstill status verification could not obtain a successful axis status.",
                innerException)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            FailedStatus = failedStatus;
        }

        public LMCAxisStableStandstillWaitEvidence Evidence
        {
            get;
            private set;
        }
        public LMCReadStatusResult FailedStatus { get; private set; }
    }

    public sealed class LMCAxisStableStandstillInterferenceException
        : InvalidOperationException
    {
        internal LMCAxisStableStandstillInterferenceException(
            LMCAxisStableStandstillWaitEvidence evidence)
            : base(
                "A process-local same-axis mutation occurred during stable standstill verification; the status proof is inconclusive. Mutations from PLC logic, another process, direct SDO, or group operations are outside this detection boundary.")
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }

        public LMCAxisStableStandstillWaitEvidence Evidence
        {
            get;
            private set;
        }
        public long BaselineMutationGeneration
        {
            get { return Evidence.BaselineMutationGeneration; }
        }
        public long ObservedMutationGeneration
        {
            get { return Evidence.ObservedMutationGeneration; }
        }
    }
}

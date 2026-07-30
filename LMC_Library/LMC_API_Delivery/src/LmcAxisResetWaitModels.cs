using System;
using System.Threading;

namespace LasalMotionControlLib
{
    public enum LMCAxisResetSubmissionOutcome
    {
        NotAttempted = 0,
        Rejected = 1,
        OutcomeUncertain = 2,
        Accepted = 3
    }

    public enum LMCAxisResetWaitContinuationState
    {
        Pending = 1,
        Completed = 2,
        SupersededByNewerReset = 3,
        SupersededBySafetyStop = 4
    }

    /// <summary>
    /// Session-bound evidence that one Axis Reset acknowledgement was
    /// accepted. Resuming this continuation performs status-only 0x2028
    /// polling and never sends another 0x2024 request. Mutation attribution
    /// covers only LMCSingleAxis writes in this process, connection session,
    /// and AxisReference. PLC logic, another RPC client, direct SDO writes,
    /// and group operations are outside that attribution boundary.
    /// </summary>
    public sealed class LMCAxisResetWaitContinuation
    {
        private readonly object stateSync;
        private readonly LMCConnection ownerConnection;
        private readonly LMCAxisResetWaitTracker tracker;
        private LMCAxisResetWaitContinuationState state;
        private LMCAxisStopWaitContinuation
            supersedingSafetyStopContinuation;

        internal LMCAxisResetWaitContinuation(
            LMCAxisPowerOnWaitCoordinator coordinator,
            LMCConnection ownerConnection,
            string axisName,
            ushort axisReference,
            long sessionGeneration,
            LMCAxisResetWaitTracker tracker)
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
            state = LMCAxisResetWaitContinuationState.Pending;
        }

        internal LMCAxisPowerOnWaitCoordinator Coordinator
        {
            get;
            private set;
        }

        public string AxisName { get; private set; }
        public ushort AxisReference { get; private set; }
        public long SessionGeneration { get; private set; }

        public LMCAxisResetWaitContinuationState State
        {
            get
            {
                lock (stateSync)
                {
                    return state;
                }
            }
        }

        public LMCAxisStopWaitContinuation
            SupersedingSafetyStopContinuation
        {
            get
            {
                lock (stateSync)
                {
                    return supersedingSafetyStopContinuation;
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
                        .ResetAcknowledgement;
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

        public int StableErrorClearSampleCount
        {
            get
            {
                lock (stateSync)
                {
                    return tracker.CaptureEvidence(0)
                        .StableErrorClearSampleCount;
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

        public long ResetMutationGeneration
        {
            get
            {
                lock (stateSync)
                {
                    return tracker.CaptureEvidence(0)
                        .ResetMutationGeneration;
                }
            }
        }

        public long ObservedMutationGeneration
        {
            get
            {
                lock (stateSync)
                {
                    return tracker.CaptureEvidence(0)
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
                    return tracker.CaptureEvidence(0)
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
                        == LMCAxisResetWaitContinuationState.Pending;
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
                        == LMCAxisResetWaitContinuationState
                            .SupersededByNewerReset
                        || state
                        == LMCAxisResetWaitContinuationState
                            .SupersededBySafetyStop;
                }
            }
        }

        internal bool IsCompleted
        {
            get
            {
                lock (stateSync)
                {
                    return state
                        == LMCAxisResetWaitContinuationState.Completed;
                }
            }
        }

        internal bool HasStableErrorClearProof
        {
            get
            {
                lock (stateSync)
                {
                    return tracker.HasStableErrorClearProof;
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

        internal LMCAxisResetWaitEvidence CaptureEvidence(
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
                state = LMCAxisResetWaitContinuationState.Completed;
            }
        }

        internal void MarkSuperseded()
        {
            lock (stateSync)
            {
                if (state == LMCAxisResetWaitContinuationState.Pending)
                {
                    state = LMCAxisResetWaitContinuationState
                        .SupersededByNewerReset;
                }
            }
        }

        internal void MarkSupersededBySafetyStop(
            LMCAxisStopWaitContinuation stopContinuation)
        {
            if (stopContinuation == null)
            {
                throw new ArgumentNullException("stopContinuation");
            }

            lock (stateSync)
            {
                if (state != LMCAxisResetWaitContinuationState.Pending)
                {
                    throw new InvalidOperationException(
                        "Only a pending Axis Reset can be superseded by an accepted safety Stop.");
                }

                supersedingSafetyStopContinuation = stopContinuation;
                state = LMCAxisResetWaitContinuationState
                    .SupersededBySafetyStop;
            }
        }
    }

    /// <summary>
    /// Controls one Axis Reset dispatch and the following status-only wait for
    /// stable error clearance. The timeout is a total deadline that includes
    /// gate admission, Reset exchange, status exchanges, and poll delays.
    /// </summary>
    public sealed class LMCAxisResetWaitOptions
    {
        public const int DefaultTimeoutMilliseconds = 5000;
        public const int DefaultPollIntervalMilliseconds = 50;
        public const int DefaultStableSampleCount = 3;

        public LMCAxisResetWaitOptions()
        {
            TimeoutMilliseconds = DefaultTimeoutMilliseconds;
            PollIntervalMilliseconds = DefaultPollIntervalMilliseconds;
            StableSampleCount = DefaultStableSampleCount;
        }

        public int TimeoutMilliseconds { get; set; }
        public int PollIntervalMilliseconds { get; set; }
        public int StableSampleCount { get; set; }

        internal LMCAxisResetWaitOptions SnapshotAndValidate()
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

            return new LMCAxisResetWaitOptions
            {
                TimeoutMilliseconds = TimeoutMilliseconds,
                PollIntervalMilliseconds = PollIntervalMilliseconds,
                StableSampleCount = StableSampleCount
            };
        }
    }

    /// <summary>
    /// Immutable evidence for one Reset-and-wait call. A successful Reset ACK
    /// proves acceptance only. Completion requires consecutive successful
    /// 0x2028 reads that report no native axis error.
    /// </summary>
    public sealed class LMCAxisResetWaitEvidence
    {
        internal LMCAxisResetWaitEvidence(
            LMCAxisResetSubmissionOutcome submissionOutcome,
            LMC_Response acknowledgement,
            LMCReadStatusResult lastObservedStatus,
            int statusPollCount,
            int stableErrorClearSampleCount,
            int requiredStableSampleCount,
            long resetMutationGeneration,
            long observedMutationGeneration,
            bool transportInvalidatedAtDeadline,
            long elapsedMilliseconds)
        {
            SubmissionOutcome = submissionOutcome;
            ResetAcknowledgement = acknowledgement;
            LastObservedStatus = lastObservedStatus;
            StatusPollCount = statusPollCount;
            StableErrorClearSampleCount = stableErrorClearSampleCount;
            RequiredStableSampleCount = requiredStableSampleCount;
            ResetMutationGeneration = resetMutationGeneration;
            ObservedMutationGeneration = observedMutationGeneration;
            TransportInvalidatedAtDeadline =
                transportInvalidatedAtDeadline;
            ElapsedMilliseconds = Math.Max(0, elapsedMilliseconds);
        }

        public LMCAxisResetSubmissionOutcome SubmissionOutcome
        {
            get;
            private set;
        }

        public bool CommandMayHaveBeenSent
        {
            get
            {
                return SubmissionOutcome
                    != LMCAxisResetSubmissionOutcome.NotAttempted;
            }
        }

        public LMC_Response ResetAcknowledgement { get; private set; }

        public bool ResetAccepted
        {
            get
            {
                return SubmissionOutcome
                    == LMCAxisResetSubmissionOutcome.Accepted;
            }
        }

        public LMCReadStatusResult LastObservedStatus { get; private set; }
        public int StatusPollCount { get; private set; }
        public int StableErrorClearSampleCount { get; private set; }
        public int RequiredStableSampleCount { get; private set; }
        public long ResetMutationGeneration { get; private set; }
        public long ObservedMutationGeneration { get; private set; }
        public bool InterveningMutationDetected
        {
            get
            {
                return ResetMutationGeneration > 0
                    && ObservedMutationGeneration > 0
                    && ResetMutationGeneration != ObservedMutationGeneration;
            }
        }
        public bool TransportInvalidatedAtDeadline { get; private set; }
        public long ElapsedMilliseconds { get; private set; }
    }

    internal sealed class LMCAxisResetWaitTracker
    {
        private readonly int requiredStableSampleCount;
        private LMCAxisResetSubmissionOutcome submissionOutcome;
        private LMC_Response acknowledgement;
        private LMCReadStatusResult lastObservedStatus;
        private int statusPollCount;
        private int stableErrorClearSampleCount;
        private long resetMutationGeneration;
        private long observedMutationGeneration;
        private bool transportInvalidatedAtDeadline;

        internal LMCAxisResetWaitTracker(int requiredStableSampleCount)
        {
            this.requiredStableSampleCount = requiredStableSampleCount;
        }

        internal bool HasStableErrorClearProof
        {
            get
            {
                return stableErrorClearSampleCount
                    >= requiredStableSampleCount;
            }
        }

        internal long ResetMutationGeneration
        {
            get { return resetMutationGeneration; }
        }

        internal void SetResetMutationGeneration(long value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException("value");
            }

            resetMutationGeneration = value;
            observedMutationGeneration = value;
        }

        internal void ObserveMutationGeneration(long value)
        {
            observedMutationGeneration = value;
        }

        internal void MarkSubmissionOutcomeUncertain()
        {
            if (submissionOutcome
                == LMCAxisResetSubmissionOutcome.NotAttempted)
            {
                submissionOutcome =
                    LMCAxisResetSubmissionOutcome.OutcomeUncertain;
            }
        }

        internal void SetAcknowledgement(LMC_Response value)
        {
            acknowledgement = value;
            submissionOutcome = value != null
                    && value.IsFrameValid
                    && value.IsSuccess
                ? LMCAxisResetSubmissionOutcome.Accepted
                : LMCAxisResetSubmissionOutcome.Rejected;
        }

        internal void Observe(LMCReadStatusResult status)
        {
            lastObservedStatus = status;
            statusPollCount++;

            if (status != null
                && status.IsReadSuccessful
                && !status.HasAxisError)
            {
                stableErrorClearSampleCount++;
            }
            else
            {
                stableErrorClearSampleCount = 0;
            }
        }

        internal void MarkTransportInvalidatedAtDeadline()
        {
            transportInvalidatedAtDeadline = true;
        }

        internal void ResetProofCounters()
        {
            stableErrorClearSampleCount = 0;
        }

        internal LMCAxisResetWaitEvidence CaptureEvidence(
            long elapsedMilliseconds)
        {
            return new LMCAxisResetWaitEvidence(
                submissionOutcome,
                acknowledgement,
                lastObservedStatus,
                statusPollCount,
                stableErrorClearSampleCount,
                requiredStableSampleCount,
                resetMutationGeneration,
                observedMutationGeneration,
                transportInvalidatedAtDeadline,
                elapsedMilliseconds);
        }
    }

    public sealed class LMCAxisResetWaitResult
    {
        internal LMCAxisResetWaitResult(LMCAxisResetWaitEvidence evidence)
            : this(evidence, null)
        {
        }

        internal LMCAxisResetWaitResult(
            LMCAxisResetWaitEvidence evidence,
            LMCAxisResetWaitContinuation continuation)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }

        public LMCAxisResetWaitEvidence Evidence { get; private set; }
        public LMCAxisResetWaitContinuation Continuation
        {
            get;
            private set;
        }
        public LMCAxisResetSubmissionOutcome SubmissionOutcome
        {
            get { return Evidence.SubmissionOutcome; }
        }
        public LMC_Response Acknowledgement
        {
            get { return Evidence.ResetAcknowledgement; }
        }
        public bool ResetAccepted { get { return Evidence.ResetAccepted; } }
        public LMCReadStatusResult FinalStatus
        {
            get { return Evidence.LastObservedStatus; }
        }
        public int StatusPollCount { get { return Evidence.StatusPollCount; } }
        public int StableErrorClearSampleCount
        {
            get { return Evidence.StableErrorClearSampleCount; }
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

    public sealed class LMCAxisResetRejectedException
        : InvalidOperationException
    {
        internal LMCAxisResetRejectedException(
            LMCAxisResetWaitEvidence evidence)
            : base(
                "Axis Reset was rejected; no error-clear completion is claimed.")
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }

        public LMCAxisResetWaitEvidence Evidence { get; private set; }
        public LMC_Response Acknowledgement
        {
            get { return Evidence.ResetAcknowledgement; }
        }
    }

    public sealed class LMCAxisResetSubmissionException
        : InvalidOperationException
    {
        internal LMCAxisResetSubmissionException(
            LMCAxisResetWaitEvidence evidence,
            Exception innerException)
            : base(
                "Axis Reset dispatch did not produce a valid acknowledgement. Inspect CommandMayHaveBeenSent before deciding whether to retry.",
                innerException)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }

        public LMCAxisResetWaitEvidence Evidence { get; private set; }
    }

    public sealed class LMCAxisResetWaitTimeoutException
        : TimeoutException
    {
        internal LMCAxisResetWaitTimeoutException(
            LMCAxisResetWaitEvidence evidence)
            : this(evidence, null)
        {
        }

        internal LMCAxisResetWaitTimeoutException(
            LMCAxisResetWaitEvidence evidence,
            LMCAxisResetWaitContinuation continuation)
            : base(
                evidence != null
                        && evidence.TransportInvalidatedAtDeadline
                    ? "Axis Reset and stable error-clear verification exceeded the total deadline after an RPC write; the transport was invalidated and must be reconnected."
                    : "Axis Reset and stable error-clear verification did not finish before the total deadline.")
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }

        public LMCAxisResetWaitEvidence Evidence { get; private set; }
        public LMCAxisResetWaitContinuation Continuation
        {
            get;
            private set;
        }
        public LMC_Response Acknowledgement
        {
            get { return Evidence.ResetAcknowledgement; }
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

    public sealed class LMCAxisResetWaitCanceledException
        : OperationCanceledException
    {
        internal LMCAxisResetWaitCanceledException(
            LMCAxisResetWaitEvidence evidence,
            OperationCanceledException innerException,
            CancellationToken cancellationToken)
            : this(
                evidence,
                null,
                innerException,
                cancellationToken)
        {
        }

        internal LMCAxisResetWaitCanceledException(
            LMCAxisResetWaitEvidence evidence,
            LMCAxisResetWaitContinuation continuation,
            OperationCanceledException innerException,
            CancellationToken cancellationToken)
            : base(
                "Axis Reset and stable error-clear verification was canceled. Inspect the evidence before retrying.",
                innerException,
                cancellationToken)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }

        public LMCAxisResetWaitEvidence Evidence { get; private set; }
        public LMCAxisResetWaitContinuation Continuation
        {
            get;
            private set;
        }
        public LMC_Response Acknowledgement
        {
            get { return Evidence.ResetAcknowledgement; }
        }
        public LMCReadStatusResult LastObservedStatus
        {
            get { return Evidence.LastObservedStatus; }
        }
        public int StatusPollCount { get { return Evidence.StatusPollCount; } }
    }

    public sealed class LMCAxisResetStatusException
        : InvalidOperationException
    {
        internal LMCAxisResetStatusException(
            LMCAxisResetWaitEvidence evidence,
            LMCReadStatusResult failedStatus,
            Exception innerException)
            : this(evidence, null, failedStatus, innerException)
        {
        }

        internal LMCAxisResetStatusException(
            LMCAxisResetWaitEvidence evidence,
            LMCAxisResetWaitContinuation continuation,
            LMCReadStatusResult failedStatus,
            Exception innerException)
            : base(
                "Axis Reset was accepted, but error-clear verification could not obtain a successful axis status.",
                innerException)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
            FailedStatus = failedStatus;
        }

        public LMCAxisResetWaitEvidence Evidence { get; private set; }
        public LMCAxisResetWaitContinuation Continuation
        {
            get;
            private set;
        }
        public LMCReadStatusResult FailedStatus { get; private set; }
    }

    public sealed class LMCAxisResetInterferenceException
        : InvalidOperationException
    {
        internal LMCAxisResetInterferenceException(
            LMCAxisResetWaitEvidence evidence)
            : this(evidence, null)
        {
        }

        internal LMCAxisResetInterferenceException(
            LMCAxisResetWaitEvidence evidence,
            LMCAxisResetWaitContinuation continuation)
            : base(
                "Another mutation on this axis may have been sent after Axis Reset; stable error-clear proof was not attributed to the original Reset. Intentional post-Reset Power On requires a new Reset before attribution can resume.")
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }

        public LMCAxisResetWaitEvidence Evidence { get; private set; }
        public LMCAxisResetWaitContinuation Continuation
        {
            get;
            private set;
        }
        public long ExpectedMutationGeneration
        {
            get { return Evidence.ResetMutationGeneration; }
        }
        public long ObservedMutationGeneration
        {
            get { return Evidence.ObservedMutationGeneration; }
        }
    }

    public sealed class LMCAxisResetWaitPendingException
        : InvalidOperationException
    {
        internal LMCAxisResetWaitPendingException(
            LMCAxisResetWaitContinuation continuation)
            : base(
                "A pending accepted Axis Reset must be resolved, or explicitly taken over by a safety Stop, before a legacy raw Reset or Stop is sent.")
        {
            Continuation = continuation
                ?? throw new ArgumentNullException("continuation");
        }

        public LMCAxisResetWaitContinuation Continuation
        {
            get;
            private set;
        }
    }

    internal sealed class LMCAxisStableErrorClearanceWaitTracker
    {
        private readonly int requiredStableSampleCount;
        private LMCReadStatusResult lastObservedStatus;
        private int statusPollCount;
        private int stableErrorClearSampleCount;
        private bool completionPublished;
        private bool transportInvalidatedAtDeadline;
        private long baselineMutationGeneration;
        private long observedMutationGeneration;

        internal LMCAxisStableErrorClearanceWaitTracker(
            int requiredStableSampleCount)
        {
            this.requiredStableSampleCount = requiredStableSampleCount;
        }

        internal bool HasStableProof
        {
            get
            {
                return stableErrorClearSampleCount
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
                && status.IsReadSuccessful
                && !status.HasAxisError)
            {
                stableErrorClearSampleCount++;
            }
            else
            {
                stableErrorClearSampleCount = 0;
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

        internal LMCAxisStableErrorClearanceWaitEvidence CaptureEvidence(
            long elapsedMilliseconds)
        {
            return new LMCAxisStableErrorClearanceWaitEvidence(
                lastObservedStatus,
                statusPollCount,
                stableErrorClearSampleCount,
                requiredStableSampleCount,
                baselineMutationGeneration,
                observedMutationGeneration,
                transportInvalidatedAtDeadline,
                elapsedMilliseconds);
        }
    }

    /// <summary>
    /// Status-only evidence. It does not attribute error clearance to any
    /// Reset request or acknowledgement. Baseline/observed mutation
    /// generations only invalidate proof for LMCSingleAxis writes in this
    /// process, connection session, and AxisReference. PLC logic, another
    /// process, direct SDO, and group operations are outside that boundary.
    /// </summary>
    public sealed class LMCAxisStableErrorClearanceWaitEvidence
    {
        internal LMCAxisStableErrorClearanceWaitEvidence(
            LMCReadStatusResult lastObservedStatus,
            int statusPollCount,
            int stableErrorClearSampleCount,
            int requiredStableSampleCount,
            long baselineMutationGeneration,
            long observedMutationGeneration,
            bool transportInvalidatedAtDeadline,
            long elapsedMilliseconds)
        {
            LastObservedStatus = lastObservedStatus;
            StatusPollCount = statusPollCount;
            StableErrorClearSampleCount = stableErrorClearSampleCount;
            RequiredStableSampleCount = requiredStableSampleCount;
            BaselineMutationGeneration = baselineMutationGeneration;
            ObservedMutationGeneration = observedMutationGeneration;
            TransportInvalidatedAtDeadline =
                transportInvalidatedAtDeadline;
            ElapsedMilliseconds = Math.Max(0, elapsedMilliseconds);
        }

        public LMCReadStatusResult LastObservedStatus { get; private set; }
        public int StatusPollCount { get; private set; }
        public int StableErrorClearSampleCount { get; private set; }
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

    public sealed class LMCAxisStableErrorClearanceWaitResult
    {
        internal LMCAxisStableErrorClearanceWaitResult(
            LMCAxisStableErrorClearanceWaitEvidence evidence)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }

        public LMCAxisStableErrorClearanceWaitEvidence Evidence
        {
            get;
            private set;
        }
        public LMCReadStatusResult FinalStatus
        {
            get { return Evidence.LastObservedStatus; }
        }
        public int StatusPollCount { get { return Evidence.StatusPollCount; } }
        public int StableErrorClearSampleCount
        {
            get { return Evidence.StableErrorClearSampleCount; }
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

    public sealed class LMCAxisStableErrorClearanceWaitTimeoutException
        : TimeoutException
    {
        internal LMCAxisStableErrorClearanceWaitTimeoutException(
            LMCAxisStableErrorClearanceWaitEvidence evidence)
            : base(
                "Stable error-clear status was not observed before the total deadline.")
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }

        public LMCAxisStableErrorClearanceWaitEvidence Evidence
        {
            get;
            private set;
        }
    }

    public sealed class LMCAxisStableErrorClearanceWaitCanceledException
        : OperationCanceledException
    {
        internal LMCAxisStableErrorClearanceWaitCanceledException(
            LMCAxisStableErrorClearanceWaitEvidence evidence,
            OperationCanceledException innerException,
            CancellationToken cancellationToken)
            : base(
                "Stable error-clear status verification was canceled.",
                innerException,
                cancellationToken)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }

        public LMCAxisStableErrorClearanceWaitEvidence Evidence
        {
            get;
            private set;
        }
    }

    public sealed class LMCAxisStableErrorClearanceStatusException
        : InvalidOperationException
    {
        internal LMCAxisStableErrorClearanceStatusException(
            LMCAxisStableErrorClearanceWaitEvidence evidence,
            LMCReadStatusResult failedStatus,
            Exception innerException)
            : base(
                "Stable error-clear status verification could not obtain a successful axis status.",
                innerException)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            FailedStatus = failedStatus;
        }

        public LMCAxisStableErrorClearanceWaitEvidence Evidence
        {
            get;
            private set;
        }
        public LMCReadStatusResult FailedStatus { get; private set; }
    }

    public sealed class LMCAxisStableErrorClearanceInterferenceException
        : InvalidOperationException
    {
        internal LMCAxisStableErrorClearanceInterferenceException(
            LMCAxisStableErrorClearanceWaitEvidence evidence)
            : base(
                "A process-local same-axis mutation occurred during stable error-clear verification; the status proof is inconclusive. Mutations from PLC logic, another process, direct SDO, or group operations are outside this detection boundary.")
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }

        public LMCAxisStableErrorClearanceWaitEvidence Evidence
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

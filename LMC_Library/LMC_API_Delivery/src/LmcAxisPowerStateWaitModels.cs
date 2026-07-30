using System;
using System.Threading;

namespace LasalMotionControlLib
{
    internal sealed class LMCAxisPowerOnWaitCoordinator
    {
        private long mutationGeneration;

        internal LMCAxisPowerOnWaitCoordinator()
        {
            Sync = new object();
            MutationGate = new SemaphoreSlim(1, 1);
            StatusObservationGate = new SemaphoreSlim(1, 1);
        }

        internal object Sync { get; private set; }
        internal SemaphoreSlim MutationGate { get; private set; }
        internal SemaphoreSlim StatusObservationGate { get; private set; }
        internal LMCAxisPowerOnWaitContinuation PendingContinuation
        {
            get;
            set;
        }
        internal bool AcceptanceObserverInProgress { get; set; }
        internal bool WaitInProgress { get; set; }
        internal LMCAxisPowerOffWaitContinuation PendingPowerOffContinuation
        {
            get;
            set;
        }
        internal bool PowerOffAcceptanceObserverInProgress
        {
            get;
            set;
        }
        internal bool PowerOffWaitInProgress { get; set; }
        internal LMCAxisStopWaitContinuation PendingStopContinuation
        {
            get;
            set;
        }
        internal bool StopAcceptanceObserverInProgress { get; set; }
        internal bool StopWaitInProgress { get; set; }
        internal LMCAxisResetWaitContinuation PendingResetContinuation
        {
            get;
            set;
        }
        internal bool ResetAcceptanceObserverInProgress { get; set; }
        internal bool ResetWaitInProgress { get; set; }

        /// <summary>
        /// Tracks only mutation writes issued through LMCSingleAxis handles for
        /// this connection session and AxisReference. Mutations made by another
        /// RPC client, PLC logic, or a direct drive/SDO path are outside this
        /// process-local attribution boundary.
        /// </summary>
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
            lock (Sync)
            {
                mutationGeneration++;
                return mutationGeneration;
            }
        }

        /// <summary>
        /// Removes only the exact most-recent process-local mutation reservation
        /// after a structurally valid negative acknowledgement proves that the
        /// command was rejected. Callers must still hold MutationGate. A write
        /// with an unknown outcome and every accepted acknowledgement retain the
        /// reservation.
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

                mutationGeneration--;
                return true;
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

    public sealed class LMCAxisAcceptedObserverInProgressException
        : InvalidOperationException
    {
        internal LMCAxisAcceptedObserverInProgressException(
            LMCAxisStopWaitContinuation stopContinuation,
            LMCAxisResetWaitContinuation resetContinuation)
            : base(
                stopContinuation != null
                    ? "The accepted Axis Stop observer is still running."
                    : "The accepted Axis Reset observer is still running.")
        {
            StopContinuation = stopContinuation;
            ResetContinuation = resetContinuation;
        }

        public LMCAxisStopWaitContinuation StopContinuation
        {
            get;
            private set;
        }
        public LMCAxisResetWaitContinuation ResetContinuation
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// Session-bound evidence that one Axis Power Off acknowledgement was
    /// accepted. Resuming this continuation performs status-only 0x2028
    /// polling and never sends another 0x2023 request. Mutation attribution
    /// covers only LMCSingleAxis writes in this process, connection session,
    /// and AxisReference. PLC logic, another RPC client, direct SDO writes,
    /// and group operations are outside this attribution boundary.
    /// </summary>
    public sealed class LMCAxisPowerOffWaitContinuation
    {
        private readonly object stateSync;
        private readonly LMCConnection ownerConnection;
        private readonly LMCAxisPowerOffWaitTracker tracker;
        private int state;

        internal LMCAxisPowerOffWaitContinuation(
            LMCAxisPowerOnWaitCoordinator coordinator,
            LMCConnection ownerConnection,
            string axisName,
            ushort axisReference,
            long sessionGeneration,
            LMCAxisPowerOffWaitTracker tracker)
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
            state = 1;
        }

        internal LMCAxisPowerOnWaitCoordinator Coordinator
        {
            get;
            private set;
        }

        public string AxisName { get; private set; }
        public ushort AxisReference { get; private set; }
        public long SessionGeneration { get; private set; }

        public LMC_Response Acknowledgement
        {
            get
            {
                lock (stateSync)
                {
                    return tracker.CaptureEvidence(0)
                        .PowerOffAcknowledgement;
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

        public int StablePowerOffStandstillSampleCount
        {
            get
            {
                lock (stateSync)
                {
                    return tracker.CaptureEvidence(0)
                        .StablePowerOffStandstillSampleCount;
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

        public long PowerOffMutationGeneration
        {
            get
            {
                lock (stateSync)
                {
                    return tracker.CaptureEvidence(0)
                        .PowerOffMutationGeneration;
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

        internal bool HasStablePowerOffStandstillProof
        {
            get
            {
                lock (stateSync)
                {
                    return tracker.HasStablePowerOffStandstillProof;
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

        internal LMCAxisPowerOffWaitEvidence CaptureEvidence(
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
    /// Controls the total deadline and stable-sample requirement for an Axis
    /// Power On accepted-then-status wait or a read-only power-state wait.
    /// </summary>
    public sealed class LMCAxisPowerStateWaitOptions
    {
        public const int DefaultTimeoutMilliseconds = 5000;
        public const int DefaultPollIntervalMilliseconds = 50;
        public const int DefaultStableSampleCount = 3;

        public LMCAxisPowerStateWaitOptions()
        {
            TimeoutMilliseconds = DefaultTimeoutMilliseconds;
            PollIntervalMilliseconds = DefaultPollIntervalMilliseconds;
            StableSampleCount = DefaultStableSampleCount;
        }

        public int TimeoutMilliseconds { get; set; }
        public int PollIntervalMilliseconds { get; set; }
        public int StableSampleCount { get; set; }

        internal LMCAxisPowerStateWaitOptions SnapshotAndValidate()
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

            return new LMCAxisPowerStateWaitOptions
            {
                TimeoutMilliseconds = TimeoutMilliseconds,
                PollIntervalMilliseconds = PollIntervalMilliseconds,
                StableSampleCount = StableSampleCount
            };
        }
    }

    public enum LMCAxisPowerOnSubmissionOutcome
    {
        NotAttempted = 0,
        Rejected = 1,
        OutcomeUncertain = 2,
        Accepted = 3
    }

    /// <summary>
    /// Immutable evidence for one Power On-and-wait call. A successful Power
    /// On acknowledgement proves acceptance only. Completion requires the
    /// configured consecutive successful 0x2028 PowerOn observations whose
    /// process-local axis mutation generation still belongs to the accepted
    /// Power On.
    /// </summary>
    public sealed class LMCAxisPowerOnWaitEvidence
    {
        internal LMCAxisPowerOnWaitEvidence(
            LMCAxisPowerOnSubmissionOutcome submissionOutcome,
            LMC_Response acknowledgement,
            LMCReadStatusResult lastObservedStatus,
            int statusPollCount,
            int stablePowerOnSampleCount,
            int stablePowerOffStandstillSampleCount,
            int requiredStableSampleCount,
            long powerOnMutationGeneration,
            long observedMutationGeneration,
            bool transportInvalidatedAtDeadline,
            long elapsedMilliseconds)
        {
            SubmissionOutcome = submissionOutcome;
            PowerOnAcknowledgement = acknowledgement;
            LastObservedStatus = lastObservedStatus;
            StatusPollCount = statusPollCount;
            StablePowerOnSampleCount = stablePowerOnSampleCount;
            StablePowerOffStandstillSampleCount =
                stablePowerOffStandstillSampleCount;
            RequiredStableSampleCount = requiredStableSampleCount;
            PowerOnMutationGeneration = powerOnMutationGeneration;
            ObservedMutationGeneration = observedMutationGeneration;
            TransportInvalidatedAtDeadline =
                transportInvalidatedAtDeadline;
            ElapsedMilliseconds = Math.Max(0, elapsedMilliseconds);
        }

        public LMCAxisPowerOnSubmissionOutcome SubmissionOutcome
        {
            get;
            private set;
        }

        public bool CommandMayHaveBeenSent
        {
            get
            {
                return SubmissionOutcome
                    != LMCAxisPowerOnSubmissionOutcome.NotAttempted;
            }
        }

        public LMC_Response PowerOnAcknowledgement { get; private set; }

        public bool PowerOnAccepted
        {
            get
            {
                return SubmissionOutcome
                    == LMCAxisPowerOnSubmissionOutcome.Accepted;
            }
        }

        public LMCReadStatusResult LastObservedStatus { get; private set; }
        public int StatusPollCount { get; private set; }
        public int StablePowerOnSampleCount { get; private set; }
        public int StablePowerOffStandstillSampleCount { get; private set; }
        public int RequiredStableSampleCount { get; private set; }
        public long PowerOnMutationGeneration { get; private set; }
        public long ObservedMutationGeneration { get; private set; }
        public bool InterveningMutationDetected
        {
            get
            {
                return PowerOnMutationGeneration > 0
                    && ObservedMutationGeneration > 0
                    && PowerOnMutationGeneration
                        != ObservedMutationGeneration;
            }
        }
        public bool TransportInvalidatedAtDeadline { get; private set; }
        public long ElapsedMilliseconds { get; private set; }
    }

    internal sealed class LMCAxisPowerOnWaitTracker
    {
        private readonly int requiredStableSampleCount;
        private LMCAxisPowerOnSubmissionOutcome submissionOutcome;
        private LMC_Response acknowledgement;
        private LMCReadStatusResult lastObservedStatus;
        private int statusPollCount;
        private int stablePowerOnSampleCount;
        private int stablePowerOffStandstillSampleCount;
        private long powerOnMutationGeneration;
        private long observedMutationGeneration;
        private bool transportInvalidatedAtDeadline;

        internal LMCAxisPowerOnWaitTracker(int requiredStableSampleCount)
        {
            this.requiredStableSampleCount = requiredStableSampleCount;
        }

        internal bool HasStablePowerOnProof
        {
            get
            {
                return stablePowerOnSampleCount
                    >= requiredStableSampleCount;
            }
        }

        internal bool HasStablePowerOffStandstillProof
        {
            get
            {
                return stablePowerOffStandstillSampleCount
                    >= requiredStableSampleCount;
            }
        }

        internal long PowerOnMutationGeneration
        {
            get { return powerOnMutationGeneration; }
        }

        internal void SetPowerOnMutationGeneration(long value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException("value");
            }

            powerOnMutationGeneration = value;
            observedMutationGeneration = value;
        }

        internal void ObserveMutationGeneration(long value)
        {
            observedMutationGeneration = value;
        }

        internal void MarkSubmissionOutcomeUncertain()
        {
            if (submissionOutcome
                == LMCAxisPowerOnSubmissionOutcome.NotAttempted)
            {
                submissionOutcome =
                    LMCAxisPowerOnSubmissionOutcome.OutcomeUncertain;
            }
        }

        internal void SetAcknowledgement(LMC_Response value)
        {
            acknowledgement = value;
            submissionOutcome = value != null
                    && value.IsFrameValid
                    && value.IsSuccess
                ? LMCAxisPowerOnSubmissionOutcome.Accepted
                : LMCAxisPowerOnSubmissionOutcome.Rejected;
        }

        internal void Observe(LMCReadStatusResult status)
        {
            lastObservedStatus = status;
            statusPollCount++;

            if (status != null
                && status.IsReadSuccessful
                && status.IsPowerOn)
            {
                stablePowerOnSampleCount++;
            }
            else
            {
                stablePowerOnSampleCount = 0;
            }

            if (status != null
                && status.IsSuccess
                && !status.IsPowerOn
                && status.IsStandstill)
            {
                stablePowerOffStandstillSampleCount++;
            }
            else
            {
                stablePowerOffStandstillSampleCount = 0;
            }
        }

        internal void ResetProofCounters()
        {
            stablePowerOnSampleCount = 0;
            stablePowerOffStandstillSampleCount = 0;
        }

        internal void ResetPowerOffStandstillProof()
        {
            stablePowerOffStandstillSampleCount = 0;
        }

        internal void MarkTransportInvalidatedAtDeadline()
        {
            transportInvalidatedAtDeadline = true;
        }

        internal LMCAxisPowerOnWaitEvidence CaptureEvidence(
            long elapsedMilliseconds)
        {
            return new LMCAxisPowerOnWaitEvidence(
                submissionOutcome,
                acknowledgement,
                lastObservedStatus,
                statusPollCount,
                stablePowerOnSampleCount,
                stablePowerOffStandstillSampleCount,
                requiredStableSampleCount,
                powerOnMutationGeneration,
                observedMutationGeneration,
                transportInvalidatedAtDeadline,
                elapsedMilliseconds);
        }
    }

    /// <summary>
    /// Session-bound evidence that one Axis Power On acknowledgement was
    /// accepted. The continuation can only resume read-only 0x2028 polling; it
    /// never sends another 0x2023 command. Mutation attribution covers only
    /// LMCSingleAxis writes in this process, connection session, and
    /// AxisReference. PLC logic, another RPC client, direct SDO writes, and
    /// group operations are outside this attribution boundary.
    /// </summary>
    public sealed class LMCAxisPowerOnWaitContinuation
    {
        private readonly object stateSync;
        private readonly LMCConnection ownerConnection;
        private readonly LMCAxisPowerOnWaitTracker tracker;
        private int isPending;

        internal LMCAxisPowerOnWaitContinuation(
            LMCAxisPowerOnWaitCoordinator coordinator,
            LMCConnection ownerConnection,
            string axisName,
            ushort axisReference,
            long sessionGeneration,
            LMCAxisPowerOnWaitTracker tracker)
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
            isPending = 1;
        }

        internal LMCAxisPowerOnWaitCoordinator Coordinator
        {
            get;
            private set;
        }

        public string AxisName { get; private set; }
        public ushort AxisReference { get; private set; }
        public long SessionGeneration { get; private set; }

        public LMC_Response Acknowledgement
        {
            get
            {
                lock (stateSync)
                {
                    return tracker.CaptureEvidence(0)
                        .PowerOnAcknowledgement;
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

        public int PollCount
        {
            get
            {
                lock (stateSync)
                {
                    return tracker.CaptureEvidence(0).StatusPollCount;
                }
            }
        }

        public int StablePowerOnSampleCount
        {
            get
            {
                lock (stateSync)
                {
                    return tracker.CaptureEvidence(0)
                        .StablePowerOnSampleCount;
                }
            }
        }

        public int StablePowerOffStandstillSampleCount
        {
            get
            {
                lock (stateSync)
                {
                    return tracker.CaptureEvidence(0)
                        .StablePowerOffStandstillSampleCount;
                }
            }
        }

        public long PowerOnMutationGeneration
        {
            get
            {
                lock (stateSync)
                {
                    return tracker.CaptureEvidence(0)
                        .PowerOnMutationGeneration;
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
                    return isPending != 0;
                }
            }
        }

        internal bool HasStablePowerOnProof
        {
            get
            {
                lock (stateSync)
                {
                    return tracker.HasStablePowerOnProof;
                }
            }
        }

        internal bool HasStablePowerOffStandstillProof
        {
            get
            {
                lock (stateSync)
                {
                    return tracker.HasStablePowerOffStandstillProof;
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

        internal LMCAxisPowerOnWaitEvidence CaptureEvidence(
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

        internal void ResetPowerOffStandstillProof()
        {
            lock (stateSync)
            {
                tracker.ResetPowerOffStandstillProof();
            }
        }

        internal void MarkResolved()
        {
            lock (stateSync)
            {
                isPending = 0;
            }
        }
    }

    /// <summary>
    /// Immutable result of either an accepted Axis Power On wait or a read-only
    /// axis power-state wait.
    /// </summary>
    public sealed class LMCAxisPowerStateWaitResult
    {
        internal LMCAxisPowerStateWaitResult(
            bool expectedPowerOn,
            LMCAxisPowerOnWaitEvidence evidence,
            LMCAxisPowerOnWaitContinuation continuation,
            bool reusedAcceptedAcknowledgement)
        {
            ExpectedPowerOn = expectedPowerOn;
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
            ReusedAcceptedAcknowledgement = reusedAcceptedAcknowledgement;
        }

        public bool ExpectedPowerOn { get; private set; }
        public LMCAxisPowerOnWaitEvidence Evidence { get; private set; }
        public LMCAxisPowerOnSubmissionOutcome SubmissionOutcome
        {
            get { return Evidence.SubmissionOutcome; }
        }
        public bool CommandMayHaveBeenSent
        {
            get { return Evidence.CommandMayHaveBeenSent; }
        }
        public bool PowerOnAccepted { get { return Evidence.PowerOnAccepted; } }
        public LMCReadStatusResult FinalStatus
        {
            get { return Evidence.LastObservedStatus; }
        }
        public int PollCount { get { return Evidence.StatusPollCount; } }
        public int StableSampleCount
        {
            get
            {
                return ExpectedPowerOn
                    ? Evidence.StablePowerOnSampleCount
                    : Evidence.StablePowerOffStandstillSampleCount;
            }
        }
        public int RequiredStableSampleCount
        {
            get { return Evidence.RequiredStableSampleCount; }
        }
        public LMC_Response Acknowledgement
        {
            get { return Evidence.PowerOnAcknowledgement; }
        }
        public bool TransportInvalidatedAtDeadline
        {
            get { return Evidence.TransportInvalidatedAtDeadline; }
        }
        public LMCAxisPowerOnWaitContinuation Continuation { get; private set; }
        public bool ReusedAcceptedAcknowledgement { get; private set; }
    }

    public sealed class LMCAxisPowerOnRejectedException
        : InvalidOperationException
    {
        internal LMCAxisPowerOnRejectedException(
            LMCAxisPowerOnWaitEvidence evidence)
            : base(
                "Axis Power On was rejected. Status="
                + evidence.PowerOnAcknowledgement.Status
                + ", ErrorId="
                + evidence.PowerOnAcknowledgement.ErrorId
                + ".")
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }

        public LMCAxisPowerOnWaitEvidence Evidence { get; private set; }
        public LMC_Response Acknowledgement
        {
            get { return Evidence.PowerOnAcknowledgement; }
        }
    }

    public sealed class LMCAxisPowerOnSubmissionException
        : InvalidOperationException
    {
        internal LMCAxisPowerOnSubmissionException(
            LMCAxisPowerOnWaitEvidence evidence,
            Exception innerException)
            : base(
                "Axis Power On dispatch did not produce a valid acknowledgement. Inspect CommandMayHaveBeenSent before deciding whether to retry.",
                innerException)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }

        public LMCAxisPowerOnWaitEvidence Evidence { get; private set; }
    }

    public sealed class LMCAxisPowerOnPendingException
        : InvalidOperationException
    {
        internal LMCAxisPowerOnPendingException(
            LMCAxisPowerOnWaitContinuation continuation)
            : base(
                "A previously accepted Axis Power On is still pending verification. Resume the status-only continuation or prove Power Off and standstill.")
        {
            Continuation = continuation;
        }

        public LMCAxisPowerOnWaitContinuation Continuation
        {
            get;
            private set;
        }
    }

    public sealed class LMCAxisPowerOnInterferenceException
        : InvalidOperationException
    {
        internal LMCAxisPowerOnInterferenceException(
            LMCAxisPowerOnWaitEvidence evidence)
            : this(evidence, null)
        {
        }

        internal LMCAxisPowerOnInterferenceException(
            LMCAxisPowerOnWaitEvidence evidence,
            LMCAxisPowerOnWaitContinuation continuation)
            : base(
                "Another mutation on this axis may have been sent after Axis Power On; stable PowerOn proof was not attributed to the original Power On.")
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }

        public LMCAxisPowerOnWaitEvidence Evidence { get; private set; }
        public LMCAxisPowerOnWaitContinuation Continuation
        {
            get;
            private set;
        }
        public long ExpectedMutationGeneration
        {
            get { return Evidence.PowerOnMutationGeneration; }
        }
        public long ObservedMutationGeneration
        {
            get { return Evidence.ObservedMutationGeneration; }
        }
    }

    public sealed class LMCAxisPowerStateWaitTimeoutException
        : TimeoutException
    {
        internal LMCAxisPowerStateWaitTimeoutException(
            bool expectedPowerOn,
            LMCAxisPowerOnWaitEvidence evidence,
            LMCAxisPowerOnWaitContinuation continuation)
            : base(
                evidence != null
                        && evidence.TransportInvalidatedAtDeadline
                    ? "The expected axis power-state wait exceeded the total deadline after an RPC write; the transport was invalidated and must be reconnected."
                    : "The expected axis power state was not stable before the total wait deadline.")
        {
            ExpectedPowerOn = expectedPowerOn;
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }

        public bool ExpectedPowerOn { get; private set; }
        public LMCAxisPowerOnWaitEvidence Evidence { get; private set; }
        public LMCAxisPowerOnWaitContinuation Continuation { get; private set; }
        public LMC_Response Acknowledgement
        {
            get { return Evidence.PowerOnAcknowledgement; }
        }
        public LMCReadStatusResult LastObservedStatus
        {
            get { return Evidence.LastObservedStatus; }
        }
        public int PollCount { get { return Evidence.StatusPollCount; } }
        public int StableSampleCount
        {
            get
            {
                return ExpectedPowerOn
                    ? Evidence.StablePowerOnSampleCount
                    : Evidence.StablePowerOffStandstillSampleCount;
            }
        }
        public bool TransportInvalidatedAtDeadline
        {
            get { return Evidence.TransportInvalidatedAtDeadline; }
        }
    }

    public sealed class LMCAxisPowerStateWaitCanceledException
        : OperationCanceledException
    {
        internal LMCAxisPowerStateWaitCanceledException(
            bool expectedPowerOn,
            LMCAxisPowerOnWaitEvidence evidence,
            LMCAxisPowerOnWaitContinuation continuation,
            OperationCanceledException innerException,
            CancellationToken cancellationToken)
            : base(
                "The axis power-state wait was canceled. Inspect the evidence before retrying.",
                innerException,
                cancellationToken)
        {
            ExpectedPowerOn = expectedPowerOn;
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }

        public bool ExpectedPowerOn { get; private set; }
        public LMCAxisPowerOnWaitEvidence Evidence { get; private set; }
        public LMCAxisPowerOnWaitContinuation Continuation { get; private set; }
        public LMC_Response Acknowledgement
        {
            get { return Evidence.PowerOnAcknowledgement; }
        }
        public LMCReadStatusResult LastObservedStatus
        {
            get { return Evidence.LastObservedStatus; }
        }
        public int PollCount { get { return Evidence.StatusPollCount; } }
        public int StableSampleCount
        {
            get
            {
                return ExpectedPowerOn
                    ? Evidence.StablePowerOnSampleCount
                    : Evidence.StablePowerOffStandstillSampleCount;
            }
        }
    }

    public sealed class LMCAxisPowerStateStatusException
        : InvalidOperationException
    {
        internal LMCAxisPowerStateStatusException(
            bool expectedPowerOn,
            LMCAxisPowerOnWaitEvidence evidence,
            LMCAxisPowerOnWaitContinuation continuation,
            LMCReadStatusResult failedStatus,
            Exception innerException)
            : base(
                "The read-only axis power-state wait could not read a successful axis status.",
                innerException)
        {
            ExpectedPowerOn = expectedPowerOn;
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
            FailedStatus = failedStatus;
        }

        public bool ExpectedPowerOn { get; private set; }
        public LMCAxisPowerOnWaitEvidence Evidence { get; private set; }
        public LMCAxisPowerOnWaitContinuation Continuation { get; private set; }
        public LMCReadStatusResult FailedStatus { get; private set; }
    }

    public enum LMCAxisPowerOffSubmissionOutcome
    {
        NotAttempted = 0,
        Rejected = 1,
        OutcomeUncertain = 2,
        Accepted = 3
    }

    /// <summary>
    /// Immutable evidence for one Power Off-and-wait call. A successful Power
    /// Off acknowledgement proves acceptance only. Completion requires the
    /// configured consecutive 0x2028 observations whose RPC/function result
    /// succeeds, AxisErrorId is zero, and state is PowerOff and Standstill.
    /// </summary>
    public sealed class LMCAxisPowerOffWaitEvidence
    {
        internal LMCAxisPowerOffWaitEvidence(
            LMCAxisPowerOffSubmissionOutcome submissionOutcome,
            LMC_Response acknowledgement,
            LMCReadStatusResult lastObservedStatus,
            int statusPollCount,
            int stablePowerOffStandstillSampleCount,
            int requiredStableSampleCount,
            long powerOffMutationGeneration,
            long observedMutationGeneration,
            bool transportInvalidatedAtDeadline,
            long elapsedMilliseconds)
        {
            SubmissionOutcome = submissionOutcome;
            PowerOffAcknowledgement = acknowledgement;
            LastObservedStatus = lastObservedStatus;
            StatusPollCount = statusPollCount;
            StablePowerOffStandstillSampleCount =
                stablePowerOffStandstillSampleCount;
            RequiredStableSampleCount = requiredStableSampleCount;
            PowerOffMutationGeneration = powerOffMutationGeneration;
            ObservedMutationGeneration = observedMutationGeneration;
            TransportInvalidatedAtDeadline =
                transportInvalidatedAtDeadline;
            ElapsedMilliseconds = Math.Max(0, elapsedMilliseconds);
        }

        public LMCAxisPowerOffSubmissionOutcome SubmissionOutcome
        {
            get;
            private set;
        }

        public bool CommandMayHaveBeenSent
        {
            get
            {
                return SubmissionOutcome
                    != LMCAxisPowerOffSubmissionOutcome.NotAttempted;
            }
        }

        public LMC_Response PowerOffAcknowledgement { get; private set; }

        public bool PowerOffAccepted
        {
            get
            {
                return SubmissionOutcome
                    == LMCAxisPowerOffSubmissionOutcome.Accepted;
            }
        }

        public LMCReadStatusResult LastObservedStatus { get; private set; }
        public int StatusPollCount { get; private set; }
        public int StablePowerOffStandstillSampleCount { get; private set; }
        public int RequiredStableSampleCount { get; private set; }
        public long PowerOffMutationGeneration { get; private set; }
        public long ObservedMutationGeneration { get; private set; }
        public bool InterveningMutationDetected
        {
            get
            {
                return PowerOffMutationGeneration > 0
                    && ObservedMutationGeneration > 0
                    && PowerOffMutationGeneration
                        != ObservedMutationGeneration;
            }
        }
        public bool TransportInvalidatedAtDeadline { get; private set; }
        public long ElapsedMilliseconds { get; private set; }
    }

    internal sealed class LMCAxisPowerOffWaitTracker
    {
        private readonly int requiredStableSampleCount;
        private LMCAxisPowerOffSubmissionOutcome submissionOutcome;
        private LMC_Response acknowledgement;
        private LMCReadStatusResult lastObservedStatus;
        private int statusPollCount;
        private int stablePowerOffStandstillSampleCount;
        private long powerOffMutationGeneration;
        private long observedMutationGeneration;
        private bool transportInvalidatedAtDeadline;

        internal LMCAxisPowerOffWaitTracker(int requiredStableSampleCount)
        {
            this.requiredStableSampleCount = requiredStableSampleCount;
        }

        internal bool HasStablePowerOffStandstillProof
        {
            get
            {
                return stablePowerOffStandstillSampleCount
                    >= requiredStableSampleCount;
            }
        }

        internal long PowerOffMutationGeneration
        {
            get { return powerOffMutationGeneration; }
        }

        internal void SetPowerOffMutationGeneration(long value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException("value");
            }

            powerOffMutationGeneration = value;
            observedMutationGeneration = value;
        }

        internal void ObserveMutationGeneration(long value)
        {
            observedMutationGeneration = value;
        }

        internal void MarkSubmissionOutcomeUncertain()
        {
            if (submissionOutcome
                == LMCAxisPowerOffSubmissionOutcome.NotAttempted)
            {
                submissionOutcome =
                    LMCAxisPowerOffSubmissionOutcome.OutcomeUncertain;
            }
        }

        internal void SetAcknowledgement(LMC_Response value)
        {
            acknowledgement = value;
            submissionOutcome = value != null
                    && value.IsFrameValid
                    && value.IsSuccess
                ? LMCAxisPowerOffSubmissionOutcome.Accepted
                : LMCAxisPowerOffSubmissionOutcome.Rejected;
        }

        internal void Observe(LMCReadStatusResult status)
        {
            lastObservedStatus = status;
            statusPollCount++;

            if (status != null
                && status.IsSuccess
                && !status.IsPowerOn
                && status.IsStandstill)
            {
                stablePowerOffStandstillSampleCount++;
            }
            else
            {
                stablePowerOffStandstillSampleCount = 0;
            }
        }

        internal void ResetProofCounters()
        {
            stablePowerOffStandstillSampleCount = 0;
        }

        internal void MarkTransportInvalidatedAtDeadline()
        {
            transportInvalidatedAtDeadline = true;
        }

        internal LMCAxisPowerOffWaitEvidence CaptureEvidence(
            long elapsedMilliseconds)
        {
            return new LMCAxisPowerOffWaitEvidence(
                submissionOutcome,
                acknowledgement,
                lastObservedStatus,
                statusPollCount,
                stablePowerOffStandstillSampleCount,
                requiredStableSampleCount,
                powerOffMutationGeneration,
                observedMutationGeneration,
                transportInvalidatedAtDeadline,
                elapsedMilliseconds);
        }
    }

    public sealed class LMCAxisPowerOffWaitResult
    {
        internal LMCAxisPowerOffWaitResult(
            LMCAxisPowerOffWaitEvidence evidence,
            LMCAxisPowerOffWaitContinuation continuation)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation
                ?? throw new ArgumentNullException("continuation");
        }

        public LMCAxisPowerOffWaitEvidence Evidence { get; private set; }
        public LMCAxisPowerOffWaitContinuation Continuation
        {
            get;
            private set;
        }
        public LMCAxisPowerOffSubmissionOutcome SubmissionOutcome
        {
            get { return Evidence.SubmissionOutcome; }
        }
        public LMC_Response Acknowledgement
        {
            get { return Evidence.PowerOffAcknowledgement; }
        }
        public bool PowerOffAccepted { get { return Evidence.PowerOffAccepted; } }
        public LMCReadStatusResult FinalStatus
        {
            get { return Evidence.LastObservedStatus; }
        }
        public int StatusPollCount { get { return Evidence.StatusPollCount; } }
        public int StablePowerOffStandstillSampleCount
        {
            get { return Evidence.StablePowerOffStandstillSampleCount; }
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

    public sealed class LMCAxisPowerOffRejectedException
        : InvalidOperationException
    {
        internal LMCAxisPowerOffRejectedException(
            LMCAxisPowerOffWaitEvidence evidence)
            : base(
                "Axis Power Off was rejected; no PowerOff and Standstill completion is claimed.")
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }

        public LMCAxisPowerOffWaitEvidence Evidence { get; private set; }
        public LMC_Response Acknowledgement
        {
            get { return Evidence.PowerOffAcknowledgement; }
        }
    }

    public sealed class LMCAxisPowerOffSubmissionException
        : InvalidOperationException
    {
        internal LMCAxisPowerOffSubmissionException(
            LMCAxisPowerOffWaitEvidence evidence,
            Exception innerException)
            : base(
                "Axis Power Off dispatch did not produce a valid acknowledgement. Inspect CommandMayHaveBeenSent before deciding whether to retry.",
                innerException)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }

        public LMCAxisPowerOffWaitEvidence Evidence { get; private set; }
    }

    public sealed class LMCAxisPowerOffWaitTimeoutException
        : TimeoutException
    {
        internal LMCAxisPowerOffWaitTimeoutException(
            LMCAxisPowerOffWaitEvidence evidence)
            : this(evidence, null)
        {
        }

        internal LMCAxisPowerOffWaitTimeoutException(
            LMCAxisPowerOffWaitEvidence evidence,
            LMCAxisPowerOffWaitContinuation continuation)
            : base(
                evidence != null
                        && evidence.TransportInvalidatedAtDeadline
                    ? "Axis Power Off and stable PowerOff and Standstill verification exceeded the total deadline after an RPC write; the transport was invalidated and must be reconnected."
                    : "Axis Power Off and stable PowerOff and Standstill verification did not finish before the total deadline.")
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }

        public LMCAxisPowerOffWaitEvidence Evidence { get; private set; }
        public LMCAxisPowerOffWaitContinuation Continuation
        {
            get;
            private set;
        }
        public LMC_Response Acknowledgement
        {
            get { return Evidence.PowerOffAcknowledgement; }
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

    public sealed class LMCAxisPowerOffWaitCanceledException
        : OperationCanceledException
    {
        internal LMCAxisPowerOffWaitCanceledException(
            LMCAxisPowerOffWaitEvidence evidence,
            OperationCanceledException innerException,
            CancellationToken cancellationToken)
            : this(evidence, null, innerException, cancellationToken)
        {
        }

        internal LMCAxisPowerOffWaitCanceledException(
            LMCAxisPowerOffWaitEvidence evidence,
            LMCAxisPowerOffWaitContinuation continuation,
            OperationCanceledException innerException,
            CancellationToken cancellationToken)
            : base(
                "Axis Power Off and stable PowerOff and Standstill verification was canceled. Inspect the evidence before retrying.",
                innerException,
                cancellationToken)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }

        public LMCAxisPowerOffWaitEvidence Evidence { get; private set; }
        public LMCAxisPowerOffWaitContinuation Continuation
        {
            get;
            private set;
        }
        public LMC_Response Acknowledgement
        {
            get { return Evidence.PowerOffAcknowledgement; }
        }
        public LMCReadStatusResult LastObservedStatus
        {
            get { return Evidence.LastObservedStatus; }
        }
        public int StatusPollCount { get { return Evidence.StatusPollCount; } }
    }

    public sealed class LMCAxisPowerOffStatusException
        : InvalidOperationException
    {
        internal LMCAxisPowerOffStatusException(
            LMCAxisPowerOffWaitEvidence evidence,
            LMCReadStatusResult failedStatus,
            Exception innerException)
            : this(evidence, null, failedStatus, innerException)
        {
        }

        internal LMCAxisPowerOffStatusException(
            LMCAxisPowerOffWaitEvidence evidence,
            LMCAxisPowerOffWaitContinuation continuation,
            LMCReadStatusResult failedStatus,
            Exception innerException)
            : base(
                "Axis Power Off was accepted, but PowerOff and Standstill verification could not obtain a successful axis status.",
                innerException)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
            FailedStatus = failedStatus;
        }

        public LMCAxisPowerOffWaitEvidence Evidence { get; private set; }
        public LMCAxisPowerOffWaitContinuation Continuation
        {
            get;
            private set;
        }
        public LMCReadStatusResult FailedStatus { get; private set; }
    }

    public sealed class LMCAxisPowerOffInterferenceException
        : InvalidOperationException
    {
        internal LMCAxisPowerOffInterferenceException(
            LMCAxisPowerOffWaitEvidence evidence)
            : this(evidence, null)
        {
        }

        internal LMCAxisPowerOffInterferenceException(
            LMCAxisPowerOffWaitEvidence evidence,
            LMCAxisPowerOffWaitContinuation continuation)
            : base(
                "Another mutation on this axis may have been sent after Axis Power Off; stable PowerOff and Standstill proof was not attributed to the original Power Off.")
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }

        public LMCAxisPowerOffWaitEvidence Evidence { get; private set; }
        public LMCAxisPowerOffWaitContinuation Continuation
        {
            get;
            private set;
        }
        public long ExpectedMutationGeneration
        {
            get { return Evidence.PowerOffMutationGeneration; }
        }
        public long ObservedMutationGeneration
        {
            get { return Evidence.ObservedMutationGeneration; }
        }
    }
}

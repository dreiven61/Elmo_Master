using System;
using System.Threading;

namespace LasalMotionControlLib
{
    /// <summary>
    /// Immutable evidence from a read-only locked-standby wait. The wait sends
    /// only 0x2045 GroupReadStatus requests; it never sends GroupEnable.
    /// </summary>
    public sealed class LMCGroupLockedStandbyWaitEvidence
    {
        internal LMCGroupLockedStandbyWaitEvidence(
            LMCGroupReadStatusResult lastObservedStatus,
            int statusPollCount,
            int stableSampleCount,
            int requiredStableSampleCount,
            bool transportInvalidatedAtDeadline,
            long elapsedMilliseconds)
        {
            LastObservedStatus = lastObservedStatus;
            StatusPollCount = statusPollCount;
            StableSampleCount = stableSampleCount;
            RequiredStableSampleCount = requiredStableSampleCount;
            TransportInvalidatedAtDeadline =
                transportInvalidatedAtDeadline;
            ElapsedMilliseconds = Math.Max(0, elapsedMilliseconds);
        }

        public LMCGroupReadStatusResult LastObservedStatus { get; private set; }
        public int StatusPollCount { get; private set; }
        public int StableSampleCount { get; private set; }
        public int RequiredStableSampleCount { get; private set; }
        public bool TransportInvalidatedAtDeadline { get; private set; }
        public long ElapsedMilliseconds { get; private set; }
    }

    internal sealed class LMCGroupLockedStandbyWaitTracker
        : LMCGroupStatusWaitTracker
    {
        private readonly int requiredStableSampleCount;
        private LMCGroupReadStatusResult lastObservedStatus;
        private int statusPollCount;
        private int stableSampleCount;
        private bool transportInvalidatedAtDeadline;

        internal LMCGroupLockedStandbyWaitTracker(
            int requiredStableSampleCount)
        {
            this.requiredStableSampleCount = requiredStableSampleCount;
        }

        internal bool HasStableProof
        {
            get { return stableSampleCount >= requiredStableSampleCount; }
        }

        public void Observe(LMCGroupReadStatusResult status)
        {
            lastObservedStatus = status;
            statusPollCount++;

            if (status != null
                && status.IsSuccess
                && status.IsPowerOn
                && status.IsStandby)
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

        internal LMCGroupLockedStandbyWaitEvidence CaptureEvidence(
            long elapsedMilliseconds)
        {
            return new LMCGroupLockedStandbyWaitEvidence(
                lastObservedStatus,
                statusPollCount,
                stableSampleCount,
                requiredStableSampleCount,
                transportInvalidatedAtDeadline,
                elapsedMilliseconds);
        }
    }

    public sealed class LMCGroupLockedStandbyWaitResult
    {
        internal LMCGroupLockedStandbyWaitResult(
            LMCGroupLockedStandbyWaitEvidence evidence)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }

        public LMCGroupLockedStandbyWaitEvidence Evidence { get; private set; }
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
        public bool TransportInvalidatedAtDeadline
        {
            get { return Evidence.TransportInvalidatedAtDeadline; }
        }
        public long ElapsedMilliseconds
        {
            get { return Evidence.ElapsedMilliseconds; }
        }
    }

    public sealed class LMCGroupLockedStandbyWaitTimeoutException
        : TimeoutException
    {
        internal LMCGroupLockedStandbyWaitTimeoutException(
            LMCGroupLockedStandbyWaitEvidence evidence)
            : base(
                evidence != null
                        && evidence.TransportInvalidatedAtDeadline
                    ? "The locked-standby wait exceeded its total deadline after a status write; the transport was invalidated and must be reconnected."
                    : "Locked standby was not stable before the total wait deadline.")
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }

        public LMCGroupLockedStandbyWaitEvidence Evidence { get; private set; }
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

    public sealed class LMCGroupLockedStandbyWaitCanceledException
        : OperationCanceledException
    {
        internal LMCGroupLockedStandbyWaitCanceledException(
            LMCGroupLockedStandbyWaitEvidence evidence,
            OperationCanceledException innerException,
            CancellationToken cancellationToken)
            : base(
                "The read-only locked-standby wait was canceled.",
                innerException,
                cancellationToken)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
        }

        public LMCGroupLockedStandbyWaitEvidence Evidence { get; private set; }
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

    public sealed class LMCGroupLockedStandbyStatusException
        : InvalidOperationException
    {
        internal LMCGroupLockedStandbyStatusException(
            LMCGroupLockedStandbyWaitEvidence evidence,
            LMCGroupReadStatusResult failedStatus,
            Exception innerException)
            : base(
                "Locked-standby verification could not read a successful group status.",
                innerException)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            FailedStatus = failedStatus;
        }

        public LMCGroupLockedStandbyWaitEvidence Evidence { get; private set; }
        public LMCGroupReadStatusResult FailedStatus { get; private set; }
        public LMCGroupReadStatusResult LastObservedStatus
        {
            get { return Evidence.LastObservedStatus; }
        }
        public int StatusPollCount { get { return Evidence.StatusPollCount; } }
        public bool TransportInvalidatedAtDeadline
        {
            get { return Evidence.TransportInvalidatedAtDeadline; }
        }
    }
}

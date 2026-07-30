using System;
using System.Collections.Generic;

namespace LasalMotionControlApiExample
{
    internal enum TopologyIoLiveMonitorSkipReason
    {
        None = 0,
        Disabled,
        Disconnected,
        Busy,
        MissingTopology,
        MissingCapabilities,
        InFlight,
        Backoff
    }

    internal enum TopologyIoLiveMonitorReadKind
    {
        None = 0,
        NodeHealth,
        SelectedInput
    }

    internal sealed class TopologyIoLiveMonitorRequest
    {
        internal TopologyIoLiveMonitorRequest(
            bool enabled,
            bool connected,
            bool busy,
            bool supportsTopology,
            bool supportsNodeHealth,
            bool supportsDigitalInput,
            uint topologyRevision,
            IReadOnlyList<uint> healthNodeIds,
            uint selectedInputNodeId,
            uint selectedInputReference,
            byte selectedInputWidth,
            DateTime utcNow)
        {
            Enabled = enabled;
            Connected = connected;
            Busy = busy;
            SupportsTopology = supportsTopology;
            SupportsNodeHealth = supportsNodeHealth;
            SupportsDigitalInput = supportsDigitalInput;
            TopologyRevision = topologyRevision;
            HealthNodeIds = healthNodeIds;
            SelectedInputNodeId = selectedInputNodeId;
            SelectedInputReference = selectedInputReference;
            SelectedInputWidth = selectedInputWidth;
            UtcNow = utcNow;
        }

        internal bool Enabled { get; private set; }
        internal bool Connected { get; private set; }
        internal bool Busy { get; private set; }
        internal bool SupportsTopology { get; private set; }
        internal bool SupportsNodeHealth { get; private set; }
        internal bool SupportsDigitalInput { get; private set; }
        internal uint TopologyRevision { get; private set; }
        internal IReadOnlyList<uint> HealthNodeIds { get; private set; }
        internal uint SelectedInputNodeId { get; private set; }
        internal uint SelectedInputReference { get; private set; }
        internal byte SelectedInputWidth { get; private set; }
        internal DateTime UtcNow { get; private set; }
    }

    internal sealed class TopologyIoLiveMonitorLease
    {
        internal TopologyIoLiveMonitorLease(
            long leaseId,
            long sessionGeneration,
            long topologyGeneration,
            long selectionGeneration,
            TopologyIoLiveMonitorReadKind readKind,
            uint topologyRevision,
            uint healthNodeId,
            uint selectedInputNodeId,
            uint selectedInputReference,
            byte selectedInputWidth)
        {
            LeaseId = leaseId;
            SessionGeneration = sessionGeneration;
            TopologyGeneration = topologyGeneration;
            SelectionGeneration = selectionGeneration;
            ReadKind = readKind;
            TopologyRevision = topologyRevision;
            HealthNodeId = healthNodeId;
            SelectedInputNodeId = selectedInputNodeId;
            SelectedInputReference = selectedInputReference;
            SelectedInputWidth = selectedInputWidth;
        }

        internal long LeaseId { get; private set; }
        internal long SessionGeneration { get; private set; }
        internal long TopologyGeneration { get; private set; }
        internal long SelectionGeneration { get; private set; }
        internal TopologyIoLiveMonitorReadKind ReadKind { get; private set; }
        internal uint TopologyRevision { get; private set; }
        internal uint HealthNodeId { get; private set; }
        internal uint SelectedInputNodeId { get; private set; }
        internal uint SelectedInputReference { get; private set; }
        internal byte SelectedInputWidth { get; private set; }

        internal bool ReadsHealth
        {
            get
            {
                return ReadKind == TopologyIoLiveMonitorReadKind.NodeHealth
                    && HealthNodeId != 0;
            }
        }

        internal bool ReadsSelectedInput
        {
            get
            {
                return ReadKind == TopologyIoLiveMonitorReadKind.SelectedInput
                    && SelectedInputNodeId != 0
                    && SelectedInputReference != 0
                    && SelectedInputWidth != 0;
            }
        }
    }

    /// <summary>
    /// Pure policy for the read-only topology/I/O UI monitor. The policy owns
    /// no transport or WPF object. It serializes one bounded sampling lease,
    /// tracks UI session/topology/selection generations, and suppresses retry
    /// and repeated-error floods after a failed sample.
    /// </summary>
    internal sealed class TopologyIoLiveMonitorPolicy
    {
        internal const int DefaultIntervalMilliseconds = 500;
        internal const int MinimumIntervalMilliseconds = 250;
        internal const int MaximumIntervalMilliseconds = 5000;
        internal const int InitialFailureBackoffMilliseconds = 1000;
        internal const int MaximumFailureBackoffMilliseconds = 10000;
        internal const int DuplicateFailureLogWindowMilliseconds = 10000;

        private long sessionGeneration = 1;
        private long topologyGeneration = 1;
        private long selectionGeneration = 1;
        private long nextLeaseId;
        private long activeLeaseId;
        private int nextHealthIndex;
        private bool scheduleSelectedInputNext;
        private int consecutiveFailures;
        private DateTime nextAllowedUtc = DateTime.MinValue;
        private DateTime lastFailureLogUtc = DateTime.MinValue;
        private string lastFailureSignature;

        internal long SessionGeneration
        {
            get { return sessionGeneration; }
        }

        internal long TopologyGeneration
        {
            get { return topologyGeneration; }
        }

        internal long SelectionGeneration
        {
            get { return selectionGeneration; }
        }

        internal int ConsecutiveFailures
        {
            get { return consecutiveFailures; }
        }

        internal DateTime NextAllowedUtc
        {
            get { return nextAllowedUtc; }
        }

        internal bool IsInFlight
        {
            get { return activeLeaseId != 0; }
        }

        internal static int BoundIntervalMilliseconds(int intervalMilliseconds)
        {
            return Math.Max(
                MinimumIntervalMilliseconds,
                Math.Min(MaximumIntervalMilliseconds, intervalMilliseconds));
        }

        internal void InvalidateSession()
        {
            sessionGeneration = NextGeneration(sessionGeneration);
            topologyGeneration = NextGeneration(topologyGeneration);
            selectionGeneration = NextGeneration(selectionGeneration);
            ResetTopologyCursorAndBackoff();
        }

        internal void InvalidateTopology()
        {
            topologyGeneration = NextGeneration(topologyGeneration);
            selectionGeneration = NextGeneration(selectionGeneration);
            ResetTopologyCursorAndBackoff();
        }

        internal void InvalidateSelection()
        {
            selectionGeneration = NextGeneration(selectionGeneration);
        }

        internal bool TryBegin(
            TopologyIoLiveMonitorRequest request,
            out TopologyIoLiveMonitorLease lease,
            out TopologyIoLiveMonitorSkipReason skipReason)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            lease = null;
            skipReason = TopologyIoLiveMonitorSkipReason.None;
            if (!request.Enabled)
            {
                skipReason = TopologyIoLiveMonitorSkipReason.Disabled;
                return false;
            }

            if (!request.Connected)
            {
                skipReason = TopologyIoLiveMonitorSkipReason.Disconnected;
                return false;
            }

            if (request.Busy)
            {
                skipReason = TopologyIoLiveMonitorSkipReason.Busy;
                return false;
            }

            if (!request.SupportsTopology
                || request.TopologyRevision == 0)
            {
                skipReason = TopologyIoLiveMonitorSkipReason.MissingTopology;
                return false;
            }

            var canReadHealth = request.SupportsNodeHealth
                && request.HealthNodeIds != null
                && request.HealthNodeIds.Count != 0;
            var canReadSelectedInput = request.SupportsDigitalInput
                && request.SelectedInputNodeId != 0
                && request.SelectedInputReference != 0
                && request.SelectedInputWidth != 0;
            if (!canReadHealth && !canReadSelectedInput)
            {
                skipReason = TopologyIoLiveMonitorSkipReason.MissingCapabilities;
                return false;
            }

            if (activeLeaseId != 0)
            {
                skipReason = TopologyIoLiveMonitorSkipReason.InFlight;
                return false;
            }

            if (request.UtcNow < nextAllowedUtc)
            {
                skipReason = TopologyIoLiveMonitorSkipReason.Backoff;
                return false;
            }

            var readKind = TopologyIoLiveMonitorReadKind.None;
            uint healthNodeId = 0;
            if (canReadSelectedInput
                && (!canReadHealth || scheduleSelectedInputNext))
            {
                readKind = TopologyIoLiveMonitorReadKind.SelectedInput;
                scheduleSelectedInputNext = false;
            }
            else if (canReadHealth)
            {
                readKind = TopologyIoLiveMonitorReadKind.NodeHealth;
                if (nextHealthIndex >= request.HealthNodeIds.Count)
                {
                    nextHealthIndex = 0;
                }

                healthNodeId = request.HealthNodeIds[nextHealthIndex];
                nextHealthIndex++;
                if (healthNodeId == 0)
                {
                    throw new ArgumentException(
                        "Health node identities must be non-zero.",
                        "request");
                }

                scheduleSelectedInputNext = canReadSelectedInput;
            }

            nextLeaseId = NextGeneration(nextLeaseId);
            activeLeaseId = nextLeaseId;
            lease = new TopologyIoLiveMonitorLease(
                activeLeaseId,
                sessionGeneration,
                topologyGeneration,
                selectionGeneration,
                readKind,
                request.TopologyRevision,
                healthNodeId,
                readKind == TopologyIoLiveMonitorReadKind.SelectedInput
                    ? request.SelectedInputNodeId
                    : 0,
                readKind == TopologyIoLiveMonitorReadKind.SelectedInput
                    ? request.SelectedInputReference
                    : 0,
                readKind == TopologyIoLiveMonitorReadKind.SelectedInput
                    ? request.SelectedInputWidth
                    : (byte)0);
            return true;
        }

        internal bool CanCommitHealth(
            TopologyIoLiveMonitorLease lease,
            uint topologyRevision,
            uint nodeId)
        {
            return IsCurrentLease(lease)
                && lease.ReadsHealth
                && topologyRevision == lease.TopologyRevision
                && nodeId == lease.HealthNodeId;
        }

        internal bool CanCommitSelectedInput(
            TopologyIoLiveMonitorLease lease,
            uint topologyRevision,
            uint nodeId,
            uint ioReference,
            byte bitWidth)
        {
            return IsCurrentLease(lease)
                && lease.ReadsSelectedInput
                && topologyRevision == lease.TopologyRevision
                && nodeId == lease.SelectedInputNodeId
                && ioReference == lease.SelectedInputReference
                && bitWidth == lease.SelectedInputWidth;
        }

        internal bool CanProcessFailure(TopologyIoLiveMonitorLease lease)
        {
            return IsCurrentLease(lease);
        }

        internal void CompleteSuccess(
            TopologyIoLiveMonitorLease lease,
            DateTime utcNow)
        {
            var current = IsCurrentLease(lease);
            ReleaseLease(lease);
            if (!current)
            {
                return;
            }

            consecutiveFailures = 0;
            nextAllowedUtc = utcNow;
            lastFailureSignature = null;
            lastFailureLogUtc = DateTime.MinValue;
        }

        internal bool CompleteFailure(
            TopologyIoLiveMonitorLease lease,
            DateTime utcNow,
            string failureSignature)
        {
            var current = IsCurrentLease(lease);
            ReleaseLease(lease);
            if (!current)
            {
                return false;
            }

            consecutiveFailures = Math.Min(consecutiveFailures + 1, 30);
            var exponent = Math.Min(consecutiveFailures - 1, 4);
            var delay = Math.Min(
                MaximumFailureBackoffMilliseconds,
                InitialFailureBackoffMilliseconds * (1 << exponent));
            nextAllowedUtc = utcNow.AddMilliseconds(delay);

            var normalizedSignature = failureSignature ?? string.Empty;
            var sameFailure = string.Equals(
                normalizedSignature,
                lastFailureSignature,
                StringComparison.Ordinal);
            var suppress = sameFailure
                && lastFailureLogUtc != DateTime.MinValue
                && utcNow < lastFailureLogUtc.AddMilliseconds(
                    DuplicateFailureLogWindowMilliseconds);
            if (!suppress)
            {
                lastFailureSignature = normalizedSignature;
                lastFailureLogUtc = utcNow;
            }

            return !suppress;
        }

        internal void CompleteCancellation(TopologyIoLiveMonitorLease lease)
        {
            ReleaseLease(lease);
        }

        private bool IsCurrentLease(TopologyIoLiveMonitorLease lease)
        {
            return lease != null
                && lease.LeaseId != 0
                && lease.LeaseId == activeLeaseId
                && lease.SessionGeneration == sessionGeneration
                && lease.TopologyGeneration == topologyGeneration
                && lease.SelectionGeneration == selectionGeneration;
        }

        private void ReleaseLease(TopologyIoLiveMonitorLease lease)
        {
            if (lease != null && lease.LeaseId == activeLeaseId)
            {
                activeLeaseId = 0;
            }
        }

        private void ResetTopologyCursorAndBackoff()
        {
            nextHealthIndex = 0;
            scheduleSelectedInputNext = false;
            consecutiveFailures = 0;
            nextAllowedUtc = DateTime.MinValue;
            lastFailureLogUtc = DateTime.MinValue;
            lastFailureSignature = null;
        }

        private static long NextGeneration(long current)
        {
            var next = unchecked(current + 1);
            return next <= 0 ? 1 : next;
        }
    }
}

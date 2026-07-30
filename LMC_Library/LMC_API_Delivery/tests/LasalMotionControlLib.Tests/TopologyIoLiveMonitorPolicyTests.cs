using System;
using System.Collections.Generic;
using LasalMotionControlApiExample;

namespace LasalMotionControlLib.Tests
{
    internal static class TopologyIoLiveMonitorPolicyTests
    {
        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Policy.TopologyIoLiveMonitor.CapabilityOffNoLease",
                CapabilityOffNoLease);
            tests.Add(
                "Policy.TopologyIoLiveMonitor.RoundRobinAndSelectedInput",
                RoundRobinAndSelectedInput);
            tests.Add(
                "Policy.TopologyIoLiveMonitor.BusyAndSingleFlight",
                BusyAndSingleFlight);
            tests.Add(
                "Policy.TopologyIoLiveMonitor.InvalidationRejectsLateCommit",
                InvalidationRejectsLateCommit);
            tests.Add(
                "Policy.TopologyIoLiveMonitor.StaleFailureIsDiscardedAfterSelectionInvalidation",
                StaleFailureIsDiscardedAfterSelectionInvalidation);
            tests.Add(
                "Policy.TopologyIoLiveMonitor.FailureBackoffAndLogSuppression",
                FailureBackoffAndLogSuppression);
            tests.Add(
                "Policy.TopologyIoLiveMonitor.OutputIsNeverScheduled",
                OutputIsNeverScheduled);
        }

        private static void CapabilityOffNoLease()
        {
            var policy = new TopologyIoLiveMonitorPolicy();
            TopologyIoLiveMonitorLease lease;
            TopologyIoLiveMonitorSkipReason reason;
            var started = policy.TryBegin(
                Request(
                    supportsNodeHealth: false,
                    supportsDigitalInput: false),
                out lease,
                out reason);

            AssertEx.False(started);
            AssertEx.Equal(
                TopologyIoLiveMonitorSkipReason.MissingCapabilities,
                reason);
            AssertEx.False(policy.IsInFlight);
        }

        private static void RoundRobinAndSelectedInput()
        {
            var policy = new TopologyIoLiveMonitorPolicy();
            var now = new DateTime(2026, 7, 28, 1, 2, 3, DateTimeKind.Utc);
            TopologyIoLiveMonitorLease first;
            TopologyIoLiveMonitorSkipReason reason;
            AssertEx.True(
                policy.TryBegin(
                    Request(utcNow: now),
                    out first,
                    out reason));
            AssertEx.Equal(0x10000001u, first.HealthNodeId);
            AssertEx.True(first.ReadsHealth);
            AssertEx.False(first.ReadsSelectedInput);
            AssertEx.True(
                policy.CanCommitHealth(first, 0x15867EECu, 0x10000001u));
            policy.CompleteSuccess(first, now);

            TopologyIoLiveMonitorLease second;
            AssertEx.True(
                policy.TryBegin(
                    Request(utcNow: now.AddMilliseconds(750)),
                    out second,
                    out reason));
            AssertEx.False(second.ReadsHealth);
            AssertEx.True(second.ReadsSelectedInput);
            AssertEx.Equal(0x10000002u, second.SelectedInputNodeId);
            AssertEx.Equal(0x00010001u, second.SelectedInputReference);
            AssertEx.Equal((byte)32, second.SelectedInputWidth);
            AssertEx.True(
                policy.CanCommitSelectedInput(
                    second,
                    0x15867EECu,
                    0x10000002u,
                    0x00010001u,
                    32));
            policy.CompleteSuccess(second, now.AddMilliseconds(750));

            TopologyIoLiveMonitorLease third;
            AssertEx.True(
                policy.TryBegin(
                    Request(utcNow: now.AddMilliseconds(1500)),
                    out third,
                    out reason));
            AssertEx.Equal(0x20000001u, third.HealthNodeId);
            policy.CompleteSuccess(third, now.AddMilliseconds(1500));
        }

        private static void BusyAndSingleFlight()
        {
            var policy = new TopologyIoLiveMonitorPolicy();
            TopologyIoLiveMonitorLease lease;
            TopologyIoLiveMonitorSkipReason reason;
            AssertEx.False(
                policy.TryBegin(
                    Request(busy: true),
                    out lease,
                    out reason));
            AssertEx.Equal(TopologyIoLiveMonitorSkipReason.Busy, reason);

            AssertEx.True(
                policy.TryBegin(Request(), out lease, out reason));
            TopologyIoLiveMonitorLease duplicate;
            AssertEx.False(
                policy.TryBegin(Request(), out duplicate, out reason));
            AssertEx.Equal(TopologyIoLiveMonitorSkipReason.InFlight, reason);
            policy.CompleteCancellation(lease);
            AssertEx.False(policy.IsInFlight);
        }

        private static void InvalidationRejectsLateCommit()
        {
            var policy = new TopologyIoLiveMonitorPolicy();
            TopologyIoLiveMonitorLease lease;
            TopologyIoLiveMonitorSkipReason reason;
            AssertEx.True(
                policy.TryBegin(Request(), out lease, out reason));
            var oldSelection = policy.SelectionGeneration;
            policy.InvalidateSelection();
            AssertEx.True(policy.SelectionGeneration != oldSelection);
            AssertEx.False(
                policy.CanCommitHealth(
                    lease,
                    lease.TopologyRevision,
                    lease.HealthNodeId));
            AssertEx.False(
                policy.CanCommitSelectedInput(
                    lease,
                    lease.TopologyRevision,
                    lease.SelectedInputNodeId,
                    lease.SelectedInputReference,
                    lease.SelectedInputWidth));
            policy.CompleteCancellation(lease);
        }

        private static void StaleFailureIsDiscardedAfterSelectionInvalidation()
        {
            var policy = new TopologyIoLiveMonitorPolicy();
            var now = new DateTime(2026, 7, 28, 1, 2, 3, DateTimeKind.Utc);
            TopologyIoLiveMonitorLease lease;
            TopologyIoLiveMonitorSkipReason reason;
            AssertEx.True(
                policy.TryBegin(
                    Request(utcNow: now),
                    out lease,
                    out reason));

            policy.InvalidateSelection();

            AssertEx.False(policy.CanProcessFailure(lease));
            AssertEx.False(
                policy.CompleteFailure(lease, now, "stale selected DI failure"));
            AssertEx.Equal(0, policy.ConsecutiveFailures);
            AssertEx.Equal(DateTime.MinValue, policy.NextAllowedUtc);
            AssertEx.False(policy.IsInFlight);
        }

        private static void FailureBackoffAndLogSuppression()
        {
            var policy = new TopologyIoLiveMonitorPolicy();
            var now = new DateTime(2026, 7, 28, 1, 2, 3, DateTimeKind.Utc);
            TopologyIoLiveMonitorLease lease;
            TopologyIoLiveMonitorSkipReason reason;
            AssertEx.True(
                policy.TryBegin(
                    Request(utcNow: now),
                    out lease,
                    out reason));
            AssertEx.True(policy.CompleteFailure(lease, now, "timeout"));
            AssertEx.Equal(1, policy.ConsecutiveFailures);

            AssertEx.False(
                policy.TryBegin(
                    Request(utcNow: now.AddMilliseconds(999)),
                    out lease,
                    out reason));
            AssertEx.Equal(TopologyIoLiveMonitorSkipReason.Backoff, reason);
            AssertEx.True(
                policy.TryBegin(
                    Request(utcNow: now.AddMilliseconds(1000)),
                    out lease,
                    out reason));
            AssertEx.False(
                policy.CompleteFailure(
                    lease,
                    now.AddMilliseconds(1000),
                    "timeout"));

            AssertEx.True(
                policy.TryBegin(
                    Request(utcNow: now.AddMilliseconds(3000)),
                    out lease,
                    out reason));
            AssertEx.True(
                policy.CompleteFailure(
                    lease,
                    now.AddMilliseconds(3000),
                    "different"));
        }

        private static void OutputIsNeverScheduled()
        {
            var policy = new TopologyIoLiveMonitorPolicy();
            TopologyIoLiveMonitorLease lease;
            TopologyIoLiveMonitorSkipReason reason;
            AssertEx.True(
                policy.TryBegin(
                    Request(
                        supportsDigitalInput: true,
                        selectedInputNodeId: 0,
                        selectedInputReference: 0,
                        selectedInputWidth: 0),
                    out lease,
                    out reason));
            AssertEx.False(lease.ReadsSelectedInput);
            AssertEx.Equal(0u, lease.SelectedInputReference);
            policy.CompleteSuccess(lease, DateTime.UtcNow);
        }

        private static TopologyIoLiveMonitorRequest Request(
            bool enabled = true,
            bool connected = true,
            bool busy = false,
            bool supportsTopology = true,
            bool supportsNodeHealth = true,
            bool supportsDigitalInput = true,
            uint selectedInputNodeId = 0x10000002u,
            uint selectedInputReference = 0x00010001u,
            byte selectedInputWidth = 32,
            DateTime? utcNow = null)
        {
            return new TopologyIoLiveMonitorRequest(
                enabled,
                connected,
                busy,
                supportsTopology,
                supportsNodeHealth,
                supportsDigitalInput,
                0x15867EECu,
                new[] { 0x10000001u, 0x20000001u },
                selectedInputNodeId,
                selectedInputReference,
                selectedInputWidth,
                utcNow ?? new DateTime(
                    2026,
                    7,
                    28,
                    1,
                    2,
                    3,
                    DateTimeKind.Utc));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LasalMotionControlApiExample;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class D5SdoPendingCleanupOrchestratorTests
    {
        private const uint DiagnosticsBootId = 0x1234ABCDu;
        private const uint MapRevision = 0x957F101Eu;
        private const int PollMilliseconds = 25;
        private static readonly DateTime CurrentUtc =
            new DateTime(2026, 7, 27, 1, 2, 3, DateTimeKind.Utc);

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Qualification.D5PendingCleanup.PreflightFailsClosed",
                PreflightFailsClosed);
            tests.Add(
                "Qualification.D5PendingCleanup.IdentityMismatchQuarantines",
                IdentityMismatchQuarantines);
            tests.Add(
                "Qualification.D5PendingCleanup.CachedTerminalSkipsDispatch",
                CachedTerminalSkipsDispatch);
            tests.Add(
                "Qualification.D5PendingCleanup.CachedPendingRefreshes",
                CachedPendingRefreshes);
            tests.Add(
                "Qualification.D5PendingCleanup.QueuedCancelRequiresCancelled",
                QueuedCancelRequiresCancelled);
            tests.Add(
                "Qualification.D5PendingCleanup.InvalidStateRaceWaits",
                InvalidStateRaceWaits);
            tests.Add(
                "Qualification.D5PendingCleanup.RunningWaitsWithoutCancel",
                RunningWaitsWithoutCancel);
            tests.Add(
                "Qualification.D5PendingCleanup.CommandFailuresPreserved",
                CommandFailuresPreserved);
            tests.Add(
                "Qualification.D5PendingCleanup.TimeoutAndWaitBounds",
                TimeoutAndWaitBounds);
        }

        private static void PreflightFailsClosed()
        {
            using (var current = new LMCConnection())
            using (var previous = new LMCConnection())
            {
                var foreignTicket = Ticket(previous, 1);
                var ownerHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                var ownerError = AssertEx.Throws<InvalidOperationException>(
                    () => Resolve(
                        new D5SdoPendingCleanupRequest(
                            foreignTicket,
                            null,
                            previous,
                            current,
                            MapRevision,
                            null),
                        ownerHarness));
                AssertEx.Contains(
                    "earlier LMCConnection",
                    ownerError.Message);
                ownerHarness.AssertNoDispatch();

                var ownerClaimHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                AssertEx.Throws<InvalidOperationException>(
                    () => Resolve(
                        new D5SdoPendingCleanupRequest(
                            foreignTicket,
                            null,
                            current,
                            current,
                            MapRevision,
                            null),
                        ownerClaimHarness));
                ownerClaimHarness.AssertNoDispatch();

                var currentTicket = Ticket(current, 2);
                var mapHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                var mapError = AssertEx.Throws<InvalidOperationException>(
                    () => Resolve(
                        new D5SdoPendingCleanupRequest(
                            currentTicket,
                            null,
                            current,
                            current,
                            MapRevision + 1,
                            null),
                        mapHarness));
                AssertEx.Contains("MapRevision", mapError.Message);
                mapHarness.AssertNoDispatch();

                var wrongStatus = Status(
                    foreignTicket,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success);
                var statusHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                AssertEx.Throws<InvalidOperationException>(
                    () => Resolve(
                        new D5SdoPendingCleanupRequest(
                            currentTicket,
                            wrongStatus,
                            current,
                            current,
                            MapRevision,
                            null),
                        statusHarness));
                statusHarness.AssertNoDispatch();
            }
        }

        private static void IdentityMismatchQuarantines()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = Ticket(connection, 10);
                var bootHarness = new Harness(
                    DiagnosticsBootId + 1,
                    MapRevision);
                var bootResult = Resolve(
                    Request(connection, ticket, null, null),
                    bootHarness);
                AssertEx.False(bootResult.IsResolved);
                AssertEx.Equal(
                    D5SdoPendingCleanupDisposition
                        .QuarantineDiagnosticsBootIdChanged,
                    bootResult.Disposition);
                AssertEx.Equal(
                    "diagnostics_boot_id_changed",
                    bootResult.QuarantineReason);
                AssertEx.Equal(1, bootHarness.CapabilitiesCount);
                AssertEx.Equal(0, bootHarness.StatusCount);
                AssertEx.Equal(0, bootHarness.CancelCount);

                var mapHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision + 1);
                var mapResult = Resolve(
                    Request(connection, ticket, null, null),
                    mapHarness);
                AssertEx.Equal(
                    D5SdoPendingCleanupDisposition
                        .QuarantineMapRevisionChanged,
                    mapResult.Disposition);
                AssertEx.Equal(
                    "diagnostics_map_revision_changed",
                    mapResult.QuarantineReason);
                AssertEx.Equal(1, mapHarness.CapabilitiesCount);
                AssertEx.Equal(0, mapHarness.StatusCount);
                AssertEx.Equal(0, mapHarness.CancelCount);

                var bothChangedHarness = new Harness(
                    DiagnosticsBootId + 1,
                    MapRevision + 1);
                var bothChangedResult = Resolve(
                    Request(connection, ticket, null, null),
                    bothChangedHarness);
                AssertEx.Equal(
                    D5SdoPendingCleanupDisposition
                        .QuarantineDiagnosticsBootIdChanged,
                    bothChangedResult.Disposition);
                AssertEx.Equal(
                    "diagnostics_boot_id_changed",
                    bothChangedResult.QuarantineReason);
                AssertEx.Equal(0, bothChangedHarness.StatusCount);
                AssertEx.Equal(0, bothChangedHarness.CancelCount);

                var nullCapabilitiesHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                nullCapabilitiesHarness.ReturnNullCapabilities = true;
                AssertEx.Throws<InvalidOperationException>(
                    () => Resolve(
                        Request(connection, ticket, null, null),
                        nullCapabilitiesHarness));
                AssertEx.Equal(1, nullCapabilitiesHarness.CapabilitiesCount);
                AssertEx.Equal(0, nullCapabilitiesHarness.StatusCount);
                AssertEx.Equal(0, nullCapabilitiesHarness.CancelCount);

                var zeroBootHarness = new Harness(0, MapRevision);
                AssertEx.Throws<InvalidOperationException>(
                    () => Resolve(
                        Request(connection, ticket, null, null),
                        zeroBootHarness));
                AssertEx.Equal(1, zeroBootHarness.CapabilitiesCount);
                AssertEx.Equal(0, zeroBootHarness.StatusCount);
                AssertEx.Equal(0, zeroBootHarness.CancelCount);

                var zeroMapHarness = new Harness(DiagnosticsBootId, 0);
                AssertEx.Throws<InvalidOperationException>(
                    () => Resolve(
                        Request(connection, ticket, null, null),
                        zeroMapHarness));
                AssertEx.Equal(1, zeroMapHarness.CapabilitiesCount);
                AssertEx.Equal(0, zeroMapHarness.StatusCount);
                AssertEx.Equal(0, zeroMapHarness.CancelCount);
            }
        }

        private static void CachedTerminalSkipsDispatch()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = Ticket(connection, 20);
                var states = new[]
                {
                    LMCOperationState.Completed,
                    LMCOperationState.Failed,
                    LMCOperationState.Cancelled,
                    LMCOperationState.Expired
                };
                var outcomes = new[]
                {
                    LMCOperationOutcome.Success,
                    LMCOperationOutcome.Failed,
                    LMCOperationOutcome.Cancelled,
                    LMCOperationOutcome.TimedOut
                };

                for (var index = 0; index < states.Length; index++)
                {
                    var terminal = Status(
                        ticket,
                        states[index],
                        outcomes[index]);
                    var harness = new Harness(
                        DiagnosticsBootId,
                        MapRevision);

                    var result = Resolve(
                        Request(connection, ticket, terminal, null),
                        harness);

                    AssertEx.True(result.IsResolved);
                    AssertEx.True(result.UsedCachedTerminal);
                    AssertEx.True(ReferenceEquals(terminal, result.Status));
                    AssertEx.False(result.CancelAttempted);
                    AssertEx.Equal(0, result.StatusReadCount);
                    AssertEx.Equal(0, result.WaitMilliseconds);
                    AssertEx.Equal(1, harness.CapabilitiesCount);
                    AssertEx.Equal(0, harness.StatusCount);
                    AssertEx.Equal(0, harness.CancelCount);
                    AssertEx.Equal(0, harness.ObservedCount);
                    AssertEventOrder(harness, "cleanup-started");
                }
            }
        }

        private static void CachedPendingRefreshes()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = Ticket(connection, 25);
                var cachedQueued = Status(
                    ticket,
                    LMCOperationState.Queued,
                    LMCOperationOutcome.NoneOrPending);
                var queuedToRunning = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                queuedToRunning.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending));
                queuedToRunning.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success));

                var runningResult = Resolve(
                    Request(connection, ticket, cachedQueued, null),
                    queuedToRunning);
                AssertEx.True(runningResult.IsResolved);
                AssertEx.False(runningResult.UsedCachedTerminal);
                AssertEx.False(runningResult.CancelAttempted);
                AssertEx.Equal(0, queuedToRunning.CancelCount);
                AssertEx.Equal(2, queuedToRunning.StatusCount);

                var cachedRunning = Status(
                    ticket,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending);
                var runningToQueued = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                runningToQueued.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Queued,
                    LMCOperationOutcome.NoneOrPending));
                runningToQueued.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Cancelled,
                    LMCOperationOutcome.Cancelled));

                var queuedResult = Resolve(
                    Request(connection, ticket, cachedRunning, null),
                    runningToQueued);
                AssertEx.True(queuedResult.IsResolved);
                AssertEx.True(queuedResult.CancelAttempted);
                AssertEx.True(queuedResult.CancelAccepted);
                AssertEx.Equal(1, runningToQueued.CancelCount);
                AssertEx.Equal(2, runningToQueued.StatusCount);
            }
        }

        private static void QueuedCancelRequiresCancelled()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = Ticket(connection, 30);
                var harness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                harness.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Queued,
                    LMCOperationOutcome.NoneOrPending));
                var cancelled = Status(
                    ticket,
                    LMCOperationState.Cancelled,
                    LMCOperationOutcome.Cancelled);
                harness.Statuses.Enqueue(cancelled);

                var result = Resolve(
                    Request(connection, ticket, null, null),
                    harness);

                AssertEx.True(result.IsResolved);
                AssertEx.True(result.CancelAttempted);
                AssertEx.True(result.CancelAccepted);
                AssertEx.False(result.CancelRaceResolved);
                AssertEx.True(ReferenceEquals(cancelled, result.Status));
                AssertEx.Equal(2, result.StatusReadCount);
                AssertEx.Equal(1, harness.CancelCount);
                AssertEx.Equal(2, harness.ObservedCount);
                AssertEventOrder(
                    harness,
                    "cleanup-started",
                    "status",
                    "cancel-accepted",
                    "status");

                var invalidHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                invalidHarness.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Queued,
                    LMCOperationOutcome.NoneOrPending));
                invalidHarness.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success));
                var error = AssertEx.Throws<InvalidOperationException>(
                    () => Resolve(
                        Request(connection, ticket, null, null),
                        invalidHarness));
                AssertEx.Contains("Cancelled/Cancelled", error.Message);
                AssertEx.Equal(1, invalidHarness.CancelCount);
            }
        }

        private static void InvalidStateRaceWaits()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = Ticket(connection, 40);
                var harness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                harness.CancelError = DiagnosticsError(
                    LMCDiagnosticsDetailCode.InvalidState);
                harness.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Queued,
                    LMCOperationOutcome.NoneOrPending));
                harness.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending));
                var completed = Status(
                    ticket,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success);
                harness.Statuses.Enqueue(completed);

                var result = Resolve(
                    Request(connection, ticket, null, null),
                    harness);

                AssertEx.True(result.IsResolved);
                AssertEx.True(result.CancelAttempted);
                AssertEx.False(result.CancelAccepted);
                AssertEx.True(result.CancelRaceResolved);
                AssertEx.True(ReferenceEquals(completed, result.Status));
                AssertEx.Equal(3, result.StatusReadCount);
                AssertEx.Equal(1, harness.CancelCount);
                AssertEx.Equal(1, harness.DelayCount);
                AssertEventOrder(
                    harness,
                    "cleanup-started",
                    "status",
                    "cancel-race-resolved",
                    "status",
                    "status");
            }
        }

        private static void RunningWaitsWithoutCancel()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = Ticket(connection, 50);
                var harness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                harness.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending));
                var failed = Status(
                    ticket,
                    LMCOperationState.Failed,
                    LMCOperationOutcome.Failed);
                harness.Statuses.Enqueue(failed);

                var result = Resolve(
                    Request(connection, ticket, null, null),
                    harness);

                AssertEx.True(result.IsResolved);
                AssertEx.False(result.CancelAttempted);
                AssertEx.Equal(0, harness.CancelCount);
                AssertEx.True(ReferenceEquals(failed, result.Status));
                AssertEx.Equal(2, result.StatusReadCount);
            }
        }

        private static void CommandFailuresPreserved()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = Ticket(connection, 60);
                var capabilitiesFailure = DiagnosticsError(
                    LMCDiagnosticsDetailCode.ResourceBusy);
                var capabilitiesHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                capabilitiesHarness.CapabilitiesError = capabilitiesFailure;
                var observedCapabilitiesFailure =
                    AssertEx.Throws<LMCDiagnosticsCommandException>(
                        () => Resolve(
                            Request(connection, ticket, null, null),
                            capabilitiesHarness));
                AssertEx.True(ReferenceEquals(
                    capabilitiesFailure,
                    observedCapabilitiesFailure));
                AssertEx.Equal(0, capabilitiesHarness.StatusCount);
                AssertEx.Equal(0, capabilitiesHarness.CancelCount);

                var statusFailure = DiagnosticsError(
                    LMCDiagnosticsDetailCode.ResourceBusy);
                var statusHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                statusHarness.StatusError = statusFailure;
                var observedStatusFailure =
                    AssertEx.Throws<LMCDiagnosticsCommandException>(
                        () => Resolve(
                            Request(connection, ticket, null, null),
                            statusHarness));
                AssertEx.True(ReferenceEquals(
                    statusFailure,
                    observedStatusFailure));

                var cancelFailure = DiagnosticsError(
                    LMCDiagnosticsDetailCode.ResourceBusy);
                var cancelHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                cancelHarness.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Queued,
                    LMCOperationOutcome.NoneOrPending));
                cancelHarness.CancelError = cancelFailure;
                var observedCancelFailure =
                    AssertEx.Throws<LMCDiagnosticsCommandException>(
                        () => Resolve(
                            Request(connection, ticket, null, null),
                            cancelHarness));
                AssertEx.True(ReferenceEquals(
                    cancelFailure,
                    observedCancelFailure));
                AssertEx.Equal(1, cancelHarness.StatusCount);
                AssertEx.Equal(1, cancelHarness.CancelCount);

                var nullResponseCancel = new LMCDiagnosticsCommandException(
                    "cancel response missing",
                    null);
                var nullResponseHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                nullResponseHarness.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Queued,
                    LMCOperationOutcome.NoneOrPending));
                nullResponseHarness.CancelError = nullResponseCancel;
                var observedNullResponse =
                    AssertEx.Throws<LMCDiagnosticsCommandException>(
                        () => Resolve(
                            Request(connection, ticket, null, null),
                            nullResponseHarness));
                AssertEx.True(ReferenceEquals(
                    nullResponseCancel,
                    observedNullResponse));

                var timeoutFailure = new TimeoutException(
                    "transport timeout");
                var timeoutHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                timeoutHarness.StatusError = timeoutFailure;
                var observedTimeoutFailure =
                    AssertEx.Throws<TimeoutException>(
                        () => Resolve(
                            Request(connection, ticket, null, null),
                            timeoutHarness));
                AssertEx.True(ReferenceEquals(
                    timeoutFailure,
                    observedTimeoutFailure));
            }
        }

        private static void TimeoutAndWaitBounds()
        {
            AssertEx.Equal(
                15000,
                D5SdoPendingCleanupOrchestrator
                    .CalculateWaitMilliseconds(null, CurrentUtc));
            AssertEx.Equal(
                15000,
                D5SdoPendingCleanupOrchestrator
                    .CalculateWaitMilliseconds(
                        CurrentUtc.AddSeconds(14),
                        CurrentUtc));
            AssertEx.Equal(
                120000,
                D5SdoPendingCleanupOrchestrator
                    .CalculateWaitMilliseconds(
                        CurrentUtc.AddSeconds(119),
                        CurrentUtc));
            AssertEx.Equal(
                21000,
                D5SdoPendingCleanupOrchestrator
                    .CalculateWaitMilliseconds(
                        CurrentUtc.AddSeconds(20),
                        CurrentUtc));
            AssertEx.Equal(
                120000,
                D5SdoPendingCleanupOrchestrator
                    .CalculateWaitMilliseconds(
                        CurrentUtc.AddMinutes(10),
                        CurrentUtc));

            using (var connection = new LMCConnection())
            {
                var ticket = Ticket(connection, 70);
                var harness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                harness.DelayAdvanceMilliseconds = 15001;
                harness.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending));
                harness.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending));

                var error = AssertEx.Throws<TimeoutException>(
                    () => Resolve(
                        Request(connection, ticket, null, null),
                        harness));
                AssertEx.Contains("15000 ms cleanup bound", error.Message);
                AssertEx.Equal(2, harness.StatusCount);
                AssertEx.Equal(0, harness.CancelCount);
                AssertEx.Equal(1, harness.DelayCount);

                var boundaryHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                boundaryHarness.DelayAdvanceMilliseconds = 15000;
                boundaryHarness.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending));
                boundaryHarness.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending));
                boundaryHarness.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending));

                AssertEx.Throws<TimeoutException>(
                    () => Resolve(
                        Request(connection, ticket, null, null),
                        boundaryHarness));
                AssertEx.Equal(3, boundaryHarness.StatusCount);
                AssertEx.Equal(2, boundaryHarness.DelayCount);
            }
        }

        private static D5SdoPendingCleanupResult Resolve(
            D5SdoPendingCleanupRequest request,
            Harness harness)
        {
            return D5SdoPendingCleanupOrchestrator.CleanupAsync(
                    request,
                    harness.CreateOperations(),
                    PollMilliseconds)
                .GetAwaiter()
                .GetResult();
        }

        private static void AssertEventOrder(
            Harness harness,
            params string[] expected)
        {
            AssertEx.Equal(expected.Length, harness.Events.Count);
            for (var index = 0; index < expected.Length; index++)
            {
                AssertEx.Equal(expected[index], harness.Events[index]);
            }
        }

        private static D5SdoPendingCleanupRequest Request(
            LMCConnection connection,
            LMCOperationTicket ticket,
            LMCOperationStatus cachedStatus,
            DateTime? deadlineUtc)
        {
            return new D5SdoPendingCleanupRequest(
                ticket,
                cachedStatus,
                connection,
                connection,
                MapRevision,
                deadlineUtc);
        }

        private static LMCOperationTicket Ticket(
            LMCConnection connection,
            uint ticketId)
        {
            return new LMCOperationTicket(
                ticketId,
                LMCOperationKind.SDORead,
                10,
                DiagnosticsBootId,
                MapRevision,
                1,
                connection.Diagnostics,
                true,
                1,
                LMCSignalValueType.Int8);
        }

        private static LMCOperationStatus Status(
            LMCOperationTicket ticket,
            LMCOperationState state,
            LMCOperationOutcome outcome)
        {
            var pending = state == LMCOperationState.Queued
                || state == LMCOperationState.Running;
            var successful = state == LMCOperationState.Completed
                && outcome == LMCOperationOutcome.Success;
            return new LMCOperationStatus(
                Response(LMCDiagnosticsDetailCode.None),
                ticket.TicketId,
                ticket.OperationKind,
                state,
                10,
                pending ? 0u : 11u,
                outcome,
                pending || successful ? (short)0 : (short)-1,
                pending || successful ? 0u : 1u,
                successful ? 1u : 0u,
                successful
                    ? LMCSignalValueType.Int8
                    : LMCSignalValueType.Invalid,
                successful ? new byte[] { 8 } : new byte[0],
                ticket.DiagnosticsBootId);
        }

        private static LMCDiagnosticsCommandException DiagnosticsError(
            LMCDiagnosticsDetailCode detail)
        {
            return new LMCDiagnosticsCommandException(
                "test diagnostics failure",
                Response(detail));
        }

        private static LMCDiagnosticsResponse Response(
            LMCDiagnosticsDetailCode detail)
        {
            return new LMCDiagnosticsResponse(
                new LMC_Response
                {
                    IsFrameValid = true,
                    HeaderStatus = 0
                },
                1,
                LMCDiagnosticsResponseFlags.None,
                detail == LMCDiagnosticsDetailCode.None
                    ? (ushort)0
                    : (ushort)1,
                detail == LMCDiagnosticsDetailCode.None
                    ? (short)0
                    : (short)-32000,
                1,
                (uint)detail);
        }

        private sealed class Harness
        {
            private readonly uint diagnosticsBootId;
            private readonly uint mapRevision;

            internal Harness(uint diagnosticsBootId, uint mapRevision)
            {
                this.diagnosticsBootId = diagnosticsBootId;
                this.mapRevision = mapRevision;
                Statuses = new Queue<LMCOperationStatus>();
                Events = new List<string>();
                DelayAdvanceMilliseconds = PollMilliseconds;
            }

            internal Queue<LMCOperationStatus> Statuses { get; private set; }
            internal List<string> Events { get; private set; }
            internal Exception CapabilitiesError { get; set; }
            internal Exception StatusError { get; set; }
            internal Exception CancelError { get; set; }
            internal bool ReturnNullCapabilities { get; set; }
            internal int DelayAdvanceMilliseconds { get; set; }
            internal int CapabilitiesCount { get; private set; }
            internal int StatusCount { get; private set; }
            internal int CancelCount { get; private set; }
            internal int DelayCount { get; private set; }
            internal int ObservedCount { get; private set; }
            internal long ElapsedMilliseconds { get; private set; }

            internal D5SdoPendingCleanupOperations CreateOperations()
            {
                return new D5SdoPendingCleanupOperations
                {
                    ReadCapabilitiesAsync = () =>
                    {
                        CapabilitiesCount++;
                        if (CapabilitiesError != null)
                        {
                            return TaskFromException<
                                LMCDiagnosticCapabilities>(
                                    CapabilitiesError);
                        }

                        if (ReturnNullCapabilities)
                        {
                            return Task.FromResult<LMCDiagnosticCapabilities>(
                                null);
                        }

                        return Task.FromResult(Capabilities(
                            diagnosticsBootId,
                            mapRevision));
                    },
                    ReadStatusAsync = ticket =>
                    {
                        StatusCount++;
                        if (StatusError != null)
                        {
                            return TaskFromException<LMCOperationStatus>(
                                StatusError);
                        }

                        if (Statuses.Count == 0)
                        {
                            throw new InvalidOperationException(
                                "No queued test status remains.");
                        }

                        return Task.FromResult(Statuses.Dequeue());
                    },
                    CancelAsync = ticket =>
                    {
                        CancelCount++;
                        if (CancelError != null)
                        {
                            return TaskFromException(CancelError);
                        }

                        return Task.FromResult(true);
                    },
                    DelayAsync = milliseconds =>
                    {
                        DelayCount++;
                        ElapsedMilliseconds +=
                            DelayAdvanceMilliseconds;
                        return Task.FromResult(true);
                    },
                    ReadUtcNow = () => CurrentUtc,
                    ReadWaitElapsedMilliseconds =
                        () => ElapsedMilliseconds,
                    StatusObserved = status =>
                    {
                        ObservedCount++;
                        Events.Add("status");
                    },
                    CleanupStarted = () => Events.Add("cleanup-started"),
                    CancelAccepted = () => Events.Add("cancel-accepted"),
                    CancelRaceResolved =
                        () => Events.Add("cancel-race-resolved")
                };
            }

            internal void AssertNoDispatch()
            {
                AssertEx.Equal(0, CapabilitiesCount);
                AssertEx.Equal(0, StatusCount);
                AssertEx.Equal(0, CancelCount);
            }

            private static LMCDiagnosticCapabilities Capabilities(
                uint diagnosticsBootId,
                uint mapRevision)
            {
                return new LMCDiagnosticCapabilities(
                    Response(LMCDiagnosticsDetailCode.None),
                    1,
                    1,
                    (uint)LMCDiagnosticCapability.SDORead,
                    mapRevision,
                    0,
                    0,
                    0,
                    0,
                    0,
                    1000,
                    1320,
                    2040,
                    1280,
                    80,
                    16,
                    0,
                    12,
                    diagnosticsBootId);
            }

            private static Task<T> TaskFromException<T>(Exception error)
            {
                var completion = new TaskCompletionSource<T>();
                completion.SetException(error);
                return completion.Task;
            }

            private static Task TaskFromException(Exception error)
            {
                var completion = new TaskCompletionSource<bool>();
                completion.SetException(error);
                return completion.Task;
            }
        }
    }
}
